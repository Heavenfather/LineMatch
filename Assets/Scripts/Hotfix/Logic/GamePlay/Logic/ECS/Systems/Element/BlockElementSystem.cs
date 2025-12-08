namespace Hotfix.Logic.GamePlay
{
    public class BlockElementSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _sideBlockFilter; //旁消障碍物处理

        private IElementFactoryService _elementFactory;
        private EcsPool<EliminatedTag> _eliminatePool;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _elementFactory = MatchBoot.Container.Resolve<IElementFactoryService>();
            _sideBlockFilter = _world.Filter<BlockElementComponent>().Include<AdjacentEliminationComponent>()
                .Include<EliminatedTag>().End();
            _eliminatePool = _world.GetPool<EliminatedTag>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _sideBlockFilter)
            {
                _elementFactory.AddDestroyElementTag2Entity(_world, entity);
                // _eliminatePool.Del(entity);
            }
        }
    }
}