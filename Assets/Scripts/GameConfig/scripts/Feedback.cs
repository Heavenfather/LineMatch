/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 问卷调查.xlsx
*/

namespace GameConfig
{
    
    public readonly struct Feedback
    {
        
        /// <summary>
        /// 唯一id
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 0:1-3星 1:4-5星
        /// </summary>
        public int num { get; }
        
        /// <summary>
        /// 选项
        /// </summary>
        public string desc { get; }
        
        internal Feedback(int id, int num, string desc)
        {
            this.id = id;
            this.num = num;
            this.desc = desc;
        }
    }
}