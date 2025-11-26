using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class BombAndBombRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.BombAndBomb;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
        }
    }
}