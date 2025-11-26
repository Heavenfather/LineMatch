using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class BombAndColorBallRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.BombAndColorBall;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
        }
    }
}