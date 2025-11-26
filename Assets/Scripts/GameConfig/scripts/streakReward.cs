/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    
    public readonly struct streakReward
    {
        
        /// <summary>
        /// id
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 是否立即使用道具
        /// </summary>
        public bool isUseItem { get; }
        
        /// <summary>
        /// 奖励
        /// </summary>
        public string reward { get; }
        
        /// <summary>
        /// 连胜次数
        /// </summary>
        public string streakNum { get; }
        
        internal streakReward(int id, bool isUseItem, string reward, string streakNum)
        {
            this.id = id;
            this.isUseItem = isUseItem;
            this.reward = reward;
            this.streakNum = streakNum;
        }
    }
}