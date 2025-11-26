/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 模块开关配置.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class DummyComparisonDB : ConfigBase
    {
        private DummyComparison[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new DummyComparison[]
            {
                new(activity_id: 2001),
                new(activity_id: 2002),
                new(activity_id: 10001)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly DummyComparison this[int activity_id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(activity_id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[DummyComparison] activity_id: {activity_id} not found");
                return ref _data[idx];
            }
        }
        
        public DummyComparison[] All => _data;
        
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
                _idToIdx[_data[i].activity_id] = i;
            }
        }
    }
}