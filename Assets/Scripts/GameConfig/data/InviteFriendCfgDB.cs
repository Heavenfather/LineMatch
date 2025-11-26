/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 邀请好友.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class InviteFriendCfgDB : ConfigBase
    {
        private InviteFriendCfg[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new InviteFriendCfg[]
            {
                new(id: 1, inviteCount: 1, rewards: "coin*100|BrownPack*1"),
                new(id: 2, inviteCount: 2, rewards: "live*5|eliminateHammer*1"),
                new(id: 3, inviteCount: 3, rewards: "coin*100|BrownPack*2"),
                new(id: 4, inviteCount: 4, rewards: "live*5|eliminateHammer*2"),
                new(id: 5, inviteCount: 5, rewards: "coin*150|BrownPack*2"),
                new(id: 6, inviteCount: 6, rewards: "live*5|eliminateDice*2"),
                new(id: 7, inviteCount: 7, rewards: "coin*150|BrownPack*3"),
                new(id: 8, inviteCount: 8, rewards: "live*5|eliminateDice*2"),
                new(id: 9, inviteCount: 9, rewards: "coin*150|BrownPack*3"),
                new(id: 10, inviteCount: 10, rewards: "live*10|eliminateRock*2"),
                new(id: 11, inviteCount: 12, rewards: "coin*200|GreenPack*2"),
                new(id: 12, inviteCount: 14, rewards: "live*10|eliminateRock*3"),
                new(id: 13, inviteCount: 16, rewards: "coin*200|GreenPack*2"),
                new(id: 14, inviteCount: 18, rewards: "live*10|eliminateColored*3"),
                new(id: 15, inviteCount: 20, rewards: "coin*200|GreenPack*3")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly InviteFriendCfg this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[InviteFriendCfg] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public InviteFriendCfg[] All => _data;
        
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