/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 商城配置表.xlsx
*/

namespace GameConfig
{
    
    public readonly struct CoinShopItems
    {
        
        /// <summary>
        /// 唯一ID
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// 金币数量
        /// </summary>
        public int coin_price { get; }
        
        /// <summary>
        /// 礼包类型：0-广告，1—商城显示，2—商城不显示
        /// </summary>
        public int type { get; }
        
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
        
        /// <summary>
        /// 标签
        /// </summary>
        public CoinExchangeLabType label { get; }
        
        internal CoinShopItems(int Id, int coin_price, int type, string alias, string limit, string name, string reward, CoinExchangeLabType label)
        {
            this.Id = Id;
            this.coin_price = coin_price;
            this.type = type;
            this.alias = alias;
            this.limit = limit;
            this.name = name;
            this.reward = reward;
            this.label = label;
        }
    }
}