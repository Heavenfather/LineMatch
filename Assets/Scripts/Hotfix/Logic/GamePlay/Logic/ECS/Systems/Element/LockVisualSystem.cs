namespace Hotfix.Logic.GamePlay
{
    public class LockVisualSystem : IEcsInitSystem,IEcsRunSystem
    {
        private EcsFilter _filter;
        private EcsPool<ElementRenderComponent> _renderPool;
        private EcsPool<ElementPositionComponent> _posPool;
        private EcsPool<ElementComponent> _elePool;
        private GameStateContext _context;
        
        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _filter = world.Filter<LockComponent>()
                .Include<ElementRenderComponent>()
                .Include<ElementPositionComponent>()
                .End();
            
        }

        public void Run(IEcsSystems systems)
        {
            
        }
    }
}