/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 广告配置.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class AdRewardDB : ConfigBase
    {
        private AdReward[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new AdReward[]
            {
                new(Id: 1, getreward_num: 5, ad_reward: "coin*50"),
                new(Id: 2, getreward_num: 1, ad_reward: "puzzleLoupe*1"),
                new(Id: 3, getreward_num: 1, ad_reward: "puzzlePosition*1"),
                new(Id: 4, getreward_num: 1, ad_reward: "puzzleVacuum*1"),
                new(Id: 5, getreward_num: 1, ad_reward: "eliminateDice*1"),
                new(Id: 6, getreward_num: 1, ad_reward: "eliminateHammer*1"),
                new(Id: 7, getreward_num: 1, ad_reward: "eliminateArrow*1"),
                new(Id: 8, getreward_num: 1, ad_reward: "eliminateBullet*1"),
                new(Id: 9, getreward_num: 1, ad_reward: "eliminateColored*1"),
                new(Id: 10, getreward_num: 1, ad_reward: "eliminateDye*1"),
                new(Id: 11, getreward_num: 1, ad_reward: "")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly AdReward this[int Id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(Id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[AdReward] Id: {Id} not found");
                return ref _data[idx];
            }
        }
        
        public AdReward[] All => _data;
        
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
                _idToIdx[_data[i].Id] = i;
            }
        }
    }
}