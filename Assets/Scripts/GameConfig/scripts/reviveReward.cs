/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    
    public readonly struct reviveReward
    {
        
        /// <summary>
        /// 增加步数
        /// </summary>
        public int addSteps { get; }
        
        /// <summary>
        /// id
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 是否立即使用道具
        /// </summary>
        public bool isUseItem { get; }
        
        /// <summary>
        /// 需要的金币数量
        /// </summary>
        public int needCoin { get; }
        
        /// <summary>
        /// 复活的次数
        /// </summary>
        public int reviveNum { get; }
        
        /// <summary>
        /// 增加的道具
        /// </summary>
        public string addItem { get; }
        
        internal reviveReward(int addSteps, int id, bool isUseItem, int needCoin, int reviveNum, string addItem)
        {
            this.addSteps = addSteps;
            this.id = id;
            this.isUseItem = isUseItem;
            this.needCoin = needCoin;
            this.reviveNum = reviveNum;
            this.addItem = addItem;
        }
    }
}