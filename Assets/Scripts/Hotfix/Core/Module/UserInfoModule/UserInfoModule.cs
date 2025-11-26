using Hotfix.Define;
using Hotfix.Utils;

namespace HotfixCore.Module
{
    public class UserInfoModule : IModuleAwake, IModuleDestroy
    {
        private int _userId;
        public int UserId => _userId;

        private int _createTime;
        public int CreateTime => _createTime;

        private string _nickname;
        public string NickName => _nickname;

        private string _channelNickName;
        public string ChannelNickName => _channelNickName;

        private string _avatar;
        public string Avatar => _avatar;
        
        private string _channelAvatar;
        public string ChannelAvatar => _channelAvatar;

        private string _avatarFrame;
        public string AvatarFrame => _avatarFrame;

        private int _firstPassCount;
        public int FirstPassCount => _firstPassCount;

        private int _maxRank;
        public int MaxRank => _maxRank;

        private int _maxWinStreak;
        public int MaxWinStreak => _maxWinStreak;

        private string _usingMedal;
        public string UsingMedal => _usingMedal;

        private string _nameColor;
        public string NameColor => _nameColor;

        private int _registerTime;
        public int RegisterTime => _registerTime;

        private string _invite_code;
        public string InviteCode => _invite_code;


        public void Awake(object parameter)
        {
        }

        public void Destroy()
        {
        }

        public void SetUserId(int userId)
        {
            _userId = userId;
        }

        public void SetNickname(string nickname)
        {
            _nickname = nickname;
            G.EventModule.DispatchEvent(GameEventDefine.OnUserInfoChangeNickName);
        }

        public void SetAvatar(string avatar)
        {
            _avatar = avatar;
            G.EventModule.DispatchEvent(GameEventDefine.OnUserInfoChangeHead);
        }

        public void SetAvatarFrame(string avatarFrame)
        {
            _avatarFrame = avatarFrame;
            if (avatarFrame == "") {
                _avatarFrame = "1";
            }
            G.EventModule.DispatchEvent(GameEventDefine.OnUserInfoChangeHeadFrame);
        }

        public void SetFirstPassCount(int firstPassCount)
        {
            _firstPassCount = firstPassCount;
        }

        public void SetMaxRank(int maxRank)
        {
            _maxRank = maxRank;
        }

        public void SetMaxWinStreak(int maxWinStreak)
        {
            _maxWinStreak = maxWinStreak;
        }

        public void CheckMaxWinStreak(int winStreak)
        {
            if (winStreak > _maxWinStreak)
            {
                _maxWinStreak = winStreak;
            }
        }

        public void AddFirstPassCount()
        {
            _firstPassCount++;
        }

        public void SetUsingMedal(string usingMedal)
        {
            _usingMedal = usingMedal;
            G.EventModule.DispatchEvent(GameEventDefine.OnUserInfoChangeMedal);
        }

        public void SetCreateTime(int createTime)
        {
            _createTime = createTime;
        }
        
        public void SetNameColorID(string nameColorID) {
            _nameColor = nameColorID;
            G.EventModule.DispatchEvent(GameEventDefine.OnUserInfoChangeNameColor);
        }

        public void SetChannelNickName(string channelNickName)
        {
            _channelNickName = channelNickName;
        }

        public void SetChannelAvatar(string channelAvatar)
        {
            _channelAvatar = channelAvatar;
        }

        public void SetRegisterTime(int registerTime)
        {
            _registerTime = registerTime;
        }

        public void SetInviteCode(string inviteCode) {
            _invite_code = inviteCode;
        }

        public bool IsRegister24Hours() {
            bool isRegister24Hours = CommonUtil.GetNowTime() - _registerTime < 86400;
            return isRegister24Hours;
        }
    }
}
