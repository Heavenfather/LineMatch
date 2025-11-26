/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 集卡配置表.xlsx
*/

namespace GameConfig
{
    
    public readonly struct Card
    {
        
        /// <summary>
        /// 卡片ID 前三位为所属的卡册id
        /// </summary>
        public int cardId { get; }
        
        /// <summary>
        /// 是否是金卡
        /// </summary>
        public bool isGold { get; }
        
        /// <summary>
        /// 所属卡组
        /// </summary>
        public int packId { get; }
        
        /// <summary>
        /// 星级
        /// </summary>
        public int star { get; }
        
        /// <summary>
        /// 卡片所属主题id
        /// </summary>
        public int themeId { get; }
        
        /// <summary>
        /// 卡片名字中文
        /// </summary>
        public string cardName_cn { get; }
        
        /// <summary>
        /// 卡片的描述
        /// </summary>
        public string desc_cn { get; }
        
        internal Card(int cardId, bool isGold, int packId, int star, int themeId, string cardName_cn, string desc_cn)
        {
            this.cardId = cardId;
            this.isGold = isGold;
            this.packId = packId;
            this.star = star;
            this.themeId = themeId;
            this.cardName_cn = cardName_cn;
            this.desc_cn = desc_cn;
        }
    }
}