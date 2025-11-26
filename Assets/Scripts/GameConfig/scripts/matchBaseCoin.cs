/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    
    public readonly struct matchBaseCoin
    {
        
        /// <summary>
        /// 容易关卡基础金币
        /// </summary>
        public int ease { get; }
        
        /// <summary>
        /// 极难关卡基础金币
        /// </summary>
        public int extreme { get; }
        
        /// <summary>
        /// 难关基础金币
        /// </summary>
        public int hard { get; }
        
        /// <summary>
        /// id
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 一星加成比例
        /// </summary>
        public float starMultiplier1 { get; }
        
        /// <summary>
        /// 二星加成比例
        /// </summary>
        public float starMultiplier2 { get; }
        
        /// <summary>
        /// 三星加成比例
        /// </summary>
        public float starMultiplier3 { get; }
        
        internal matchBaseCoin(int ease, int extreme, int hard, int id, float starMultiplier1, float starMultiplier2, float starMultiplier3)
        {
            this.ease = ease;
            this.extreme = extreme;
            this.hard = hard;
            this.id = id;
            this.starMultiplier1 = starMultiplier1;
            this.starMultiplier2 = starMultiplier2;
            this.starMultiplier3 = starMultiplier3;
        }
    }
}