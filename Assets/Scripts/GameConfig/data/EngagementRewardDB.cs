/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 每日任务.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class EngagementRewardDB : ConfigBase
    {
        private EngagementReward[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new EngagementReward[]
            {
                new(id: 1, num: 100, reward: new List<string>() { "live*30" }, taskType: 1, desc: LocalizationPool.Get("EngagementReward/8f90")),
                new(id: 2, num: 250, reward: new List<string>() { "eliminateHammer*2" }, taskType: 1, desc: LocalizationPool.Get("EngagementReward/bb1a")),
                new(id: 3, num: 585, reward: new List<string>() { "live*100" }, taskType: 1, desc: LocalizationPool.Get("EngagementReward/2fdd")),
                new(id: 4, num: 825, reward: new List<string>() { "eliminateArrow*1", "eliminateBullet*1" }, taskType: 1, desc: LocalizationPool.Get("EngagementReward/0937")),
                new(id: 5, num: 1055, reward: new List<string>() { "live*175", "eliminateColored*2", "BluePack*1" }, taskType: 1, desc: LocalizationPool.Get("EngagementReward/0068")),
                new(id: 6, num: 30, reward: new List<string>() { "live*10" }, taskType: 8, desc: LocalizationPool.Get("EngagementReward/9f7f")),
                new(id: 7, num: 50, reward: new List<string>() { "live*15" }, taskType: 8, desc: LocalizationPool.Get("EngagementReward/0241")),
                new(id: 8, num: 80, reward: new List<string>() { "BrownPack*1" }, taskType: 8, desc: LocalizationPool.Get("EngagementReward/762d"))
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly EngagementReward this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[EngagementReward] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public EngagementReward[] All => _data;
        
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