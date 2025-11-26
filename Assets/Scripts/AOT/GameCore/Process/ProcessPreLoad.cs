using GameCore.Localization;
using GameCore.Log;
using GameCore.Logic;
using GameCore.Resource;
using YooAsset;

namespace GameCore.Process
{
    public class ProcessPreLoad : IProcess
    {
        private int _loadedCount = 0;
        private int _preloadTotal = 0;
        private bool _isLoadFail = false;
        
        public void Init()
        {
            
        }

        public void Enter()
        {
            _isLoadFail = false;
            _loadedCount = 0;
            HotUpdateManager.Instance.UpdateProgress(EUpdateState.UpdatePreloading);
            StartPreloading();
        }

        public void Leave()
        {
            
        }

        public void Update()
        {
            if(_loadedCount < _preloadTotal)
                return;
            ProcessManager.Instance.Enter<ProcessLoadAssembly>();
        }

        private void StartPreloading()
        {
            var info = GetPreLoadAssets();
            _preloadTotal = info.Length;
            var package = ResourceModuleDriver.Instance.GetDefaultPackage();
            for (int i = 0; i < _preloadTotal; i++)
            {
                //预热bundle资源
                var handle = package.LoadAssetAsync(info[i]);
                handle.Completed += OnLoadAssetCompleted;
            }
        }
        
        /// <summary>
        /// 从默认资源包中加载预加载标签PreLoad的资源
        /// </summary>
        /// <returns></returns>
        private AssetInfo[] GetPreLoadAssets()
        {
            var package = ResourceModuleDriver.Instance.GetDefaultPackage();
            return package.GetAssetInfos("PreLoad");
        }

        /// <summary>
        /// 资源加载回调
        /// </summary>
        /// <param name="handle"></param>
        private void OnLoadAssetCompleted(AssetHandle handle)
        {
            if(!handle.IsDone)
                return;
            if (handle.AssetObject == null || handle.Status != EOperationStatus.Succeed)
            {
                if (!_isLoadFail)
                {
                    _isLoadFail = true;
                    
                    Logger.Error($"预加载资源失败，失败信息:{handle.LastError}");
                    HotUpdateManager.Instance.ShowRetryInfo(LocalizationPool.Get( "UpdatePreloadFail"), this.Enter);
                }
                return;
            }
            _loadedCount++;
        }
    }
}