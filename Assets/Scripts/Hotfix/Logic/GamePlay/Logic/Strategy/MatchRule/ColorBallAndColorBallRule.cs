using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class ColorBallAndColorBallRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.ColorBallAndColorBall;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
        }
    }
}