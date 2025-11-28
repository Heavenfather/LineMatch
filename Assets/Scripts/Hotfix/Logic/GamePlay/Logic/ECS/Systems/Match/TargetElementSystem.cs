using System.Collections.Generic;
using DG.Tweening;
using GameConfig;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 收集类型的障碍物 它只关心是否有普通棋子被消了
    /// </summary>
    public class TargetElementSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _eliminatedFilter; // 所有刚死的普通棋子
        private EcsFilter _targetFilter;

        private EcsPool<DestroyElementTagComponent> _destroyTagPool;
        private EcsPool<ElementComponent> _elePool;
        private EcsPool<GridCellComponent> _gridPool;
        private EcsPool<NormalElementComponent> _normalElement;
        private IBoard _board;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            var context = systems.GetShared<GameStateContext>();
            _board = context.Board;

            _eliminatedFilter = _world.Filter<EliminatedTag>().Include<NormalElementComponent>().Include<ElementComponent>().End();
            _targetFilter = _world.Filter<TargetElementComponent>().Include<ElementComponent>().End();
            _destroyTagPool = _world.GetPool<DestroyElementTagComponent>();
            _normalElement = _world.GetPool<NormalElementComponent>();

            _elePool = _world.GetPool<ElementComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            // 这一帧没有死亡的普通棋子
            if (_eliminatedFilter.GetEntitiesCount() == 0) return;

            foreach (var entity in _targetFilter)
            {
                ref var targetCom = ref _world.GetPool<TargetElementComponent>().Get(entity);
                ref var eleCom = ref _elePool.Get(entity);
                if(eleCom.LogicState == ElementLogicalState.Dying)
                    continue;
                
                // 遍历这一帧所有死亡的普通棋子
                foreach (var eliminatedEntity in _eliminatedFilter)
                {
                    if(!_normalElement.Has(eliminatedEntity)) //按理说不会找不到了，因为Filter已经过滤了一遍
                        continue;
                    
                    ref var normalCom = ref _normalElement.Get(eliminatedEntity);
                    if(normalCom.IsOtherElementHandleThis)
                        continue; //它被其它元素处理了
                    
                    ref var deadEle = ref _elePool.Get(eliminatedEntity);
                    if (IsTarget(targetCom.TargetConfigId, deadEle.ConfigId))
                    {
                        // 减少收集数
                        targetCom.RemainTargetNum--;
                        // 收集这些棋子，播放表现
                        if (targetCom.CollectedTargetEntities == null)
                            targetCom.CollectedTargetEntities = new HashSet<int>();
                        targetCom.CollectedTargetEntities.Add(eliminatedEntity);
                        normalCom.IsOtherElementHandleThis = true;

                        if (targetCom.RemainTargetNum <= 0)
                        {
                            break;
                        }
                    }
                }

                // 1.这个实体查询完了，看有没有收集的目标
                if (targetCom.CollectedTargetEntities != null && targetCom.CollectedTargetEntities.Count > 0)
                {
                    // 2.有收集的目标，播放表现，让目标飞向Target
                    PlayFlyToTarget(entity, targetCom.CollectedTargetEntities);
                }
                
            }
        }
        
        private bool IsTarget(int targetId, int deadId)
        {
            return targetId == deadId;
        }

        private void PlayFlyToTarget(int targetEntity, HashSet<int> collectedTargetEntities)
        {
            // 1.让 collectedTargetEntities 所有实体飞向 targetEntity
            Sequence seq = DOTween.Sequence();
            ref var elementCom = ref _elePool.Get(targetEntity);
            Vector3 endPos = MatchPosUtil.CalculateWorldPosition(elementCom.OriginGridPosition.x,
                elementCom.OriginGridPosition.y, elementCom.Width, elementCom.Height, ElementDirection.None);
            int index = 0;
            foreach (var eliminateEntity in collectedTargetEntities)
            {
                ref var renderCom = ref _world.GetPool<ElementRenderComponent>().Get(eliminateEntity);
                if (renderCom.ViewInstance != null)
                {
                    seq.Append(renderCom.ViewInstance.transform.DOMove(endPos, 0.3f).SetDelay(index * 0.03f).OnComplete(
                        () =>
                        {
                            ref var eleCom = ref _elePool.Get(eliminateEntity);
                            eleCom.LogicState = ElementLogicalState.Dying;
                            // 移动结束，打上销毁标签，让它死亡
                            _destroyTagPool.Add(eliminateEntity);
                        }));
                    index++;
                }
            }

            seq.OnComplete(() =>
            {
                ref var targetCom = ref _world.GetPool<TargetElementComponent>().Get(targetEntity);
                // 本身该不该消失
                if (targetCom.RemainTargetNum <= 0)
                    _destroyTagPool.Add(targetEntity);
            });
        }
    }
}