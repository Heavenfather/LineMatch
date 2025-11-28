using System.Collections.Generic;
using GameConfig;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public class NormalMatchRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.PlayerLine;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
            var rule = PickCombMatchRule(ctx);
            if (rule != null)
            {
                rule.Evaluate(ctx, ref outActions);
                return;
            }

            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            // 遍历所有连线的棋子
            foreach (var entity in ctx.Request.InvolvedEntities)
            {
                // 1. 生成扣次数指令
                ref var positionPos = ref ctx.World.GetPool<ElementPositionComponent>().Get(entity);
                outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage,
                    new Vector2Int(positionPos.X, positionPos.Y), 1,
                    entity));
            }

            // 2. 生成加分指令 
            BlockDiffScoreDB db = ConfigMemoryPool.Get<BlockDiffScoreDB>();
            outActions.Add(factory.CreateAtomicAction(MatchActionType.AddScore,
                value: db.CalScoreNotRect(ctx.Request.ConfigId, ctx.Request.InvolvedEntities.Count)));
        }

        /// <summary>
        /// 选择最终的匹配规则
        /// 因为这里相连可能是功能棋子相连
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private IMatchRule PickCombMatchRule(MatchRuleContext ctx)
        {
            IMatchRule rule = ctx.MatchService.GetMatchRule(ctx.World, ctx.Request.InvolvedEntities);
            if (rule != null)
            {
                return rule;
            }

            return null;
        }
    }
}