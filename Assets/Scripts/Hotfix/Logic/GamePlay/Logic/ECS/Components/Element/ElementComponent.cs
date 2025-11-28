using GameConfig;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 棋子元素组件，标记元素实体
    /// </summary>
    public struct ElementComponent
    {
        /// <summary>
        /// 配置表ID
        /// </summary>
        public int ConfigId;

        /// <summary>
        /// 对于多格物体，记录原始位置 确保渲染的准确位置
        /// </summary>
        public Vector2Int OriginGridPosition;

        /// <summary>
        ///  类型
        /// </summary>
        public ElementType Type;

        /// <summary>
        /// 消除方式
        /// </summary>
        public EliminateStyle EliminateStyle;

        /// <summary>
        /// 剩余消除次数
        /// </summary>
        public int EliminateCount;
        
        /// <summary>
        /// 最大消除次数
        /// </summary>
        public int MaxEliminateCount;

        /// <summary>
        /// 消除后变成的ID，0表示销毁
        /// </summary>
        public int NextConfigId;

        /// <summary>
        /// 排序层级
        /// </summary>
        public int Layer;

        /// <summary>
        /// 元素占据的宽度
        /// </summary>
        public int Width;

        /// <summary>
        /// 元素占据的高度
        /// </summary>
        public int Height;

        /// <summary>
        /// 是否可移动
        /// </summary>
        public bool IsMovable;

        /// <summary>
        /// 是否可进行普通匹配
        /// </summary>
        public bool IsMatchable;

        /// <summary>
        /// 是否受击了
        /// </summary>
        public bool IsDamageDirty;
        
        /// <summary>
        /// 棋子逻辑状态
        /// </summary>
        public ElementLogicalState LogicState;
    }
}