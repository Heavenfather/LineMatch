using GameCore.Log;

namespace GameConfig
{
    public partial class InviteFriendCfgDB
    {
        public InviteFriendCfg GetCurInviteFriend(int curCount) {
            for (int i = 0; i < _data.Length; i++) {
                if (_data[i].inviteCount > curCount) {
                    return _data[i];
                }
            }
            
            Logger.Error("No invite friend found for count: " + curCount);
            return _data[0];
        }
    }
}