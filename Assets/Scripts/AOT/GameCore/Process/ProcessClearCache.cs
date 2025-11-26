using GameCore.Logic;
using GameCore.Resource;
using YooAsset;

namespace GameCore.Process
{
    public class ProcessClearCache : IProcess
    {
        public void Init()
        {
        }

        public void Enter()
        {
            HotUpdateManager.Instance.UpdateProgress(EUpdateState.DownloadDoneClearCache);
            //当前只清理主资源包，其它资源包再在热更代码中执行清理
            var package = ResourceModuleDriver.Instance.GetDefaultPackage();
            var handle = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            handle.Completed += OnClearCacheComplete;
        }

        public void Leave()
        {
        }

        public void Update()
        {
            
        }
        
        private void OnClearCacheComplete(AsyncOperationBase obj)
        {
            if(obj.Status != EOperationStatus.Succeed)
                return;
            ProcessManager.Instance.Enter<ProcessPreLoad>();
        }
    }
}