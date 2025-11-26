using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class RocketAndColorBallRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.RocketAndColorBall;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
        }
    }
}