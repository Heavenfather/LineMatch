/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 道具配置表.xlsx
*/

namespace GameConfig
{
    
    public readonly struct ItemEnum
    {
        
        /// <summary>
        /// 唯一ID
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// 别名
        /// </summary>
        public string alias { get; }
        
        /// <summary>
        /// 描述
        /// </summary>
        public string comment { get; }
        
        /// <summary>
        /// 图标底颜色
        /// </summary>
        public string iconColor { get; }
        
        /// <summary>
        /// 道具描述
        /// </summary>
        public string iconDes { get; }
        
        /// <summary>
        /// 道具名
        /// </summary>
        public string name { get; }
        
        /// <summary>
        /// 标签
        /// </summary>
        public ItemEnumType tags { get; }
        
        internal ItemEnum(int Id, string alias, string comment, string iconColor, string iconDes, string name, ItemEnumType tags)
        {
            this.Id = Id;
            this.alias = alias;
            this.comment = comment;
            this.iconColor = iconColor;
            this.iconDes = iconDes;
            this.name = name;
            this.tags = tags;
        }
    }
}