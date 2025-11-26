/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 签到奖励.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class dailyRewardDB : ConfigBase
    {
        private dailyReward[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new dailyReward[]
            {
                new(id: 1, day1: "liveBuff*15", day2: "eliminateDiceBuff*15", day3: "coin*100", day4: "eliminateHammerBuff*15", day5: "coin*200", day6: "eliminateRockBuff*15", day7: "liveBuff*15|eliminateDiceBuff*15|coin*300")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly dailyReward this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[dailyReward] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public dailyReward[] All => _data;
        
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
                _idToIdx[_data[i].id] = i;
            }
        }
    }
}