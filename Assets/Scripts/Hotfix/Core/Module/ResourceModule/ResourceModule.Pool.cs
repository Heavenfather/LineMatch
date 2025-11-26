using System;

namespace HotfixCore.Module
{
    public partial class ResourceModule
    {
        private IObjectPool<AssetObject> _assetPool;

        /// <summary>
        /// 获取或设置资源对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float AssetAutoReleaseInterval = 60.0f;

        /// <summary>
        /// 获取或设置资源对象池的容量。
        /// </summary>
        public int AssetCapacity = 100;

        /// <summary>
        /// 获取或设置资源对象池对象过期秒数。
        /// </summary>
        public float AssetExpireTime = 60;

        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源。</param>
        public void UnloadAsset(object asset)
        {
            if (_assetPool != null)
            {
                _assetPool.Unspawn(asset);
            }
        }

        /// <summary>
        /// 设置对象池管理器。
        /// </summary>
        /// <param name="objectPoolModule">对象池管理器。</param>
        public void SetObjectPoolModule(IObjectPoolModule objectPoolModule)
        {
            if (objectPoolModule == null)
            {
                throw new Exception("Object pool manager is invalid.");
            }

            //创建资源对象池
            _assetPool = objectPoolModule.CreateMultiSpawnObjectPool<AssetObject>("Asset Pool",
                AssetAutoReleaseInterval, AssetCapacity, AssetExpireTime, 0);
        }
    }
}