/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 大厅关卡图.xlsx
*/

namespace GameConfig
{
    using UnityEngine;

    public readonly struct LevelSpineTouch
    {
        
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// 按钮位置
        /// </summary>
        public Vector2 btnPos { get; }
        
        /// <summary>
        /// 按钮大小
        /// </summary>
        public Vector2 btnSize { get; }
        
        internal LevelSpineTouch(string Id, Vector2 btnPos, Vector2 btnSize)
        {
            this.Id = Id;
            this.btnPos = btnPos;
            this.btnSize = btnSize;
        }
    }
}