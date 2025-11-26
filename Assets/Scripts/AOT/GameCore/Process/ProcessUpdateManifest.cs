
using Cysharp.Threading.Tasks;
using GameCore.Localization;
using GameCore.Log;
using GameCore.Logic;
using GameCore.Resource;
using GameCore.Settings;
using YooAsset;

namespace GameCore.Process
{
    public class ProcessUpdateManifest : IProcess
    {
        public void Init()
        {
            
        }

        public void Enter()
        {
            HotUpdateManager.Instance.UpdateProgress(EUpdateState.UpdateResManifest);
            UpdateManifest().Forget();
        }

        public void Leave()
        {
        }

        public void Update()
        {
        }

        
        private async UniTaskVoid UpdateManifest()
        {
            ResourcePackage defaultPackage = ResourceModuleDriver.Instance.GetDefaultPackage();
            var operation = defaultPackage.UpdatePackageManifestAsync(HotUpdateManager.Instance.PackageVersion);
            await operation.Task;
            if (operation.Status != EOperationStatus.Succeed)
            {
                //重试
                Logger.Error($"更新 [{defaultPackage.PackageName}] 版本 [{HotUpdateManager.Instance.PackageVersion}] 失败");
                HotUpdateManager.Instance.ShowRetryInfo(LocalizationPool.Get("UpdatePackageManifestFail"), this.Enter);
                return;
            }

            if (GameSettings.Instance.ProjectSetting.PlayMode == EPlayMode.WebPlayMode)
            {
                //WEBGL边玩边下，直接进入到预加载流程
                ProcessManager.Instance.Enter<ProcessPreLoad>();
                return;
            }
            ProcessManager.Instance.Enter<ProcessDownloader>();
        }

    }
}