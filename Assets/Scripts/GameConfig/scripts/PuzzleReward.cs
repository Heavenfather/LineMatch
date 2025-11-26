/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 寻宝关卡奖励.xlsx
*/

namespace GameConfig
{
    
    public readonly struct PuzzleReward
    {
        
        /// <summary>
        /// 唯一id
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 关卡id
        /// </summary>
        public int levelId { get; }
        
        /// <summary>
        /// 寻宝图的序章
        /// </summary>
        public int mapId { get; }
        
        /// <summary>
        /// 开启金币数
        /// </summary>
        public int openCoin { get; }
        
        /// <summary>
        /// 开启星星数
        /// </summary>
        public int openStar { get; }
        
        /// <summary>
        /// 关卡奖励
        /// </summary>
        public string reward { get; }
        
        internal PuzzleReward(int id, int levelId, int mapId, int openCoin, int openStar, string reward)
        {
            this.id = id;
            this.levelId = levelId;
            this.mapId = mapId;
            this.openCoin = openCoin;
            this.openStar = openStar;
            this.reward = reward;
        }
    }
}