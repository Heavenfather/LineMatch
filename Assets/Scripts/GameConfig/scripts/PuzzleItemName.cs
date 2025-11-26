/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 寻宝关卡奖励.xlsx
*/

namespace GameConfig
{
    
    public readonly struct PuzzleItemName
    {
        
        /// <summary>
        /// 寻宝物件中文名
        /// </summary>
        public string ItemCCName { get; }
        
        /// <summary>
        /// 寻宝的物件
        /// </summary>
        public string ItemId { get; }
        
        /// <summary>
        /// 寻宝物件的图片名
        /// </summary>
        public string ItemName { get; }
        
        internal PuzzleItemName(string ItemCCName, string ItemId, string ItemName)
        {
            this.ItemCCName = ItemCCName;
            this.ItemId = ItemId;
            this.ItemName = ItemName;
        }
    }
}