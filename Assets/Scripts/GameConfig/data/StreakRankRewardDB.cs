/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 排行榜奖励.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class StreakRankRewardDB : ConfigBase
    {
        private StreakRankReward[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new StreakRankReward[]
            {
                new(id: 1, rankBegin: 1, rankEnd: "1", reward: "Frame23*1|coin*3000|eliminateRock*3|eliminateBoom*3|eliminateBall*1|eliminateRockBuff*60|eliminateBoomBuff*60"),
                new(id: 2, rankBegin: 2, rankEnd: "2", reward: "Frame23*1|coin*2000|eliminateRock*2|eliminateBoom*2|eliminateRockBuff*30|eliminateBoomBuff*30"),
                new(id: 3, rankBegin: 3, rankEnd: "3", reward: "Frame23*1|coin*1000|eliminateRock*1|eliminateBoom*1|eliminateRockBuff*15|eliminateBoomBuff*15"),
                new(id: 4, rankBegin: 4, rankEnd: "10", reward: "GreenPack*1|eliminateRock*1|eliminateBoom*1")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly StreakRankReward this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[StreakRankReward] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public StreakRankReward[] All => _data;
        
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