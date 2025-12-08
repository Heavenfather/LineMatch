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
        private EcsPool<NormalElementComponent> _normalPool;
        private EcsPool<ElementPositionComponent> _positionPool;
        private EcsPool<BackgroundComponent> _backgroundPool;

        private GameStateContext _context;
        private IElementFactoryService _elementService;
        private IElementTransitionRuleService _transitionRule;
        private IMatchService _matchService;
        private IBoard _board;
        private ElementMapDB _elementMapDB;

        private List<AtomicAction> _optimizedActions = new List<AtomicAction>();

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

            _backgroundPool = _world.GetPool<BackgroundComponent>();
            _positionPool = _world.GetPool<ElementPositionComponent>();
            _normalPool = _world.GetPool<NormalElementComponent>();
            _actionPool = _world.GetPool<PendingActionsComponent>();
            _elePool = _world.GetPool<ElementComponent>();
            _gridPool = _world.GetPool<GridCellComponent>();
            _busyPool = _world.GetPool<VisualBusyComponent>();
            _eliminateTagPool = _world.GetPool<EliminatedTag>();
            _targetTagPool = _world.GetPool<TargetElementComponent>();

            _optimizedActions = new List<AtomicAction>();
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
        /// 时间分片优化
        /// </summary>
        private List<AtomicAction> OptimizeActionList(List<AtomicAction> rawActions)
        {
            _optimizedActions.Clear();

            // 暂时不做任何优化，直接返回原始列表
            // 后续可以添加对Score、Audio等指令的合并优化
            return rawActions;
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
                    ExecuteTransform(action);
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
        /// 执行元素转换（多层障碍物消除一层后转换到下一层）
        /// </summary>
        private void ExecuteTransform(AtomicAction action)
        {
            int oldEntity = action.Value;
            int nextConfigId = (int)action.ExtraData;
            Vector2Int gridPos = action.GridPos;

            // 检查旧实体是否还存在
            if (!_world.IsEntityAliveInternal(oldEntity))
                return;

            if (!_elePool.Has(oldEntity))
                return;

            ref var oldElement = ref _elePool.Get(oldEntity);

            // 获取旧实体的位置和尺寸
            int width = oldElement.Width;
            int height = oldElement.Height;

            // 销毁旧实体
            _elementService.AddDestroyElementTag2Entity(_world, oldEntity);

            // 创建新实体
            int newEntity = _elementService.CreateElementEntity(
                _context,
                _matchService,
                nextConfigId,
                ElementBuildSource.Dynamic,
                gridPos.x, gridPos.y,
                width, height
            );

            // 将新实体添加到格子
            RegisterToGrid(-1, newEntity, gridPos.x, gridPos.y, width, height);
        }

        /// <summary>
        /// 应用伤害到指定格子
        /// </summary>
        private void ApplyDamage(AtomicAction action)
        {
            int damageValue = action.Value; // 这次伤害命令的伤害值（通常为1）
            Vector2Int gridPos = action.GridPos;

            // 1. 找到格子上最顶层可伤害的实体
            int targetEntity = FindTopDamageableEntity(gridPos);

            if (targetEntity < 0)
                return;

            if (!_elePool.Has(targetEntity))
                return;

            ref var ele = ref _elePool.Get(targetEntity);

            // 2. 累积伤害到EliminatedTag
            // 各元素System读取Tag.EliminateCount来决定如何处理
            AddOrAccumulateEliminateTag(targetEntity, damageValue, EliminateReason.Damage);
        }

        /// <summary>
        /// 添加或累积EliminatedTag
        /// </summary>
        private void AddOrAccumulateEliminateTag(int entityId, int damage, EliminateReason reason)
        {
            if (damage <= 0)
                return;

            if (_eliminateTagPool.Has(entityId))
            {
                // 累积伤害次数
                _elementService.AddEliminateTag2Entity(_world, entityId, damage, reason);
            }
            else
            {
                // 首次添加标签
                _elementService.AddEliminateTag2Entity(_world, entityId, damage, reason);
                
                // Normal元素继续传递伤害给Background
                if (_normalPool.Has(entityId) && reason == EliminateReason.Damage)
                {
                    ref var pos = ref _positionPool.Get(entityId);
                    if (_board.TryGetGridEntity(pos.X, pos.Y, out int gridEntity))
                    {
                        ref var grid = ref _gridPool.Get(gridEntity);
                        for (int i = 0; i < grid.StackedEntityIds.Count; i++)
                        {
                            if(grid.StackedEntityIds[i] == entityId)
                                continue;
                            if (_backgroundPool.Has(grid.StackedEntityIds[i]))
                            {
                                ref var backgroundEle = ref _elePool.Get(grid.StackedEntityIds[i]);
                                backgroundEle.LogicState = ElementLogicalState.Acting;
                                if (!_eliminateTagPool.Has(grid.StackedEntityIds[i]))
                                {
                                    ref var backgroundTag = ref _eliminateTagPool.Add(grid.StackedEntityIds[i]);
                                    backgroundTag.EliminateCount = 1;
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 在指定格子上寻找"最顶层"且"可伤害"的实体
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

            // 2. 按Layer从高到低排序，找到第一个可伤害的实体
            List<int> sortedEntities = new List<int>(grid.StackedEntityIds);
            sortedEntities.Sort((a, b) =>
            {
                if (!_elePool.Has(a) || !_elePool.Has(b))
                    return 0;
                ref var eleA = ref _elePool.Get(a);
                ref var eleB = ref _elePool.Get(b);
                return eleB.Layer.CompareTo(eleA.Layer); // 从高到低
            });

            // 3. 遍历排序后的实体，找到第一个可伤害的
            foreach (var entId in sortedEntities)
            {
                if (!_elePool.Has(entId)) continue;
                ref var ele = ref _elePool.Get(entId);

                // 水元素不能被销毁
                if (ele.Type == ElementType.SpreadWater)
                    continue;

                // 有些障碍物是直接盖住底层的其它棋子，其它棋子根本就不能被消除（如卷帘）
                if (_targetTagPool.Has(entId))
                    break;

                // 有些障碍物根本不能被销毁，它有自己的销毁机制
                if (!_elementService.IsElementCanSelected(ele.Type, _world, entId))
                    continue;

                // 如果是循环元素，就始终占着这个格子
                if (ele.IsCycleElement())
                {
                    return entId;
                }

                // 关键判断：检查实体是否已经死亡
                if (ele.LogicState == ElementLogicalState.Dying)
                {
                    // 已经死亡，继续查找下一个
                    continue;
                }

                // 如果实体正在Acting，检查它是否还能承受伤害
                if (ele.LogicState == ElementLogicalState.Acting)
                {
                    // 检查累积的伤害是否已经超过实体的承受能力
                    if (_eliminateTagPool.Has(entId))
                    {
                        ref var tag = ref _eliminateTagPool.Get(entId);
                        
                        // 计算实体还能承受多少伤害
                        int maxDamageCapacity = CalculateMaxDamageCapacity(ref ele);
                        
                        // 如果已经达到或超过承受上限，说明实体已经"死亡"，查找下一个
                        if (tag.EliminateCount >= maxDamageCapacity)
                        {
                            continue;
                        }
                    }
                    
                    // 还能承受伤害，返回这个实体
                    return entId;
                }

                // 正常状态的实体，可以伤害
                return entId;
            }

            return -1;
        }

        /// <summary>
        /// 计算实体能承受的最大伤害容量
        /// 包括当前形态的EliminateCount + 所有可能的多态转换形态的EliminateCount
        /// </summary>
        private int CalculateMaxDamageCapacity(ref ElementComponent ele)
        {
            int capacity = ele.EliminateCount;

            // 如果有多态转换，递归计算所有形态的承受能力
            int currentConfigId = ele.ConfigId;
            int transitionCount = 0;
            const int MAX_TRANSITIONS = 10;

            while (transitionCount < MAX_TRANSITIONS)
            {
                transitionCount++;

                // 尝试转换到下一层
                if (!_transitionRule.TryTransitionToNextElement(currentConfigId, _matchService, out var nextConfigID))
                {
                    break;
                }

                ref readonly ElementMap nextConfig = ref _elementMapDB[nextConfigID];

                if (nextConfig.eliminateCount < 0)
                {
                    break;
                }

                capacity += ele.EliminateCount;
                currentConfigId = nextConfigID;
            }

            return capacity;
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
                    AddOrAccumulateEliminateTag(genEntities[i], genEle.EliminateCount, EliminateReason.Replace);
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
                    AddOrAccumulateEliminateTag(oldEntity, oldEle.MaxEliminateCount, EliminateReason.Replace);
                }
            }

            // 创建新实体
            int newEntityId = _elementService.CreateElementEntity(
                _context,
                _matchService,
                itemData.ConfigId,
                ElementBuildSource.Dynamic,
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
                }
            }
        }
    }
}
