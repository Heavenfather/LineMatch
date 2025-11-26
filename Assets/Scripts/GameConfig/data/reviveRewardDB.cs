/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class reviveRewardDB : ConfigBase
    {
        private reviveReward[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new reviveReward[]
            {
                new(id: 1, reviveNum: 1, needCoin: 900, addSteps: 5, addItem: "", isUseItem: false),
                new(id: 2, reviveNum: 2, needCoin: 1900, addSteps: 5, addItem: "eliminateRock*1", isUseItem: true),
                new(id: 3, reviveNum: 3, needCoin: 2900, addSteps: 5, addItem: "eliminateBoom*1|eliminateRock*1", isUseItem: true),
                new(id: 4, reviveNum: 4, needCoin: 3900, addSteps: 5, addItem: "eliminateBoom*1|eliminateBall*1", isUseItem: true),
                new(id: 5, reviveNum: 5, needCoin: 4900, addSteps: 5, addItem: "eliminateBoom*1|eliminateBall*1", isUseItem: true)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly reviveReward this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[reviveReward] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public reviveReward[] All => _data;
        
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