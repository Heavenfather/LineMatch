/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    
    public readonly struct ObjectiveType
    {
        
        /// <summary>
        /// id
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 棋子类型
        /// </summary>
        public TargetTaskType Type { get; }
        
        internal ObjectiveType(int id, TargetTaskType Type)
        {
            this.id = id;
            this.Type = Type;
        }
    }
}