/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: J 金币关配置表.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class CoinLevelDB : ConfigBase
    {
        private CoinLevel[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new CoinLevel[]
            {
                new(id: 20, coinMax: 1000),
                new(id: 40, coinMax: 1000),
                new(id: 60, coinMax: 1000),
                new(id: 80, coinMax: 1000),
                new(id: 100, coinMax: 1000),
                new(id: 120, coinMax: 1000),
                new(id: 140, coinMax: 1000),
                new(id: 160, coinMax: 1000),
                new(id: 180, coinMax: 1000),
                new(id: 200, coinMax: 1000),
                new(id: 230, coinMax: 1000),
                new(id: 260, coinMax: 1000),
                new(id: 300, coinMax: 1000),
                new(id: 350, coinMax: 1000),
                new(id: 400, coinMax: 1000),
                new(id: 450, coinMax: 1000),
                new(id: 500, coinMax: 1000),
                new(id: 550, coinMax: 1000),
                new(id: 600, coinMax: 1000),
                new(id: 650, coinMax: 1000),
                new(id: 700, coinMax: 1000),
                new(id: 750, coinMax: 1000),
                new(id: 800, coinMax: 1000),
                new(id: 850, coinMax: 1000),
                new(id: 900, coinMax: 1000),
                new(id: 950, coinMax: 1000),
                new(id: 1000, coinMax: 1000)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly CoinLevel this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[CoinLevel] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public CoinLevel[] All => _data;
        
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