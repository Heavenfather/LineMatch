/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class SpecialPieceScoreDB : ConfigBase
    {
        private SpecialPieceScore[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new SpecialPieceScore[]
            {
                new(id: 1, rockPiece: 2.0f, bombPiece: 2.0f, colorballPiece: 2.0f, randBPiece: 2.5f, candrbPiece: 3.0f, candCPiece: 5.0f)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly SpecialPieceScore this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[SpecialPieceScore] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public SpecialPieceScore[] All => _data;
        
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