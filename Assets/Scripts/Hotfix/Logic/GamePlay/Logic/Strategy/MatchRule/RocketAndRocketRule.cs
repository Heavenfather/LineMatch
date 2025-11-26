using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class RocketAndRocketRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.RocketAndRocket;
        
        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
            
        }
    }
}