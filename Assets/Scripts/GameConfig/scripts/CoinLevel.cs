/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: J 金币关配置表.xlsx
*/

namespace GameConfig
{
    
    public readonly struct CoinLevel
    {
        
        /// <summary>
        /// 最大获取的金币数量
        /// </summary>
        public int coinMax { get; }
        
        /// <summary>
        /// 关卡Id
        /// </summary>
        public int id { get; }
        
        internal CoinLevel(int coinMax, int id)
        {
            this.coinMax = coinMax;
            this.id = id;
        }
    }
}