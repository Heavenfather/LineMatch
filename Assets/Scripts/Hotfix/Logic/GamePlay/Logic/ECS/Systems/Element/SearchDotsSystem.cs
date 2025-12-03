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
        private IMatchServiceFactory _factory;
        private EcsPool<ElementRenderComponent> _renderPool;
        private EcsPool<ElementPositionComponent> _positionPool;

        private EcsPool<SearchDotComponent> _searchDotPool;

        private EcsPool<EliminatedTag> _eliminatePool;
        private IElementFactoryService _elementService;
        private IMatchRequestService _requestService;

        public void Init(IEcsSystems systems)
        {
            _factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            _requestService = MatchBoot.Container.Resolve<IMatchRequestService>();
            _elementService = MatchBoot.Container.Resolve<IElementFactoryService>();
            var context = systems.GetShared<GameStateContext>();
            _trailEmitter = context.SceneView.GetSceneRootComponent<TrailEmitter>("MatchCanvas", "GridBoard");

            _world = systems.GetWorld();
            _filter = _world.Filter<ElementRenderComponent>().Include<SearchDotComponent>().End();

            _positionPool = _world.GetPool<ElementPositionComponent>();
            _renderPool = _world.GetPool<ElementRenderComponent>();
            _searchDotPool = _world.GetPool<SearchDotComponent>();
            _eliminatePool = _world.GetPool<EliminatedTag>();
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
                    _eliminatePool.Del(entity);

                    ref var searchCom = ref _searchDotPool.Get(entity);
                    ref var positionCom = ref _positionPool.Get(entity);
                    if (searchCom.SearchDotsEntities != null && searchCom.SearchDotsEntities.Count > 0)
                    {
                        // 发射效果飞向那几个消除点
                        Vector3 startPos = positionCom.WorldPosition;
                        List<Vector3> targetPositions = new List<Vector3>(searchCom.SearchDotsEntities.Count);
                        for (int i = 0; i < searchCom.SearchDotsEntities.Count; i++)
                        {
                            ref var position = ref _positionPool.Get(searchCom.SearchDotsEntities[i]);
                            targetPositions.Add(position.WorldPosition);
                        }

                        _trailEmitter.Emitter(startPos, targetPositions, (index) => { }, () => DelayDelSelf(entity));
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