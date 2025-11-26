using Cysharp.Threading.Tasks;
using GameCore.Localization;
using GameCore.Logic;
using GameCore.Resource;
using UnityEngine;
using YooAsset;
using Logger = GameCore.Log.Logger;

namespace GameCore.Process
{
    public class ProcessDownloader : IProcess
    {
        private const int DOWNLOAD_MAX_NUM = 10;
        private const int RETRY_COUNT = 5;
        
        private ResourceDownloaderOperation _downloader;

        private int _totalDownloadCount;
        private int _curRetryCount;
        private bool _showRetryTag;
        
        public void Init()
        {
            
        }

        public void Enter()
        {
            Logger.Info($"Start Downloader");
            HotUpdateManager.Instance.UpdateProgress(EUpdateState.DownloadRes);
            if (_downloader != null)
            {
                _downloader.CancelDownload();
            }
            CreateDownloader().Forget();
        }

        public void Leave()
        {
        }

        public void Update()
        {
        }

        private async UniTask CreateDownloader()
        {
            await UniTask.Yield();
            
            //强制更新的包必定为首资源包(默认资源包)
            ResourcePackage package = ResourceModuleDriver.Instance.GetDefaultPackage();
            _downloader = package.CreateResourceDownloader(DOWNLOAD_MAX_NUM, RETRY_COUNT);
            if (_downloader.TotalDownloadCount <= 0)
            {
                Logger.Info("当前无需下载，直接进入游戏");
                ProcessManager.Instance.Enter<ProcessClearCache>();
                return;
            }
            Logger.Info($"当前需要下载总bundle数:{_downloader.TotalDownloadCount}");
            //需要检查磁盘空间 TODO....
            long totalDownloadBytes = _downloader.TotalDownloadBytes;
            float sizeMb = totalDownloadBytes / 1048576f;
            sizeMb = Mathf.Clamp(sizeMb, 0.1f, float.MaxValue);
            
            BeginDownload().Forget();
        }

        private async UniTask BeginDownload()
        {
            _downloader.DownloadErrorCallback = OnDownloadErrorCallback;
            _downloader.DownloadUpdateCallback = OnDownloadProgressCallback;
            _downloader.BeginDownload();
            await _downloader;
            //检测下载结果
            if(_downloader.Status != EOperationStatus.Succeed)
                return;
            
            //下载完成，清空本地无用bundle
            ProcessManager.Instance.Enter<ProcessClearCache>();
        }

        private void OnDownloadProgressCallback(DownloadUpdateData data)
        {
            HotUpdateManager.Instance.UpdateProgress(EUpdateState.DownloadRes, data.Progress);
        }

        private void OnDownloadErrorCallback(DownloadErrorData data)
        {
            Logger.Error($"Main package download fail :[{data.FileName}] : {data.ErrorInfo}");
            if(_showRetryTag)
                return;
            _showRetryTag = true;
            if (_curRetryCount >= RETRY_COUNT)
            {
                _curRetryCount = 0;
                
                HotUpdateManager.Instance.ShowRetryInfo(LocalizationPool.Get("DownloadResErrorCheckRetry"), () =>
                {
                    _showRetryTag = false;
                    _curRetryCount++;
                    HotUpdateManager.Instance.UpdateProgress(EUpdateState.RetryState);
                    this.Enter();
                });
            }
            else
            {
                HotUpdateManager.Instance.ShowRetryInfo(LocalizationPool.Get("DownloadResErrorRetry"), () =>
                {
                    _showRetryTag = false;
                    _curRetryCount++;
                    HotUpdateManager.Instance.UpdateProgress(EUpdateState.RetryState);
                    this.Enter();
                });
            }
        }
    }
}