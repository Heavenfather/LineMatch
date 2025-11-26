using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class ColorBallAndNormalRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.ColorBallAndNormal;
        
        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
            
        }
    }
}