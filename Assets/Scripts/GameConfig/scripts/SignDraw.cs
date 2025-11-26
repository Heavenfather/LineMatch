/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 抽签配置.xlsx
*/

namespace GameConfig
{
    
    public readonly struct SignDraw
    {
        
        /// <summary>
        /// 配置索引
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 图标的数量
        /// </summary>
        public int num { get; }
        
        /// <summary>
        /// 抽签标签：   1- 超级上上签   2- 上上签   3- 上签  4- 大吉   5- 中吉
        /// </summary>
        public int tag { get; }
        
        /// <summary>
        /// 描述
        /// </summary>
        public string desc { get; }
        
        internal SignDraw(int id, int num, int tag, string desc)
        {
            this.id = id;
            this.num = num;
            this.tag = tag;
            this.desc = desc;
        }
    }
}