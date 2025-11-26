/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 每日任务.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;

    public readonly struct DailyTask
    {
        
        /// <summary>
        /// 配置索引
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 1：新手七天任务第1天 2：新手七天任务第2天 3：新手七天任务第3天 4：新手七天任务第4天 5：新手七天任务第5天 6：新手七天任务第6天 7：新手七天任务第7天 8：每日任务简单任务 9：每日任务困难任务
        /// </summary>
        public int taskType { get; }
        
        /// <summary>
        /// 对应任务表中的任务id
        /// </summary>
        public List<int> tags { get; }
        
        /// <summary>
        /// 不同的任务id抽取权重（100为固定任务）
        /// </summary>
        public List<float> weight { get; }
        
        internal DailyTask(int id, int taskType, List<int> tags, List<float> weight)
        {
            this.id = id;
            this.taskType = taskType;
            this.tags = tags;
            this.weight = weight;
        }
    }
}