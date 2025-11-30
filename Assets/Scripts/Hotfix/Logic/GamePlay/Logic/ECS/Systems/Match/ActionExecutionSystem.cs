using System.Collections.Generic;
using GameConfig;
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
        private EcsPool<GridCellComponent> _gridPool;
        private EcsPool<VisualBusyComponent> _busyPool;
        private EcsPool<EliminatedTag> _eliminateTagPool;
        private EcsPool<TargetElementComponent> _targetTagPool;

        private GameStateContext _context;
        private IElementFactoryService _elementService;
        private IElementTransitionRuleService _transitionRule;
        private IMatchService _matchService;
        private IBoard _board; // 缓存Board引用方便获取EntityID
        private List<AtomicAction> _mergedActions;
        private Dictionary<Vector2Int, int> _damageMap;
        private ElementMapDB _elementMapDB;

        public void Init(IEcsSystems systems)
        {
            _elementMapDB = ConfigMemoryPool.Get<ElementMapDB>();
            _context = systems.GetShared<GameStateContext>();
            _transitionRule = MatchBoot.Container.Resolve<IElementTransitionRuleService>();
            _elementService = MatchBoot.Container.Resolve<IElementFactoryService>();
            _matchService = _context.ServiceFactory.GetService(_context.CurrentMatchType);
            _board = _context.Board;
            _world = systems.GetWorld();

            _actionFilter = _world.Filter<PendingActionsComponent>().End();

            _actionPool = _world.GetPool<PendingActionsComponent>();
            _elePool = _world.GetPool<ElementComponent>();
            _gridPool = _world.GetPool<GridCellComponent>();
            _busyPool = _world.GetPool<VisualBusyComponent>();
            _eliminateTagPool = _world.GetPool<EliminatedTag>();
            _targetTagPool = _world.GetPool<TargetElementComponent>();

            _mergedActions = new List<AtomicAction>();
            _damageMap = new Dictionary<Vector2Int, int>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _actionFilter)
            {
                // 1. 如果当前指令包被视觉锁住了 (VisualBusy)，则跳过，等待 View 层回调解锁
                if (_busyPool.Has(entity)) continue;

                ref var pending = ref _actionPool.Get(entity);
                bool needWait = false;
                // 2. 合并同一格子的Damage Action，将相同位置的伤害值累加
                List<AtomicAction> mergedActions = MergeDamageActions(pending.Actions);

                // 3. 遍历执行所有指令（合并后的）
                // 先执行所有即时动作，如果有异步动作，执行并挂锁
                for (int i = 0; i < mergedActions.Count; i++)
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
                            ExecuteSpawnOrTransform(action);
                            // needWait = true; //现在是直接生成了，不用等待
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
            // 1.根据 GridPos 查找当前最应该受击的实体 所以Damage的Action一定要传格子信息
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
                if (ele.LogicState == ElementLogicalState.Acting || ele.LogicState == ElementLogicalState.Dying)
                {
                    return;
                }

                // 循环类型的元素
                if (ele.IsCycleElement())
                {
                    // 次数全作用在该元素上
                    AddEliminateTag(targetEntity, action.Value, ref ele);
                    return;
                }

                // 处理剩余伤害
                int remainingDamage = action.Value;
                int damageToApply = Mathf.Min(remainingDamage, ele.EliminateCount);
                ele.EliminateCount -= damageToApply;
                int totalDamage = damageToApply;
                remainingDamage -= damageToApply;
                if (ele.EliminateCount <= 0)
                {
                    if (remainingDamage <= 0)
                    {
                        // 实体死亡并且没有了剩余伤害
                        AddEliminateTag(targetEntity, totalDamage, ref ele);
                        return;
                    }

                    // 还有剩余次数，检查当前元素是否是多态的，可以转换成另外一层元素
                    int currentConfigID = ele.ConfigId;
                    while (remainingDamage > 0 &&
                           _transitionRule.TryTransitionToNextElement(currentConfigID, _matchService,
                               out var nextConfigID))
                    {
                        ref readonly ElementMap nextConfig = ref _elementMapDB[nextConfigID];
                        if (nextConfig.eliminateCount < 0)
                            break;
                        int damage = Mathf.Min(remainingDamage, nextConfig.eliminateCount);
                        totalDamage += damage;
                        remainingDamage -= damage;

                        if (remainingDamage > 0)
                            currentConfigID = nextConfigID;
                    }

                    AddEliminateTag(targetEntity, totalDamage, ref ele);
                    if (remainingDamage > 0)
                    {
                        // 继续递归应用堆叠的实体
                        ApplyDamage(new AtomicAction
                        {
                            Type = MatchActionType.Damage,
                            GridPos = action.GridPos,
                            Value = remainingDamage
                        });
                    }
                }
                else
                {
                    AddEliminateTag(targetEntity, totalDamage, ref ele);
                }
            }
        }

        private void AddEliminateTag(int entity, int count, ref ElementComponent element)
        {
            if (!_eliminateTagPool.Has(entity))
            {
                element.LogicState = ElementLogicalState.Acting;
                ref var tag = ref _eliminateTagPool.Add(entity);
                tag.EliminateCount = count;
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

                // 忽略待处理的实体
                if (ele.LogicState == ElementLogicalState.Acting ||
                    ele.LogicState == ElementLogicalState.Dying) continue;

                // 有些障碍物是直接盖住底层的其它棋子，其它棋子根本就不能被消 如卷帘
                if (_targetTagPool.Has(entId))
                    break;
                // 有些障碍物根本不能被销毁，它有自己的销毁机制，这里就交给各自的构建器去负责分析
                if (!_elementService.IsElementCanSelected(ele.Type, _world, entId))
                    continue;
                // 如果是循环的元素，就说明这个元素就始终占着这个格子
                if (ele.IsCycleElement())
                {
                    bestEntity = entId;
                    break;
                }

                // 找 Layer 最大的
                if (ele.Layer > highestLayer)
                {
                    highestLayer = ele.Layer;
                    bestEntity = entId;
                }
            }

            return bestEntity;
        }

        /// <summary>
        /// 合并同一格子的Damage Action，将相同位置的伤害值累加
        /// </summary>
        /// <param name="actions">原始Action列表</param>
        /// <returns>合并后的Action列表</returns>
        private List<AtomicAction> MergeDamageActions(List<AtomicAction> actions)
        {
            _mergedActions.Clear();
            _damageMap.Clear();

            // 遍历所有动作
            foreach (var action in actions)
            {
                if (action.Type == MatchActionType.Damage)
                {
                    // 如果是Damage类型，进行合并处理
                    Vector2Int gridPos = action.GridPos;
                    if (_damageMap.ContainsKey(gridPos))
                    {
                        // 位置已存在，累加伤害值
                        _damageMap[gridPos] += action.Value;
                    }
                    else
                    {
                        // 位置不存在，添加到字典
                        _damageMap[gridPos] = action.Value;
                    }
                }
                else
                {
                    // 非Damage类型的Action直接添加到结果列表
                    _mergedActions.Add(action);
                }
            }

            // 将合并后的Damage Action添加到结果列表
            foreach (var kvp in _damageMap)
            {
                AtomicAction mergedDamageAction = new AtomicAction
                {
                    Type = MatchActionType.Damage,
                    GridPos = kvp.Key,
                    Value = kvp.Value
                };
                _mergedActions.Add(mergedDamageAction);
            }

            return _mergedActions;
        }

        private void ExecuteSpawnOrTransform(AtomicAction action)
        {
            // 1. 解析生成数据
            if (action.ExtraData is MatchGenerateFunctionItem genData)
            {
                foreach (var itemData in genData.GenItemsData)
                {
                    // 2. 创建实体
                    int newEntityId = _elementService.CreateElementEntity(
                        _context,
                        _matchService,
                        itemData.ConfigId,
                        itemData.GenCoord.x,
                        itemData.GenCoord.y,
                        itemData.ElementSize.x,
                        itemData.ElementSize.y
                    );

                    // 3.立即将新实体注册到 Grid 数据中
                    RegisterToGrid(newEntityId,itemData.GenCoord.x, itemData.GenCoord.y, itemData.ElementSize.x, itemData.ElementSize.y);
                    //后续就是交由ElementSpawnSystem 和 ElementViewInitSystem 来处理新生了
                }
            }
        }

        private void RegisterToGrid(int entityId, int startX, int startY, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    // start这里在上一步已经填充了，这里只需要补充它占据的其它格子
                    int cx = startX + i;
                    int cy = startY + j;

                    // 获取 (cx, cy) 处的格子组件
                    var gridEntity = _context.Board[cx, cy];
                    ref var gridComp = ref _gridPool.Get(gridEntity);
                    gridComp.StackedEntityIds.Add(entityId);
                }
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