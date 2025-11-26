/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 无尽宝藏.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class TreasureConfigDB : ConfigBase
    {
        private TreasureConfig[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new TreasureConfig[]
            {
                new(Id: 1, reward: "live*10", price: 0, shopId: 0, groupType: 1, giftType: ""),
                new(Id: 2, reward: "coin*199", price: 0, shopId: 0, groupType: 1, giftType: ""),
                new(Id: 3, reward: "eliminateRock*1", price: 0, shopId: 0, groupType: 1, giftType: ""),
                new(Id: 4, reward: "liveBuff*10", price: 3, shopId: 11001, groupType: 2, giftType: LocalizationPool.Get("TreasureConfig/bffb")),
                new(Id: 5, reward: "coin*1499", price: 3, shopId: 0, groupType: 2, giftType: ""),
                new(Id: 6, reward: "eliminateHammer*1", price: 3, shopId: 0, groupType: 2, giftType: ""),
                new(Id: 7, reward: "eliminateDice*1", price: 3, shopId: 0, groupType: 2, giftType: ""),
                new(Id: 8, reward: "eliminateRock*1", price: 3, shopId: 0, groupType: 2, giftType: ""),
                new(Id: 9, reward: "eliminateHammer*1", price: 3, shopId: 0, groupType: 2, giftType: ""),
                new(Id: 10, reward: "coin*4499", price: 6, shopId: 11002, groupType: 3, giftType: LocalizationPool.Get("TreasureConfig/bffb")),
                new(Id: 11, reward: "eliminateBall*1", price: 6, shopId: 0, groupType: 3, giftType: ""),
                new(Id: 12, reward: "eliminateBoom*2", price: 6, shopId: 0, groupType: 3, giftType: ""),
                new(Id: 13, reward: "liveBuff*10", price: 6, shopId: 0, groupType: 3, giftType: ""),
                new(Id: 14, reward: "eliminateColored*1", price: 6, shopId: 0, groupType: 3, giftType: ""),
                new(Id: 15, reward: "eliminateDice*1", price: 6, shopId: 0, groupType: 3, giftType: ""),
                new(Id: 16, reward: "liveBuff*20", price: 12, shopId: 11003, groupType: 4, giftType: LocalizationPool.Get("TreasureConfig/bffb")),
                new(Id: 17, reward: "coin*10999", price: 12, shopId: 0, groupType: 4, giftType: ""),
                new(Id: 18, reward: "eliminateBoom*2", price: 12, shopId: 0, groupType: 4, giftType: ""),
                new(Id: 19, reward: "eliminateArrow*3", price: 12, shopId: 0, groupType: 4, giftType: ""),
                new(Id: 20, reward: "eliminateRock*2", price: 12, shopId: 0, groupType: 4, giftType: ""),
                new(Id: 21, reward: "eliminateDice*2", price: 12, shopId: 0, groupType: 4, giftType: ""),
                new(Id: 22, reward: "liveBuff*20|eliminateHammer*5", price: 30, shopId: 11004, groupType: 5, giftType: LocalizationPool.Get("TreasureConfig/bffb")),
                new(Id: 23, reward: "coin*15999", price: 30, shopId: 0, groupType: 5, giftType: ""),
                new(Id: 24, reward: "eliminateBullet*5", price: 30, shopId: 0, groupType: 5, giftType: ""),
                new(Id: 25, reward: "eliminateBoom*7", price: 30, shopId: 0, groupType: 5, giftType: ""),
                new(Id: 26, reward: "coin*13999", price: 30, shopId: 0, groupType: 5, giftType: ""),
                new(Id: 27, reward: "eliminateArrow*7", price: 30, shopId: 0, groupType: 5, giftType: ""),
                new(Id: 28, reward: "coin*79999|eliminateBullet*12", price: 68, shopId: 11005, groupType: 6, giftType: LocalizationPool.Get("TreasureConfig/bffb")),
                new(Id: 29, reward: "eliminateBall*12", price: 68, shopId: 0, groupType: 6, giftType: ""),
                new(Id: 30, reward: "liveBuff*15", price: 68, shopId: 0, groupType: 6, giftType: ""),
                new(Id: 31, reward: "eliminateBoom*15", price: 68, shopId: 0, groupType: 6, giftType: ""),
                new(Id: 32, reward: "eliminateArrow*15", price: 68, shopId: 0, groupType: 6, giftType: ""),
                new(Id: 33, reward: "liveBuff*15", price: 68, shopId: 0, groupType: 6, giftType: ""),
                new(Id: 34, reward: "coin*129999|eliminateBall*20", price: 128, shopId: 11006, groupType: 7, giftType: LocalizationPool.Get("TreasureConfig/bffb")),
                new(Id: 35, reward: "eliminateColored*20", price: 128, shopId: 0, groupType: 7, giftType: ""),
                new(Id: 36, reward: "coin*69999", price: 128, shopId: 0, groupType: 7, giftType: ""),
                new(Id: 37, reward: "eliminateBullet*20", price: 128, shopId: 0, groupType: 7, giftType: ""),
                new(Id: 38, reward: "eliminateArrow*25", price: 128, shopId: 0, groupType: 7, giftType: ""),
                new(Id: 39, reward: "eliminateRock*25", price: 128, shopId: 0, groupType: 7, giftType: "")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly TreasureConfig this[int Id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(Id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[TreasureConfig] Id: {Id} not found");
                return ref _data[idx];
            }
        }
        
        public TreasureConfig[] All => _data;
        
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
                _idToIdx[_data[i].Id] = i;
            }
        }
    }
}