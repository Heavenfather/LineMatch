using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Singleton;
using UnityEngine;
using Object = UnityEngine.Object;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.Module
{
    /// <summary>
    /// 资源管理者
    /// 定期检测加载资源是否可回收
    /// </summary>
    public partial class ResourceHandler : MonoSingleton<ResourceHandler>
    {
        private readonly TimeoutController _timeoutController = new TimeoutController();

        /// <summary>
        /// 正在加载的资源列表。
        /// </summary>
        private readonly HashSet<string> _assetLoadingList = new HashSet<string>();

        /// <summary>
        /// 检查是否可以释放间隔
        /// </summary>
        private float checkCanReleaseInterval = 30f;

        private float _checkCanReleaseTime = 0.0f;

        /// <summary>
        /// 对象池自动释放时间间隔
        /// </summary>
        private float autoReleaseInterval = 60f;

        /// <summary>
        /// 保存加载的图片对象
        /// </summary>
        private LinkedList<LoadAssetObject> _loadAssetObjectsLinkedList;

        /// <summary>
        /// 散图集合对象池
        /// </summary>
        private IObjectPool<AssetItemObject> _assetItemPool;

        public void Initialize()
        {
            _assetItemPool =
                G.ObjectPoolModule.CreateMultiSpawnObjectPool<AssetItemObject>("SetAssetPool", autoReleaseInterval, 16,
                    60, 0);
            _loadAssetObjectsLinkedList = new LinkedList<LoadAssetObject>();

            InitializedResources();
        }

        protected override void OnUpdate()
        {
            _checkCanReleaseTime += Time.unscaledDeltaTime;
            if (_checkCanReleaseTime < (double)checkCanReleaseInterval)
            {
                return;
            }

            ReleaseUnused();
        }

        /// <summary>
        /// 回收无用资产
        /// </summary>
        private void ReleaseUnused()
        {
            if (_loadAssetObjectsLinkedList == null)
            {
                return;
            }

            LinkedListNode<LoadAssetObject> current = _loadAssetObjectsLinkedList.First;
            while (current != null)
            {
                var next = current.Next;
                if (current.Value.AssetObject.IsCanRelease())
                {
                    _assetItemPool.Unspawn(current.Value.AssetTarget);
                    MemoryPool.MemoryPool.Release(current.Value.AssetObject);
                    _loadAssetObjectsLinkedList.Remove(current);
                }

                current = next;
            }

            _checkCanReleaseTime = 0f;
        }

        private void SetAsset(ISetAssetObject setAssetObject, Object assetObject)
        {
            _loadAssetObjectsLinkedList.AddLast(new LoadAssetObject(setAssetObject, assetObject));
            setAssetObject.SetAsset(assetObject);
        }

        private async UniTask TryWaitingLoading(string assetObjectKey)
        {
            if (_assetLoadingList.Contains(assetObjectKey))
            {
                try
                {
                    await UniTask.WaitUntil(
                            () => !_assetLoadingList.Contains(assetObjectKey))
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
    }
}