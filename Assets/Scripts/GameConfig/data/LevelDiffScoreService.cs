using UnityEngine;

namespace GameConfig
{
    public partial class LevelDiffScoreDB
    {
        /// <summary>
        /// 基础分100
        /// </summary>
        private const int BaseScore = 300; 

        /// <summary>
        /// 计算剩余步数得分
        /// </summary>
        /// <returns></returns>
        public int CalScore(MatchDifficulty difficulty, int step)
        {
            return Mathf.CeilToInt(step * BaseScore * GetCoefficient(difficulty));
        }

        /// <summary>
        /// 获取关卡分数计算难度系数
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public float GetCoefficient(MatchDifficulty difficulty)
        {
            if (difficulty == MatchDifficulty.Normal)
                return this[1].easeParam;

            if (difficulty == MatchDifficulty.Hard)
                return this[1].hardParam;

            if (difficulty == MatchDifficulty.Crazy)
                return this[1].extremeParam;

            return 0;
        }
    }
}