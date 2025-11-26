/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    
    public readonly struct LevelDiffScore
    {
        
        /// <summary>
        /// 容易关卡系数
        /// </summary>
        public float easeParam { get; }
        
        /// <summary>
        /// 极难关卡系数
        /// </summary>
        public float extremeParam { get; }
        
        /// <summary>
        /// 难关系数
        /// </summary>
        public float hardParam { get; }
        
        /// <summary>
        /// id
        /// </summary>
        public int id { get; }
        
        internal LevelDiffScore(float easeParam, float extremeParam, float hardParam, int id)
        {
            this.easeParam = easeParam;
            this.extremeParam = extremeParam;
            this.hardParam = hardParam;
            this.id = id;
        }
    }
}