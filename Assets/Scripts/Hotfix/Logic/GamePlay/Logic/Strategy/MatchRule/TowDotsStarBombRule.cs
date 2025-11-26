using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class TowDotsStarBombRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.TowDotsStarBomb;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
        }
    }
}