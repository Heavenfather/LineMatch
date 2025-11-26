/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 装扮配置.xlsx
*/

namespace GameConfig
{
    
    public readonly struct DressEnum
    {
        
        /// <summary>
        /// 唯一ID
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// 获取方式   0:免费获取 1:金币购买 2:其他方式
        /// </summary>
        public int buyType { get; }
        
        /// <summary>
        /// 非免费对应的道具ID
        /// </summary>
        public string itemName { get; }
        
        /// <summary>
        /// 标签
        /// </summary>
        public ItemEnumType tags { get; }
        
        internal DressEnum(int Id, int buyType, string itemName, ItemEnumType tags)
        {
            this.Id = Id;
            this.buyType = buyType;
            this.itemName = itemName;
            this.tags = tags;
        }
    }
}