using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Resource;
using GameCore.Settings;
using UnityEngine;
using YooAsset;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.Module
{
    public partial class ResourceModule : IResourceModule, IModuleAwake
    {
        /// <summary>
        /// 资源加载最多等待时间
        /// </summary>
        public const int WAIT_MAX_SECONDS = 30;

        #region Prop

        private int downloadingMaxNum = 10;

        /// <summary>
        /// 资源信息列表。
        /// </summary>
        private readonly Dictionary<string, AssetInfo> _assetInfoMap = new Dictionary<string, AssetInfo>();

        /// <summary>
        /// 正在加载的资源列表。
        /// </summary>
        private readonly HashSet<string> _assetLoadingList = new HashSet<string>();

        #endregion

        /// <summary>
        /// 获取或设置同时最大下载数目。
        /// </summary>
        public int DownloadingMaxNum
        {
            get => downloadingMaxNum;
            set => downloadingMaxNum = value;
        }

        private int failedTryAgain = 5;

        /// <summary>
        /// 下载失败重试次数
        /// </summary>
        public int FailedTryAgain
        {
            get => failedTryAgain;
            set => failedTryAgain = value;
        }

        public void Awake(System.Object parameter)
        {
            YooAssets.SetOperationSystemMaxTimeSlice(Milliseconds);

            SetObjectPoolModule(G.ObjectPoolModule);
        }

        #region 资源包

        /// <summary>
        /// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）
        /// </summary>
        public long Milliseconds { get; set; } = 30;

        /// <summary>
        /// 获取指定资源包版本。
        /// </summary>
        /// <param name="customPackageName">指定资源包的名称</param>
        /// <returns>资源包版本。</returns>
        public string GetPackageVersion(string customPackageName)
        {
            ResourceModuleDriver.Instance.TryGetValue(customPackageName, out var package);
            if (package == null)
            {
                return string.Empty;
            }

            return package.GetPackageVersion();
        }

        /// <summary>
        /// 异步更新最新包的版本。
        /// </summary>
        /// <param name="customPackageName">指定资源包的名称</param>
        /// <param name="appendTimeTicks">请求URL是否需要带时间戳。</param>
        /// <param name="timeout">超时时间。</param>
        /// <returns>请求远端包裹的最新版本操作句柄。</returns>
        public RequestPackageVersionOperation RequestPackageVersionAsync(string customPackageName,
            bool appendTimeTicks = false, int timeout = 60)
        {
            ResourceModuleDriver.Instance.TryGetValue(customPackageName, out var package);
            return package.RequestPackageVersionAsync(appendTimeTicks, timeout);
        }

        /// <summary>
        /// 向网络端请求并更新清单
        /// </summary>
        /// <param name="packageVersion">更新的包裹版本</param>
        /// <param name="timeout">超时时间（默认值：60秒）</param>
        /// <param name="customPackageName">指定资源包的名称</param>
        public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout = 60,
            string customPackageName = "")
        {
            ResourceModuleDriver.Instance.TryGetValue(customPackageName, out var package);
            return package.UpdatePackageManifestAsync(packageVersion, timeout);
        }

        /// <summary>
        /// 创建资源下载器，用于下载当前资源版本所有的资源包文件。
        /// </summary>
        /// <param name="customPackageName">指定资源包的名称</param>
        public ResourceDownloaderOperation CreateResourceDownloader(string customPackageName)
        {
            ResourceModuleDriver.Instance.TryGetValue(customPackageName, out var package);
            var downloader = package.CreateResourceDownloader(DownloadingMaxNum, FailedTryAgain);
            return downloader;
        }

        /// <summary>
        /// 清理包裹未使用的缓存文件。
        /// </summary>
        /// <param name="customPackageName">指定资源包的名称</param>
        /// <param name="clearMode">文件清理方式。</param>
        public ClearCacheFilesOperation ClearCacheFilesAsync(string customPackageName,
            EFileClearMode clearMode = EFileClearMode.ClearUnusedBundleFiles)
        {
            ResourceModuleDriver.Instance.TryGetValue(customPackageName, out var package);
            return package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
        }

        /// <summary>
        /// 清理沙盒路径。
        /// </summary>
        /// <param name="customPackageName">指定资源包的名称</param>
        public void ClearAllBundleFiles(string customPackageName)
        {
            ResourceModuleDriver.Instance.TryGetValue(customPackageName, out var package);
            package.ClearCacheFilesAsync(EFileClearMode.ClearAllBundleFiles);
        }

        #endregion

        #region 资源回收

        /// <summary>
        /// 低内存
        /// </summary>
        public void OnLowMemory()
        {
            UnloadAllUnusedAssets(true).Forget();
        }

        /// <summary>
        /// 卸载所有未使用资源
        /// </summary>
        /// <param name="needGC">是否需要GC</param>
        public async UniTask UnloadAllUnusedAssets(bool needGC = false)
        {
            Logger.Debug("Unload unused assets...");
            UnloadUnusedAssets();
            await Resources.UnloadUnusedAssets().ToUniTask();
            if (needGC)
            {
                GC.Collect();
            }
        }

        /// <summary>
        /// 资源回收（卸载引用计数为零的资源）。
        /// </summary>
        public void UnloadUnusedAssets()
        {
            _assetPool.ReleaseAllUnused();
            foreach (var package in ResourceModuleDriver.Instance.PackageMap.Values)
            {
                if (package is { InitializeStatus: EOperationStatus.Succeed })
                {
                    package.UnloadUnusedAssetsAsync();
                }
            }
        }

        #endregion

        #region 获取资源信息

        /// <summary>
        /// 是否需要从远端更新下载。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="packageName">资源包名称。</param>
        public bool IsNeedDownloadFromRemote(string location, string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.IsNeedDownloadFromRemote(location);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.IsNeedDownloadFromRemote(location);
            }
        }

        /// <summary>
        /// 是否需要从远端更新下载。
        /// </summary>
        /// <param name="assetInfo">资源信息。</param>
        /// <param name="packageName">资源包名称。</param>
        public bool IsNeedDownloadFromRemote(AssetInfo assetInfo, string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.IsNeedDownloadFromRemote(assetInfo);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.IsNeedDownloadFromRemote(assetInfo);
            }
        }

        /// <summary>
        /// 获取资源信息列表。
        /// </summary>
        /// <param name="tag">资源标签。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <returns>资源信息列表。</returns>
        public AssetInfo[] GetAssetInfos(string tag, string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.GetAssetInfos(tag);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.GetAssetInfos(tag);
            }
        }

        /// <summary>
        /// 获取资源信息列表。
        /// </summary>
        /// <param name="tags">资源标签列表。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <returns>资源信息列表。</returns>
        public AssetInfo[] GetAssetInfos(string[] tags, string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.GetAssetInfos(tags);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.GetAssetInfos(tags);
            }
        }

        /// <summary>
        /// 获取资源信息。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <returns>资源信息。</returns>
        public AssetInfo GetAssetInfo(string location, string packageName)
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new Exception("Asset name is invalid.");
            }

            if (string.IsNullOrEmpty(packageName))
            {
                if (_assetInfoMap.TryGetValue(location, out AssetInfo assetInfo))
                {
                    return assetInfo;
                }

                assetInfo = YooAssets.GetAssetInfo(location);
                _assetInfoMap[location] = assetInfo;
                return assetInfo;
            }
            else
            {
                string key = $"{packageName}/{location}";
                if (_assetInfoMap.TryGetValue(key, out AssetInfo assetInfo))
                {
                    return assetInfo;
                }

                var package = YooAssets.GetPackage(packageName);
                if (package == null)
                {
                    throw new Exception($"The package does not exist. Package Name :{packageName}");
                }

                assetInfo = package.GetAssetInfo(location);
                _assetInfoMap[key] = assetInfo;
                return assetInfo;
            }
        }

        /// <summary>
        /// 检查资源是否存在。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <returns>检查资源是否存在的结果。</returns>
        public HasAssetResult HasAsset(string location, string packageName)
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new Exception("Asset name is invalid.");
            }

            AssetInfo assetInfo = GetAssetInfo(location, packageName);

            if (!CheckLocationValid(location, packageName))
            {
                return HasAssetResult.Valid;
            }

            if (assetInfo == null)
            {
                return HasAssetResult.NotExist;
            }

            if (IsNeedDownloadFromRemote(assetInfo, packageName))
            {
                return HasAssetResult.AssetOnline;
            }

            return HasAssetResult.AssetOnDisk;
        }

        /// <summary>
        /// 检查资源定位地址是否有效。
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="packageName">资源包名称。</param>
        public bool CheckLocationValid(string location, string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.CheckLocationValid(location);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.CheckLocationValid(location);
            }
        }

        #endregion

        #region 资源加载

        /// <summary>
        /// 异步加载资源。
        /// 内部已做池子优化，外部无需关心释放问题.
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="callback">回调函数。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        /// <param name="needShowWait">是否需要显示等待中UI</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        public async UniTaskVoid LoadAssetAsync<T>(string location, Action<T> callback, string packageName = "",
            bool needShowWait = false)
            where T : UnityEngine.Object
        {
            location = location.ToLower();
            if (string.IsNullOrEmpty(location))
            {
                Logger.Error("Asset name is invalid.");
                return;
            }

            if (string.IsNullOrEmpty(location))
            {
                throw new Exception("Asset name is invalid.");
            }

            string assetObjectKey = GetCacheKey(location, packageName);

            await TryWaitingLoading(assetObjectKey);

            AssetObject assetObject = _assetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                await UniTask.Yield();
                callback?.Invoke(assetObject.Target as T);
                return;
            }

            _assetLoadingList.Add(assetObjectKey);

            AssetHandle handle = GetHandleAsync<T>(location, packageName: packageName, needShowWait: needShowWait);

            handle.Completed += assetHandle =>
            {
                _assetLoadingList.Remove(assetObjectKey);

                if (assetHandle.AssetObject != null)
                {
                    assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle, this);
                    _assetPool.Register(assetObject, true);

                    callback?.Invoke(assetObject.Target as T);
                }
                else
                {
                    callback?.Invoke(null);
                }
            };
        }

        /// <summary>
        /// 异步加载资源
        /// 内部已做池子优化，外部无需关心释放问题.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="packageName"></param>
        /// <param name="needShowWait">是否需要显示等待中UI</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async UniTask<T> LoadAssetAsync<T>(string location, CancellationToken cancellationToken = default,
            string packageName = "", bool needShowWait = false) where T : UnityEngine.Object
        {
            location = location.ToLower();
            if (string.IsNullOrEmpty(location))
            {
                throw new Exception("Asset name is invalid.");
            }

            string assetObjectKey = GetCacheKey(location, packageName);

            await TryWaitingLoading(assetObjectKey);

            AssetObject assetObject = _assetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                await UniTask.Yield();
                return assetObject.Target as T;
            }

            _assetLoadingList.Add(assetObjectKey);

            AssetHandle handle = GetHandleAsync<T>(location, packageName: packageName, needShowWait);

            bool cancelOrFailed = await handle.ToUniTask().AttachExternalCancellation(cancellationToken)
                .SuppressCancellationThrow();

            if (cancelOrFailed)
            {
                _assetLoadingList.Remove(assetObjectKey);
                return null;
            }

            assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle, this);
            _assetPool.Register(assetObject, true);

            _assetLoadingList.Remove(assetObjectKey);

            return handle.AssetObject as T;
        }

        /// <summary>
        /// 异步加载并实例化GameObject,项目所有生成GameObject都使用该接口
        /// 内部已做池子优化，外部无需关心释放问题.
        /// </summary>
        /// <param name="location">资源寻址地址</param>
        /// <param name="parent">附加的父节点</param>
        /// <param name="cancellationToken"></param>
        /// <param name="packageName">从哪个资源包加载，不传默认从主资源包</param>
        /// <param name="needShowWait">是否需要显示等待中UI</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async UniTask<GameObject> LoadGameObjectAsync(string location, Transform parent = null,
            CancellationToken cancellationToken = default, string packageName = "", bool needShowWait = false)
        {
            location = location.ToLower();
            if (string.IsNullOrEmpty(location))
            {
                throw new Exception("Asset name is invalid.");
            }

            string assetObjectKey = GetCacheKey(location, packageName);

            await TryWaitingLoading(assetObjectKey);

            AssetObject assetObject = _assetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                await UniTask.Yield();
                return AssetsReference.Instantiate(assetObject.Target as GameObject, parent).gameObject;
            }

            _assetLoadingList.Add(assetObjectKey);

            AssetHandle handle = GetHandleAsync<GameObject>(location, packageName: packageName, needShowWait);

            bool cancelOrFailed = await handle.ToUniTask().AttachExternalCancellation(cancellationToken)
                .SuppressCancellationThrow();

            if (cancelOrFailed)
            {
                _assetLoadingList.Remove(assetObjectKey);
                return null;
            }

            GameObject gameObject = AssetsReference.Instantiate(handle.AssetObject as GameObject, parent).gameObject;

            assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle, this);
            _assetPool.Register(assetObject, true);

            _assetLoadingList.Remove(assetObjectKey);

            return gameObject;
        }

        /// <summary>
        /// 异步加载并实例化GameObject,项目所有生成GameObject都使用该接口
        /// 内部已做池子优化，外部无需关心释放问题.
        /// </summary>
        /// <param name="location">资源寻址地址</param>
        /// <param name="callback">资源回调</param>
        /// <param name="parent">附加的父节点</param>
        /// <param name="cancellationToken"></param>
        /// <param name="packageName"></param>
        public async UniTask LoadGameObjectAsync(string location,Action<GameObject> callback, Transform parent = null, CancellationToken cancellationToken = default, string packageName = "")
        {
            var gameObject = await LoadGameObjectAsync(location, parent, cancellationToken, packageName);
            callback?.Invoke(gameObject);
        }

        /// <summary>
        /// 同步从池子中加载游戏对象
        /// </summary>
        /// <param name="location"></param>
        /// <param name="parent"></param>
        /// <param name="packageName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public GameObject LoadGameObjectByPool(string location, Transform parent = null, string packageName = "")
        {
            location = location.ToLower();
            if (string.IsNullOrEmpty(location))
            {
                throw new Exception("Asset name is invalid.");
            }
            string assetObjectKey = GetCacheKey(location, packageName);
            AssetObject assetObject = _assetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                return AssetsReference.Instantiate(assetObject.Target as GameObject, parent).gameObject;
            }

            Logger.Error($"{location} 同步加载失败，请确保至少预加载过该资源并确保还没有被释放!");
            return null;
        }

        /// <summary>
        /// 异步加载资源。
        /// 内部已做池子优化，外部无需关心释放问题.
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包。</param>
        /// <param name="needShowWait">是否需要显示等待中UI</param>
        public async void LoadAssetAsync(string location, int priority, LoadAssetCallbacks loadAssetCallbacks,
            object userData, string packageName = "", bool needShowWait = false)
        {
            location = location.ToLower();
            if (string.IsNullOrEmpty(location))
            {
                throw new Exception("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new Exception("Load asset callbacks is invalid.");
            }

            string assetObjectKey = GetCacheKey(location, packageName);

            await TryWaitingLoading(assetObjectKey);

            float duration = Time.time;

            AssetObject assetObject = _assetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                await UniTask.Yield();
                loadAssetCallbacks.LoadAssetSuccessCallback(location, assetObject.Target, Time.time - duration,
                    userData);
                return;
            }

            _assetLoadingList.Add(assetObjectKey);

            AssetInfo assetInfo = GetAssetInfo(location, packageName);

            if (!string.IsNullOrEmpty(assetInfo.Error))
            {
                _assetLoadingList.Remove(assetObjectKey);

                string errorMessage =
                    string.Format("Can not load asset '{0}' because :'{1}'.", location, assetInfo.Error);
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(location, LoadResourceStatus.NotExist, errorMessage,
                        userData);
                    return;
                }

                throw new Exception(errorMessage);
            }

            AssetHandle handle = GetHandleAsync(location, assetInfo.AssetType, packageName: packageName, needShowWait);

            if (loadAssetCallbacks.LoadAssetUpdateCallback != null)
            {
                InvokeProgress(location, handle, loadAssetCallbacks.LoadAssetUpdateCallback, userData).Forget();
            }

            await handle.ToUniTask();

            if (handle.AssetObject == null || handle.Status == EOperationStatus.Failed)
            {
                _assetLoadingList.Remove(assetObjectKey);

                string errorMessage = string.Format("Can not load asset '{0}'.", location);
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(location, LoadResourceStatus.NotReady, errorMessage,
                        userData);
                    return;
                }

                throw new Exception(errorMessage);
            }

            assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle, this);
            _assetPool.Register(assetObject, true);

            _assetLoadingList.Remove(assetObjectKey);

            if (loadAssetCallbacks.LoadAssetSuccessCallback != null)
            {
                duration = Time.time - duration;

                loadAssetCallbacks.LoadAssetSuccessCallback(location, handle.AssetObject, duration, userData);
            }
        }

        private async UniTaskVoid InvokeProgress(string location, AssetHandle assetHandle,
            LoadAssetUpdateCallback loadAssetUpdateCallback, object userData)
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new Exception("Asset name is invalid.");
            }

            if (loadAssetUpdateCallback != null)
            {
                while (assetHandle is { IsValid: true, IsDone: false })
                {
                    await UniTask.Yield();

                    loadAssetUpdateCallback.Invoke(location, assetHandle.Progress, userData);
                }
            }
        }

        /// <summary>
        /// 获取异步加载的资源操作句柄。
        /// 外部自主管理加载资源
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <returns>资源操作句柄。</returns>
        public AssetHandle LoadAssetAsyncHandle<T>(string location, string packageName = "")
            where T : UnityEngine.Object
        {
            location = location.ToLower();
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.LoadAssetAsync<T>(location);
            }

            var package = YooAssets.GetPackage(packageName);
            return package.LoadAssetAsync<T>(location);
        }

        private readonly TimeoutController _timeoutController = new TimeoutController();

        /// <summary>
        /// 处理异步同时间快速加载同资产的等待
        /// </summary>
        /// <param name="assetObjectKey"></param>
        private async UniTask TryWaitingLoading(string assetObjectKey)
        {
            if (_assetLoadingList.Contains(assetObjectKey))
            {
                try
                {
                    await UniTask.WaitUntil(() => !_assetLoadingList.Contains(assetObjectKey))
#if UNITY_EDITOR
                        .AttachExternalCancellation(_timeoutController.Timeout(TimeSpan.FromSeconds(60)));
                    _timeoutController.Reset();
#else
                    ;
#endif
                }
                catch (OperationCanceledException ex)
                {
                    if (_timeoutController.IsTimeout())
                    {
                        Logger.Error($"LoadAssetAsync Waiting {assetObjectKey} timeout. reason:{ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 获取资源定位地址的缓存Key。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <returns>资源定位地址的缓存Key。</returns>
        private string GetCacheKey(string location, string packageName)
        {
            string defaultPackageName = GameSettings.Instance.ProjectSetting.DefaultPackageName;
            if (string.IsNullOrEmpty(packageName) || packageName.Equals(defaultPackageName))
            {
                return location;
            }

            return $"{packageName}/{location}";
        }

        /// <summary>
        /// 获取异步资源句柄。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="packageName">指定资源包的名称</param>
        /// <param name="needShowWait">是否需要显示等待中UI</param>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <returns>资源句柄。</returns>
        private AssetHandle GetHandleAsync<T>(string location, string packageName, bool needShowWait)
            where T : UnityEngine.Object
        {
            var handle = GetHandleAsync(location, typeof(T), packageName, needShowWait);
            return handle;
        }

        private AssetHandle GetHandleAsync(string location, Type assetType, string packageName, bool needShowWait)
        {
            ResourceModuleDriver.Instance.TryGetValue(packageName, out var package);
            var handle = package.LoadAssetAsync(location, assetType);
            //TODO...
            // if (needShowWait)
            // {
            //     float time = 0;
            //     G.UIModule.SetWaitingVisible(true);

            // while (!handle.IsDone)
            // {
            // if (handle.IsDone)
            // {
            //     G.UIModule.SetWaitingVisible(false);
            //     break;
            // }
            // time += Time.unscaledDeltaTime;
            // if (time >= WAIT_MAX_SECONDS)
            // {
            //     G.UIModule.SetWaitingVisible(false);
            //     Logger.Warning($"So Long time to load to resource asset [{location}]");
            //     break;
            // }
            // }
            // }

            return handle;
        }

        #endregion
    }
}