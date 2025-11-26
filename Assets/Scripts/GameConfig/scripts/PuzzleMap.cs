/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 寻宝关卡奖励.xlsx
*/

namespace GameConfig
{
    
    public readonly struct PuzzleMap
    {
        
        /// <summary>
        /// 寻宝图的序章
        /// </summary>
        public int mapId { get; }
        
        /// <summary>
        /// 关卡8的名字
        /// </summary>
        public string EnvirAudio { get; }
        
        /// <summary>
        /// 引导图片颜色
        /// </summary>
        public string GuideColor { get; }
        
        /// <summary>
        /// 奖励图标
        /// </summary>
        public string GuideReward { get; }
        
        /// <summary>
        /// 关卡1的名字
        /// </summary>
        public string LeveName1 { get; }
        
        /// <summary>
        /// 关卡2的名字
        /// </summary>
        public string LeveName2 { get; }
        
        /// <summary>
        /// 关卡3的名字
        /// </summary>
        public string LeveName3 { get; }
        
        /// <summary>
        /// 关卡4的名字
        /// </summary>
        public string LeveName4 { get; }
        
        /// <summary>
        /// 关卡5的名字
        /// </summary>
        public string LeveName5 { get; }
        
        /// <summary>
        /// 关卡6的名字
        /// </summary>
        public string LeveName6 { get; }
        
        /// <summary>
        /// 关卡7的名字
        /// </summary>
        public string LeveName7 { get; }
        
        /// <summary>
        /// 关卡8的名字
        /// </summary>
        public string LeveName8 { get; }
        
        /// <summary>
        /// 寻宝图英文标识
        /// </summary>
        public string MapName { get; }
        
        /// <summary>
        /// 寻宝图中文名
        /// </summary>
        public string mapCNName { get; }
        
        internal PuzzleMap(int mapId, string EnvirAudio, string GuideColor, string GuideReward, string LeveName1, string LeveName2, string LeveName3, string LeveName4, string LeveName5, string LeveName6, string LeveName7, string LeveName8, string MapName, string mapCNName)
        {
            this.mapId = mapId;
            this.EnvirAudio = EnvirAudio;
            this.GuideColor = GuideColor;
            this.GuideReward = GuideReward;
            this.LeveName1 = LeveName1;
            this.LeveName2 = LeveName2;
            this.LeveName3 = LeveName3;
            this.LeveName4 = LeveName4;
            this.LeveName5 = LeveName5;
            this.LeveName6 = LeveName6;
            this.LeveName7 = LeveName7;
            this.LeveName8 = LeveName8;
            this.MapName = MapName;
            this.mapCNName = mapCNName;
        }
    }
}