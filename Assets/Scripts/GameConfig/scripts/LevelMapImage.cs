/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 大厅关卡图.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;
    using UnityEngine;

    public readonly struct LevelMapImage
    {
        
        /// <summary>
        /// 唯一ID
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// 层级最多支持6层(0-5)
        /// </summary>
        public int layerType { get; }
        
        /// <summary>
        /// 图片文件名
        /// </summary>
        public string imgName { get; }
        
        /// <summary>
        /// 消除背景图颜色
        /// </summary>
        public string matchBgColor { get; }
        
        /// <summary>
        /// 底图色系
        /// </summary>
        public MatchColorScheme lastLine { get; }
        
        /// <summary>
        /// 消除连线颜色
        /// </summary>
        public Dictionary<int, string> lineColorMap { get; }
        
        /// <summary>
        /// spine文件名
        /// </summary>
        public List<string> spineName { get; }
        
        /// <summary>
        /// spine坐标
        /// </summary>
        public List<Vector2> spinePos { get; }
        
        internal LevelMapImage(int Id, int layerType, string imgName, string matchBgColor, MatchColorScheme lastLine, Dictionary<int, string> lineColorMap, List<string> spineName, List<Vector2> spinePos)
        {
            this.Id = Id;
            this.layerType = layerType;
            this.imgName = imgName;
            this.matchBgColor = matchBgColor;
            this.lastLine = lastLine;
            this.lineColorMap = lineColorMap;
            this.spineName = spineName;
            this.spinePos = spinePos;
        }
    }
}