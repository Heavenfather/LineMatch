/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 问卷调查.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class FeedbackDB : ConfigBase
    {
        private Feedback[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new Feedback[]
            {
                new(id: 1, num: 0, desc: LocalizationPool.Get("Feedback/2d30")),
                new(id: 2, num: 0, desc: LocalizationPool.Get("Feedback/192b")),
                new(id: 3, num: 0, desc: LocalizationPool.Get("Feedback/1b3e")),
                new(id: 4, num: 0, desc: LocalizationPool.Get("Feedback/27eb")),
                new(id: 5, num: 0, desc: LocalizationPool.Get("Feedback/3b04")),
                new(id: 6, num: 0, desc: LocalizationPool.Get("Feedback/abf9")),
                new(id: 7, num: 1, desc: LocalizationPool.Get("Feedback/fad7")),
                new(id: 8, num: 1, desc: LocalizationPool.Get("Feedback/2f44")),
                new(id: 9, num: 1, desc: LocalizationPool.Get("Feedback/44ce")),
                new(id: 10, num: 1, desc: LocalizationPool.Get("Feedback/3f52")),
                new(id: 11, num: 1, desc: LocalizationPool.Get("Feedback/0fd9")),
                new(id: 12, num: 1, desc: LocalizationPool.Get("Feedback/1310"))
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly Feedback this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[Feedback] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public Feedback[] All => _data;
        
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