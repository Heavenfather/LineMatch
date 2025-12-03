using System.Collections.Generic;
using GameConfig;
using HotfixLogic.Match;
using UnityEngine;
using Logger = GameCore.Log.Logger;

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
        private IBoard _board;
        private ElementMapDB _elementMapDB;


        private List<AtomicAction> _optimizedActions = new List<AtomicAction>();
        private Dictionary<Vector2Int, int> _segmentDamageMap = new Dictionary<Vector2Int, int>();

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

            _optimizedActions = new List<AtomicAction>();
            _segmentDamageMap = new Dictionary<Vector2Int, int>();
        }

        public void Run(IEcsSystems systems)
        {
            float dt = Time.deltaTime;

            foreach (var entity in _actionFilter)
            {
                if (_busyPool.Has(entity)) continue;

                ref var pending = ref _actionPool.Get(entity);
                if (pending.Actions == null || pending.Actions.Count == 0)
                {
                    _world.DelEntity(entity);
                    continue;
                }

                // 使用时间分片算法进行优化，仅在第一次执行前处理
                if (!pending.IsOptimized)
                {
                    pending.Actions = OptimizeActionList(pending.Actions);
                    pending.IsOptimized = true;
                }

                var actions = pending.Actions;

                bool isPaused = false;

                // 状态机执行逻辑
                while (pending.ExecutionIndex < actions.Count)
                {
                    var action = actions[pending.ExecutionIndex];

                    // --- Delay 指令处理 ---
                    if (action.Type == MatchActionType.Delay)
                    {
                        if (pending.CurrentWaitTimer <= 0)
                        {
                            pending.CurrentWaitTimer = action.Value / 1000f;
                        }

                        pending.CurrentWaitTimer -= dt;

                        if (pending.CurrentWaitTimer > 0)
                        {
                            isPaused = true;
                            break; // 暂停，下一帧继续
                        }

                        // 时间到，重置计时器，指向下一条
                        pending.CurrentWaitTimer = 0;
                        pending.ExecutionIndex++;
                        continue;
                    }

                    // --- 普通指令 ---
                    ExecuteSingleAction(action);
                    pending.ExecutionIndex++;
                }

                // 全部执行完毕才销毁
                if (!isPaused && pending.ExecutionIndex >= actions.Count)
                {
                    _world.DelEntity(entity);
                }
            }
        }

        /// <summary>
        /// 时间分片合并
        /// 在保持 Delay/Spawn 顺序的前提下，合并它们之间的 Damage
        /// </summary>
        private List<AtomicAction> OptimizeActionList(List<AtomicAction> rawActions)
        {
            _optimizedActions.Clear();
            _segmentDamageMap.Clear();

            foreach (var action in rawActions)
            {
                // 1. 伤害指令：暂存并累加
                if (action.Type == MatchActionType.Damage)
                {
                    if (_segmentDamageMap.ContainsKey(action.GridPos))
                    {
                        _segmentDamageMap[action.GridPos] += action.Value;
                    }
                    else
                    {
                        _segmentDamageMap[action.GridPos] = action.Value;
                    }
                }
                else
                {
                    // 2. 非伤害指令：检查是否是"阻断点"
                    if (IsBarrierAction(action.Type))
                    {
                        // 遇到阻断（如Delay/Spawn），必须先把之前积攒的伤害结算并写入列表
                        FlushPendingDamages();

                        // 然后写入阻断指令本身 (保持了它在原始列表中的相对位置)
                        _optimizedActions.Add(action);
                    }
                    else
                    {
                        // 其他非阻断指令 (如 Score/Audio)，直接加入，不打断伤害合并
                        _optimizedActions.Add(action);
                    }
                }
            }

            // 循环结束，结算最后残留的伤害
            FlushPendingDamages();

            // 返回一个新的列表
            return new List<AtomicAction>(_optimizedActions);
        }

        private void FlushPendingDamages()
        {
            if (_segmentDamageMap.Count == 0) return;

            foreach (var kvp in _segmentDamageMap)
            {
                _optimizedActions.Add(new AtomicAction
                {
                    Type = MatchActionType.Damage,
                    GridPos = kvp.Key,
                    Value = kvp.Value
                });
            }

            _segmentDamageMap.Clear();
        }

        private void ExecuteSingleAction(AtomicAction action)
        {
            switch (action.Type)
            {
                case MatchActionType.Damage:
                    ApplyDamage(action);
                    break;
                case MatchActionType.AddScore:
                    MatchManager.Instance.AddScore(action.Value);
                    break;
                case MatchActionType.Transform:
                    // TODO: 变换逻辑
                    break;
                case MatchActionType.Spawn2Other:
                    ExecuteSpawn2Other(action);
                    break;
                case MatchActionType.Shuffle:
                    ExecuteShuffle(action);
                    break;
            }
        }

        /// <summary>
        /// 执行洗牌
        /// </summary>
        private void ExecuteShuffle(AtomicAction action)
        {
            // 创建洗牌请求实体
            int requestEntity = _world.NewEntity();
            var shufflePool = _world.GetPool<ShuffleRequestComponent>();
            ref var request = ref shufflePool.Add(requestEntity);
        }


        /// <summary>
        /// 应用伤害到指定格子
        /// </summary>
        private void ApplyDamage(AtomicAction action)
        {
            int remainingDamage = action.Value;
            Vector2Int gridPos = action.GridPos;

            // 防止死循环：最多处理 5 层堆叠
            const int MAX_ITERATIONS = 5;
            int iteration = 0;

            // 循环处理伤害，直到伤害耗尽或没有更多可消除的实体
            while (remainingDamage > 0 && iteration < MAX_ITERATIONS)
            {
                iteration++;

                // 1. 查找当前格子上最顶层的可消除实体
                int targetEntity = FindTopDamageableEntity(gridPos);

                // 如果没有找到可消除的实体，伤害结束
                if (targetEntity <= 0 || !_world.IsEntityAliveInternal(targetEntity))
                {
                    break;
                }

                // 2. 检查实体是否有效
                if (!_elePool.Has(targetEntity))
                {
                    break;
                }

                ref var ele = ref _elePool.Get(targetEntity);

                // 如果实体正在处理中，跳过
                if (ele.LogicState == ElementLogicalState.Acting ||
                    ele.LogicState == ElementLogicalState.Dying)
                {
                    break;
                }

                // 3. 处理循环类型的元素（特殊元素，伤害全部作用在它身上）
                if (ele.IsCycleElement())
                {
                    AddEliminateTag(targetEntity, remainingDamage, ref ele);
                    remainingDamage = 0;
                    break;
                }

                // 4. 应用伤害到当前实体
                int damageApplied = ApplyDamageToEntity(targetEntity, ref ele, remainingDamage);
                remainingDamage -= damageApplied;

                // 如果实体没有被完全消除（还有血量），伤害结束
                if (ele.EliminateCount > 0)
                {
                    break;
                }

                // 实体被消除，继续处理下一层（如果还有剩余伤害）
            }

            // 防止死循环的日志
            if (iteration >= MAX_ITERATIONS)
            {
                Logger.Error($"ApplyDamage: Max iterations reached at {gridPos}, possible infinite loop!");
            }
        }

        /// <summary>
        /// 应用伤害到单个实体
        /// </summary>
        private int ApplyDamageToEntity(int entityId, ref ElementComponent ele, int damage)
        {
            int totalDamageApplied = 0;
            int remainingDamage = damage;

            // 1. 扣除当前实体的消除次数
            int damageToApply = Mathf.Min(remainingDamage, ele.EliminateCount);
            ele.EliminateCount -= damageToApply;
            totalDamageApplied += damageToApply;
            remainingDamage -= damageToApply;

            // 2. 如果实体被消除
            if (ele.EliminateCount <= 0)
            {
                // 检查是否有多态转换
                if (remainingDamage > 0)
                {
                    int additionalDamage = ApplyTransitionDamage(ref ele, remainingDamage);
                    totalDamageApplied += additionalDamage;
                }

                // 添加消除标签
                AddEliminateTag(entityId, totalDamageApplied, ref ele);
            }
            else
            {
                // 实体没有被完全消除，只添加消除标签
                AddEliminateTag(entityId, totalDamageApplied, ref ele);
            }

            return totalDamageApplied;
        }

        /// <summary>
        /// 应用多态转换伤害
        /// 返回：额外消耗的伤害值
        /// </summary>
        private int ApplyTransitionDamage(ref ElementComponent ele, int remainingDamage)
        {
            int additionalDamage = 0;
            int currentConfigID = ele.ConfigId;

            // 防止死循环：最多转换 10 次
            const int MAX_TRANSITIONS = 10;
            int transitionCount = 0;

            while (remainingDamage > 0 && transitionCount < MAX_TRANSITIONS)
            {
                transitionCount++;

                // 尝试转换到下一层元素
                if (!_transitionRule.TryTransitionToNextElement(currentConfigID, _matchService, out var nextConfigID))
                {
                    // 没有下一层，转换结束
                    break;
                }

                // 获取下一层元素的配置
                ref readonly ElementMap nextConfig = ref _elementMapDB[nextConfigID];

                // 如果下一层元素的消除次数无效，转换结束
                if (nextConfig.eliminateCount < 0)
                {
                    break;
                }

                // 应用伤害到下一层
                int damage = Mathf.Min(remainingDamage, nextConfig.eliminateCount);
                additionalDamage += damage;
                remainingDamage -= damage;

                // 如果下一层也被消除，继续转换
                if (remainingDamage > 0)
                {
                    currentConfigID = nextConfigID;
                }
                else
                {
                    break;
                }
            }

            return additionalDamage;
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
        /// 判断是否是阻断性指令
        /// </summary>
        private bool IsBarrierAction(MatchActionType type)
        {
            // Delay: 时间阻断，前后逻辑必须分开
            // Spawn/Transform: 空间阻断，改变了格子上的实体引用，必须先结算之前的伤害
            return type == MatchActionType.Delay ||
                   type == MatchActionType.Spawn2Other ||
                   type == MatchActionType.Transform;
        }

        private void ExecuteSpawn2Other(AtomicAction action)
        {
            if (action.ExtraData is MatchGenerateFunctionItem genData)
            {
                List<int> genEntities = new List<int>(genData.GenItemsData.Count);
                foreach (var itemData in genData.GenItemsData)
                {
                    int entity = ExecuteSingleSpawnAction(itemData);
                    genEntities.Add(entity);
                }

                // 批量自动生成，需要立刻爆
                for (int i = 0; i < genEntities.Count; i++)
                {
                    ref var genEle = ref _elePool.Get(genEntities[i]);
                    AddEliminateTag(genEntities[i], genEle.EliminateCount, ref genEle);
                }
            }
            else if (action.ExtraData is GenItemData itemData)
            {
                ExecuteSingleSpawnAction(itemData);
            }
        }

        private int ExecuteSingleSpawnAction(GenItemData itemData)
        {
            // 在生成前，自动查找并清理该位置的旧棋子
            int oldEntity = FindTopDamageableEntity(itemData.GenCoord);

            if (oldEntity > 0 && _world.IsEntityAliveInternal(oldEntity))
            {
                if (_elePool.Has(oldEntity))
                {
                    ref var oldEle = ref _elePool.Get(oldEntity);
                    // 标记为 Acting (防止同一帧重复选中) 并打上 EliminatedTag
                    AddEliminateTag(oldEntity, oldEle.MaxEliminateCount, ref oldEle);
                }
            }

            // 创建新实体
            int newEntityId = _elementService.CreateElementEntity(
                _context,
                _matchService,
                itemData.ConfigId,
                itemData.GenCoord.x,
                itemData.GenCoord.y,
                itemData.ElementSize.x,
                itemData.ElementSize.y
            );

            // 注册新实体
            RegisterToGrid(oldEntity, newEntityId, itemData.GenCoord.x, itemData.GenCoord.y, itemData.ElementSize.x,
                itemData.ElementSize.y);
            return newEntityId;
        }

        private void RegisterToGrid(int oldEntity, int entityId, int startX, int startY, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int cx = startX + i;
                    int cy = startY + j;

                    if (!_board.TryGetGridEntity(cx, cy, out int gridEntity)) continue;
                    if (!_gridPool.Has(gridEntity)) continue;

                    ref var gridComp = ref _gridPool.Get(gridEntity);
                    if (gridComp.StackedEntityIds == null)
                        gridComp.StackedEntityIds = new List<int>();
                    if (gridComp.StackedEntityIds.Contains(oldEntity))
                        gridComp.StackedEntityIds.Remove(oldEntity);
                    gridComp.StackedEntityIds.Add(entityId);
                    gridComp.IsBlank = false;
                }
            }
        }
    }
}