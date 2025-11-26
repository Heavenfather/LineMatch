/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 集卡配置表.xlsx
*/

namespace GameConfig
{
    
    public readonly struct HaveOrNotProbability
    {
        
        /// <summary>
        /// 唯一id
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// 自增概率
        /// </summary>
        public float addPro { get; }
        
        /// <summary>
        /// 获取未有的卡的概率
        /// </summary>
        public float notHaveCardPro { get; }
        
        /// <summary>
        /// 自增起始次数
        /// </summary>
        public int openNum { get; }
        
        internal HaveOrNotProbability(int Id, float addPro, float notHaveCardPro, int openNum)
        {
            this.Id = Id;
            this.addPro = addPro;
            this.notHaveCardPro = notHaveCardPro;
            this.openNum = openNum;
        }
    }
}