namespace Hotfix.Logic.GamePlay
{
    public class BackgroundElementSystem : IEcsInitSystem,IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _backgroundFilter;
        private EcsFilter _normalElementFilter;
        private IElementFactoryService _elementService;

        private EcsPool<ElementComponent> _elementPool;
        private EcsPool<ElementPositionComponent> _posPool;
        
        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _elementService = MatchBoot.Container.Resolve<IElementFactoryService>();

            _normalElementFilter = _world.Filter<NormalElementComponent>().Include<EliminatedTag>().End();
            _backgroundFilter = _world.Filter<BackgroundComponent>().End();
            _elementPool = _world.GetPool<ElementComponent>();
            _posPool = _world.GetPool<ElementPositionComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _backgroundFilter)
            {
                // 1.可能它自身就被多层爆炸爆到了，就可直接销毁了
                ref var elementCom = ref _elementPool.Get(entity);
                if (elementCom.LogicState == ElementLogicalState.Acting)
                {
                    DestroyElement(entity);
                    continue;
                }
                // 2.遍历被打上了标签的普通棋子
                foreach (var normalEntity in _normalElementFilter)
                {
                    ref var backgroundPos = ref _posPool.Get(entity);
                    ref var normalPos = ref _posPool.Get(normalEntity);
                    if (backgroundPos.X == normalPos.X && backgroundPos.Y == normalPos.Y)
                    {
                        DestroyElement(entity);
                        break;
                    }
                }
            }
        }
        
        private void DestroyElement(int entity)
        {
            _elementService.AddDestroyElementTag2Entity(_world, entity);
        }
    }
}