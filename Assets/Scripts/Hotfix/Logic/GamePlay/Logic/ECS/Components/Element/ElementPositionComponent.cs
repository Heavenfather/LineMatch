using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 元素位置组件
    /// </summary>
    public struct ElementPositionComponent
    {
        /// <summary>
        /// 起始单元格X轴
        /// </summary>
        public int X;

        /// <summary>
        /// 起始单元格Y轴
        /// </summary>
        public int Y;

        /// <summary>
        /// 本地位置
        /// </summary>
        public Vector3 LocalPosition;
        
        /// <summary>
        /// 当前的世界位置
        /// </summary>
        public Vector3 WorldPosition;
    }
}