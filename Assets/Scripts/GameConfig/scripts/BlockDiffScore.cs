/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    
    public readonly struct BlockDiffScore
    {
        
        /// <summary>
        /// 方块等级
        /// </summary>
        public int BlockLevel { get; }
        
        /// <summary>
        /// 系数
        /// </summary>
        public float BlockLevelScore { get; }
        
        /// <summary>
        /// id
        /// </summary>
        public int id { get; }
        
        internal BlockDiffScore(int BlockLevel, float BlockLevelScore, int id)
        {
            this.BlockLevel = BlockLevel;
            this.BlockLevelScore = BlockLevelScore;
            this.id = id;
        }
    }
}