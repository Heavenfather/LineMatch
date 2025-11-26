/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 寻宝关卡奖励.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class PuzzleMapDB : ConfigBase
    {
        private PuzzleMap[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new PuzzleMap[]
            {
                new(mapId: 1001, MapName: "MonsterCity", mapCNName: LocalizationPool.Get("PuzzleMap/9532"), LeveName1: LocalizationPool.Get("PuzzleMap/ff3c"), LeveName2: LocalizationPool.Get("PuzzleMap/bf01"), LeveName3: LocalizationPool.Get("PuzzleMap/816c"), LeveName4: LocalizationPool.Get("PuzzleMap/5cac"), LeveName5: LocalizationPool.Get("PuzzleMap/cdad"), LeveName6: LocalizationPool.Get("PuzzleMap/28df"), LeveName7: "", LeveName8: "", EnvirAudio: "puzzle_enviraudio_2", GuideColor: "#C9FCF7", GuideReward: "Head04*1"),
                new(mapId: 1002, MapName: "PandoraPlanet", mapCNName: LocalizationPool.Get("PuzzleMap/f953"), LeveName1: LocalizationPool.Get("PuzzleMap/d793"), LeveName2: LocalizationPool.Get("PuzzleMap/a159"), LeveName3: LocalizationPool.Get("PuzzleMap/f4b9"), LeveName4: LocalizationPool.Get("PuzzleMap/7c67"), LeveName5: LocalizationPool.Get("PuzzleMap/d2d9"), LeveName6: LocalizationPool.Get("PuzzleMap/4146"), LeveName7: "", LeveName8: "", EnvirAudio: "puzzle_enviraudio_5", GuideColor: "#D7D4FF", GuideReward: "Head09*1"),
                new(mapId: 1003, MapName: "RobotDance", mapCNName: LocalizationPool.Get("PuzzleMap/f321"), LeveName1: LocalizationPool.Get("PuzzleMap/861a"), LeveName2: LocalizationPool.Get("PuzzleMap/199f"), LeveName3: LocalizationPool.Get("PuzzleMap/b5c9"), LeveName4: LocalizationPool.Get("PuzzleMap/d84e"), LeveName5: LocalizationPool.Get("PuzzleMap/521d"), LeveName6: LocalizationPool.Get("PuzzleMap/788f"), LeveName7: "", LeveName8: "", EnvirAudio: "puzzle_enviraudio_3", GuideColor: "#EEE2FA", GuideReward: "Head18*1"),
                new(mapId: 1004, MapName: "TangOdyssey", mapCNName: LocalizationPool.Get("PuzzleMap/8044"), LeveName1: LocalizationPool.Get("PuzzleMap/6f64"), LeveName2: LocalizationPool.Get("PuzzleMap/4f05"), LeveName3: LocalizationPool.Get("PuzzleMap/ef6f"), LeveName4: LocalizationPool.Get("PuzzleMap/67b5"), LeveName5: LocalizationPool.Get("PuzzleMap/8d12"), LeveName6: LocalizationPool.Get("PuzzleMap/8f35"), LeveName7: "", LeveName8: "", EnvirAudio: "puzzle_enviraudio_1", GuideColor: "#FFD9A8", GuideReward: "Head12*1"),
                new(mapId: 1005, MapName: "ElfKingdom", mapCNName: LocalizationPool.Get("PuzzleMap/1985"), LeveName1: LocalizationPool.Get("PuzzleMap/6020"), LeveName2: LocalizationPool.Get("PuzzleMap/6996"), LeveName3: LocalizationPool.Get("PuzzleMap/438e"), LeveName4: LocalizationPool.Get("PuzzleMap/faba"), LeveName5: LocalizationPool.Get("PuzzleMap/e3fa"), LeveName6: LocalizationPool.Get("PuzzleMap/8180"), LeveName7: "", LeveName8: "", EnvirAudio: "puzzle_enviraudio_4", GuideColor: "", GuideReward: "Head01*1"),
                new(mapId: 1006, MapName: "GhostManor", mapCNName: LocalizationPool.Get("PuzzleMap/382e"), LeveName1: LocalizationPool.Get("PuzzleMap/a3dc"), LeveName2: LocalizationPool.Get("PuzzleMap/bb65"), LeveName3: LocalizationPool.Get("PuzzleMap/4c15"), LeveName4: LocalizationPool.Get("PuzzleMap/d74c"), LeveName5: LocalizationPool.Get("PuzzleMap/fb74"), LeveName6: LocalizationPool.Get("PuzzleMap/9da2"), LeveName7: "", LeveName8: "", EnvirAudio: "puzzle_enviraudio_2", GuideColor: "", GuideReward: "Head01*1"),
                new(mapId: 1007, MapName: "NeonFigurine", mapCNName: LocalizationPool.Get("PuzzleMap/0185"), LeveName1: LocalizationPool.Get("PuzzleMap/42a0"), LeveName2: LocalizationPool.Get("PuzzleMap/063a"), LeveName3: LocalizationPool.Get("PuzzleMap/a0a7"), LeveName4: LocalizationPool.Get("PuzzleMap/ccd9"), LeveName5: LocalizationPool.Get("PuzzleMap/da27"), LeveName6: LocalizationPool.Get("PuzzleMap/8daf"), LeveName7: LocalizationPool.Get("PuzzleMap/a2e9"), LeveName8: LocalizationPool.Get("PuzzleMap/cf9e"), EnvirAudio: "puzzle_enviraudio_3", GuideColor: "#FFDFEE", GuideReward: "Head27*1"),
                new(mapId: 1008, MapName: "Flavors", mapCNName: LocalizationPool.Get("PuzzleMap/4b0c"), LeveName1: LocalizationPool.Get("PuzzleMap/f9c1"), LeveName2: LocalizationPool.Get("PuzzleMap/5db1"), LeveName3: LocalizationPool.Get("PuzzleMap/4118"), LeveName4: LocalizationPool.Get("PuzzleMap/b9c5"), LeveName5: LocalizationPool.Get("PuzzleMap/8192"), LeveName6: LocalizationPool.Get("PuzzleMap/892e"), LeveName7: LocalizationPool.Get("PuzzleMap/514b"), LeveName8: LocalizationPool.Get("PuzzleMap/a360"), EnvirAudio: "puzzle_enviraudio_1", GuideColor: "#FFEBC6", GuideReward: "Head08*1"),
                new(mapId: 1009, MapName: "SweetDream", mapCNName: LocalizationPool.Get("PuzzleMap/9866"), LeveName1: LocalizationPool.Get("PuzzleMap/8adb"), LeveName2: LocalizationPool.Get("PuzzleMap/58a1"), LeveName3: LocalizationPool.Get("PuzzleMap/a80f"), LeveName4: LocalizationPool.Get("PuzzleMap/e50a"), LeveName5: LocalizationPool.Get("PuzzleMap/b715"), LeveName6: LocalizationPool.Get("PuzzleMap/901b"), LeveName7: LocalizationPool.Get("PuzzleMap/2cd2"), LeveName8: LocalizationPool.Get("PuzzleMap/65eb"), EnvirAudio: "puzzle_enviraudio_2", GuideColor: "#E3FBEB", GuideReward: "Head25*1"),
                new(mapId: 1010, MapName: "NewAttire", mapCNName: LocalizationPool.Get("PuzzleMap/3e16"), LeveName1: LocalizationPool.Get("PuzzleMap/d98c"), LeveName2: LocalizationPool.Get("PuzzleMap/0945"), LeveName3: LocalizationPool.Get("PuzzleMap/916d"), LeveName4: LocalizationPool.Get("PuzzleMap/8491"), LeveName5: LocalizationPool.Get("PuzzleMap/a544"), LeveName6: LocalizationPool.Get("PuzzleMap/486b"), LeveName7: LocalizationPool.Get("PuzzleMap/c6f6"), LeveName8: LocalizationPool.Get("PuzzleMap/a4c2"), EnvirAudio: "puzzle_enviraudio_5", GuideColor: "#FDDEFE", GuideReward: "Head17*1"),
                new(mapId: 1011, MapName: "Jiangnan", mapCNName: LocalizationPool.Get("PuzzleMap/737b"), LeveName1: LocalizationPool.Get("PuzzleMap/ad21"), LeveName2: LocalizationPool.Get("PuzzleMap/f2ae"), LeveName3: LocalizationPool.Get("PuzzleMap/a218"), LeveName4: LocalizationPool.Get("PuzzleMap/ef7f"), LeveName5: LocalizationPool.Get("PuzzleMap/7504"), LeveName6: LocalizationPool.Get("PuzzleMap/bc70"), LeveName7: LocalizationPool.Get("PuzzleMap/025a"), LeveName8: LocalizationPool.Get("PuzzleMap/9e18"), EnvirAudio: "puzzle_enviraudio_6", GuideColor: "#DCFAFC", GuideReward: "Head14*1"),
                new(mapId: 1012, MapName: "FlyingApsaras", mapCNName: LocalizationPool.Get("PuzzleMap/c3c6"), LeveName1: LocalizationPool.Get("PuzzleMap/b7a1"), LeveName2: LocalizationPool.Get("PuzzleMap/8140"), LeveName3: LocalizationPool.Get("PuzzleMap/abc4"), LeveName4: LocalizationPool.Get("PuzzleMap/2eb8"), LeveName5: LocalizationPool.Get("PuzzleMap/8678"), LeveName6: LocalizationPool.Get("PuzzleMap/d114"), LeveName7: LocalizationPool.Get("PuzzleMap/d40a"), LeveName8: LocalizationPool.Get("PuzzleMap/387d"), EnvirAudio: "puzzle_enviraudio_3", GuideColor: "#FEF2DC", GuideReward: "Head22*1"),
                new(mapId: 1013, MapName: "CaloriesStarlight", mapCNName: LocalizationPool.Get("PuzzleMap/2fc0"), LeveName1: LocalizationPool.Get("PuzzleMap/2bd7"), LeveName2: LocalizationPool.Get("PuzzleMap/b2bb"), LeveName3: LocalizationPool.Get("PuzzleMap/88bf"), LeveName4: LocalizationPool.Get("PuzzleMap/0604"), LeveName5: LocalizationPool.Get("PuzzleMap/a063"), LeveName6: LocalizationPool.Get("PuzzleMap/239c"), LeveName7: LocalizationPool.Get("PuzzleMap/f41a"), LeveName8: LocalizationPool.Get("PuzzleMap/980f"), EnvirAudio: "puzzle_enviraudio_5", GuideColor: "#E6FAE1", GuideReward: "Head21*1"),
                new(mapId: 1014, MapName: "Transylvania", mapCNName: LocalizationPool.Get("PuzzleMap/86a6"), LeveName1: LocalizationPool.Get("PuzzleMap/9ec1"), LeveName2: LocalizationPool.Get("PuzzleMap/256b"), LeveName3: LocalizationPool.Get("PuzzleMap/3d6a"), LeveName4: LocalizationPool.Get("PuzzleMap/d5f9"), LeveName5: LocalizationPool.Get("PuzzleMap/c4f4"), LeveName6: LocalizationPool.Get("PuzzleMap/4a13"), LeveName7: LocalizationPool.Get("PuzzleMap/237b"), LeveName8: LocalizationPool.Get("PuzzleMap/e594"), EnvirAudio: "puzzle_enviraudio_4", GuideColor: "#E6FAE1", GuideReward: "Head16*1")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly PuzzleMap this[int mapId]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(mapId, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[PuzzleMap] mapId: {mapId} not found");
                return ref _data[idx];
            }
        }
        
        public PuzzleMap[] All => _data;
        
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