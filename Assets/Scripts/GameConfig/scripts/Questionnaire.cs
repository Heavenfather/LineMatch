/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 问卷调查.xlsx
*/

namespace GameConfig
{
    
    public readonly struct Questionnaire
    {
        
        /// <summary>
        /// 唯一id
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 0:单选 1:多选 3:文本
        /// </summary>
        public int tag { get; }
        
        /// <summary>
        /// 问卷选项
        /// </summary>
        public string desc { get; }
        
        /// <summary>
        /// 选择的下一个ID
        /// </summary>
        public string linkID { get; }
        
        /// <summary>
        /// 问卷题目
        /// </summary>
        public string title { get; }
        
        internal Questionnaire(int id, int tag, string desc, string linkID, string title)
        {
            this.id = id;
            this.tag = tag;
            this.desc = desc;
            this.linkID = linkID;
            this.title = title;
        }
    }
}