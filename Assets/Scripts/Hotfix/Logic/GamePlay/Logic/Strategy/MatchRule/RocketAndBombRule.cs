using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class RocketAndBombRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.RocketAndBomb;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
        }
    }
}