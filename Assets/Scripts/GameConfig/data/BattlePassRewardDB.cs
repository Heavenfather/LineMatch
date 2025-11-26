/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 通行证.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class BattlePassRewardDB : ConfigBase
    {
        private BattlePassReward[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new BattlePassReward[]
            {
                new(id: 0, key: 0, reward1: "eliminateBoom*1", reward2: "health3Buff*160|Frame21*1|NameColor02*1"),
                new(id: 1, key: 1, reward1: "eliminateBall*1", reward2: "liveBuff*15"),
                new(id: 2, key: 2, reward1: "eliminateRock*1", reward2: "eliminateHammer*1"),
                new(id: 3, key: 3, reward1: "BrownPack*1", reward2: "BluePack*1"),
                new(id: 4, key: 5, reward1: "coins*100", reward2: "eliminateArrow*1"),
                new(id: 5, key: 5, reward1: "BrownPack*1|liveBuff*15|puzzleLoupe*1", reward2: "BluePack*1|coins*200|eliminateBoomBuff*15|puzzlePosition*1"),
                new(id: 6, key: 10, reward1: "eliminateHammer*1", reward2: "eliminateBall*2"),
                new(id: 7, key: 10, reward1: "eliminateBoom*1", reward2: "eliminateRock*2"),
                new(id: 8, key: 10, reward1: "GreenPack*1|eliminateBallBuff*15", reward2: "PurplePack*1|eliminateBullet*2"),
                new(id: 9, key: 10, reward1: "eliminateRock*1", reward2: "eliminateHammer*2"),
                new(id: 10, key: 15, reward1: "GreenPack*1|puzzlePosition*1|eliminateBall*1|eliminateColored*1", reward2: "BluePack*1|coins*200|eliminateRock*2|eliminateBoom*2|eliminateBall*2|puzzleVacuum*1"),
                new(id: 11, key: 15, reward1: "eliminateArrow*1", reward2: "eliminateRockBuff*30"),
                new(id: 12, key: 15, reward1: "coins*100", reward2: "eliminateColored*2"),
                new(id: 13, key: 15, reward1: "BluePack*1", reward2: "PurplePack*1"),
                new(id: 14, key: 15, reward1: "eliminateRock*1", reward2: "eliminateBullet*2"),
                new(id: 15, key: 20, reward1: "GreenPack*1|puzzleVacuum*1|eliminateArrow*1", reward2: "BluePack*1|coins*300|liveBuff*60|puzzleLoupe*2|eliminateBullet*2"),
                new(id: 16, key: 20, reward1: "eliminateBoom*1", reward2: "eliminateArrow*2"),
                new(id: 17, key: 20, reward1: "coins*100", reward2: "eliminateBoomBuff*30"),
                new(id: 18, key: 20, reward1: "eliminateBall*1", reward2: "eliminateHammer*2"),
                new(id: 19, key: 20, reward1: "eliminateColored*1", reward2: "eliminateRock*2"),
                new(id: 20, key: 25, reward1: "BluePack*1|eliminateRock*1|puzzleLoupe*1|eliminateBullet*1", reward2: "PurplePack*1|coins*400|puzzleLoupe*2|eliminateBullet*2|eliminateArrow*2|eliminateColored*2"),
                new(id: 21, key: 25, reward1: "liveBuff*30", reward2: "eliminateBullet*2"),
                new(id: 22, key: 25, reward1: "eliminateRock*1", reward2: "eliminateBoom*2"),
                new(id: 23, key: 25, reward1: "BluePack*1", reward2: "PurplePack*1"),
                new(id: 24, key: 25, reward1: "eliminateBoom*1", reward2: "eliminateRock*3"),
                new(id: 25, key: 30, reward1: "GreenPack*1|liveBuff*30|eliminateBall*1|puzzlePosition*1", reward2: "BluePack*1|coins*400|eliminateRockBuff*30|eliminateBoomBuff*30|puzzlePosition*2"),
                new(id: 26, key: 30, reward1: "eliminateBullet*1", reward2: "eliminateBall*3"),
                new(id: 27, key: 30, reward1: "coins*200", reward2: "liveBuff*60"),
                new(id: 28, key: 30, reward1: "eliminateBoom*2", reward2: "eliminateBullet*3"),
                new(id: 29, key: 30, reward1: "eliminateDice*1", reward2: "eliminateArrow*3"),
                new(id: 30, key: 30, reward1: "PurplePack*1|eliminateRock*3|eliminateBoom*3|eliminateBall*3|puzzleVacuum*2", reward2: "RedPack*1|coins*1000|liveBuff*60|eliminateRockBuff*60|eliminateBoomBuff*60|eliminateBallBuff*60|puzzleVacuum*4")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly BattlePassReward this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[BattlePassReward] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public BattlePassReward[] All => _data;
        
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