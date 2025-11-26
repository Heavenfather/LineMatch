/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 集卡配置表.xlsx
*/

namespace GameConfig
{
    
    public readonly struct Cardprobability
    {
        
        /// <summary>
        /// 唯一id
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// 5星卡概率
        /// </summary>
        public float fiveStarPro { get; }
        
        /// <summary>
        /// 4星卡概率
        /// </summary>
        public float fourStarPro { get; }
        
        /// <summary>
        /// 6星卡概率
        /// </summary>
        public float goldCardPro { get; }
        
        /// <summary>
        /// 1星卡概率
        /// </summary>
        public float oneStarPro { get; }
        
        /// <summary>
        /// 3星卡概率
        /// </summary>
        public float threeStarPro { get; }
        
        /// <summary>
        /// 2星卡概率
        /// </summary>
        public float twoStarPro { get; }
        
        internal Cardprobability(int Id, float fiveStarPro, float fourStarPro, float goldCardPro, float oneStarPro, float threeStarPro, float twoStarPro)
        {
            this.Id = Id;
            this.fiveStarPro = fiveStarPro;
            this.fourStarPro = fourStarPro;
            this.goldCardPro = goldCardPro;
            this.oneStarPro = oneStarPro;
            this.threeStarPro = threeStarPro;
            this.twoStarPro = twoStarPro;
        }
    }
}