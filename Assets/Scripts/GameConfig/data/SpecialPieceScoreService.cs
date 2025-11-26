using UnityEngine;

namespace GameConfig
{
    public partial class SpecialPieceScoreDB
    {
        
        /// <summary>
        /// 获取功能棋子消除得分
        /// </summary>
        /// <returns></returns>
        public int CalScore(SpecialElementType type, int firstId, int secondId = 0)
        {
            var elementDB = ConfigMemoryPool.Get<ElementMapDB>();
            if ((int)type <= (int)SpecialElementType.ColorBall)
            {
                int baseScore = elementDB[firstId].score;
                return Mathf.CeilToInt(baseScore * GetCoefficient(type));
            }

            int firstScore = elementDB[firstId].score;
            int secondScore = elementDB[secondId].score;
            float coefficient = GetCoefficient(type);
            return Mathf.CeilToInt((firstScore + secondScore) * coefficient);
        }
        
        /// <summary>
        /// 获取功能棋子和组合分数难度系数
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public float GetCoefficient(SpecialElementType type)
        {
            if (type == SpecialElementType.Rocket)
                return this[1].rockPiece;
            if (type == SpecialElementType.Bomb)
                return this[1].bombPiece;
            if (type == SpecialElementType.ColorBall)
                return this[1].colorballPiece;
            if (type == SpecialElementType.RandB)
                return this[1].randBPiece;
            if (type == SpecialElementType.CandRB)
                return this[1].candrbPiece;
            if (type == SpecialElementType.RandC)
                return this[1].candCPiece;
            return 0.0f;
        }
    }
}