using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public class BombElementSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private IElementFactoryService _elementService;
        private IMatchRequestService _requestService;
        private EcsPool<ElementPositionComponent> _posPool;
        private EcsPool<BoardStableCheckTag> _stableCheckPool;
        private EcsPool<BombComponent> _bombPool;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _elementService = MatchBoot.Container.Resolve<IElementFactoryService>();
            _requestService = MatchBoot.Container.Resolve<IMatchRequestService>();
            _posPool = _world.GetPool<ElementPositionComponent>();
            _stableCheckPool = _world.GetPool<BoardStableCheckTag>();
            _bombPool = _world.GetPool<BombComponent>();

            _filter = _world.Filter<ElementComponent>()
                .Include<BoardStableCheckTag>()
                .Include<BombComponent>().Include<EliminatedTag>()
                .End();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                ref var bombCom = ref _bombPool.Get(entity);
                // if (bombCom.AutoBomb)
                {
                    ref var posCom = ref _posPool.Get(entity);
                    _requestService.RequestBomb(_world, new Vector2Int(posCom.X, posCom.Y));

                    _elementService.AddDestroyElementTag2Entity(_world, entity);
                    // 立刻移除
                    _stableCheckPool.Del(entity);
                }
            }
        }
    }
}