/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 集卡配置表.xlsx
*/

namespace GameConfig
{
    
    public readonly struct PackTotalReward
    {
        
        /// <summary>
        /// 是否有勋章奖励，1是有，0是没有，有就从PachTheme中取相应主题的勋章
        /// </summary>
        public bool IsMedalReward { get; }
        
        /// <summary>
        /// 星星所需的数量
        /// </summary>
        public int StarNumber { get; }
        
        /// <summary>
        /// 星星等级
        /// </summary>
        public int levelID { get; }
        
        /// <summary>
        /// 最近7天充值最低达到多少使用
        /// </summary>
        public int rechargeLimit { get; }
        
        /// <summary>
        /// 关卡总奖励
        /// </summary>
        public string reward { get; }
        
        internal PackTotalReward(bool IsMedalReward, int StarNumber, int levelID, int rechargeLimit, string reward)
        {
            this.IsMedalReward = IsMedalReward;
            this.StarNumber = StarNumber;
            this.levelID = levelID;
            this.rechargeLimit = rechargeLimit;
            this.reward = reward;
        }
    }
}