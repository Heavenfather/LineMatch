/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class ObjectiveTypeDB : ConfigBase
    {
        private ObjectiveType[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new ObjectiveType[]
            {
                new(id: 1, Type: TargetTaskType.Normal),
                new(id: 2, Type: TargetTaskType.Rocket),
                new(id: 3, Type: TargetTaskType.Bomb),
                new(id: 4, Type: TargetTaskType.Square),
                new(id: 5, Type: TargetTaskType.LightBall)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly ObjectiveType this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[ObjectiveType] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public ObjectiveType[] All => _data;
        
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