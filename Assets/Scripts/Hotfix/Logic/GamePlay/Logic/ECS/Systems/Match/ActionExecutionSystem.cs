using HotfixLogic.Match;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 消除执行系统，负责消费 PendingActionsComponent，执行 Action。
    /// </summary>
    public class ActionExecutionSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _actionFilter;
        private EcsPool<PendingActionsComponent> _actionPool;
        private EcsPool<ElementComponent> _elePool;
        private EcsPool<VisualBusyComponent> _busyPool; // 视觉锁组件
        private EcsPool<ProjectileRequestComponent> _projectilePool;

        private GameStateContext _context;
        private IElementFactoryService _elementFactory;

        public void Init(IEcsSystems systems)
        {
            _context = systems.GetShared<GameStateContext>();
            _world = systems.GetWorld();
            // 筛选有指令包的实体
            _actionFilter = _world.Filter<PendingActionsComponent>().End();

            _actionPool = _world.GetPool<PendingActionsComponent>();
            _elePool = _world.GetPool<ElementComponent>();
            _busyPool = _world.GetPool<VisualBusyComponent>();
            _projectilePool = _world.GetPool<ProjectileRequestComponent>();
            
            _elementFactory = _context.ServiceFactory.GetElementFactoryService();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _actionFilter)
            {
                // 1. 如果当前指令包被视觉锁住了 (VisualBusy)，则跳过，等待 View 层回调解锁
                if (_busyPool.Has(entity)) continue;

                ref var pending = ref _actionPool.Get(entity);
                bool needWait = false;
                // 2. 遍历执行所有指令
                // 如果遇到需要等待的指令，暂停并在下一帧继续
                // 要么全做，要么有异步锁住整个包
                
                // 先执行所有即时动作，如果有异步动作，执行并挂锁
                for (int i = 0; i < pending.Actions.Count; i++)
                {
                    var action = pending.Actions[i];
                    switch (action.Type)
                    {
                        case MatchActionType.Damage:
                            ApplyDamage(action);
                            break;
                        case MatchActionType.AddScore:
                            MatchManager.Instance.AddScore(action.Value);
                            break;
                        case MatchActionType.Transform:
                        case MatchActionType.Spawn2Other:
                            ExecuteSpawnProjectile(action); 
                            // needWait = true; //现在是直接生成了，不用等待
                            break;
                        case MatchActionType.CollectTarget:
                            // 触发收集物目标的指令
                            break;
                    }
                }

                // 3. 如果没有异步操作，销毁指令包实体
                if (!needWait)
                {
                    _world.DelEntity(entity);
                }
                else
                {
                    // 如果有异步操作，挂上 Busy 锁，防止下一帧重复执行
                    // 同时也防止被销毁
                    _busyPool.Add(entity);
                    
                    // 注意：需要在 ExecuteSpawnProjectile 里注册回调，
                    // 回调里移除 VisualBusyComponent，并清空已执行的 Actions
                }
            }
        }
        
        private void ApplyDamage(AtomicAction action)
        {
            if (!_world.IsEntityAliveInternal(action.TargetEntity)) return;
            
            if (_elePool.Has(action.TargetEntity))
            {
                ref var ele = ref _elePool.Get(action.TargetEntity);
                if(ele.LogicState == ElementLogicalState.Dying)
                    return;
                ele.EliminateCount -= action.Value;

                // 如果血量归零，标记为 Dying，并发送销毁请求给 ElementDestroySystem
                if (ele.EliminateCount <= 0)
                {
                    ele.LogicState = ElementLogicalState.Dying;
                    // 添加 DestroyTag，交由 ElementDestroySystem 处理真正的销毁和特效
                    var tag = _world.GetPool<DestroyElementTagComponent>();
                    if (!tag.Has(action.TargetEntity))
                        tag.Add(action.TargetEntity);
                }
            }
        }

        private void ExecuteSpawnProjectile(AtomicAction action)
        {
            // 生成棋子
            var genData = (MatchGenerateFunctionItem)action.ExtraData;
            foreach (var item in genData.GenItemsData)
            {
                
            }
        }
        
        // View 层回调接口
        public void OnVisualComplete(int actionEntity)
        {
            if (_busyPool.Has(actionEntity))
                _busyPool.Del(actionEntity);
                 
            // 如果 Actions 都执行完了
            _world.DelEntity(actionEntity);
        }
    }
}