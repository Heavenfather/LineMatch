/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class LevelDiffScoreDB : ConfigBase
    {
        private LevelDiffScore[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new LevelDiffScore[]
            {
                new(id: 1, easeParam: 1.0f, hardParam: 1.2f, extremeParam: 1.5f)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly LevelDiffScore this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[LevelDiffScore] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public LevelDiffScore[] All => _data;
        
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