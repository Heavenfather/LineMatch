using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class TowDotsColorBallLineNormalRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.TowDotsColorBallLineNormal;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
        }
    }
}