/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 集卡配置表.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class HaveOrNotProbabilityDB : ConfigBase
    {
        private HaveOrNotProbability[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new HaveOrNotProbability[]
            {
                new(Id: 1, notHaveCardPro: 0.01f, openNum: 25, addPro: 0.026f)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly HaveOrNotProbability this[int Id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(Id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[HaveOrNotProbability] Id: {Id} not found");
                return ref _data[idx];
            }
        }
        
        public HaveOrNotProbability[] All => _data;
        
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