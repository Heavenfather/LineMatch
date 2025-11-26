/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 商城配置表.xlsx
*/

namespace GameConfig
{
    
    public readonly struct ShopItems
    {
        
        /// <summary>
        /// 唯一ID
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// 折扣(不填和100）表示不打折
        /// </summary>
        public int discount { get; }
        
        /// <summary>
        /// 标签类型：1—最实惠，2—必买爆款，3—最畅销，4—N折扣
        /// </summary>
        public int discountType { get; }
        
        /// <summary>
        /// 礼包类型：0-广告，1—商城普通礼包，2—商城折扣礼包，3—道具付费礼包，4—限时特惠，5-月卡，6-结算金币不足，7-礼包
        /// </summary>
        public int giftType { get; }
        
        /// <summary>
        /// 商品原价
        /// </summary>
        public int original_price { get; }
        
        /// <summary>
        /// 价格
        /// </summary>
        public int price { get; }
        
        /// <summary>
        /// 抖音钻石价格
        /// </summary>
        public int tt_diamond { get; }
        
        /// <summary>
        /// 别名
        /// </summary>
        public string alias { get; }
        
        /// <summary>
        /// 购买限次(1-1终身1次、2-1每天1次、3-1每星期1次、4-1每月1次、5-1每季度1次)
        /// </summary>
        public string limit { get; }
        
        /// <summary>
        /// 道具名
        /// </summary>
        public string name { get; }
        
        /// <summary>
        /// 商品内容
        /// </summary>
        public string reward { get; }
        
        internal ShopItems(int Id, int discount, int discountType, int giftType, int original_price, int price, int tt_diamond, string alias, string limit, string name, string reward)
        {
            this.Id = Id;
            this.discount = discount;
            this.discountType = discountType;
            this.giftType = giftType;
            this.original_price = original_price;
            this.price = price;
            this.tt_diamond = tt_diamond;
            this.alias = alias;
            this.limit = limit;
            this.name = name;
            this.reward = reward;
        }
    }
}