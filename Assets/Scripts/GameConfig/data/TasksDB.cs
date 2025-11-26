/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 每日任务.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class TasksDB : ConfigBase
    {
        private Tasks[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new Tasks[]
            {
                new(id: 1, tag: 1, num: 1, reward: new List<string>() { "dailyNewEg*10", "live*5" }, desc: LocalizationPool.Get("Tasks/55bd")),
                new(id: 2, tag: 23, num: 1, reward: new List<string>() { "dailyNewEg*10", "live*5" }, desc: LocalizationPool.Get("Tasks/8d88")),
                new(id: 3, tag: 3, num: 40, reward: new List<string>() { "dailyNewEg*20", "eliminateHammer*1" }, desc: LocalizationPool.Get("Tasks/619f")),
                new(id: 4, tag: 4, num: 1100, reward: new List<string>() { "dailyNewEg*20", "live*15" }, desc: LocalizationPool.Get("Tasks/5b9f")),
                new(id: 5, tag: 5, num: 5, reward: new List<string>() { "dailyNewEg*20", "live*15" }, desc: LocalizationPool.Get("Tasks/779e")),
                new(id: 6, tag: 1, num: 2, reward: new List<string>() { "dailyNewEg*10", "live*5" }, desc: LocalizationPool.Get("Tasks/8bbf")),
                new(id: 7, tag: 3, num: 70, reward: new List<string>() { "dailyNewEg*15", "live*10" }, desc: LocalizationPool.Get("Tasks/b9e1")),
                new(id: 8, tag: 4, num: 2200, reward: new List<string>() { "dailyNewEg*20", "live*20" }, desc: LocalizationPool.Get("Tasks/8027")),
                new(id: 9, tag: 5, num: 10, reward: new List<string>() { "dailyNewEg*25", "live*10" }, desc: LocalizationPool.Get("Tasks/893e")),
                new(id: 10, tag: 6, num: 2, reward: new List<string>() { "dailyNewEg*30", "coin*200" }, desc: LocalizationPool.Get("Tasks/06b2")),
                new(id: 11, tag: 1, num: 3, reward: new List<string>() { "dailyNewEg*10", "live*10" }, desc: LocalizationPool.Get("Tasks/46af")),
                new(id: 12, tag: 5, num: 15, reward: new List<string>() { "dailyNewEg*20", "live*10" }, desc: LocalizationPool.Get("Tasks/2630")),
                new(id: 13, tag: 3, num: 100, reward: new List<string>() { "dailyNewEg*20", "eliminateHammer*1" }, desc: LocalizationPool.Get("Tasks/a74d")),
                new(id: 14, tag: 4, num: 3300, reward: new List<string>() { "dailyNewEg*20", "live*30" }, desc: LocalizationPool.Get("Tasks/9a7c")),
                new(id: 15, tag: 8, num: 1, reward: new List<string>() { "dailyNewEg*80", "coin*300" }, desc: LocalizationPool.Get("Tasks/c4ab")),
                new(id: 16, tag: 1, num: 4, reward: new List<string>() { "dailyNewEg*20", "live*10" }, desc: LocalizationPool.Get("Tasks/2cd8")),
                new(id: 17, tag: 3, num: 130, reward: new List<string>() { "dailyNewEg*20", "live*10" }, desc: LocalizationPool.Get("Tasks/cae0")),
                new(id: 18, tag: 9, num: 305, reward: new List<string>() { "dailyNewEg*20", "live*20" }, desc: LocalizationPool.Get("Tasks/f9ef")),
                new(id: 19, tag: 4, num: 4400, reward: new List<string>() { "dailyNewEg*40", "live*30" }, desc: LocalizationPool.Get("Tasks/fe4f")),
                new(id: 20, tag: 10, num: 1000, reward: new List<string>() { "dailyNewEg*50", "BluePack*1" }, desc: LocalizationPool.Get("Tasks/356b")),
                new(id: 21, tag: 1, num: 5, reward: new List<string>() { "dailyNewEg*20", "live*10" }, desc: LocalizationPool.Get("Tasks/b20b")),
                new(id: 22, tag: 11, num: 5, reward: new List<string>() { "dailyNewEg*30", "live*20" }, desc: LocalizationPool.Get("Tasks/7269")),
                new(id: 23, tag: 3, num: 160, reward: new List<string>() { "dailyNewEg*40", "live*20" }, desc: LocalizationPool.Get("Tasks/574c")),
                new(id: 24, tag: 4, num: 5500, reward: new List<string>() { "dailyNewEg*40", "live*30" }, desc: LocalizationPool.Get("Tasks/04f8")),
                new(id: 25, tag: 9, num: 425, reward: new List<string>() { "dailyNewEg*80", "BrownPack*1" }, desc: LocalizationPool.Get("Tasks/c25f")),
                new(id: 26, tag: 1, num: 6, reward: new List<string>() { "dailyNewEg*20", "live*10" }, desc: LocalizationPool.Get("Tasks/fcb8")),
                new(id: 27, tag: 11, num: 10, reward: new List<string>() { "dailyNewEg*40", "live*20" }, desc: LocalizationPool.Get("Tasks/a4e9")),
                new(id: 28, tag: 3, num: 190, reward: new List<string>() { "dailyNewEg*50", "live*20" }, desc: LocalizationPool.Get("Tasks/1189")),
                new(id: 29, tag: 4, num: 6600, reward: new List<string>() { "dailyNewEg*50", "eliminateArrow*1" }, desc: LocalizationPool.Get("Tasks/05cb")),
                new(id: 30, tag: 9, num: 25, reward: new List<string>() { "dailyNewEg*50", "BrownPack*1" }, desc: LocalizationPool.Get("Tasks/a6ab")),
                new(id: 31, tag: 1, num: 7, reward: new List<string>() { "dailyNewEg*30", "live*15" }, desc: LocalizationPool.Get("Tasks/01cf")),
                new(id: 32, tag: 3, num: 210, reward: new List<string>() { "dailyNewEg*45", "live*20" }, desc: LocalizationPool.Get("Tasks/00c7")),
                new(id: 33, tag: 6, num: 5, reward: new List<string>() { "dailyNewEg*60", "coin*300" }, desc: LocalizationPool.Get("Tasks/39c0")),
                new(id: 34, tag: 5, num: 30, reward: new List<string>() { "dailyNewEg*60", "live*20" }, desc: LocalizationPool.Get("Tasks/5542")),
                new(id: 35, tag: 10, num: 2000, reward: new List<string>() { "dailyNewEg*60", "PurplePack*1" }, desc: LocalizationPool.Get("Tasks/7a55")),
                new(id: 36, tag: 2, num: 1, reward: new List<string>() { "dailyEg*10", "live*10" }, desc: LocalizationPool.Get("Tasks/4074")),
                new(id: 37, tag: 23, num: 1, reward: new List<string>() { "dailyEg*10", "live*10" }, desc: LocalizationPool.Get("Tasks/8d88")),
                new(id: 38, tag: 11, num: 1, reward: new List<string>() { "dailyEg*10", "live*10" }, desc: LocalizationPool.Get("Tasks/c554")),
                new(id: 39, tag: 12, num: 30, reward: new List<string>() { "dailyEg*10", "live*10" }, desc: LocalizationPool.Get("Tasks/8eb2")),
                new(id: 40, tag: 13, num: 2, reward: new List<string>() { "dailyEg*10", "live*10" }, desc: LocalizationPool.Get("Tasks/d54b")),
                new(id: 41, tag: 14, num: 2, reward: new List<string>() { "dailyEg*10", "live*10" }, desc: LocalizationPool.Get("Tasks/1784")),
                new(id: 42, tag: 15, num: 1, reward: new List<string>() { "dailyEg*10", "live*10" }, desc: LocalizationPool.Get("Tasks/90f5")),
                new(id: 43, tag: 3, num: 3, reward: new List<string>() { "dailyEg*10", "live*10" }, desc: LocalizationPool.Get("Tasks/ba98")),
                new(id: 44, tag: 9, num: 20, reward: new List<string>() { "dailyEg*10", "live*10" }, desc: LocalizationPool.Get("Tasks/d489")),
                new(id: 45, tag: 24, num: 15, reward: new List<string>() { "dailyEg*20", "live*15" }, desc: LocalizationPool.Get("Tasks/83e4")),
                new(id: 46, tag: 4, num: 450, reward: new List<string>() { "dailyEg*20", "live*15" }, desc: LocalizationPool.Get("Tasks/d50d")),
                new(id: 47, tag: 5, num: 10, reward: new List<string>() { "dailyEg*20", "live*15" }, desc: LocalizationPool.Get("Tasks/bfed")),
                new(id: 48, tag: 17, num: 2, reward: new List<string>() { "dailyEg*20", "live*15" }, desc: LocalizationPool.Get("Tasks/f905")),
                new(id: 49, tag: 21, num: 1, reward: new List<string>() { "dailyEg*20", "live*15" }, desc: LocalizationPool.Get("Tasks/1091")),
                new(id: 50, tag: 9, num: 75, reward: new List<string>() { "dailyEg*20", "live*15" }, desc: LocalizationPool.Get("Tasks/7a89"))
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly Tasks this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[Tasks] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public Tasks[] All => _data;
        
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