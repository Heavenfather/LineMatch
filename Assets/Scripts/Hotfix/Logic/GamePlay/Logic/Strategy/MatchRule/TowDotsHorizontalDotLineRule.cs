using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class TowDotsHorizontalDotLineRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.TowDotsHorizontalDotLine;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
        }
    }
}