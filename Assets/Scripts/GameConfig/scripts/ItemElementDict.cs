/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 道具对应棋子字典.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;

    public readonly struct ItemElementDict
    {
        
        /// <summary>
        /// 道具ID
        /// </summary>
        public string itemId { get; }
        
        /// <summary>
        /// 棋子ID
        /// </summary>
        public List<int> elementId { get; }
        
        internal ItemElementDict(string itemId, List<int> elementId)
        {
            this.itemId = itemId;
            this.elementId = elementId;
        }
    }
}