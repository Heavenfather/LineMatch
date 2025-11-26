using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class BombRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.Bomb;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
        }
    }
}