/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 月卡配置.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class MonthCardCfgDB : ConfigBase
    {
        private MonthCardCfg[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new MonthCardCfg[]
            {
                new(id: 1001, dailyReward: "coin*1000|live*5|eliminateHammer*1", immediatelyReward: "coin*18000|liveBuff*10", name: LocalizationPool.Get("MonthCardCfg/0f62"), price: 30, discount: 200),
                new(id: 1002, dailyReward: "coin*3000|live*5|eliminateHammer*3", immediatelyReward: "coin*45000|liveBuff*30", name: LocalizationPool.Get("MonthCardCfg/edd3"), price: 68, discount: 500)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly MonthCardCfg this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[MonthCardCfg] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public MonthCardCfg[] All => _data;
        
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