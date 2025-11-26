/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    
    public readonly struct ObjectiveReward
    {
        
        /// <summary>
        /// 目标物的值-普通棋子(1号棋子)
        /// </summary>
        public int Num1 { get; }
        
        /// <summary>
        /// 目标物的值-火箭
        /// </summary>
        public int Num2 { get; }
        
        /// <summary>
        /// 目标物的值-炸药
        /// </summary>
        public int Num3 { get; }
        
        /// <summary>
        /// 目标物的值-四方格
        /// </summary>
        public int Num4 { get; }
        
        /// <summary>
        /// 目标物的值-彩球
        /// </summary>
        public int Num5 { get; }
        
        /// <summary>
        /// id
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 奖励部分功能未解锁(有多个奖励展示宝箱，单个显示对应的奖励icon)
        /// </summary>
        public string reward1 { get; }
        
        /// <summary>
        /// 奖励解锁了所有功能
        /// </summary>
        public string reward2 { get; }
        
        internal ObjectiveReward(int Num1, int Num2, int Num3, int Num4, int Num5, int id, string reward1, string reward2)
        {
            this.Num1 = Num1;
            this.Num2 = Num2;
            this.Num3 = Num3;
            this.Num4 = Num4;
            this.Num5 = Num5;
            this.id = id;
            this.reward1 = reward1;
            this.reward2 = reward2;
        }
    }
}