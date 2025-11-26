/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 集卡配置表.xlsx
*/

namespace GameConfig
{
    
    public readonly struct PackTheme
    {
        
        /// <summary>
        /// 主题id
        /// </summary>
        public int themeId { get; }
        
        /// <summary>
        /// 主题有效时间
        /// </summary>
        public string themeDateBegin { get; }
        
        /// <summary>
        /// 主题有效时间
        /// </summary>
        public string themeDateEnd { get; }
        
        /// <summary>
        /// 主题勋章ID
        /// </summary>
        public string themeMedalID { get; }
        
        /// <summary>
        /// 主题勋章名
        /// </summary>
        public string themeMedalName { get; }
        
        /// <summary>
        /// 主题名
        /// </summary>
        public string themeName { get; }
        
        /// <summary>
        /// 主题中文名
        /// </summary>
        public string themeName_cn { get; }
        
        internal PackTheme(int themeId, string themeDateBegin, string themeDateEnd, string themeMedalID, string themeMedalName, string themeName, string themeName_cn)
        {
            this.themeId = themeId;
            this.themeDateBegin = themeDateBegin;
            this.themeDateEnd = themeDateEnd;
            this.themeMedalID = themeMedalID;
            this.themeMedalName = themeMedalName;
            this.themeName = themeName;
            this.themeName_cn = themeName_cn;
        }
    }
}