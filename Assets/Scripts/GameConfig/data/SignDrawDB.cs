/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 抽签配置.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class SignDrawDB : ConfigBase
    {
        private SignDraw[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new SignDraw[]
            {
                new(id: 1, tag: 4, num: 5, desc: LocalizationPool.Get("SignDraw/06f2")),
                new(id: 2, tag: 2, num: 5, desc: LocalizationPool.Get("SignDraw/4c6b")),
                new(id: 3, tag: 1, num: 6, desc: LocalizationPool.Get("SignDraw/57a8")),
                new(id: 4, tag: 3, num: 4, desc: LocalizationPool.Get("SignDraw/254e")),
                new(id: 5, tag: 2, num: 5, desc: LocalizationPool.Get("SignDraw/9fd9")),
                new(id: 6, tag: 5, num: 4, desc: LocalizationPool.Get("SignDraw/4440")),
                new(id: 7, tag: 1, num: 6, desc: LocalizationPool.Get("SignDraw/36e7")),
                new(id: 8, tag: 1, num: 6, desc: LocalizationPool.Get("SignDraw/2587")),
                new(id: 9, tag: 1, num: 6, desc: LocalizationPool.Get("SignDraw/b470")),
                new(id: 10, tag: 1, num: 6, desc: LocalizationPool.Get("SignDraw/1d06")),
                new(id: 11, tag: 2, num: 5, desc: LocalizationPool.Get("SignDraw/439e")),
                new(id: 12, tag: 2, num: 5, desc: LocalizationPool.Get("SignDraw/1b1e")),
                new(id: 13, tag: 2, num: 5, desc: LocalizationPool.Get("SignDraw/953a")),
                new(id: 14, tag: 2, num: 5, desc: LocalizationPool.Get("SignDraw/3459")),
                new(id: 15, tag: 3, num: 4, desc: LocalizationPool.Get("SignDraw/7211")),
                new(id: 16, tag: 3, num: 4, desc: LocalizationPool.Get("SignDraw/b4af")),
                new(id: 17, tag: 3, num: 4, desc: LocalizationPool.Get("SignDraw/9eb2")),
                new(id: 18, tag: 3, num: 4, desc: LocalizationPool.Get("SignDraw/afe5")),
                new(id: 19, tag: 4, num: 5, desc: LocalizationPool.Get("SignDraw/8fe8")),
                new(id: 20, tag: 4, num: 5, desc: LocalizationPool.Get("SignDraw/77a6")),
                new(id: 21, tag: 4, num: 5, desc: LocalizationPool.Get("SignDraw/9cf4")),
                new(id: 22, tag: 4, num: 5, desc: LocalizationPool.Get("SignDraw/fbd4")),
                new(id: 23, tag: 5, num: 4, desc: LocalizationPool.Get("SignDraw/e056")),
                new(id: 24, tag: 5, num: 4, desc: LocalizationPool.Get("SignDraw/f1ef")),
                new(id: 25, tag: 5, num: 4, desc: LocalizationPool.Get("SignDraw/3eac")),
                new(id: 26, tag: 5, num: 4, desc: LocalizationPool.Get("SignDraw/b1b2"))
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly SignDraw this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[SignDraw] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public SignDraw[] All => _data;
        
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