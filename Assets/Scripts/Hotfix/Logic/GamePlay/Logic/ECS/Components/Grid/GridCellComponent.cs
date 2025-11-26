using System.Collections.Generic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 格子组件-棋盘上一个物理格子的数据
    /// </summary>
    public struct GridCellComponent
    {
        /// <summary>
        /// 格子单位坐标
        /// </summary>
        public Vector2Int Position;
        
        /// <summary>
        /// 格子世界坐标
        /// </summary>
        public Vector3 WorldPosition;

        /// <summary>
        /// 是否为空白格子
        /// </summary>
        public bool IsBlank;

        /// <summary>
        /// 存储该格子上当前堆叠的所有元素实体Id
        /// </summary>
        public List<int> StackedEntityIds;
    }
}