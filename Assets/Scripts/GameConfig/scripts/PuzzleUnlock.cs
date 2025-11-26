/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 寻宝关卡奖励.xlsx
*/

namespace GameConfig
{
    
    public readonly struct PuzzleUnlock
    {
        
        /// <summary>
        /// 寻宝图的序章
        /// </summary>
        public int mapId { get; }
        
        /// <summary>
        /// 解锁的关卡
        /// </summary>
        public int unlockLevel { get; }
        
        internal PuzzleUnlock(int mapId, int unlockLevel)
        {
            this.mapId = mapId;
            this.unlockLevel = unlockLevel;
        }
    }
}