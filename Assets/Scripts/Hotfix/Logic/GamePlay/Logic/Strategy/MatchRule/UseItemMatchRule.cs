using System.Collections.Generic;
using GameConfig;
using Hotfix.Define;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 道具使用匹配规则
    /// 处理所有道具的消除逻辑
    /// </summary>
    public class UseItemMatchRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.UseItem;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            int itemId = ctx.Request.ItemId;
            Vector2Int coord = ctx.Request.TargetCoord;

            Logger.Debug($"[UseItemRule] 执行道具效果: ItemId={itemId}, Coord={coord}");

            switch ((ItemDef)itemId)
            {
                case ItemDef.EliminateDice:
                    // 骰子：洗牌
                    ExecuteDice(ref outActions, factory);
                    break;

                case ItemDef.EliminateHammer:
                    // 锤子：消除单个
                    ExecuteHammer(ctx, ref outActions, factory, coord);
                    break;

                case ItemDef.EliminateBullet:
                    // 弓箭：消除同列
                    ExecuteArrow(ctx, ref outActions, factory, coord);
                    break;

                case ItemDef.EliminateArrow:
                    // 子弹：消除同行
                    ExecuteBullet(ctx, ref outActions, factory, coord);
                    break;

                case ItemDef.EliminateColored:
                    // 炫彩冲击：消除同色
                    ExecuteColored(ctx, ref outActions, factory, coord);
                    break;

                case ItemDef.EliminateCross:
                    // 精准射击：消除十字
                    ExecuteCross(ctx, ref outActions, factory, coord);
                    break;
            }
        }

        /// <summary>
        /// 执行骰子效果：洗牌
        /// </summary>
        private void ExecuteDice(ref List<AtomicAction> outActions,
            IMatchServiceFactory factory)
        {
            Logger.Debug("[UseItemRule] 执行骰子洗牌");
            // 生成洗牌指令
            var shuffleAction = factory.CreateAtomicAction(MatchActionType.Shuffle);
            outActions.Add(shuffleAction);
        }

        /// <summary>
        /// 执行锤子效果：消除单个元素
        /// </summary>
        private void ExecuteHammer(MatchRuleContext ctx, ref List<AtomicAction> outActions,
            IMatchServiceFactory factory, Vector2Int coord)
        {
            Logger.Debug($"[UseItemRule] 执行锤子消除: {coord}");

            int gridEntity = ctx.Board[coord.x, coord.y];
            if (gridEntity < 0) return;

            var gridPool = ctx.World.GetPool<GridCellComponent>();
            ref var grid = ref gridPool.Get(gridEntity);

            if (grid.StackedEntityIds == null || grid.StackedEntityIds.Count == 0) return;

            // 找到可消除的元素
            var elePool = ctx.World.GetPool<ElementComponent>();
            for (int i = grid.StackedEntityIds.Count - 1; i >= 0; i--)
            {
                int eleEntity = grid.StackedEntityIds[i];
                ref var ele = ref elePool.Get(eleEntity);
                // 找到可消除的元素
                if (ele.HoldGrid >= 1)
                {
                    var damageAction = factory.CreateAtomicAction(MatchActionType.Damage, coord, 1, eleEntity);
                    outActions.Add(damageAction);
                    break;
                }
            }
        }

        /// <summary>
        /// 执行子弹效果：消除同行
        /// </summary>
        private void ExecuteBullet(MatchRuleContext ctx, ref List<AtomicAction> outActions,
            IMatchServiceFactory factory, Vector2Int coord)
        {
            Logger.Debug($"[UseItemRule] 执行子弹消除同行: {coord}");

            var elePool = ctx.World.GetPool<ElementComponent>();
            var posPool = ctx.World.GetPool<ElementPositionComponent>();
            var gridPool = ctx.World.GetPool<GridCellComponent>();

            // 遍历同行的所有格子
            for (int x = 0; x < ctx.Board.Width; x++)
            {
                int gridEntity = ctx.Board[x, coord.y];
                if (gridEntity < 0) continue;

                ref var grid = ref gridPool.Get(gridEntity);
                if (grid.IsBlank) continue;

                // 消除该格子上的可消除元素
                if (grid.StackedEntityIds != null)
                {
                    if (!MatchElementUtil.IsCanDamageThisGrid(ctx.World, grid.StackedEntityIds))
                        continue;
                    foreach (var eleEntity in grid.StackedEntityIds)
                    {
                        ref var ele = ref elePool.Get(eleEntity);
                        if (ele.LogicState == ElementLogicalState.Idle && ele.HoldGrid >= 1)
                        {
                            ref var pos = ref posPool.Get(eleEntity);
                            var damageAction = factory.CreateAtomicAction(MatchActionType.Damage,
                                new Vector2Int(pos.X, pos.Y), 1, eleEntity);
                            outActions.Add(damageAction);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 执行弓箭效果：消除同列
        /// </summary>
        private void ExecuteArrow(MatchRuleContext ctx, ref List<AtomicAction> outActions,
            IMatchServiceFactory factory, Vector2Int coord)
        {
            Logger.Debug($"[UseItemRule] 执行弓箭消除同列: {coord}");

            var elePool = ctx.World.GetPool<ElementComponent>();
            var posPool = ctx.World.GetPool<ElementPositionComponent>();
            var gridPool = ctx.World.GetPool<GridCellComponent>();

            // 遍历同列的所有格子
            for (int y = 0; y < ctx.Board.Height; y++)
            {
                int gridEntity = ctx.Board[coord.x, y];
                if (gridEntity < 0) continue;

                ref var grid = ref gridPool.Get(gridEntity);
                if (grid.IsBlank) continue;

                // 消除该格子上的可消除元素
                if (grid.StackedEntityIds != null)
                {
                    if (!MatchElementUtil.IsCanDamageThisGrid(ctx.World, grid.StackedEntityIds))
                        continue;
                    foreach (var eleEntity in grid.StackedEntityIds)
                    {
                        ref var ele = ref elePool.Get(eleEntity);
                        if (ele.LogicState == ElementLogicalState.Idle && ele.HoldGrid >= 1)
                        {
                            ref var pos = ref posPool.Get(eleEntity);
                            var damageAction = factory.CreateAtomicAction(MatchActionType.Damage,
                                new Vector2Int(pos.X, pos.Y), 1, eleEntity);
                            outActions.Add(damageAction);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 执行炫彩冲击效果：消除同色
        /// </summary>
        private void ExecuteColored(MatchRuleContext ctx, ref List<AtomicAction> outActions,
            IMatchServiceFactory factory, Vector2Int coord)
        {
            Logger.Debug($"[UseItemRule] 执行炫彩冲击消除同色: {coord}");

            int gridEntity = ctx.Board[coord.x, coord.y];
            if (gridEntity < 0) return;

            var gridPool = ctx.World.GetPool<GridCellComponent>();
            ref var grid = ref gridPool.Get(gridEntity);

            // 找到目标位置的普通元素
            int targetConfigId = -1;
            var elePool = ctx.World.GetPool<ElementComponent>();

            if (grid.StackedEntityIds != null)
            {
                foreach (var eleEntity in grid.StackedEntityIds)
                {
                    ref var ele = ref elePool.Get(eleEntity);
                    if (ele.Type == ElementType.Normal)
                    {
                        targetConfigId = ele.ConfigId;
                        break;
                    }
                }
            }

            if (targetConfigId < 0)
            {
                Logger.Warning("[UseItemRule] 未找到目标普通元素");
                return;
            }

            ctx.BanDropElementId = targetConfigId;
            // 遍历所有普通元素，消除同色的
            var normalFilter = ctx.World.Filter<NormalElementComponent>()
                .Include<ElementComponent>()
                .Include<ElementPositionComponent>()
                .End();

            var posPool = ctx.World.GetPool<ElementPositionComponent>();

            foreach (var entity in normalFilter)
            {
                ref var ele = ref elePool.Get(entity);
                if (ele.ConfigId == targetConfigId && ele.LogicState == ElementLogicalState.Idle)
                {
                    ref var pos = ref posPool.Get(entity);
                    ref var gridCom = ref gridPool.Get(ctx.Board[pos.X, pos.Y]);
                    if(!MatchElementUtil.IsCanDamageThisGrid(ctx.World, gridCom.StackedEntityIds))
                        continue;
                    var damageAction = factory.CreateAtomicAction(MatchActionType.Damage,
                        new Vector2Int(pos.X, pos.Y), 1, entity);
                    outActions.Add(damageAction);
                }
            }
        }

        /// <summary>
        /// 执行精准射击效果：消除十字（同行+同列）
        /// </summary>
        private void ExecuteCross(MatchRuleContext ctx, ref List<AtomicAction> outActions,
            IMatchServiceFactory factory, Vector2Int coord)
        {
            Logger.Debug($"[UseItemRule] 执行精准射击消除十字: {coord}");

            var elePool = ctx.World.GetPool<ElementComponent>();
            var posPool = ctx.World.GetPool<ElementPositionComponent>();
            var gridPool = ctx.World.GetPool<GridCellComponent>();

            HashSet<int> processedEntities = new HashSet<int>();

            // 消除同行
            for (int x = 0; x < ctx.Board.Width; x++)
            {
                int gridEntity = ctx.Board[x, coord.y];
                if (gridEntity < 0) continue;

                ref var grid = ref gridPool.Get(gridEntity);
                if (grid.IsBlank) continue;

                if (grid.StackedEntityIds != null)
                {
                    if(!MatchElementUtil.IsCanDamageThisGrid(ctx.World, grid.StackedEntityIds))
                        continue;
                    foreach (var eleEntity in grid.StackedEntityIds)
                    {
                        if (processedEntities.Contains(eleEntity)) continue;

                        ref var ele = ref elePool.Get(eleEntity);
                        if (ele.LogicState == ElementLogicalState.Idle && ele.HoldGrid >= 1)
                        {
                            ref var pos = ref posPool.Get(eleEntity);
                            var damageAction = factory.CreateAtomicAction(MatchActionType.Damage,
                                new Vector2Int(pos.X, pos.Y), 1, eleEntity);
                            outActions.Add(damageAction);
                            processedEntities.Add(eleEntity);
                        }
                    }
                }
            }

            // 消除同列
            for (int y = 0; y < ctx.Board.Height; y++)
            {
                int gridEntity = ctx.Board[coord.x, y];
                if (gridEntity < 0) continue;

                ref var grid = ref gridPool.Get(gridEntity);
                if (grid.IsBlank) continue;

                if (grid.StackedEntityIds != null)
                {
                    if(!MatchElementUtil.IsCanDamageThisGrid(ctx.World, grid.StackedEntityIds))
                        continue;
                    foreach (var eleEntity in grid.StackedEntityIds)
                    {
                        if (processedEntities.Contains(eleEntity)) continue;

                        ref var ele = ref elePool.Get(eleEntity);
                        if (ele.LogicState == ElementLogicalState.Idle && ele.HoldGrid >= 1)
                        {
                            ref var pos = ref posPool.Get(eleEntity);
                            var damageAction = factory.CreateAtomicAction(MatchActionType.Damage,
                                new Vector2Int(pos.X, pos.Y), 1, eleEntity);
                            outActions.Add(damageAction);
                            processedEntities.Add(eleEntity);
                        }
                    }
                }
            }
        }
    }
}