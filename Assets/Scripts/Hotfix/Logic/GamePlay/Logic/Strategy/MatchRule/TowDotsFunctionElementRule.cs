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

        public void Evaluate(MatchRuleContext context, ref List<AtomicAction> outActions)
        {
            var invokeEntities = context.Request.InvolvedEntities;
            var searchPool = context.World.GetPool<SearchDotComponent>();
            if (_addedEntities.Count > 0)
                _addedEntities.Clear();

            foreach (var entity in invokeEntities)
            {
                if (searchPool.Has(entity))
                {
                    RefSearchDotsActions(context, entity, ref outActions);
                }
                // else if() // 再处理其它的元素
                MatchElementUtil.AddSingleScore(context.World, entity);
            }

            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            var posPool = context.World.GetPool<ElementPositionComponent>();

            // 其它跟随着一起连线的棋子最后再添加伤害
            foreach (var entity in invokeEntities)
            {
                if (!_addedEntities.Contains(entity))
                {
                    ref var posCom = ref posPool.Get(entity);
                    outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage,
                        new Vector2Int(posCom.X, posCom.Y), entity));
                }
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
                ref var targetPos = ref posPool.Get(targetEntity);
                outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage,
                    new Vector2Int(targetPos.X, targetPos.Y), targetEntity));
                _addedEntities.Add(targetEntity);
                MatchElementUtil.AddSingleScore(context.World, entity);
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
                        ref var pos = ref posPool.Get(entityId);
                        if (context.Request.InvolvedEntities.Contains(entityId) && x == pos.Y && y == pos.Y)
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
            AddTargetsFromList(searchDotCandidates, result, searchGridPos, posPool);
            if (result.Count < SearchTargetCount)
            {
                AddTargetsFromList(starBombCandidates, result, searchGridPos, posPool);
            }

            if (result.Count < SearchTargetCount)
            {
                AddTargetsFromList(blockElementCandidates, result, searchGridPos, posPool);
            }

            if (result.Count < SearchTargetCount)
            {
                AddTargetsFromList(normalElementCandidates, result, searchGridPos, posPool);
            }

            searchCom.SearchDotsEntities = result;
            return result;
        }

        /// <summary>
        /// 从候选列表中添加目标，按距离排序
        /// </summary>
        private void AddTargetsFromList(List<int> candidates, List<int> result, Vector2Int searchPos,
            EcsPool<ElementPositionComponent> posPool)
        {
            if (candidates.Count == 0)
                return;

            // 按距离排序（从近到远）
            candidates.Shuffle();

            // 添加到结果列表，直到达到目标数量
            int needCount = SearchTargetCount - result.Count;
            int addCount = Mathf.Min(needCount, candidates.Count);
            for (int i = 0; i < addCount; i++)
            {
                result.Add(candidates[i]);
            }
        }
    }
}