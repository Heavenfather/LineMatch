using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HotfixCore.Extensions;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// TowDots模式下的搜寻点，这里只负责做表现，具体的数据来源在 TowDotsFunctionElementRule 中处理
    /// </summary>
    public class SearchDotsSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private TrailEmitter _trailEmitter;
        private EcsPool<ElementRenderComponent> _renderPool;
        private EcsPool<ElementPositionComponent> _positionPool;

        private EcsPool<PendingActionsComponent> _pendingActionsPool;
        private EcsPool<SearchDotComponent> _searchDotPool;
        private EcsPool<ElementComponent> _elementPool;

        private EcsPool<EliminatedTag> _eliminatePool;
        private IElementFactoryService _elementService;
        private IMatchRequestService _requestService;
        private List<int> _targetEntities;

        public void Init(IEcsSystems systems)
        {
            _requestService = MatchBoot.Container.Resolve<IMatchRequestService>();
            _elementService = MatchBoot.Container.Resolve<IElementFactoryService>();
            var context = systems.GetShared<GameStateContext>();
            _trailEmitter = context.SceneView.GetSceneRootComponent<TrailEmitter>("MatchCanvas", "GridBoard");

            _targetEntities = new List<int>();
            _world = systems.GetWorld();
            _filter = _world.Filter<ElementRenderComponent>().Include<SearchDotComponent>().End();

            _positionPool = _world.GetPool<ElementPositionComponent>();
            _renderPool = _world.GetPool<ElementRenderComponent>();
            _searchDotPool = _world.GetPool<SearchDotComponent>();
            _eliminatePool = _world.GetPool<EliminatedTag>();
            _pendingActionsPool = _world.GetPool<PendingActionsComponent>();
            _elementPool = _world.GetPool<ElementComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                if (!_world.IsEntityAliveInternal(entity))
                    continue;
                ref var dotCom = ref _searchDotPool.Get(entity);
                if (dotCom.IsColorDirty)
                {
                    SetElementColor(entity, MatchElementUtil.GetElementColor(dotCom.SearchDotBaseElementId));
                    dotCom.IsColorDirty = false;
                }

                if (_eliminatePool.Has(entity))
                {
                    // _eliminatePool.Del(entity);

                    ref var searchCom = ref _searchDotPool.Get(entity);
                    ref var positionCom = ref _positionPool.Get(entity);
                    if (searchCom.SearchDotsEntities != null && searchCom.SearchDotsEntities.Count > 0)
                    {
                        // 发射效果飞向那几个消除点
                        _targetEntities.Clear();
                        Vector3 startPos = positionCom.WorldPosition;
                        List<Vector3> targetPositions = new List<Vector3>(searchCom.SearchDotsEntities.Count);
                        for (int i = 0; i < searchCom.SearchDotsEntities.Count; i++)
                        {
                            if (_world.IsEntityAliveInternal(searchCom.SearchDotsEntities[i]))
                            {
                                ref var elementCom = ref _elementPool.Get(searchCom.SearchDotsEntities[i]);
                                if(elementCom.LogicState != ElementLogicalState.Idle)
                                    continue;
                                _targetEntities.Add(searchCom.SearchDotsEntities[i]);
                                ref var position = ref _positionPool.Get(searchCom.SearchDotsEntities[i]);
                                targetPositions.Add(position.WorldPosition);
                            }
                        }

                        _trailEmitter.Emitter(startPos, targetPositions, OnEmitterStepComplete, () => DelayDelSelf(entity));
                    }
                    else
                    {
                        // 被动触发的
                        _requestService.RequestSearchDot(_world, entity);
                    }
                }
            }
        }

        private void DelayDelSelf(int entity)
        {
            // await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
            _elementService.AddDestroyElementTag2Entity(_world, entity);
        }

        private void OnEmitterStepComplete(int index)
        {
            List<AtomicAction> actions = new List<AtomicAction>();

            int entity = _targetEntities[index];
            ref var position = ref _positionPool.Get(entity);
            actions.Add(new AtomicAction()
            {
                Type = MatchActionType.Damage,
                Value = 1,
                GridPos = new Vector2Int(position.X, position.Y)
            });
            MatchElementUtil.AddSingleNormalElementScore(_world, entity);
            
            int pendingEntity = _world.NewEntity();
            ref var pending = ref _pendingActionsPool.Add(pendingEntity);
            pending.Actions = actions;
        }

        private void SetElementColor(int entity, Color color)
        {
            ref var renderComponent = ref _renderPool.Get(entity);
            if (renderComponent.ViewInstance != null && renderComponent.ViewInstance.Icon != null)
            {
                renderComponent.ViewInstance.Icon.color = color;
                var flashIcon = renderComponent.ViewInstance.GetPart("FlashIcon");
                if (flashIcon != null)
                {
                    flashIcon.SetVisible(false);
                }
            }
        }
    }
}