using System.Collections.Generic;
using HotfixCore.Extensions;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public class TowDotsFunctionElementRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.TowDotsFunctionElement;

        // 搜寻点目标数量，可以从配置读取
        private const int SearchTargetCount = 4;
        private HashSet<int> _addedEntities = new HashSet<int>();
        private bool _isHaveConnectNormal;

        public void Evaluate(MatchRuleContext context, ref List<AtomicAction> outActions)
        {
            var invokeEntities = context.Request.InvolvedEntities;
            var searchPool = context.World.GetPool<SearchDotComponent>();
            if (_addedEntities.Count > 0)
                _addedEntities.Clear();

            var starBombPool = context.World.GetPool<StarBombComponent>();
            var horizontalDotPool = context.World.GetPool<HorizontalDotComponent>();
            var bombDotPool = context.World.GetPool<TowDotsBombDotComponent>();
            var coloredBallPool = context.World.GetPool<TowDotsColoredBallComponent>();
            var normalPool = context.World.GetPool<NormalElementComponent>();

            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            var posPool = context.World.GetPool<ElementPositionComponent>();

            // 跟随着一起连线的棋子伤害
            _isHaveConnectNormal = false;
            foreach (var entity in invokeEntities)
            {
                if (normalPool.Has(entity))
                {
                    _isHaveConnectNormal = true;
                    ref var posCom = ref posPool.Get(entity);
                    outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage,
                        new Vector2Int(posCom.X, posCom.Y), entity));
                    _addedEntities.Add(entity);
                }
            }
            
            foreach (var entity in invokeEntities)
            {
                if (searchPool.Has(entity))
                {
                    RefSearchDotsActions(context, entity, ref outActions);
                }
                else if (starBombPool.Has(entity))
                {
                    RefStarBombActions(context, entity, ref outActions);
                }
                else if (horizontalDotPool.Has(entity))
                {
                    RefHorizontalDotActions(context, entity, ref outActions);
                }
                else if (bombDotPool.Has(entity))
                {
                    RefBombDotActions(context, entity, ref outActions);
                }
                else if (coloredBallPool.Has(entity))
                {
                    RefColoredBallActions(context, entity, ref outActions);
                }
                // else if() // 再处理其它的元素
            }

        }

        private void RefSearchDotsActions(MatchRuleContext context, int entity, ref List<AtomicAction> outActions)
        {
            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            var posPool = context.World.GetPool<ElementPositionComponent>();
            ref var searchPos = ref posPool.Get(entity);
            // 1.添加对搜寻点自身的伤害
            outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage, new Vector2Int(searchPos.X, searchPos.Y),
                entity));
            _addedEntities.Add(entity);
            // 2.添加延迟，等待搜寻点自身播放完发射动作
            outActions.Add(new AtomicAction
            {
                Type = MatchActionType.Delay,
                Value = 600
            });
            // 3.添加对搜寻点自身所搜到的点进行伤害
            var targetEntities = FindTargetEntities(context, entity);
            foreach (var targetEntity in targetEntities)
            {
                _addedEntities.Add(targetEntity);
            }
        }

        private void RefStarBombActions(MatchRuleContext context, int entity, ref List<AtomicAction> outActions)
        {
            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            var posPool = context.World.GetPool<ElementPositionComponent>();
            ref var starPos = ref posPool.Get(entity);

            // 1.添加对星爆点自身的伤害
            outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage, new Vector2Int(starPos.X, starPos.Y),
                entity));

            // 2.添加延迟，等待星爆点自身播放完爆炸动作
            outActions.Add(new AtomicAction
            {
                Type = MatchActionType.Delay,
                Value = 300
            });

            // 3.计算并添加对3x3范围+一行一列的伤害
            var targetEntities = FindStarBombTargets(context, entity);
            foreach (var targetEntity in targetEntities)
            {
                // 直接就在这里添加伤害
                ref var targetPos = ref posPool.Get(targetEntity);
                outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage,
                    new Vector2Int(targetPos.X, targetPos.Y), targetEntity));
            }
        }

        private void RefHorizontalDotActions(MatchRuleContext context, int entity, ref List<AtomicAction> outActions)
        {
            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            var posPool = context.World.GetPool<ElementPositionComponent>();
            var variablePool = context.World.GetPool<VariableColorComponent>();

            ref var pos = ref posPool.Get(entity);
            ref var variableColor = ref variablePool.Get(entity);
            
            // 1.添加对直线消除点自身的伤害
            outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage, new Vector2Int(pos.X, pos.Y), entity));

            // 2.添加延迟，等待直线消除点播放动画
            outActions.Add(new AtomicAction
            {
                Type = MatchActionType.Delay,
                Value = 200
            });

            // 3.计算并添加对一整行的伤害
            var targetEntities = FindHorizontalDotTargets(context, entity, variableColor.Y);

            foreach (var targetEntity in targetEntities)
            {
                ref var targetPos = ref posPool.Get(targetEntity);
                outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage,
                    new Vector2Int(targetPos.X, targetPos.Y), targetEntity));
            }
        }

        /// <summary>
        /// 在棋盘上查找目标棋子
        /// 优先级：SearchDotComponent > StarBombComponent > BlockElementComponent
        /// </summary>
        private List<int> FindTargetEntities(MatchRuleContext context, int searchEntity)
        {
            var result = new List<int>();
            var world = context.World;
            var board = context.Board;

            // 获取搜寻点的位置
            var posPool = world.GetPool<ElementPositionComponent>();
            ref var searchPos = ref posPool.Get(searchEntity);
            Vector2Int searchGridPos = new Vector2Int(searchPos.X, searchPos.Y);

            // 获取组件池
            var searchDotPool = world.GetPool<SearchDotComponent>();
            ref var searchCom = ref searchDotPool.Get(searchEntity);
            var starBombPool = world.GetPool<StarBombComponent>();
            var blockElementPool = world.GetPool<BlockElementComponent>();
            var elementPool = world.GetPool<ElementComponent>();
            var gridCellPool = world.GetPool<GridCellComponent>();

            // 按优先级收集候选目标
            var searchDotCandidates = new List<int>();
            var starBombCandidates = new List<int>();
            var blockElementCandidates = new List<int>();
            var normalElementCandidates = new List<int>();

            // 遍历棋盘
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    // 跳过搜寻点自身
                    if (x == searchGridPos.x && y == searchGridPos.y)
                        continue;

                    // 获取格子实体
                    var gridEntity = board[x, y];
                    ref var gridCell = ref gridCellPool.Get(gridEntity);

                    // 跳过空白格子
                    if (gridCell.IsBlank || gridCell.StackedEntityIds == null || gridCell.StackedEntityIds.Count == 0)
                        continue;

                    // 遍历格子上的所有实体
                    foreach (var entityId in gridCell.StackedEntityIds)
                    {
                        // 检查实体是否有效且处于空闲状态
                        if (!elementPool.Has(entityId))
                            continue;

                        ref var element = ref elementPool.Get(entityId);
                        if (element.LogicState != ElementLogicalState.Idle)
                            continue;
                        // 跳过已相连的棋子
                        if (_addedEntities.Contains(entityId))
                            continue;

                        // 按优先级分类
                        if (searchDotPool.Has(entityId))
                        {
                            searchDotCandidates.Add(entityId);
                        }
                        else if (starBombPool.Has(entityId))
                        {
                            starBombCandidates.Add(entityId);
                        }
                        else if (blockElementPool.Has(entityId))
                        {
                            blockElementCandidates.Add(entityId);
                        }
                        else
                        {
                            normalElementCandidates.Add(entityId);
                        }
                    }
                }
            }

            // 按优先级选择目标，直到达到目标数量
            AddTargetsFromList(searchDotCandidates, result);
            if (result.Count < SearchTargetCount)
            {
                AddTargetsFromList(starBombCandidates, result);
            }

            if (result.Count < SearchTargetCount)
            {
                AddTargetsFromList(blockElementCandidates, result);
            }

            if (result.Count < SearchTargetCount)
            {
                AddTargetsFromList(normalElementCandidates, result);
            }

            searchCom.SearchDotsEntities = result;
            return result;
        }

        /// <summary>
        /// 从候选列表中添加目标，按距离排序
        /// </summary>
        private void AddTargetsFromList(List<int> candidates, List<int> result)
        {
            if (candidates.Count == 0)
                return;
            
            candidates.Shuffle();

            // 添加到结果列表，直到达到目标数量
            int needCount = SearchTargetCount - result.Count;
            int addCount = Mathf.Min(needCount, candidates.Count);
            for (int i = 0; i < addCount; i++)
            {
                result.Add(candidates[i]);
            }
        }

        /// <summary>
        /// 查找星爆点的消除目标：3x3范围+一行一列
        /// </summary>
        private List<int> FindStarBombTargets(MatchRuleContext context, int starBombEntity)
        {
            var result = new List<int>();
            var world = context.World;
            var board = context.Board;

            // 获取星爆点的位置
            var posPool = world.GetPool<ElementPositionComponent>();
            ref var starPos = ref posPool.Get(starBombEntity);
            int centerX = starPos.X;
            int centerY = starPos.Y;

            // 获取组件池
            var starBombPool = world.GetPool<StarBombComponent>();
            ref var starBombCom = ref starBombPool.Get(starBombEntity);
            var elementPool = world.GetPool<ElementComponent>();
            var gridCellPool = world.GetPool<GridCellComponent>();

            HashSet<int> addedEntities = new HashSet<int>();
            addedEntities.Add(starBombEntity);

            // 1. 收集3x3范围内的目标
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // 跳过中心点（星爆点自身）
                    if (dx == 0 && dy == 0)
                        continue;

                    int x = centerX + dx;
                    int y = centerY + dy;

                    CollectTargetsAtPosition(x, y, board, gridCellPool, elementPool, posPool,
                        context.Request.InvolvedEntities, starBombEntity, addedEntities, result);
                }
            }

            // 2. 收集同一行的目标（排除3x3范围内已添加的）
            for (int x = 0; x < board.Width; x++)
            {
                // 跳过3x3范围内的格子
                if (x >= centerX - 1 && x <= centerX + 1)
                    continue;

                CollectTargetsAtPosition(x, centerY, board, gridCellPool, elementPool, posPool,
                    context.Request.InvolvedEntities, starBombEntity, addedEntities, result);
            }

            // 3. 收集同一列的目标（排除3x3范围内已添加的）
            for (int y = 0; y < board.Height; y++)
            {
                // 跳过3x3范围内的格子
                if (y >= centerY - 1 && y <= centerY + 1)
                    continue;

                CollectTargetsAtPosition(centerX, y, board, gridCellPool, elementPool, posPool,
                    context.Request.InvolvedEntities, starBombEntity, addedEntities, result);
            }

            starBombCom.TargetEntities = result;
            return result;
        }

        /// <summary>
        /// 在指定位置收集可消除的目标实体
        /// </summary>
        private void CollectTargetsAtPosition(int x, int y, IBoard board,
            EcsPool<GridCellComponent> gridCellPool,
            EcsPool<ElementComponent> elementPool,
            EcsPool<ElementPositionComponent> posPool,
            List<int> involvedEntities,
            int selfEntity,
            HashSet<int> addedEntities,
            List<int> result)
        {
            // 检查坐标是否在棋盘范围内
            if (x < 0 || x >= board.Width || y < 0 || y >= board.Height)
                return;

            // 获取格子实体
            var gridEntity = board[x, y];
            if (!gridCellPool.Has(gridEntity))
                return;

            ref var gridCell = ref gridCellPool.Get(gridEntity);

            // 跳过空白格子
            if (gridCell.IsBlank || gridCell.StackedEntityIds == null || gridCell.StackedEntityIds.Count == 0)
                return;

            // 遍历格子上的所有实体
            foreach (var entityId in gridCell.StackedEntityIds)
            {
                // 跳过自身
                if (entityId == selfEntity)
                    continue;

                // 跳过已添加的实体
                if (addedEntities.Contains(entityId))
                    continue;

                // 检查实体是否有效
                if (!elementPool.Has(entityId))
                    continue;

                ref var element = ref elementPool.Get(entityId);

                // 跳过非空闲状态的实体
                if (element.LogicState != ElementLogicalState.Idle)
                    continue;

                // 跳过已相连的棋子（在同一次连线中）
                ref var pos = ref posPool.Get(entityId);
                if (involvedEntities.Contains(entityId) && x == pos.X && y == pos.Y)
                    continue;

                // 添加到结果列表
                result.Add(entityId);
                addedEntities.Add(entityId);
            }
        }

        /// <summary>
        /// 查找直线消除点的消除目标：指定行的所有棋子
        /// </summary>
        private List<int> FindHorizontalDotTargets(MatchRuleContext context, int horizontalDotEntity, int effectRow)
        {
            var result = new List<int>();
            var world = context.World;
            var board = context.Board;

            // 获取组件池
            var horizontalDotPool = world.GetPool<HorizontalDotComponent>();
            ref var horizontalDot = ref horizontalDotPool.Get(horizontalDotEntity);
            var elementPool = world.GetPool<ElementComponent>();
            var gridCellPool = world.GetPool<GridCellComponent>();
            var posPool = world.GetPool<ElementPositionComponent>();

            HashSet<int> addedEntities = new HashSet<int>();

            // 遍历指定行的所有格子
            for (int x = 0; x < board.Width; x++)
            {
                CollectTargetsAtPosition(x, effectRow, board, gridCellPool, elementPool, posPool,
                    context.Request.InvolvedEntities, horizontalDotEntity, addedEntities, result);
            }

            horizontalDot.TargetEntities = result;
            return result;
        }

        /// <summary>
        /// 处理爆炸点的消除动作
        /// </summary>
        private void RefBombDotActions(MatchRuleContext context, int entity, ref List<AtomicAction> outActions)
        {
            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            var posPool = context.World.GetPool<ElementPositionComponent>();
            var variablePool = context.World.GetPool<VariableColorComponent>();

            ref var pos = ref posPool.Get(entity);
            ref var variableColor = ref variablePool.Get(entity);

            // 1.添加对爆炸点自身的伤害
            outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage, new Vector2Int(pos.X, pos.Y), entity));

            // 2.添加延迟，等待爆炸点播放动画
            outActions.Add(new AtomicAction
            {
                Type = MatchActionType.Delay,
                Value = 200
            });

            // 3.计算并添加对3x3范围的伤害
            // 使用VariableColorComponent中的X和Y作为爆炸中心
            var targetEntities = FindBombDotTargets(context, entity, variableColor.X, variableColor.Y);

            foreach (var targetEntity in targetEntities)
            {
                ref var targetPos = ref posPool.Get(targetEntity);
                outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage,
                    new Vector2Int(targetPos.X, targetPos.Y), targetEntity));
            }
        }

        private void RefColoredBallActions(MatchRuleContext ctx, int dotEntity, ref List<AtomicAction> outActions)
        {
            var posPool = ctx.World.GetPool<ElementPositionComponent>();
            var variablePool = ctx.World.GetPool<VariableColorComponent>();
            var colorBallPool = ctx.World.GetPool<TowDotsColoredBallComponent>();
            ref var pos = ref posPool.Get(dotEntity);
            ref var variableColor = ref variablePool.Get(dotEntity);
            if (variableColor.CurrentColorId <= 0 && _isHaveConnectNormal == false)
            {
                // 炫彩冲击的白点没有连到普通棋子，随机选棋盘上最多的普通棋子
                variableColor.CurrentColorId = FindMaxNormalElementConfigId(ctx.World);
            }
            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();

            // 自身的伤害
            outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage, new Vector2Int(pos.X, pos.Y), dotEntity));
            ref var colorBallCom = ref colorBallPool.Get(dotEntity);
            colorBallCom.CollectedEntities ??= new HashSet<int>();
            colorBallCom.CollectedEntities.Clear();

            // 1.遍历所有连线的棋子 包括同色的棋子
            ctx.BanDropElementId = variableColor.CurrentColorId;
            EcsFilter normalElementFilter = ctx.World.Filter<NormalElementComponent>().Include<ElementComponent>()
                .Include<ElementRenderComponent>().Include<ElementPositionComponent>().End();
            foreach (var entity in normalElementFilter)
            {
                ref var ele = ref ctx.World.GetPool<ElementComponent>().Get(entity);
                if (ele.LogicState == ElementLogicalState.Idle &&
                    ele.ConfigId == variableColor.CurrentColorId)
                {
                    colorBallCom.CollectedEntities.Add(entity);
                }
            }

            // 同色的星爆点和搜寻点
            EcsFilter searchDotFilter = ctx.World.Filter<SearchDotComponent>().End();
            foreach (var entity in searchDotFilter)
            {
                ref var ele = ref ctx.World.GetPool<ElementComponent>().Get(entity);
                ref var dotCom = ref ctx.World.GetPool<SearchDotComponent>().Get(entity);
                if (ele.LogicState == ElementLogicalState.Idle &&
                    dotCom.SearchDotBaseElementId == variableColor.CurrentColorId)
                {
                    colorBallCom.CollectedEntities.Add(entity);
                }
            }

            EcsFilter starBombDotFilter = ctx.World.Filter<StarBombComponent>().End();
            foreach (var entity in starBombDotFilter)
            {
                ref var ele = ref ctx.World.GetPool<ElementComponent>().Get(entity);
                ref var dotCom = ref ctx.World.GetPool<StarBombComponent>().Get(entity);
                if (ele.LogicState == ElementLogicalState.Idle &&
                    dotCom.StarDotBaseElementId == variableColor.CurrentColorId)
                {
                    colorBallCom.CollectedEntities.Add(entity);
                }
            }
        }

        private int FindMaxNormalElementConfigId(EcsWorld world)
        {
            var maxConfigId = 0;
            Dictionary<int,HashSet<int>> elementConfigIdDict = new Dictionary<int, HashSet<int>>();
            EcsFilter normalElementFilter = world.Filter<NormalElementComponent>().End();
            foreach (var entity in normalElementFilter)
            {
                ref var ele = ref world.GetPool<ElementComponent>().Get(entity);
                // 先不考虑被覆盖的情况了
                if(!elementConfigIdDict.ContainsKey(ele.ConfigId))
                    elementConfigIdDict.Add(ele.ConfigId, new HashSet<int>());
                elementConfigIdDict[ele.ConfigId].Add(entity);
            }
            foreach (var kv in elementConfigIdDict)
            {
                if (kv.Value.Count > maxConfigId)
                    maxConfigId = kv.Key;
            }
            return maxConfigId;
        }

        /// <summary>
        /// 查找爆炸点的消除目标：以指定位置为中心的3x3范围
        /// </summary>
        private List<int> FindBombDotTargets(MatchRuleContext context, int bombDotEntity, int centerX, int centerY)
        {
            var result = new List<int>();
            var world = context.World;
            var board = context.Board;

            // 获取组件池
            var bombDotPool = world.GetPool<TowDotsBombDotComponent>();
            ref var bombDot = ref bombDotPool.Get(bombDotEntity);
            var elementPool = world.GetPool<ElementComponent>();
            var gridCellPool = world.GetPool<GridCellComponent>();
            var posPool = world.GetPool<ElementPositionComponent>();

            HashSet<int> addedEntities = new HashSet<int>();
            addedEntities.Add(bombDotEntity);

            // 遍历3x3范围
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // 跳过中心点（爆炸点自身）
                    if (dx == 0 && dy == 0)
                        continue;

                    int x = centerX + dx;
                    int y = centerY + dy;

                    CollectTargetsAtPosition(x, y, board, gridCellPool, elementPool, posPool,
                        context.Request.InvolvedEntities, bombDotEntity, addedEntities, result);
                }
            }

            bombDot.TargetEntities = result;
            return result;
        }
    }
}