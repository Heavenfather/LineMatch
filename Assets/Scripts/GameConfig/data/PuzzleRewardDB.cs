/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 寻宝关卡奖励.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class PuzzleRewardDB : ConfigBase
    {
        private PuzzleReward[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new PuzzleReward[]
            {
                new(id: 1, mapId: 1001, levelId: 1, openCoin: 0, openStar: 2, reward: "live*5"),
                new(id: 2, mapId: 1001, levelId: 2, openCoin: 0, openStar: 9, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 3, mapId: 1001, levelId: 3, openCoin: 700, openStar: 12, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 4, mapId: 1001, levelId: 4, openCoin: 1500, openStar: 15, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 5, mapId: 1001, levelId: 5, openCoin: 1500, openStar: 18, reward: "Head04*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 6, mapId: 1001, levelId: 6, openCoin: 1000, openStar: 21, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 7, mapId: 1002, levelId: 1, openCoin: 0, openStar: 9, reward: "BrownPack*1"),
                new(id: 8, mapId: 1002, levelId: 2, openCoin: 0, openStar: 12, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 9, mapId: 1002, levelId: 3, openCoin: 700, openStar: 15, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 10, mapId: 1002, levelId: 4, openCoin: 1500, openStar: 18, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 11, mapId: 1002, levelId: 5, openCoin: 1500, openStar: 21, reward: "Head09*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 12, mapId: 1002, levelId: 6, openCoin: 1000, openStar: 24, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 13, mapId: 1003, levelId: 1, openCoin: 0, openStar: 12, reward: "BrownPack*1"),
                new(id: 14, mapId: 1003, levelId: 2, openCoin: 0, openStar: 15, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 15, mapId: 1003, levelId: 3, openCoin: 700, openStar: 18, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 16, mapId: 1003, levelId: 4, openCoin: 1500, openStar: 21, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 17, mapId: 1003, levelId: 5, openCoin: 1500, openStar: 24, reward: "Head18*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 18, mapId: 1003, levelId: 6, openCoin: 1000, openStar: 27, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 19, mapId: 1004, levelId: 1, openCoin: 0, openStar: 15, reward: "BrownPack*1"),
                new(id: 20, mapId: 1004, levelId: 2, openCoin: 0, openStar: 18, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 21, mapId: 1004, levelId: 3, openCoin: 700, openStar: 21, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 22, mapId: 1004, levelId: 4, openCoin: 1500, openStar: 24, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 23, mapId: 1004, levelId: 5, openCoin: 1500, openStar: 27, reward: "Head12*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 24, mapId: 1004, levelId: 6, openCoin: 1000, openStar: 30, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 25, mapId: 1005, levelId: 1, openCoin: 0, openStar: 15, reward: "BrownPack*1"),
                new(id: 26, mapId: 1005, levelId: 2, openCoin: 0, openStar: 18, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 27, mapId: 1005, levelId: 3, openCoin: 700, openStar: 21, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 28, mapId: 1005, levelId: 4, openCoin: 1500, openStar: 24, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 29, mapId: 1005, levelId: 5, openCoin: 1500, openStar: 27, reward: "Head01*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 30, mapId: 1005, levelId: 6, openCoin: 1000, openStar: 30, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 31, mapId: 1006, levelId: 1, openCoin: 0, openStar: 15, reward: "BrownPack*1"),
                new(id: 32, mapId: 1006, levelId: 2, openCoin: 0, openStar: 18, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 33, mapId: 1006, levelId: 3, openCoin: 700, openStar: 21, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 34, mapId: 1006, levelId: 4, openCoin: 1500, openStar: 24, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 35, mapId: 1006, levelId: 5, openCoin: 1500, openStar: 27, reward: "Head01*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 36, mapId: 1006, levelId: 6, openCoin: 1000, openStar: 30, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 37, mapId: 1007, levelId: 1, openCoin: 0, openStar: 18, reward: "BrownPack*1"),
                new(id: 38, mapId: 1007, levelId: 2, openCoin: 0, openStar: 21, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 39, mapId: 1007, levelId: 3, openCoin: 700, openStar: 24, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 40, mapId: 1007, levelId: 4, openCoin: 1500, openStar: 27, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 41, mapId: 1007, levelId: 5, openCoin: 1500, openStar: 30, reward: "Head27*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 42, mapId: 1007, levelId: 6, openCoin: 2000, openStar: 27, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 43, mapId: 1007, levelId: 7, openCoin: 3000, openStar: 30, reward: "Head01*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 44, mapId: 1007, levelId: 8, openCoin: 1500, openStar: 33, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 45, mapId: 1008, levelId: 1, openCoin: 0, openStar: 18, reward: "BrownPack*1"),
                new(id: 46, mapId: 1008, levelId: 2, openCoin: 0, openStar: 21, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 47, mapId: 1008, levelId: 3, openCoin: 700, openStar: 24, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 48, mapId: 1008, levelId: 4, openCoin: 1500, openStar: 27, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 49, mapId: 1008, levelId: 5, openCoin: 1500, openStar: 30, reward: "Head08*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 50, mapId: 1008, levelId: 6, openCoin: 2000, openStar: 27, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 51, mapId: 1008, levelId: 7, openCoin: 3000, openStar: 30, reward: "Head01*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 52, mapId: 1008, levelId: 8, openCoin: 1500, openStar: 33, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 53, mapId: 1009, levelId: 1, openCoin: 0, openStar: 18, reward: "BrownPack*1"),
                new(id: 54, mapId: 1009, levelId: 2, openCoin: 0, openStar: 21, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 55, mapId: 1009, levelId: 3, openCoin: 700, openStar: 24, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 56, mapId: 1009, levelId: 4, openCoin: 1500, openStar: 27, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 57, mapId: 1009, levelId: 5, openCoin: 1500, openStar: 30, reward: "Head25*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 58, mapId: 1009, levelId: 6, openCoin: 2000, openStar: 27, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 59, mapId: 1009, levelId: 7, openCoin: 3000, openStar: 30, reward: "Head01*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 60, mapId: 1009, levelId: 8, openCoin: 1500, openStar: 33, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 61, mapId: 1010, levelId: 1, openCoin: 0, openStar: 18, reward: "BrownPack*1"),
                new(id: 62, mapId: 1010, levelId: 2, openCoin: 0, openStar: 21, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 63, mapId: 1010, levelId: 3, openCoin: 700, openStar: 24, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 64, mapId: 1010, levelId: 4, openCoin: 1500, openStar: 27, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 65, mapId: 1010, levelId: 5, openCoin: 1500, openStar: 30, reward: "Head17*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 66, mapId: 1010, levelId: 6, openCoin: 2000, openStar: 27, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 67, mapId: 1010, levelId: 7, openCoin: 3000, openStar: 30, reward: "Head01*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 68, mapId: 1010, levelId: 8, openCoin: 1500, openStar: 33, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 69, mapId: 1011, levelId: 1, openCoin: 0, openStar: 18, reward: "BrownPack*1"),
                new(id: 70, mapId: 1011, levelId: 2, openCoin: 0, openStar: 21, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 71, mapId: 1011, levelId: 3, openCoin: 700, openStar: 24, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 72, mapId: 1011, levelId: 4, openCoin: 1500, openStar: 27, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 73, mapId: 1011, levelId: 5, openCoin: 1500, openStar: 30, reward: "Head14*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 74, mapId: 1011, levelId: 6, openCoin: 2000, openStar: 27, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 75, mapId: 1011, levelId: 7, openCoin: 3000, openStar: 30, reward: "Head01*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 76, mapId: 1011, levelId: 8, openCoin: 1500, openStar: 33, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 77, mapId: 1012, levelId: 1, openCoin: 0, openStar: 18, reward: "BrownPack*1"),
                new(id: 78, mapId: 1012, levelId: 2, openCoin: 0, openStar: 21, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 79, mapId: 1012, levelId: 3, openCoin: 700, openStar: 24, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 80, mapId: 1012, levelId: 4, openCoin: 1500, openStar: 27, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 81, mapId: 1012, levelId: 5, openCoin: 1500, openStar: 30, reward: "Head22*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 82, mapId: 1012, levelId: 6, openCoin: 2000, openStar: 27, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 83, mapId: 1012, levelId: 7, openCoin: 3000, openStar: 30, reward: "Head01*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 84, mapId: 1012, levelId: 8, openCoin: 1500, openStar: 33, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 85, mapId: 1013, levelId: 1, openCoin: 0, openStar: 18, reward: "BrownPack*1"),
                new(id: 86, mapId: 1013, levelId: 2, openCoin: 0, openStar: 21, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 87, mapId: 1013, levelId: 3, openCoin: 700, openStar: 24, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 88, mapId: 1013, levelId: 4, openCoin: 1500, openStar: 27, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 89, mapId: 1013, levelId: 5, openCoin: 1500, openStar: 30, reward: "Head21*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 90, mapId: 1013, levelId: 6, openCoin: 2000, openStar: 27, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 91, mapId: 1013, levelId: 7, openCoin: 3000, openStar: 30, reward: "Head01*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 92, mapId: 1013, levelId: 8, openCoin: 1500, openStar: 33, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 93, mapId: 1014, levelId: 1, openCoin: 0, openStar: 18, reward: "BrownPack*1"),
                new(id: 94, mapId: 1014, levelId: 2, openCoin: 0, openStar: 21, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 95, mapId: 1014, levelId: 3, openCoin: 700, openStar: 24, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 96, mapId: 1014, levelId: 4, openCoin: 1500, openStar: 27, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 97, mapId: 1014, levelId: 5, openCoin: 1500, openStar: 30, reward: "Head16*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 98, mapId: 1014, levelId: 6, openCoin: 2000, openStar: 27, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 99, mapId: 1014, levelId: 7, openCoin: 3000, openStar: 30, reward: "Head01*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 100, mapId: 1014, levelId: 8, openCoin: 1500, openStar: 33, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 101, mapId: 1015, levelId: 1, openCoin: 0, openStar: 18, reward: "BrownPack*1"),
                new(id: 102, mapId: 1015, levelId: 2, openCoin: 0, openStar: 21, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 103, mapId: 1015, levelId: 3, openCoin: 700, openStar: 24, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 104, mapId: 1015, levelId: 4, openCoin: 1500, openStar: 27, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 105, mapId: 1015, levelId: 5, openCoin: 1500, openStar: 30, reward: "Head01*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 106, mapId: 1015, levelId: 6, openCoin: 2000, openStar: 27, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 107, mapId: 1015, levelId: 7, openCoin: 3000, openStar: 30, reward: "Head01*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 108, mapId: 1015, levelId: 8, openCoin: 1500, openStar: 33, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 109, mapId: 1016, levelId: 1, openCoin: 0, openStar: 18, reward: "BrownPack*1"),
                new(id: 110, mapId: 1016, levelId: 2, openCoin: 0, openStar: 21, reward: "GreenPack*1|eliminateDice*1|eliminateHammer*1"),
                new(id: 111, mapId: 1016, levelId: 3, openCoin: 700, openStar: 24, reward: "GreenPack*1|eliminateArrow*1|eliminateHammer*1"),
                new(id: 112, mapId: 1016, levelId: 4, openCoin: 1500, openStar: 27, reward: "BluePack*1|eliminateBullet*1|coin*100"),
                new(id: 113, mapId: 1016, levelId: 5, openCoin: 1500, openStar: 30, reward: "Head01*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 114, mapId: 1016, levelId: 6, openCoin: 2000, openStar: 27, reward: "coin*200|eliminateArrow*1|eliminateBullet*1"),
                new(id: 115, mapId: 1016, levelId: 7, openCoin: 3000, openStar: 30, reward: "Head01*1|eliminateHammer*1|liveBuff*20|eliminateColored*1"),
                new(id: 116, mapId: 1016, levelId: 8, openCoin: 1500, openStar: 33, reward: "coin*200|eliminateArrow*1|eliminateBullet*1")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly PuzzleReward this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[PuzzleReward] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public PuzzleReward[] All => _data;
        
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