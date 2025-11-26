/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 关卡固定调控值表.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class LevelStrategyDB : ConfigBase
    {
        private LevelStrategy[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new LevelStrategy[]
            {
                new(id: 1, value: 0),
                new(id: 2, value: 0),
                new(id: 3, value: 0),
                new(id: 4, value: 0),
                new(id: 5, value: 0),
                new(id: 6, value: 0),
                new(id: 7, value: 0),
                new(id: 8, value: 0),
                new(id: 9, value: 0),
                new(id: 10, value: 0),
                new(id: 11, value: 0),
                new(id: 12, value: 0),
                new(id: 13, value: 0),
                new(id: 14, value: 0),
                new(id: 15, value: 0),
                new(id: 16, value: 0),
                new(id: 17, value: 0)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly LevelStrategy this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[LevelStrategy] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public LevelStrategy[] All => _data;
        
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