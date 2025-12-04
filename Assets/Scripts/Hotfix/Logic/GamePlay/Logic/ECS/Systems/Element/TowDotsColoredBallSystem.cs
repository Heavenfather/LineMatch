using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// TowDots模式彩球点
    /// </summary>
    public class TowDotsColoredBallSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private TrailEmitter _trailEmitter;
        private EcsPool<EliminatedTag> _eliminatePool;
        private IElementFactoryService _elementService;
        private EcsPool<ElementPositionComponent> _positionPool;
        private EcsPool<TowDotsColoredBallComponent> _coloredBallPool;
        private EcsPool<PendingActionsComponent> _pendingActionsPool;
        private EcsPool<VisualBusyComponent> _busyPool;

        private List<int> _targetEntities;

        public void Init(IEcsSystems systems)
        {
            _elementService = MatchBoot.Container.Resolve<IElementFactoryService>();

            var context = systems.GetShared<GameStateContext>();
            _trailEmitter = context.SceneView.GetSceneRootComponent<TrailEmitter>("MatchCanvas", "GridBoard");
            _world = systems.GetWorld();
            _filter = _world.Filter<ElementRenderComponent>().Include<TowDotsColoredBallComponent>()
                .Include<VariableColorComponent>().End();

            _targetEntities = new List<int>();
            _coloredBallPool = _world.GetPool<TowDotsColoredBallComponent>();
            _eliminatePool = _world.GetPool<EliminatedTag>();
            _positionPool = _world.GetPool<ElementPositionComponent>();
            _pendingActionsPool = _world.GetPool<PendingActionsComponent>();
            _busyPool = _world.GetPool<VisualBusyComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                if (!_world.IsEntityAliveInternal(entity))
                    continue;
                // 处理消除标签
                if (_eliminatePool.Has(entity))
                {
                    _eliminatePool.Del(entity);
                    _busyPool.Add(entity);

                    ref var com = ref _coloredBallPool.Get(entity);
                    if (com.CollectedEntities != null && com.CollectedEntities.Count > 0)
                    {
                        // 发射效果飞向消除点
                        _targetEntities.Clear();
                        ref var positionCom = ref _positionPool.Get(entity);
                        Vector3 startPos = positionCom.WorldPosition;
                        List<Vector3> targetPositions = new List<Vector3>(com.CollectedEntities.Count);

                        foreach (var targetEntity in com.CollectedEntities)
                        {
                            if (_world.IsEntityAliveInternal(targetEntity))
                            {
                                _targetEntities.Add(targetEntity);
                                ref var position = ref _positionPool.Get(targetEntity);
                                targetPositions.Add(position.WorldPosition);
                            }
                        }

                        _trailEmitter.Emitter(startPos, targetPositions, OnEmitterStepComplete,
                            () => DelayDestroySelf(entity, 0.3f).Forget());
                    }
                    else
                    {
                        DelayDestroySelf(entity, 0).Forget();
                    }
                }
            }
        }

        private void OnEmitterStepComplete(int index)
        {
            List<AtomicAction> actions = new List<AtomicAction>();

            int entity = _targetEntities[index];
            if(!_world.IsEntityAliveInternal(entity))
                return;
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

        private async UniTask DelayDestroySelf(int entity, float delay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            // 延迟销毁，给动画播放时间
            _elementService.AddDestroyElementTag2Entity(_world, entity);
            _busyPool.Del(entity);
        }
    }
}