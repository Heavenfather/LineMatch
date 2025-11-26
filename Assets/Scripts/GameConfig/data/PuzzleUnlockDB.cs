/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 寻宝关卡奖励.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class PuzzleUnlockDB : ConfigBase
    {
        private PuzzleUnlock[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new PuzzleUnlock[]
            {
                new(mapId: 1001, unlockLevel: 10),
                new(mapId: 1002, unlockLevel: 20),
                new(mapId: 1003, unlockLevel: 40),
                new(mapId: 1004, unlockLevel: 70),
                new(mapId: 1005, unlockLevel: 80),
                new(mapId: 1006, unlockLevel: 90),
                new(mapId: 1007, unlockLevel: 100),
                new(mapId: 1008, unlockLevel: 200),
                new(mapId: 1009, unlockLevel: 250),
                new(mapId: 1010, unlockLevel: 300),
                new(mapId: 1011, unlockLevel: 380),
                new(mapId: 1012, unlockLevel: 460),
                new(mapId: 1013, unlockLevel: 550),
                new(mapId: 1014, unlockLevel: 650)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly PuzzleUnlock this[int mapId]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(mapId, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[PuzzleUnlock] mapId: {mapId} not found");
                return ref _data[idx];
            }
        }
        
        public PuzzleUnlock[] All => _data;
        
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
                _idToIdx[_data[i].mapId] = i;
            }
        }
    }
}