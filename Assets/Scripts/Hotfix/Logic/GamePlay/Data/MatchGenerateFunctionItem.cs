using System.Collections.Generic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public struct GenItemData
    {
        /// <summary>
        /// 功能棋子Id
        /// </summary>
        public int ConfigId;
        
        /// <summary>
        /// 生成位置
        /// </summary>
        public Vector2Int GenCoord;
    }
    
    /// <summary>
    /// 通过消除生成/转成下一个的棋子结构
    /// </summary>
    public struct MatchGenerateFunctionItem
    {
        /// <summary>
        /// 生成的功能棋子数据
        /// </summary>
        public List<GenItemData> GenItemsData;
    }
}