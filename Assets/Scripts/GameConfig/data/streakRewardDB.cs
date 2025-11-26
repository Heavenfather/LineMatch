/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class streakRewardDB : ConfigBase
    {
        private streakReward[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new streakReward[]
            {
                new(id: 1, streakNum: "1", reward: "eliminateRock*1", isUseItem: true),
                new(id: 2, streakNum: "2", reward: "eliminateRock*1|eliminateBoom*1", isUseItem: true),
                new(id: 3, streakNum: "3", reward: "eliminateRock*1|eliminateBoom*1|eliminateBall*1", isUseItem: true)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly streakReward this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[streakReward] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public streakReward[] All => _data;
        
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