/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 通行证.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class BattlePassLastRewardDB : ConfigBase
    {
        private BattlePassLastReward[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new BattlePassLastReward[]
            {
                new(id: 31, key: 10, reward1: "coins*50", reward2: "coins*100")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly BattlePassLastReward this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[BattlePassLastReward] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public BattlePassLastReward[] All => _data;
        
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