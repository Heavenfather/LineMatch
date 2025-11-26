/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 消除目标任务等配置.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class ObjectiveRewardDB : ConfigBase
    {
        private ObjectiveReward[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new ObjectiveReward[]
            {
                new(id: 1, Num1: 50, Num2: 10, Num3: 10, Num4: 10, Num5: 5, reward1: "live*5", reward2: "eliminateBoom*1"),
                new(id: 2, Num1: 100, Num2: 20, Num3: 20, Num4: 20, Num5: 10, reward1: "coin*50", reward2: "BrownPack*1"),
                new(id: 3, Num1: 200, Num2: 40, Num3: 40, Num4: 40, Num5: 20, reward1: "eliminateRockBuff*10", reward2: "eliminateRockBuff*10"),
                new(id: 4, Num1: 800, Num2: 160, Num3: 160, Num4: 160, Num5: 80, reward1: "doubleCollectBuff*15", reward2: "doubleCollectBuff*15"),
                new(id: 5, Num1: 200, Num2: 40, Num3: 40, Num4: 40, Num5: 20, reward1: "liveBuff*15", reward2: "liveBuff*15"),
                new(id: 6, Num1: 600, Num2: 120, Num3: 120, Num4: 120, Num5: 60, reward1: "BrownPack*1", reward2: "eliminateRockBuff*10"),
                new(id: 7, Num1: 1200, Num2: 240, Num3: 240, Num4: 240, Num5: 120, reward1: "eliminateRock*1", reward2: "GreenPack*1"),
                new(id: 8, Num1: 400, Num2: 80, Num3: 80, Num4: 80, Num5: 40, reward1: "doubleCollectBuff*20", reward2: "doubleCollectBuff*20"),
                new(id: 9, Num1: 800, Num2: 160, Num3: 160, Num4: 160, Num5: 80, reward1: "eliminateArrow*1|eliminateBoom*1|GreenPack*1", reward2: "eliminateArrow*1|eliminateBullet*1|GreenPack*1"),
                new(id: 10, Num1: 1000, Num2: 200, Num3: 200, Num4: 200, Num5: 100, reward1: "coin*100", reward2: "coin*100"),
                new(id: 11, Num1: 1600, Num2: 320, Num3: 320, Num4: 320, Num5: 160, reward1: "eliminateBoomBuff*10", reward2: "eliminateBoomBuff*10"),
                new(id: 12, Num1: 800, Num2: 160, Num3: 160, Num4: 160, Num5: 80, reward1: "doubleCollectBuff*30", reward2: "doubleCollectBuff*30"),
                new(id: 13, Num1: 2000, Num2: 400, Num3: 400, Num4: 400, Num5: 200, reward1: "eliminateRockBuff*15", reward2: "eliminateRockBuff*15"),
                new(id: 14, Num1: 800, Num2: 160, Num3: 160, Num4: 160, Num5: 80, reward1: "liveBuff*20", reward2: "liveBuff*20"),
                new(id: 15, Num1: 2200, Num2: 440, Num3: 440, Num4: 440, Num5: 220, reward1: "eliminateDice*1|eliminateHammer*1|eliminateArrow*1|eliminateBullet*1|GreenPack*1", reward2: "eliminateDice*1|eliminateHammer*1|eliminateArrow*1|eliminateBullet*1|GreenPack*1"),
                new(id: 16, Num1: 1200, Num2: 240, Num3: 240, Num4: 240, Num5: 120, reward1: "eliminateBoom*1", reward2: "eliminateBoom*1"),
                new(id: 17, Num1: 4000, Num2: 800, Num3: 800, Num4: 800, Num5: 400, reward1: "doubleCollectBuff*30", reward2: "doubleCollectBuff*30"),
                new(id: 18, Num1: 1600, Num2: 320, Num3: 320, Num4: 320, Num5: 160, reward1: "eliminateHammer*3", reward2: "eliminateHammer*3"),
                new(id: 19, Num1: 4400, Num2: 880, Num3: 880, Num4: 880, Num5: 440, reward1: "coin*200", reward2: "coin*200"),
                new(id: 20, Num1: 8000, Num2: 1600, Num3: 1600, Num4: 1600, Num5: 800, reward1: "doubleCollectBuff*30", reward2: "doubleCollectBuff*30"),
                new(id: 21, Num1: 4000, Num2: 800, Num3: 800, Num4: 800, Num5: 400, reward1: "eliminateArrow*3", reward2: "eliminateArrow*3"),
                new(id: 22, Num1: 6000, Num2: 1200, Num3: 1200, Num4: 1200, Num5: 600, reward1: "BluePack*1", reward2: "BluePack*1"),
                new(id: 23, Num1: 8000, Num2: 1600, Num3: 1600, Num4: 1600, Num5: 800, reward1: "eliminateBoomBuff*15", reward2: "eliminateBoomBuff*15"),
                new(id: 24, Num1: 10000, Num2: 2000, Num3: 2000, Num4: 2000, Num5: 1000, reward1: "eliminateDice*2|eliminateHammer*2|eliminateArrow*2|eliminateBullet*2|liveBuff*30|PurplePack*1", reward2: "eliminateDice*2|eliminateHammer*2|eliminateArrow*2|eliminateBullet*2|liveBuff*30|PurplePack*1"),
                new(id: 25, Num1: 12000, Num2: 2400, Num3: 2400, Num4: 2400, Num5: 1200, reward1: "coin*1000", reward2: "coin*1000")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly ObjectiveReward this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[ObjectiveReward] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public ObjectiveReward[] All => _data;
        
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