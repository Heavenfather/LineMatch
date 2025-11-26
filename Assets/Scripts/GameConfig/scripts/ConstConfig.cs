/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 通用配置.xlsx
*/

namespace GameConfig
{
    
    public readonly struct ConstConfig
    {
        
        /// <summary>
        /// 配置int值
        /// </summary>
        public int intValue { get; }
        
        /// <summary>
        /// 配置索引
        /// </summary>
        public string id { get; }
        
        /// <summary>
        /// 配置string值
        /// </summary>
        public string strValue { get; }
        
        internal ConstConfig(int intValue, string id, string strValue)
        {
            this.intValue = intValue;
            this.id = id;
            this.strValue = strValue;
        }
    }
}