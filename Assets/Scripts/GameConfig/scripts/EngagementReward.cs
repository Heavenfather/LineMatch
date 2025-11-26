/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 每日任务.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;

    public readonly struct EngagementReward
    {
        
        /// <summary>
        /// 配置索引
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 活跃度目标（活跃度到达阶段性目标进度获得奖励）
        /// </summary>
        public int num { get; }
        
        /// <summary>
        /// 1：七日任务活跃度 8：每日任务活跃度
        /// </summary>
        public int taskType { get; }
        
        /// <summary>
        /// 奖励描述
        /// </summary>
        public string desc { get; }
        
        /// <summary>
        /// 奖励（道具用id或者名字都可以） 道具1*数量|道具2*数量|
        /// </summary>
        public List<string> reward { get; }
        
        internal EngagementReward(int id, int num, int taskType, string desc, List<string> reward)
        {
            this.id = id;
            this.num = num;
            this.taskType = taskType;
            this.desc = desc;
            this.reward = reward;
        }
    }
}