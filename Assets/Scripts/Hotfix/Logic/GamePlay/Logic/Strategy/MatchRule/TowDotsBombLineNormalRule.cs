using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class TowDotsBombLineNormalRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.TowDotsBombLineNormal;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
        }
    }
}