/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 问卷调查.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class QuestionnaireDB : ConfigBase
    {
        private Questionnaire[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new Questionnaire[]
            {
                new(id: 1, tag: 0, desc: LocalizationPool.Get("Questionnaire/38fa"), title: LocalizationPool.Get("Questionnaire/12d7"), linkID: ""),
                new(id: 2, tag: 0, desc: LocalizationPool.Get("Questionnaire/38fa"), title: LocalizationPool.Get("Questionnaire/fe03"), linkID: "4|4|3|3|3"),
                new(id: 3, tag: 1, desc: LocalizationPool.Get("Questionnaire/c673"), title: LocalizationPool.Get("Questionnaire/48e7"), linkID: ""),
                new(id: 4, tag: 0, desc: LocalizationPool.Get("Questionnaire/38fa"), title: LocalizationPool.Get("Questionnaire/5bb0"), linkID: "6|6|5|5|5"),
                new(id: 5, tag: 1, desc: LocalizationPool.Get("Questionnaire/d0ee"), title: LocalizationPool.Get("Questionnaire/7aca"), linkID: ""),
                new(id: 6, tag: 1, desc: LocalizationPool.Get("Questionnaire/96b1"), title: LocalizationPool.Get("Questionnaire/b42d"), linkID: ""),
                new(id: 7, tag: 1, desc: LocalizationPool.Get("Questionnaire/f767"), title: LocalizationPool.Get("Questionnaire/1bf3"), linkID: "")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly Questionnaire this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[Questionnaire] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public Questionnaire[] All => _data;
        
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