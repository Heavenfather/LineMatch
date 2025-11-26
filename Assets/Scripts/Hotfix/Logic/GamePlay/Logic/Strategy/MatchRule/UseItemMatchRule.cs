using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 使用道具指令
    /// </summary>
    public class UseItemMatchRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.UseItem;
        
        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
            // 根据道具ID进行不同的检测,生成不同的指令列表
        }
    }
}