/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 排行榜奖励.xlsx
*/

namespace GameConfig
{
    
    public readonly struct StreakRankReward
    {
        
        /// <summary>
        /// 唯一id
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 排行开始
        /// </summary>
        public int rankBegin { get; }
        
        /// <summary>
        /// 排名结束
        /// </summary>
        public string rankEnd { get; }
        
        /// <summary>
        /// 关卡奖励
        /// </summary>
        public string reward { get; }
        
        internal StreakRankReward(int id, int rankBegin, string rankEnd, string reward)
        {
            this.id = id;
            this.rankBegin = rankBegin;
            this.rankEnd = rankEnd;
            this.reward = reward;
        }
    }
}