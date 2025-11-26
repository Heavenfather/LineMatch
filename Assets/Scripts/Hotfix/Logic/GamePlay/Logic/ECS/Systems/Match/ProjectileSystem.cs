using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 处理消除新生成填充的棋子请求
    /// </summary>
    public class ProjectileSystem : IEcsInitSystem,IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<ProjectileRequestComponent> _requestPool;

        private GameStateContext _context;
        private IBoard _board;
        private ElementMapDB _elementDb;
        private IElementFactoryService _elementFactory;
        private IMatchService _matchService;
        
        public void Init(IEcsSystems systems)
        {
            _elementDb = ConfigMemoryPool.Get<ElementMapDB>();
            _context = systems.GetShared<GameStateContext>();
            _board = _context.Board;
            _matchService = _context.ServiceFactory.GetService(_context.CurrentMatchType);
            _elementFactory = _context.ServiceFactory.GetElementFactoryService();
            
            _world = systems.GetWorld();
            _filter = _world.Filter<ProjectileRequestComponent>().End();
            _requestPool = _world.GetPool<ProjectileRequestComponent>();
            
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                ref var request = ref _requestPool.Get(entity);
                ApplyGenRequest(request);
                
                //生成完成，移除实体
                _requestPool.Del(entity);
            }
        }

        private void ApplyGenRequest(in ProjectileRequestComponent request)
        {
            // 1.生成新的棋子实体
            // ref readonly ElementMap config = ref _elementDb[request.ConfigId];
            // int entity = _elementFactory.CreateElementEntity()
        }
    }
}