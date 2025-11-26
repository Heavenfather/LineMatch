/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: C 场景配置.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class GiftSceneIDDB : ConfigBase
    {
        private GiftSceneID[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new GiftSceneID[]
            {
                new(Id: 100001),
                new(Id: 100002),
                new(Id: 100003),
                new(Id: 100101),
                new(Id: 110001),
                new(Id: 110002)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly GiftSceneID this[int Id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(Id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[GiftSceneID] Id: {Id} not found");
                return ref _data[idx];
            }
        }
        
        public GiftSceneID[] All => _data;
        
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