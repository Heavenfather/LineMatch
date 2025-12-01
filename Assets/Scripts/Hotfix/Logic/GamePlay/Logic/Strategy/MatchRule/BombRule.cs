using System.Collections.Generic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public class BombRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.Bomb;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            var request = ctx.Request;
            Vector2Int bombCoord = (Vector2Int)request.ExtraData;
            List<Vector2Int> bombList = new List<Vector2Int>(ctx.MatchService.GetBombPos(new Vector2Int(bombCoord.x, bombCoord.x)));
            bombList.Add(new Vector2Int(bombCoord.x, bombCoord.x));
            foreach (var pos in bombList)
            {
                outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage,
                    new Vector2Int(pos.x, pos.y), 1));
            }
        }
    }
}