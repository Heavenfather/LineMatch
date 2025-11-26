using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 掉落后的自动检测
    /// </summary>
    public class PostDropCheckRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.PostDropCheck;
        
        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
            
        }
    }
}