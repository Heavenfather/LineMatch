using Cysharp.Threading.Tasks;
using GameCore.Log;

namespace HotfixCore.Module
{
    public partial class ResourceHandler
    {
        private LoadAssetCallbacks _loadAssetCallbacks;

        private void InitializedResources()
        {
            _loadAssetCallbacks = new LoadAssetCallbacks(OnLoadAssetSuccess, OnLoadAssetFailure);
        }

        private void OnLoadAssetFailure(string assetName, LoadResourceStatus status, string errormessage,
            object userdata)
        {
            _assetLoadingList.Remove(assetName);
            Logger.ErrorFormat("Can not load asset from '{1}' with error message '{2}'.", assetName, errormessage);
        }

        private void OnLoadAssetSuccess(string assetName, object asset, float duration, object userdata)
        {
            _assetLoadingList.Remove(assetName);
            ISetAssetObject setAssetObject = (ISetAssetObject)userdata;
            UnityEngine.Object assetObject = asset as UnityEngine.Object;
            if (assetObject != null)
            {
                _assetItemPool.Register(AssetItemObject.Create(setAssetObject.Location, assetObject), true);
                SetAsset(setAssetObject, assetObject);
            }
            else
            {
                Logger.Error($"Load failure asset type is {asset.GetType()}.");
            }
        }

        /// <summary>
        /// 通过资源系统设置资源。
        /// </summary>
        /// <param name="setAssetObject">需要设置的对象。</param>
        public async UniTaskVoid SetAssetByResources<T>(ISetAssetObject setAssetObject) where T : UnityEngine.Object
        {
            await TryWaitingLoading(setAssetObject.Location);

            if (_assetItemPool.CanSpawn(setAssetObject.Location))
            {
                var assetObject = (T)_assetItemPool.Spawn(setAssetObject.Location).Target;
                SetAsset(setAssetObject, assetObject);
            }
            else
            {
                _assetLoadingList.Add(setAssetObject.Location);
                // G.ResourceModule.LoadAssetAsync(setAssetObject.Location, 0, _loadAssetCallbacks, setAssetObject);
                G.ResourceModule.LoadAssetAsync<T>(setAssetObject.Location, assetObject =>
                {
                    SetAsset(setAssetObject, assetObject);
                }).Forget();
            }
        }
    }
}