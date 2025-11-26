/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 每日任务.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class DailyTaskDB : ConfigBase
    {
        private DailyTask[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new DailyTask[]
            {
                new(id: 1, tags: new List<int>() { 1, 2, 3, 4, 5 }, weight: new List<float>() { 100.0f, 100.0f, 100.0f, 100.0f, 100.0f }, taskType: 1),
                new(id: 2, tags: new List<int>() { 6, 7, 8, 9, 10 }, weight: new List<float>() { 100.0f, 100.0f, 100.0f, 100.0f, 100.0f }, taskType: 2),
                new(id: 3, tags: new List<int>() { 11, 12, 13, 14, 15 }, weight: new List<float>() { 100.0f, 100.0f, 100.0f, 100.0f, 100.0f }, taskType: 3),
                new(id: 4, tags: new List<int>() { 16, 17, 18, 19, 20 }, weight: new List<float>() { 100.0f, 100.0f, 100.0f, 100.0f, 100.0f }, taskType: 4),
                new(id: 5, tags: new List<int>() { 21, 22, 23, 24, 25 }, weight: new List<float>() { 100.0f, 100.0f, 100.0f, 100.0f, 100.0f }, taskType: 5),
                new(id: 6, tags: new List<int>() { 26, 27, 28, 29, 30 }, weight: new List<float>() { 100.0f, 100.0f, 100.0f, 100.0f, 100.0f }, taskType: 6),
                new(id: 7, tags: new List<int>() { 31, 32, 33, 34, 35 }, weight: new List<float>() { 100.0f, 100.0f, 100.0f, 100.0f, 100.0f }, taskType: 7),
                new(id: 8, tags: new List<int>() { 36, 37, 38, 39, 40, 41, 42, 43, 44 }, weight: new List<float>() { 100.0f, 12.5f, 12.5f, 12.5f, 12.5f, 12.5f, 12.5f, 12.5f, 12.5f }, taskType: 8),
                new(id: 9, tags: new List<int>() { 45, 46, 47, 48, 49, 50 }, weight: new List<float>() { 100.0f, 20.0f, 20.0f, 20.0f, 20.0f, 20.0f }, taskType: 9)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly DailyTask this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[DailyTask] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public DailyTask[] All => _data;
        
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