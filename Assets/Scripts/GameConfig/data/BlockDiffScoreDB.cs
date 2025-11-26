/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class BlockDiffScoreDB : ConfigBase
    {
        private BlockDiffScore[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new BlockDiffScore[]
            {
                new(id: 1, BlockLevel: 1, BlockLevelScore: 1.0f),
                new(id: 2, BlockLevel: 2, BlockLevelScore: 1.1f),
                new(id: 3, BlockLevel: 3, BlockLevelScore: 1.2f),
                new(id: 4, BlockLevel: 4, BlockLevelScore: 1.3f),
                new(id: 5, BlockLevel: 5, BlockLevelScore: 1.4f),
                new(id: 6, BlockLevel: 6, BlockLevelScore: 1.5f),
                new(id: 7, BlockLevel: 7, BlockLevelScore: 1.8f)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly BlockDiffScore this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[BlockDiffScore] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public BlockDiffScore[] All => _data;
        
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