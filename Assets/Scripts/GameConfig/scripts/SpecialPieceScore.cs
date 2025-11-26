/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    
    public readonly struct SpecialPieceScore
    {
        
        /// <summary>
        /// 炸药系数
        /// </summary>
        public float bombPiece { get; }
        
        /// <summary>
        /// 功能棋子组合（彩球&彩球）
        /// </summary>
        public float candCPiece { get; }
        
        /// <summary>
        /// 功能棋子组合（彩球&火箭/炸药）
        /// </summary>
        public float candrbPiece { get; }
        
        /// <summary>
        /// 彩球系数
        /// </summary>
        public float colorballPiece { get; }
        
        /// <summary>
        /// id
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 功能棋子组合（火箭/炸药&火箭/炸药）
        /// </summary>
        public float randBPiece { get; }
        
        /// <summary>
        /// 火箭系数
        /// </summary>
        public float rockPiece { get; }
        
        internal SpecialPieceScore(float bombPiece, float candCPiece, float candrbPiece, float colorballPiece, int id, float randBPiece, float rockPiece)
        {
            this.bombPiece = bombPiece;
            this.candCPiece = candCPiece;
            this.candrbPiece = candrbPiece;
            this.colorballPiece = colorballPiece;
            this.id = id;
            this.randBPiece = randBPiece;
            this.rockPiece = rockPiece;
        }
    }
}