/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 寻宝关卡奖励.xlsx
*/

namespace GameConfig
{
    using UnityEngine;

    public readonly struct SpineBoxArea
    {
        
        /// <summary>
        /// Spine节点的名字
        /// </summary>
        public string spineID { get; }
        
        /// <summary>
        /// 偏移量
        /// </summary>
        public Vector2 offset { get; }
        
        /// <summary>
        /// 点击范围
        /// </summary>
        public Vector2 size { get; }
        
        internal SpineBoxArea(string spineID, Vector2 offset, Vector2 size)
        {
            this.spineID = spineID;
            this.offset = offset;
            this.size = size;
        }
    }
}