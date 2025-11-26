/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 集卡配置表.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class PackItemsRewardDB : ConfigBase
    {
        private PackItemsReward[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new PackItemsReward[]
            {
                new(packId: 101, packName: LocalizationPool.Get("PackItemsReward/e50f"), oneStarNum: 8, twoStarNum: 1, threeStarNum: 0, fourStarNum: 0, fiveStarNum: 0, goldCardNum: 0, reward: "eliminateRock*1"),
                new(packId: 102, packName: LocalizationPool.Get("PackItemsReward/98e9"), oneStarNum: 7, twoStarNum: 2, threeStarNum: 0, fourStarNum: 0, fiveStarNum: 0, goldCardNum: 0, reward: "eliminateBoom*1"),
                new(packId: 103, packName: LocalizationPool.Get("PackItemsReward/07d5"), oneStarNum: 7, twoStarNum: 1, threeStarNum: 1, fourStarNum: 0, fiveStarNum: 0, goldCardNum: 0, reward: "eliminateBall*1"),
                new(packId: 104, packName: LocalizationPool.Get("PackItemsReward/9d5b"), oneStarNum: 5, twoStarNum: 3, threeStarNum: 1, fourStarNum: 0, fiveStarNum: 0, goldCardNum: 0, reward: "eliminateDice*1"),
                new(packId: 105, packName: LocalizationPool.Get("PackItemsReward/7c12"), oneStarNum: 3, twoStarNum: 4, threeStarNum: 1, fourStarNum: 1, fiveStarNum: 0, goldCardNum: 0, reward: "eliminateHammer*1"),
                new(packId: 106, packName: LocalizationPool.Get("PackItemsReward/2625"), oneStarNum: 2, twoStarNum: 4, threeStarNum: 1, fourStarNum: 1, fiveStarNum: 1, goldCardNum: 0, reward: "eliminateBullet*1"),
                new(packId: 107, packName: LocalizationPool.Get("PackItemsReward/0742"), oneStarNum: 1, twoStarNum: 3, threeStarNum: 2, fourStarNum: 1, fiveStarNum: 1, goldCardNum: 1, reward: "eliminateDice*1"),
                new(packId: 108, packName: LocalizationPool.Get("PackItemsReward/838f"), oneStarNum: 0, twoStarNum: 4, threeStarNum: 2, fourStarNum: 1, fiveStarNum: 1, goldCardNum: 1, reward: "eliminateRock*2"),
                new(packId: 109, packName: LocalizationPool.Get("PackItemsReward/d193"), oneStarNum: 0, twoStarNum: 3, threeStarNum: 2, fourStarNum: 1, fiveStarNum: 2, goldCardNum: 1, reward: "eliminateBoom*2"),
                new(packId: 110, packName: LocalizationPool.Get("PackItemsReward/4a07"), oneStarNum: 0, twoStarNum: 2, threeStarNum: 2, fourStarNum: 2, fiveStarNum: 1, goldCardNum: 2, reward: "eliminateBall*2"),
                new(packId: 111, packName: LocalizationPool.Get("PackItemsReward/fb8a"), oneStarNum: 0, twoStarNum: 1, threeStarNum: 3, fourStarNum: 1, fiveStarNum: 2, goldCardNum: 2, reward: "eliminateDice*2"),
                new(packId: 112, packName: LocalizationPool.Get("PackItemsReward/e5b3"), oneStarNum: 0, twoStarNum: 0, threeStarNum: 3, fourStarNum: 2, fiveStarNum: 2, goldCardNum: 2, reward: "eliminateHammer*2"),
                new(packId: 113, packName: LocalizationPool.Get("PackItemsReward/1b29"), oneStarNum: 0, twoStarNum: 0, threeStarNum: 3, fourStarNum: 2, fiveStarNum: 1, goldCardNum: 3, reward: "eliminateBullet*2"),
                new(packId: 114, packName: LocalizationPool.Get("PackItemsReward/bb1d"), oneStarNum: 0, twoStarNum: 0, threeStarNum: 2, fourStarNum: 2, fiveStarNum: 2, goldCardNum: 3, reward: "eliminateDice*2"),
                new(packId: 115, packName: LocalizationPool.Get("PackItemsReward/fb54"), oneStarNum: 0, twoStarNum: 0, threeStarNum: 0, fourStarNum: 4, fiveStarNum: 2, goldCardNum: 3, reward: "eliminateDice*2")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly PackItemsReward this[int packId]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(packId, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[PackItemsReward] packId: {packId} not found");
                return ref _data[idx];
            }
        }
        
        public PackItemsReward[] All => _data;
        
        public int Count => _data.Length;
        
        public override void Dispose()
        {
            _data = null;
            OnDispose();
        }
        
        private void MakeIdToIdx()
        {
            _idToIdx = new Dictionary<int,int>(_data.Length);
            for (int i = 0; i < _data.Length; i++)
            {
                _idToIdx[_data[i].packId] = i;
            }
        }
    }
}