using System.Collections.Generic;
using GameConfig;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public class BombRule : IMatchRule
    {
        public MatchRequestType RuleKey => MatchRequestType.Bomb;

        public void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions)
        {
            // 注意，此时炸弹是已经销毁了，它已经进入了销毁列表中
            // 炸弹处于播放爆炸特效状态中
            
            // 1. 延迟执行对其它格子的伤害命令
            outActions.Add(new AtomicAction 
            { 
                Type = MatchActionType.Delay, 
                Value = 200
            });
            
            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            var request = ctx.Request;
            Vector2Int bombCoord = (Vector2Int)request.ExtraData;
            List<Vector2Int> bombList = new List<Vector2Int>(ctx.MatchService.GetBombPos(new Vector2Int(bombCoord.x, bombCoord.y)));
            bombList.Add(new Vector2Int(bombCoord.x, bombCoord.y));
            foreach (var pos in bombList)
            {
                outActions.Add(factory.CreateAtomicAction(MatchActionType.Damage,
                    new Vector2Int(pos.x, pos.y), 1));
            }
            
            // 2. 生成加分指令 
            SpecialPieceScoreDB db = ConfigMemoryPool.Get<SpecialPieceScoreDB>();
            int score =  db.CalScore(SpecialElementType.Bomb, ctx.MatchService.ElementType2ConfigId(ElementType.Bomb));
            outActions.Add(factory.CreateAtomicAction(MatchActionType.AddScore, value: score));
        }
    }
}