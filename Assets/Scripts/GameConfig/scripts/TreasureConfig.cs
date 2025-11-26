/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 无尽宝藏.xlsx
*/

namespace GameConfig
{
    
    public readonly struct TreasureConfig
    {
        
        /// <summary>
        /// 唯一ID
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// 组序号
        /// </summary>
        public int groupType { get; }
        
        /// <summary>
        /// 价格
        /// </summary>
        public int price { get; }
        
        /// <summary>
        /// 购买道具
        /// </summary>
        public int shopId { get; }
        
        /// <summary>
        /// 是否超值
        /// </summary>
        public string giftType { get; }
        
        /// <summary>
        /// 奖励物品
        /// </summary>
        public string reward { get; }
        
        internal TreasureConfig(int Id, int groupType, int price, int shopId, string giftType, string reward)
        {
            this.Id = Id;
            this.groupType = groupType;
            this.price = price;
            this.shopId = shopId;
            this.giftType = giftType;
            this.reward = reward;
        }
    }
}