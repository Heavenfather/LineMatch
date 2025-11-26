/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 商城配置表.xlsx
*/

namespace GameConfig
{
    
    public readonly struct GradeGift
    {
        
        /// <summary>
        /// 唯一ID
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// 对应的商城ID
        /// </summary>
        public int shopID { get; }
        
        /// <summary>
        /// 礼包类型
        /// </summary>
        public GradeGiftType giftType { get; }
        
        internal GradeGift(int Id, int shopID, GradeGiftType giftType)
        {
            this.Id = Id;
            this.shopID = shopID;
            this.giftType = giftType;
        }
    }
}