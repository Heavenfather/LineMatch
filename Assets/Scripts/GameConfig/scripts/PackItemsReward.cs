/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 集卡配置表.xlsx
*/

namespace GameConfig
{
    
    public readonly struct PackItemsReward
    {
        
        /// <summary>
        /// 5星卡数量
        /// </summary>
        public int fiveStarNum { get; }
        
        /// <summary>
        /// 4星卡数量
        /// </summary>
        public int fourStarNum { get; }
        
        /// <summary>
        /// 5星金卡数量
        /// </summary>
        public int goldCardNum { get; }
        
        /// <summary>
        /// 1星卡数量
        /// </summary>
        public int oneStarNum { get; }
        
        /// <summary>
        /// 唯一id
        /// </summary>
        public int packId { get; }
        
        /// <summary>
        /// 3星卡数量
        /// </summary>
        public int threeStarNum { get; }
        
        /// <summary>
        /// 2星卡数量
        /// </summary>
        public int twoStarNum { get; }
        
        /// <summary>
        /// 卡组名
        /// </summary>
        public string packName { get; }
        
        /// <summary>
        /// 关卡奖励
        /// </summary>
        public string reward { get; }
        
        internal PackItemsReward(int fiveStarNum, int fourStarNum, int goldCardNum, int oneStarNum, int packId, int threeStarNum, int twoStarNum, string packName, string reward)
        {
            this.fiveStarNum = fiveStarNum;
            this.fourStarNum = fourStarNum;
            this.goldCardNum = goldCardNum;
            this.oneStarNum = oneStarNum;
            this.packId = packId;
            this.threeStarNum = threeStarNum;
            this.twoStarNum = twoStarNum;
            this.packName = packName;
            this.reward = reward;
        }
    }
}