using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 玩家输入状态组件
    /// </summary>
    public struct MatchInputComponent
    {
        public bool IsDragging;             // 是否正在拖拽中
        public bool IsRectangle;            // 是否形成了闭环（方块）
        
        public int FirstConfigId;           // 首个选中元素的ConfigId（用于颜色判断）
        
        // 如果形成了闭环，这个字段存储我们连回到了哪个旧点
        public int LoopTargetEntityId;
        
        /// <summary>
        /// 当前选中的棋子实体ID列表 (有序)
        /// 对应旧代码的 _selectedGrids
        /// </summary>
        public List<int> SelectedEntityIds; 
        
        /// <summary>
        /// 当前选中的格子实体ID列表 (有序)
        /// 用于快速判断邻居关系
        /// </summary>
        public List<int> SelectedGridIds;

        /// <summary>
        /// 输入结束标记（一帧的事件标记）
        /// </summary>
        public bool IsInputComplete; 
    }
}