using System.Collections.Generic;
using HotfixCore.MemoryPool;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 消除分析系统，负责消费 MatchRequestComponent，调用 Rule，生产 PendingActionsComponent。
    /// </summary>
    public class MatchAnalysisSystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        private EcsWorld _world;
        private MatchStateContext _stateContext;
        private EcsFilter _requestFilter;
        private EcsPool<MatchRequestComponent> _requestPool;
        private EcsPool<PendingActionsComponent> _pendingActionPool;
        private EcsPool<DropAnalysisComponent> _dropAnalysisPool;
        private EcsPool<ElementComponent> _elePool;

        private IMatchServiceFactory _matchServiceFactory;
        private IMatchService _matchService; // 当前关卡的消除服务
        private MatchRuleContext _context;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            var context = systems.GetShared<GameStateContext>();
            _stateContext = context.MatchStateContext;
            _matchServiceFactory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            _matchService = context.ServiceFactory.GetService(context.CurrentMatchType);

            // 筛选所有 "消除请求"
            _requestFilter = _world.Filter<MatchRequestComponent>().End();

            _elePool = _world.GetPool<ElementComponent>();
            _requestPool = _world.GetPool<MatchRequestComponent>();
            _pendingActionPool = _world.GetPool<PendingActionsComponent>();
            _dropAnalysisPool = _world.GetPool<DropAnalysisComponent>();

            _context = MemoryPool.Acquire<MatchRuleContext>();
            _context.World = _world;
            _context.Board = systems.GetShared<GameStateContext>().Board;
            _context.MatchService = _matchService;
        }

        public void Run(IEcsSystems systems)
        {
            // 处理所有的消除请求
            foreach (var entity in _requestFilter)
            {
                ref var req = ref _requestPool.Get(entity);

                // 是否是玩家主动执行的操作
                bool isPlayerMove =
                    (req.Type == MatchRequestType.PlayerLine || req.Type == MatchRequestType.PlayerSquare);
                // 1. 根据请求类型获取规则
                IMatchRule rule = _matchServiceFactory.GetMatchRule(req.Type);

                if (rule != null)
                {
                    // 2. 构建上下文
                    _context.Request = req;

                    // 3. 执行规则评估，生成指令列表
                    List<AtomicAction> actions = new List<AtomicAction>();
                    rule.Evaluate(_context, ref actions);

                    // 4. 如果生成了指令，创建 "指令包实体" 传递给 ActionExecutionSystem
                    if (actions.Count > 0)
                    {
                        CreatePendingActions(actions, _context.BanDropElementId);

                        // 5. 锁定涉及的棋子状态，防止玩家重复操作或掉落系统干扰
                        LockEntitiesState(req.InvolvedEntities);

                        // 主动执行步骤，扣除步数
                        if (isPlayerMove)
                        {
                            _stateContext.ResumeStep();
                        }
                    }
                }


                // 6. 消费完成，立刻销毁请求实体
                _world.DelEntity(entity);
                _context.Clear();
            }
        }

        private void CreatePendingActions(List<AtomicAction> actions, int banDropId)
        {
            // 创建一个新的实体，仅用于承载指令列表
            int actionEntity = _world.NewEntity();
            ref var pending = ref _pendingActionPool.Add(actionEntity);
            pending.Actions = actions;

            // 创建一个新的掉落规则实体，用于掉落系统使用
            int dropEntity = _world.NewEntity();
            ref var drop = ref _dropAnalysisPool.Add(dropEntity);
            drop.BanDropElementId = banDropId;
        }

        private void LockEntitiesState(List<int> entities)
        {
            if (entities == null) return;
            foreach (var ent in entities)
            {
                if (_elePool.Has(ent))
                {
                    ref var state = ref _elePool.Get(ent);
                    // InputSystem 会忽略非 Idle 的棋子
                    state.LogicState = ElementLogicalState.Matching;
                }
            }
        }

        public void Destroy(IEcsSystems systems)
        {
            MemoryPool.Release(_context);
        }
    }
}