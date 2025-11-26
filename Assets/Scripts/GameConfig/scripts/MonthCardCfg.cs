/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 月卡配置.xlsx
*/

namespace GameConfig
{
    
    public readonly struct MonthCardCfg
    {
        
        /// <summary>
        /// 收益
        /// </summary>
        public int discount { get; }
        
        /// <summary>
        /// 配置索引
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 价格
        /// </summary>
        public int price { get; }
        
        /// <summary>
        /// 配置string值
        /// </summary>
        public string dailyReward { get; }
        
        /// <summary>
        /// 配置string值
        /// </summary>
        public string immediatelyReward { get; }
        
        /// <summary>
        /// 配置string值
        /// </summary>
        public string name { get; }
        
        internal MonthCardCfg(int discount, int id, int price, string dailyReward, string immediatelyReward, string name)
        {
            this.discount = discount;
            this.id = id;
            this.price = price;
            this.dailyReward = dailyReward;
            this.immediatelyReward = immediatelyReward;
            this.name = name;
        }
    }
}