using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Hotfix.Utils;
using HotfixCore.Extensions;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 收集道具飞行系统
    /// 处理收集道具从起点飞向目标点的动画
    /// </summary>
    public class CollectItemFlySystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        private EcsWorld _world;
        private GameStateContext _context;
        private CollectItemPoolService _poolService;

        private EcsFilter _flyFilter;
        private EcsPool<CollectItemFlyComponent> _flyPool;
        private EcsPool<CollectItemCallbackComponent> _callbackPool;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();

            // 从容器获取对象池服务
            var flyService = MatchBoot.Container.Resolve<ICollectItemFlyService>() as CollectItemFlyService;
            if (flyService != null)
            {
                _poolService = flyService.PoolService;
            }

            _flyFilter = _world.Filter<CollectItemFlyComponent>().End();
            _flyPool = _world.GetPool<CollectItemFlyComponent>();
            _callbackPool = _world.GetPool<CollectItemCallbackComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _flyFilter)
            {
                ref var fly = ref _flyPool.Get(entity);

                // 如果还没开始飞行，启动飞行
                if (!fly.IsFlying && !fly.IsComplete)
                {
                    fly.IsFlying = true;
                    StartFly(entity, fly).Forget();
                }

                // 如果飞行完成，清理实体
                if (fly.IsComplete)
                {
                    _world.DelEntity(entity);
                }
            }
        }

        /// <summary>
        /// 启动飞行动画
        /// </summary>
        private async UniTaskVoid StartFly(int entity, CollectItemFlyComponent fly)
        {
            if (fly.ElementId == (int)CollectItemEnum.Coin && fly.IsGameSettlement)
            {
                await StartGameSettlementCoinFly(entity,fly);
                return;
            }
            // 获取收集道具GameObject
            MatchCollectBase collectItem = await _poolService.GetCollectItem(fly.ElementId);
            if (collectItem == null)
            {
                // 如果获取失败，标记完成并执行回调
                MarkFlyComplete(entity);
                ExecuteCallback(fly.CallbackEntityId);
                return;
            }

            // 初始化位置
            collectItem.Initialize(fly.StartPosition);
            collectItem.SetVisible(true);

            // 执行飞行动画
            bool isComplete = false;
            collectItem.DoIconEffect(fly.ElementId, fly.TargetPosition, fly.FlyIndex, () =>
            {
                // 飞行完成回调
                isComplete = true;
                _poolService.RecycleCollectItem(fly.ElementId, collectItem);

                // 执行用户回调
                ExecuteCallback(fly.CallbackEntityId);

                // 标记飞行完成
                MarkFlyComplete(entity);
            });

            // 等待飞行完成（防止实体过早销毁）
            while (!isComplete)
            {
                await UniTask.Yield();
            }
        }

        /// <summary>
        /// 标记飞行完成
        /// </summary>
        private void MarkFlyComplete(int entity)
        {
            if (_world.IsEntityAliveInternal(entity) && _flyPool.Has(entity))
            {
                ref var flyComp = ref _flyPool.Get(entity);
                flyComp.IsComplete = true;
            }
        }

        /// <summary>
        /// 执行回调
        /// </summary>
        private void ExecuteCallback(int callbackEntityId)
        {
            if (callbackEntityId < 0)
                return;

            if (!_world.IsEntityAliveInternal(callbackEntityId))
                return;

            if (!_callbackPool.Has(callbackEntityId))
                return;

            ref var callback = ref _callbackPool.Get(callbackEntityId);
            callback.CallbackEntity = callbackEntityId;
            callback.Callback?.Invoke();

            // 执行完回调后删除回调实体
            _world.DelEntity(callbackEntityId);
        }

        private async UniTask StartGameSettlementCoinFly(int entity,CollectItemFlyComponent fly)
        {
            var coinList = new List<MatchCollectBase>();

            var startPosition = fly.StartPosition;
            var targetPosition = fly.TargetPosition;
            var random = new System.Random();
            var coinCount = random.Next(2, 5);
            var offsetDis = 100;

            for (int i = 0; i < coinCount; i++)
            {
                var collectItem = await _poolService.GetCollectItem((int)CollectItemEnum.Coin);
                if (collectItem == null)
                    return;

                collectItem.transform.localScale = Vector3.one * 1.3f;
                collectItem.Initialize(startPosition);
                collectItem.SetVisible(false);
                coinList.Add(collectItem);
            }

            var delayTime = 0.1f;
            var moveSpeed = 20;
            
            for (int i = 0; i < coinCount; i++)
            {
                var curIdx = i;
                var coinItem = coinList[curIdx];
                coinItem.gameObject.SetActive(true);
                var offsetPos = new Vector3(random.Next(-offsetDis, offsetDis) / 100f,
                    random.Next(-offsetDis, offsetDis) / 100f, 0);
                offsetPos += new Vector3(0, -0.4f, 0);
                var bloomPos = startPosition + offsetPos;
                var seq = DOTween.Sequence();
                seq.AppendInterval(delayTime * curIdx);
                seq.Append(coinItem.transform.DOMove(bloomPos, 0.3f).SetEase(Ease.OutBack));
                seq.AppendInterval(delayTime * (coinCount - curIdx));
                seq.AppendInterval(0.07f * curIdx);
                seq.Append(coinItem.transform
                    .DOMove(targetPosition, Vector3.Distance(startPosition, targetPosition) / moveSpeed)
                    .SetEase(Ease.InBack));
                seq.AppendCallback(() =>
                {
                    AudioUtil.PlayGetCoin();
                    coinItem.transform.localScale = Vector3.one;
                    coinItem.gameObject.SetActive(false);
                    
                    ExecuteCallback(fly.CallbackEntityId);
                    MarkFlyComplete(entity);
                });
            }

        }

        public void Destroy(IEcsSystems systems)
        {
            _poolService?.Clear();
        }
    }
}