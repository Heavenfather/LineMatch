/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 签到奖励.xlsx
*/

namespace GameConfig
{
    
    public readonly struct dailyReward
    {
        
        /// <summary>
        /// 签到期数id
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 第1天奖励
        /// </summary>
        public string day1 { get; }
        
        /// <summary>
        /// 第2天奖励
        /// </summary>
        public string day2 { get; }
        
        /// <summary>
        /// 第3天奖励
        /// </summary>
        public string day3 { get; }
        
        /// <summary>
        /// 第4天奖励
        /// </summary>
        public string day4 { get; }
        
        /// <summary>
        /// 第5天奖励
        /// </summary>
        public string day5 { get; }
        
        /// <summary>
        /// 第6天奖励
        /// </summary>
        public string day6 { get; }
        
        /// <summary>
        /// 第7天奖励
        /// </summary>
        public string day7 { get; }
        
        internal dailyReward(int id, string day1, string day2, string day3, string day4, string day5, string day6, string day7)
        {
            this.id = id;
            this.day1 = day1;
            this.day2 = day2;
            this.day3 = day3;
            this.day4 = day4;
            this.day5 = day5;
            this.day6 = day6;
            this.day7 = day7;
        }
    }
}