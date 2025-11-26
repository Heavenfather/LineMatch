/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 集卡配置表.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class CardprobabilityDB : ConfigBase
    {
        private Cardprobability[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new Cardprobability[]
            {
                new(Id: 1, oneStarPro: 0.3435f, twoStarPro: 0.2623f, threeStarPro: 0.1756f, fourStarPro: 0.1062f, fiveStarPro: 0.0625f, goldCardPro: 0.05f)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly Cardprobability this[int Id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(Id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[Cardprobability] Id: {Id} not found");
                return ref _data[idx];
            }
        }
        
        public Cardprobability[] All => _data;
        
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
                _idToIdx[_data[i].Id] = i;
            }
        }
    }
}