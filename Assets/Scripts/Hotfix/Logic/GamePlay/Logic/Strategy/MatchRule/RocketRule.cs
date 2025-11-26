using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class RocketRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.Rocket;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
        }
    }
}