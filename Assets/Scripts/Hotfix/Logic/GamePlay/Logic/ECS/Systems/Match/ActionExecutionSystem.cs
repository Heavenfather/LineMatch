using HotfixLogic.Match;
using UnityEngine;

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
        private EcsPool<GridCellComponent> _gridPool; // 新增：需要访问格子数据
        private EcsPool<VisualBusyComponent> _busyPool;
        private EcsPool<EliminatedTag> _eliminateTagPool;
        private EcsPool<TargetElementComponent> _targetTagPool;

        private GameStateContext _context;
        private IElementFactoryService _elementService;
        private IBoard _board; // 缓存Board引用方便获取EntityID

        public void Init(IEcsSystems systems)
        {
            _context = systems.GetShared<GameStateContext>();
            _elementService = MatchBoot.Container.Resolve<IElementFactoryService>();
            _board = _context.Board;
            _world = systems.GetWorld();

            _actionFilter = _world.Filter<PendingActionsComponent>().End();

            _actionPool = _world.GetPool<PendingActionsComponent>();
            _elePool = _world.GetPool<ElementComponent>();
            _gridPool = _world.GetPool<GridCellComponent>();
            _busyPool = _world.GetPool<VisualBusyComponent>();
            _eliminateTagPool = _world.GetPool<EliminatedTag>();
            _targetTagPool = _world.GetPool<TargetElementComponent>();
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
                    _busyPool.Add(entity);
                    pending.Actions.Clear();
                }
            }
        }

        private void ApplyDamage(AtomicAction action)
        {
            // 1.根据 GridPos 查找当前最应该受击的实体 所以Damage的Action一定要传
            int targetEntity = FindTopDamageableEntity(action.GridPos);
            if (targetEntity <= 0)
            {
                //可能是个无效的格子或无效的实体
                return;
            }

            // 如果还是没找到（比如空格子），直接返回
            if (!_world.IsEntityAliveInternal(targetEntity)) return;

            // 2. 执行扣血逻辑
            if (_elePool.Has(targetEntity))
            {
                ref var ele = ref _elePool.Get(targetEntity);

                if (ele.LogicState == ElementLogicalState.Acting)
                {
                    return;
                }

                ele.EliminateCount -= action.Value;

                // 3. 打上正在操作的标签，先逻辑锁定
                if (ele.EliminateCount <= 0)
                {
                    ele.LogicState = ElementLogicalState.Acting;
                    if (!_eliminateTagPool.Has(targetEntity))
                    { 
                        // 为实体打上这个棋子可以被操作的标签
                        _eliminateTagPool.Add(targetEntity);
                    }
                }
                else
                {
                    // 没死，设置元素受击脏数据，由各自的系统去管理受击后的表现
                    ele.IsDamageDirty = true;
                }
            }
        }

        /// <summary>
        /// 在指定格子上寻找“最顶层”且“活着”的可消除实体
        /// 如果一些不能被消除的，必须要在这里拦截掉，否则在执行的时候就会被标上死亡标签
        /// </summary>
        private int FindTopDamageableEntity(Vector2Int pos)
        {
            // 1. 获取格子实体 ID
            if (!_board.TryGetGridEntity(pos.x, pos.y, out int gridEntity))
                return -1;

            if (!_gridPool.Has(gridEntity)) return -1;
            ref var grid = ref _gridPool.Get(gridEntity);

            if (grid.StackedEntityIds == null || grid.StackedEntityIds.Count == 0)
                return -1;

            int bestEntity = -1;
            int highestLayer = int.MinValue;

            // 2. 遍历堆叠列表 找出 ElementComponent.Layer 最大的
            foreach (var entId in grid.StackedEntityIds)
            {
                if (!_elePool.Has(entId)) continue;
                ref var ele = ref _elePool.Get(entId);

                // 忽略已经死亡的实体
                if (ele.LogicState == ElementLogicalState.Acting) continue;

                // 有些障碍物是直接盖住底层的其它棋子，其它棋子根本就不能被消 如卷帘
                if (_targetTagPool.Has(entId))
                    break;
                // 有些障碍物根本不能被销毁，它有自己的销毁机制，这里就交给各自的构建器去负责分析
                if (!_elementService.IsElementCanSelected(ele.Type, _world, entId))
                    continue;

                // 找 Layer 最大的
                if (ele.Layer > highestLayer)
                {
                    highestLayer = ele.Layer;
                    bestEntity = entId;
                }
            }

            return bestEntity;
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