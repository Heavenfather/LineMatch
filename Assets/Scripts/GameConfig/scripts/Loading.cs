/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: Loading配置.xlsx
*/

namespace GameConfig
{
    
    public readonly struct Loading
    {
        
        /// <summary>
        /// 配置索引
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 描述文本
        /// </summary>
        public string desc { get; }
        
        /// <summary>
        /// 特殊显示的节点
        /// </summary>
        public string effName { get; }
        
        /// <summary>
        /// 示例图
        /// </summary>
        public LoadingConst loadingConst { get; }
        
        internal Loading(int id, string desc, string effName, LoadingConst loadingConst)
        {
            this.id = id;
            this.desc = desc;
            this.effName = effName;
            this.loadingConst = loadingConst;
        }
    }
}