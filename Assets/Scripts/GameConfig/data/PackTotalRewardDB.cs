/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 集卡配置表.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class PackTotalRewardDB : ConfigBase
    {
        private PackTotalReward[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new PackTotalReward[]
            {
                new(levelID: 1001, rechargeLimit: 0, reward: "eliminateRock*5|eliminateBoom*5|eliminateBall*5|coin*5000", IsMedalReward: true, StarNumber: 0),
                new(levelID: 1002, rechargeLimit: 100, reward: "eliminateRock*10|eliminateBoom*10|eliminateBall*10|coin*10000", IsMedalReward: true, StarNumber: 0),
                new(levelID: 1003, rechargeLimit: 500, reward: "eliminateRock*15|eliminateBoom*15|eliminateBall*15|coin*15000", IsMedalReward: true, StarNumber: 0),
                new(levelID: 1, StarNumber: 100, reward: "GreenPack*1|eliminateRock*1|eliminateBoom*1", rechargeLimit: 0, IsMedalReward: false),
                new(levelID: 2, StarNumber: 350, reward: "GreenPack*1|BluePack*1|eliminateRock*1|eliminateBoom*1|eliminateHammer*1", rechargeLimit: 0, IsMedalReward: false),
                new(levelID: 3, StarNumber: 650, reward: "GreenPack*1|BluePack*1|RedPack*1|eliminateRock*1|eliminateBoom*1|eliminateHammer*1|eliminateDice*1", rechargeLimit: 0, IsMedalReward: false)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly PackTotalReward this[int levelID]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(levelID, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[PackTotalReward] levelID: {levelID} not found");
                return ref _data[idx];
            }
        }
        
        public PackTotalReward[] All => _data;
        
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
                _idToIdx[_data[i].levelID] = i;
            }
        }
    }
}