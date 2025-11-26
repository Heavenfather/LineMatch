using System.Collections.Generic;
using UnityEngine;

namespace GameConfig
{
    public partial class BlockDiffScoreDB
    {
        /// <summary>
        /// 根据消除个数获取得分
        /// </summary>
        /// <returns></returns>
        public int CalScore(int matchId,int matchCount, OneTakeScoreType type)
        {
            var elementDB = ConfigMemoryPool.Get<ElementMapDB>();
            int baseScore = elementDB[matchId].score;
            int score = Mathf.CeilToInt(matchCount * baseScore * GetCoefficient(type));
            return score;
        }

        /// <summary>
        /// 非方格得分
        /// </summary>
        /// <returns></returns>
        public int CalScoreNotRect(int matchId, int matchCount)
        {
            var elementDB = ConfigMemoryPool.Get<ElementMapDB>();
            int baseScore = elementDB[matchId].score;
            float coefficient = 0;
            if (matchCount <= 4)
                coefficient = GetCoefficient(OneTakeScoreType.FourNotRect);
            else if (matchCount is >= 5 and <= 8)
                coefficient = GetCoefficient(OneTakeScoreType.FiveEightBetween);
            else
                coefficient = GetCoefficient(OneTakeScoreType.GreaterEight);
            return Mathf.CeilToInt(matchCount * baseScore * coefficient);
        }
        
        /// <summary>
        /// 获取方块消除分数系数
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public float GetCoefficient(OneTakeScoreType type)
        {
            int toId = (int)type;
            return this[toId].BlockLevelScore;
        }
    }
}