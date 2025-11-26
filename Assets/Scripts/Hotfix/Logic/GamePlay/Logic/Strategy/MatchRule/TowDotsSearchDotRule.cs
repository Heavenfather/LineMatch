using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class TowDotsSearchDotRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.TowDotsSearchDot;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
        }
    }
}