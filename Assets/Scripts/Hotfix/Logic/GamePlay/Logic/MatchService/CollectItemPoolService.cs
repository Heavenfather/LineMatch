using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Hotfix.Define;
using HotfixCore.Extensions;
using HotfixCore.Module;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 收集道具对象池服务
    /// 管理收集道具GameObject的创建、复用和回收
    /// </summary>
    public class CollectItemPoolService
    {
        private Transform _poolRoot;
        private Dictionary<CollectItemEnum, string> _collectItemNameMap;
        private Dictionary<CollectItemEnum, Queue<MatchCollectBase>> _collectItemPoolMap;
        private int _globalFlyIndex = 0;

        public CollectItemPoolService(Transform poolRoot)
        {
            _poolRoot = poolRoot;
            _collectItemPoolMap = new Dictionary<CollectItemEnum, Queue<MatchCollectBase>>();
            _collectItemNameMap = new Dictionary<CollectItemEnum, string>()
            {
                { CollectItemEnum.Normal, "NormalCollectItem" },
                { CollectItemEnum.Butterfly, "ButterflyCollectItem" },
                { CollectItemEnum.Wish, "WishCollectItem" },
                { CollectItemEnum.Bear, "BearCollectItem" },
                { CollectItemEnum.Coin, "CoinCollectItem"},
            };
        }

        /// <summary>
        /// 获取下一个飞行索引
        /// </summary>
        public int GetNextFlyIndex()
        {
            return _globalFlyIndex++;
        }

        /// <summary>
        /// 重置飞行索引
        /// </summary>
        public void ResetFlyIndex()
        {
            _globalFlyIndex = 0;
        }

        /// <summary>
        /// 获取或创建收集道具GameObject
        /// </summary>
        public async UniTask<MatchCollectBase> GetCollectItem(int elementId)
        {
            CollectItemEnum itemEnum = GetCollectItemEnum(elementId);

            // 尝试从对象池获取
            if (_collectItemPoolMap.ContainsKey(itemEnum) && _collectItemPoolMap[itemEnum].Count > 0)
            {
                var queue = _collectItemPoolMap[itemEnum];
                while (queue.Count > 0)
                {
                    var item = queue.Dequeue();
                    if (item != null && !item.gameObject.activeSelf)
                    {
                        return item;
                    }
                }
            }

            // 创建新的GameObject
            string itemName = _collectItemNameMap[itemEnum];
            string location = $"{MatchConst.ElementAddressBase}/{itemName}".ToLower();
            GameObject go = await G.ResourceModule.LoadGameObjectAsync(location, _poolRoot);

            if (go == null)
                return null;

            MatchCollectBase collectItem = go.GetComponent<MatchCollectBase>();
            return collectItem;
        }

        /// <summary>
        /// 回收收集道具到对象池
        /// </summary>
        public void RecycleCollectItem(int elementId, MatchCollectBase collectItem)
        {
            if (collectItem == null)
                return;

            CollectItemEnum itemEnum = GetCollectItemEnum(elementId);

            if (!_collectItemPoolMap.ContainsKey(itemEnum))
            {
                _collectItemPoolMap[itemEnum] = new Queue<MatchCollectBase>();
            }

            collectItem.SetVisible(false);
            _collectItemPoolMap[itemEnum].Enqueue(collectItem);
        }

        /// <summary>
        /// 清理对象池
        /// </summary>
        public void Clear()
        {
            _collectItemPoolMap?.Clear();
            _globalFlyIndex = 0;
        }

        /// <summary>
        /// 根据元素ID获取收集道具枚举
        /// </summary>
        private CollectItemEnum GetCollectItemEnum(int elementId)
        {
            if (Enum.IsDefined(typeof(CollectItemEnum), elementId))
            {
                return (CollectItemEnum)elementId;
            }
            return CollectItemEnum.Normal;
        }
    }
}
