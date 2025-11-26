/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 功能棋子形成规则表.xlsx
*/

namespace GameConfig
{
    
    public readonly struct SpecialEleRule
    {
        
        /// <summary>
        /// 包围盒最大高度 0表示无限制
        /// </summary>
        public int maxHeight { get; }
        
        /// <summary>
        /// 包围盒最大宽度 0表示无限制
        /// </summary>
        public int maxWidth { get; }
        
        /// <summary>
        /// 包围盒最小高度
        /// </summary>
        public int minHeight { get; }
        
        /// <summary>
        /// 包围盒最小宽度
        /// </summary>
        public int minWidth { get; }
        
        /// <summary>
        /// 形成优先级，数字越大越优先匹配
        /// </summary>
        public int priority { get; }
        
        /// <summary>
        /// 任务统计标签
        /// </summary>
        public int taskTag { get; }
        
        /// <summary>
        /// 形成的功能棋子类型
        /// </summary>
        public ElementType resultElement { get; }
        
        /// <summary>
        /// 计分规则
        /// </summary>
        public OneTakeScoreType scoreType { get; }
        
        internal SpecialEleRule(int maxHeight, int maxWidth, int minHeight, int minWidth, int priority, int taskTag, ElementType resultElement, OneTakeScoreType scoreType)
        {
            this.maxHeight = maxHeight;
            this.maxWidth = maxWidth;
            this.minHeight = minHeight;
            this.minWidth = minWidth;
            this.priority = priority;
            this.taskTag = taskTag;
            this.resultElement = resultElement;
            this.scoreType = scoreType;
        }
    }
}