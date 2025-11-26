/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 寻宝关卡奖励.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;

    public readonly struct PuzzleCollectAudio
    {
        
        /// <summary>
        /// 音效名
        /// </summary>
        public string audioName { get; }
        
        /// <summary>
        /// 收集物品ID
        /// </summary>
        public List<int> collectIds { get; }
        
        internal PuzzleCollectAudio(string audioName, List<int> collectIds)
        {
            this.audioName = audioName;
            this.collectIds = collectIds;
        }
    }
}