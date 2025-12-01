using System.Collections.Generic;
using GameConfig;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic.GamePlay
{
    public class SquareMatchRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.PlayerSquare;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            // 1.遍历所有连线的棋子 包括同色的棋子
            ctx.BanDropElementId = ctx.Request.ConfigId;
            EcsFilter normalElementFilter = ctx.World.Filter<NormalElementComponent>().Include<ElementComponent>()
                .Include<ElementRenderComponent>().Include<ElementPositionComponent>().End();
            foreach (var entity in normalElementFilter)
            {
                ref var ele = ref ctx.World.GetPool<ElementComponent>().Get(entity);
                ref var posCom = ref ctx.World.GetPool<ElementPositionComponent>().Get(entity);
                if (ele.LogicState == ElementLogicalState.Idle && 
                    ele.ConfigId == ctx.Request.ConfigId)
                {
                    // 生成扣次数指令
                    outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage, new Vector2Int(posCom.X, posCom.Y), 1, entity));
                }
            }

            // Logger.Debug($"总共生成伤害指令:{outActions.Count}");

            var closedPaths = GetFinalClosedLoopItems(ctx.World, ctx.Request.InvolvedEntities);
            // 2. 生成加分指令 
            if (closedPaths == null || closedPaths.Count < 4)
            {
                BlockDiffScoreDB db = ConfigMemoryPool.Get<BlockDiffScoreDB>();
                outActions.Add(factory.CreateAtomicAction(MatchActionType.AddScore,
                    value: db.CalScoreNotRect(ctx.Request.ConfigId, ctx.Request.InvolvedEntities.Count)));

                return;
            }

            // 3.通过闭环的周长匹配生成规则
            var actions = ctx.MatchService.MatchRuleAction(ctx, closedPaths);
            if (actions != null)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    outActions.Add(actions[i]);
                }
            }
        }

        /// <summary>
        /// 获取最终形成闭环的格子位置
        /// </summary>
        /// <returns></returns>
        private List<Vector2Int> GetFinalClosedLoopItems(EcsWorld world, List<int> squareEntities)
        {
            List<Vector2Int> allLineEntitiesCoord = new List<Vector2Int>(squareEntities.Count);
            var positionComponent = world.GetPool<ElementPositionComponent>();
            for (int i = 0; i < squareEntities.Count; i++)
            {
                int entity = squareEntities[i];
                ref var posComp = ref positionComponent.Get(entity);
                allLineEntitiesCoord.Add(new Vector2Int(posComp.X, posComp.Y));
            }

            var lastCoord = allLineEntitiesCoord[^1];
            // 排除最后两个格子是因为最后两个格子总是相邻的（连线路径）
            for (int i = 0; i < allLineEntitiesCoord.Count - 2; i++)
            {
                var coord = allLineEntitiesCoord[i];
                // 检查当前格子是否与最后一个格子是邻居
                if (MatchPosUtil.IsNeighbor(coord.x, coord.y, lastCoord.x, lastCoord.y))
                {
                    // 找到闭环，生成闭环格子数组
                    List<Vector2Int> result = new List<Vector2Int>();
                    // 添加从回连点到最后一个格子的所有格子
                    for (int j = i; j < allLineEntitiesCoord.Count; j++)
                    {
                        result.Add(allLineEntitiesCoord[j]);
                    }

                    return result;
                }
            }

            return null;
        }
    }
}