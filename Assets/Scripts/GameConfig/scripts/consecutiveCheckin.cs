/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 签到奖励.xlsx
*/

namespace GameConfig
{
    
    public readonly struct consecutiveCheckin
    {
        
        /// <summary>
        /// id
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 连续签到天数
        /// </summary>
        public string dayNum { get; }
        
        /// <summary>
        /// 奖励
        /// </summary>
        public string reward { get; }
        
        internal consecutiveCheckin(int id, string dayNum, string reward)
        {
            this.id = id;
            this.dayNum = dayNum;
            this.reward = reward;
        }
    }
}