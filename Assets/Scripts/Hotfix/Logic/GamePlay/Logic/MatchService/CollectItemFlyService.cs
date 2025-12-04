using System;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 收集道具飞行服务实现
    /// </summary>
    public class CollectItemFlyService : ICollectItemFlyService
    {
        private CollectItemPoolService _poolService;
        private int _globalFlyIndex = 0;

        public CollectItemPoolService PoolService => _poolService;

        public CollectItemFlyService(CollectItemPoolService poolService)
        {
            _poolService = poolService;
        }

        public void RequestCollectItemFly(EcsWorld world, int elementId, Vector3 startPosition, Vector3 targetPosition,
            Action callback = null, int flyIndex = -1,bool isGameSettlement = false)
        {
            // 创建飞行实体
            int flyEntity = world.NewEntity();
            var flyPool = world.GetPool<CollectItemFlyComponent>();
            ref var fly = ref flyPool.Add(flyEntity);

            fly.ElementId = elementId;
            fly.StartPosition = startPosition;
            fly.TargetPosition = targetPosition;
            fly.FlyIndex = flyIndex >= 0 ? flyIndex : GetNextFlyIndex();
            fly.IsFlying = false;
            fly.IsComplete = false;
            fly.IsGameSettlement = isGameSettlement;

            // 如果有回调，创建回调实体
            if (callback != null)
            {
                int callbackEntity = world.NewEntity();
                var callbackPool = world.GetPool<CollectItemCallbackComponent>();
                ref var callbackComp = ref callbackPool.Add(callbackEntity);
                callbackComp.Callback = callback;
                callbackComp.CallbackEntity = elementId;

                fly.CallbackEntityId = callbackEntity;
            }
            else
            {
                fly.CallbackEntityId = -1;
            }
        }

        public void ResetFlyIndex()
        {
            _globalFlyIndex = 0;
            _poolService?.ResetFlyIndex();
        }

        public int GetNextFlyIndex()
        {
            return _globalFlyIndex++;
        }
    }
}
