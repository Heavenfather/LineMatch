using Cysharp.Threading.Tasks;
using GameCore.Localization;
using GameCore.Log;
using GameCore.Logic;
using GameCore.Resource;
using GameCore.Utils;
using YooAsset;

namespace GameCore.Process
{
    /// <summary>
    /// 更新资源版本
    /// </summary>
    public class ProcessUpdateVersion : IProcess
    {
        public void Init()
        {
        }

        public void Enter()
        {
            HotUpdateManager.Instance.UpdateProgress(EUpdateState.UpdateResVersion);
            UpdateVersion().Forget();
        }

        public void Leave()
        {
        }

        public void Update()
        {
        }

        private async UniTaskVoid UpdateVersion()
        {
            string targetVersion = HotUpdateManager.Instance.TargetVersion;
            if (string.IsNullOrEmpty(targetVersion))
            {
                ResourcePackage defaultPackage = ResourceModuleDriver.Instance.GetDefaultPackage();
                var operation = defaultPackage.RequestPackageVersionAsync();
                await operation.Task;
                if (operation.Status != EOperationStatus.Succeed)
                {
                    //重试
                    Logger.Error(
                        $"更新资源包 [{defaultPackage.PackageName}] 失败 error:{operation.Error} state:{operation.Status}");
                    HotUpdateManager.Instance.ShowRetryInfo(LocalizationPool.Get("UpdatePackageVersionFail"),
                        this.Enter);
                    return;
                }

                targetVersion = operation.PackageVersion;
            }
            HotUpdateManager.Instance.PackageVersion = targetVersion;
            //更新版本缓存版本号
            PlayerPrefsUtil.SetString("GAME_RES_VERSION", targetVersion);
            HotUpdateManager.Instance.UpdateLoadingVersion();
            ProcessManager.Instance.Enter<ProcessUpdateManifest>();
        }
    }
}