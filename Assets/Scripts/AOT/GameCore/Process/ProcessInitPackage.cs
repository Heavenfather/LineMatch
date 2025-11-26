using System;
using Cysharp.Threading.Tasks;
using GameCore.Localization;
using GameCore.Logic;
using GameCore.Resource;
using GameCore.SDK;
using GameCore.Settings;
using YooAsset;
using Logger = GameCore.Log.Logger;

namespace GameCore.Process
{
    /// <summary>
    /// 初始化资源包
    /// </summary>
    public class ProcessInitPackage : IProcess
    {
        private bool _initSuccess;

        public void Init()
        {
            _initSuccess = true;
        }

        public void Enter()
        {
            HotUpdateManager.Instance.UpdateProgress(EUpdateState.InitializePackage);
            if (!YooAssets.Initialized)
            {
                YooAssets.Initialize(new AssetsLogger());
            }

            //初始化所有的资源包，但是进入热更前只更新主包
            //热更域后续不用初始化资源包，可以直接通过Driver获取资源包
            string[] package = GameSettings.Instance.ProjectSetting.PackageNames;
            int finishCount = 0;
            for (int i = 0; i < package.Length; i++)
            {
                int index = i;
                InitPackage(package[index], (b) =>
                {
                    if (b)
                    {
                        finishCount++;
                        if (finishCount == package.Length)
                            EnterNext();
                    }
                    else
                    {
                        if (!_initSuccess) return;
                        _initSuccess = false;
                        Logger.Error($"初始化 [{package[index]}] 资源包失败");
                        HotUpdateManager.Instance.ShowRetryInfo(LocalizationPool.Get("UpdateInitPackageFail"),
                            this.Enter);
                    }
                }).Forget();
            }
        }

        public void Leave()
        {
        }

        public void Update()
        {
        }

        private void EnterNext()
        {
            var info = SDKMgr.Instance.GetDeviceSystemInfo();
            if (info.IsEnableDebug)
            {
#if !UNITY_EDITOR
                HotUpdateManager.Instance.ShowVersionPanel(() =>
                {
                    ProcessManager.Instance.Enter<ProcessUpdateVersion>();
                });
#else
                ProcessManager.Instance.Enter<ProcessUpdateVersion>();
#endif
            }
            else
            {
                ProcessManager.Instance.Enter<ProcessUpdateVersion>();
            }
        }

        private async UniTask InitPackage(string packageName, Action<bool> onFinish)
        {
            try
            {
                if (ResourceModuleDriver.Instance.TryGetValue(packageName, out var resourcePackage))
                {
                    if (resourcePackage.InitializeStatus is EOperationStatus.Processing or EOperationStatus.Succeed)
                    {
                        Logger.Error($"ResourceSystem has already init package : {packageName}");
                        onFinish?.Invoke(false);
                    }
                    else
                    {
                        ResourceModuleDriver.Instance.RemovePackage(packageName);
                    }
                }


#if UNITY_WEBGL && !UNITY_EDITOR
                GameSettings.Instance.ProjectSetting.PlayMode = EPlayMode.WebPlayMode;
#elif (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE) && !UNITY_EDITOR
                GameSettings.Instance.ProjectSetting.PlayMode = EPlayMode.HostPlayMode;
#endif
                EPlayMode playMode = GameSettings.Instance.ProjectSetting.PlayMode;
                var package = YooAssets.TryGetPackage(packageName);
                if (package == null)
                {
                    package = YooAssets.CreatePackage(packageName);
                }

                InitializationOperation initializationOperation = null;

                // 编辑器下的模拟模式
                if (playMode == EPlayMode.EditorSimulateMode)
                {
                    var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                    var packageRoot = buildResult.PackageRootDirectory;
                    var createParameters = new EditorSimulateModeParameters();
                    createParameters.EditorFileSystemParameters =
                        FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                    initializationOperation = package.InitializeAsync(createParameters);
                }

                // 单机运行模式
                if (playMode == EPlayMode.OfflinePlayMode)
                {
                    var createParameters = new OfflinePlayModeParameters();
                    createParameters.BuildinFileSystemParameters =
                        FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                    initializationOperation = package.InitializeAsync(createParameters);
                }

                // 联机运行模式
                if (playMode == EPlayMode.HostPlayMode)
                {
                    string defaultHostServer = GameSettings.Instance.ProjectSetting.CDN;
                    string fallbackHostServer = GameSettings.Instance.ProjectSetting.FallBackCDN;
                    IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                    var createParameters = new HostPlayModeParameters();
                    createParameters.BuildinFileSystemParameters =
                        FileSystemParameters.CreateDefaultBuildinFileSystemParameters(new FileOffsetDecryption());
                    createParameters.CacheFileSystemParameters =
                        FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices,
                            new FileOffsetDecryption());
                    initializationOperation = package.InitializeAsync(createParameters);
                }


                // WebGL运行模式
                if (playMode == EPlayMode.WebPlayMode)
                {
                    var createParameters = new WebPlayModeParameters();
                    IWebDecryptionServices webDecryptionServices = new FileOffsetWebDecryption();
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
                Logger.Info("Init WechatMinigame Mode Package");
                string defaultHostServer = GameSettings.Instance.ProjectSetting.CDN;
                string fallbackHostServer = GameSettings.Instance.ProjectSetting.FallBackCDN;
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                string rootPath = $"{GameSettings.Instance.ProjectSetting.AppMode}";
                Logger.Debug($"Wechat root path : {rootPath}");
                createParameters.WebServerFileSystemParameters =
                    WechatFileSystemCreater.CreateFileSystemParameters(rootPath, remoteServices);
#elif UNITY_WEBGL && DOUYINMINIGAME && !UNITY_EDITOR
                Logger.Info("Init DouyinMinigame Mode Package");
                string defaultHostServer = GameSettings.Instance.ProjectSetting.CDN;
                string fallbackHostServer = GameSettings.Instance.ProjectSetting.FallBackCDN;
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                string rootPath = $"{GameSettings.Instance.ProjectSetting.AppMode}";
                Logger.Debug($"Douyin root path : {rootPath}");
                createParameters.WebServerFileSystemParameters =
                TiktokFileSystemCreater.CreateFileSystemParameters(rootPath,remoteServices);
#else
                    Logger.Info("Init WebGL Mode Package");
                    createParameters.WebServerFileSystemParameters =
                        FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
#endif
                    initializationOperation = package.InitializeAsync(createParameters);
                }

                await initializationOperation.ToUniTask();

                Logger.Info($"Init resource package {packageName} : {initializationOperation?.Status}");

                if (initializationOperation is { Status: EOperationStatus.Succeed })
                {
                    ResourceModuleDriver.Instance.AddPackage(packageName, package);
                    onFinish?.Invoke(true);
                }
                else
                {
                    onFinish?.Invoke(false);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                onFinish?.Invoke(false);
            }
        }
    }
}