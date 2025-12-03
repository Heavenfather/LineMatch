namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 掉落后检测的执行系统，负责棋盘稳定后检测是否有可消除的元素，并执行消除逻辑
    /// </summary>
    public class PostDropActionSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;

        private EcsFilter _fallingFilter;
        private EcsFilter _requestFilter;
        private EcsFilter _elementFilter;
        private EcsFilter _settlementFilter;
        private EcsFilter _boardSystemCheckFilter;
        private EcsFilter _destroyTagFilter;
        private EcsFilter _shuffleFilter;

        private EcsPool<ElementComponent> _elePool;
        private EcsPool<BoardStableCheckTag> _checkTagPool;
        private EcsPool<BoardStableCheckSystemTag> _systemTag; //外部系统监听是否存在BoardStableCheckSystemTag来判断棋盘是否已稳定

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();

            _elePool = _world.GetPool<ElementComponent>();
            _checkTagPool = _world.GetPool<BoardStableCheckTag>();
            _systemTag = _world.GetPool<BoardStableCheckSystemTag>();

            _boardSystemCheckFilter = _world.Filter<BoardStableCheckSystemTag>().End();
            _fallingFilter = _world.Filter<FallAnimationComponent>().End();
            _requestFilter = _world.Filter<MatchRequestComponent>().End();
            _settlementFilter = _world.Filter<GameSettlementComponent>().End();
            _destroyTagFilter = _world.Filter<DestroyElementTagComponent>().End();
            _shuffleFilter = _world.Filter<ShuffleAnimationComponent>().End();

            // 筛选所有所有需要棋盘稳定后再执行的棋子 (排除掉已经有Tag的，防止重复添加)
            // ------------ 为了避免给不必要的棋子添加Tag，就在这里指定所有需要添加Tag的元素 ------------
            _elementFilter = _world.Filter<ElementComponent>().
                Include<BombComponent>() //炸弹，在TowDots模式下需要自动爆
                .Exclude<BoardStableCheckTag>() // 如果已经有Tag就不重复加
                .End();
        }

        public void Run(IEcsSystems systems)
        {
            // 1. 只有棋盘稳定时才派发通知
            if (_fallingFilter.GetEntitiesCount() > 0 ||
                _requestFilter.GetEntitiesCount() > 0 ||
                _settlementFilter.GetEntitiesCount() > 0 ||
                _destroyTagFilter.GetEntitiesCount() > 0 ||
                _shuffleFilter.GetEntitiesCount() > 0)
            {
                RemoveStableTag();
                return;
            }

            // 2. 给所有合格的棋子发"通知单"
            foreach (var entity in _elementFilter)
            {
                ref var ele = ref _elePool.Get(entity);

                // 只有 Idle 状态且没有被视觉锁定的棋子才配接收通知
                if (ele.LogicState == ElementLogicalState.Idle)
                {
                    if (!_checkTagPool.Has(entity))
                        _checkTagPool.Add(entity);
                }
            }
            AddStableTag();
        }

        private void AddStableTag()
        {
            if(_boardSystemCheckFilter.GetEntitiesCount() > 0)
                return;
            int entity = _world.NewEntity();
            _systemTag.Add(entity);
        }
        
        private void RemoveStableTag()
        {
            if(_boardSystemCheckFilter.GetEntitiesCount() <= 0)
                return;
            foreach (var entity in _boardSystemCheckFilter)
            {
                _systemTag.Del(entity);
            }
        }
    }
}