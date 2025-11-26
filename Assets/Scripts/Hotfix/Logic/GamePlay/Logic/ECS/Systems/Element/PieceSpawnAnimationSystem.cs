namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 棋子入场时的动画系统
    /// </summary>
    public class PieceSpawnAnimationSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<ElementSpawnComponent> _spawnComponent;
        
        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<ElementSpawnComponent>().Include<ElementComponent>()
                .Include<ElementRenderComponent>().End();
            _spawnComponent = _world.GetPool<ElementSpawnComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                ref var spawnComponent = ref _spawnComponent.Get(entity);
                if(!spawnComponent.IsDirty || spawnComponent.AnimType == SpawnAnimType.None) continue;

                if (spawnComponent.AnimType == SpawnAnimType.FallDown)
                {
                    // 掉落类型 TODO....
                }
                
                spawnComponent.IsDirty = false;
            }
        }
    }
}