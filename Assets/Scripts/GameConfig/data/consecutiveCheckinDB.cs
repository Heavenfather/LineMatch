/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 签到奖励.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class consecutiveCheckinDB : ConfigBase
    {
        private consecutiveCheckin[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new consecutiveCheckin[]
            {
                new(id: 1, dayNum: "8", reward: "liveBuff*15|coin*100"),
                new(id: 2, dayNum: "15", reward: "liveBuff*15|eliminateDiceBuff*15|coin*200"),
                new(id: 3, dayNum: "30", reward: "liveBuff*15|eliminateDiceBuff*15|coin*500")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly consecutiveCheckin this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[consecutiveCheckin] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public consecutiveCheckin[] All => _data;
        
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