/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 关卡固定调控值表.xlsx
*/

namespace GameConfig
{
    
    public readonly struct LevelStrategy
    {
        
        /// <summary>
        /// 配置索引
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 调控值
        /// </summary>
        public int value { get; }
        
        internal LevelStrategy(int id, int value)
        {
            this.id = id;
            this.value = value;
        }
    }
}