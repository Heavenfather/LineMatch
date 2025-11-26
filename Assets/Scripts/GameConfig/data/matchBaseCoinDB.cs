/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class matchBaseCoinDB : ConfigBase
    {
        private matchBaseCoin[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new matchBaseCoin[]
            {
                new(id: 1, ease: 12, hard: 18, extreme: 30, starMultiplier1: 0.49f, starMultiplier2: 0.81f, starMultiplier3: 1.21f)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly matchBaseCoin this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[matchBaseCoin] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public matchBaseCoin[] All => _data;
        
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