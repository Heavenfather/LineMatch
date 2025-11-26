/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 广告配置.xlsx
*/

namespace GameConfig
{
    
    public readonly struct AdReward
    {
        
        /// <summary>
        /// 广告ID
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// 每日广告次数
        /// </summary>
        public int getreward_num { get; }
        
        /// <summary>
        /// 广告奖励
        /// </summary>
        public string ad_reward { get; }
        
        internal AdReward(int Id, int getreward_num, string ad_reward)
        {
            this.Id = Id;
            this.getreward_num = getreward_num;
            this.ad_reward = ad_reward;
        }
    }
}