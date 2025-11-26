/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 邀请好友.xlsx
*/

namespace GameConfig
{
    
    public readonly struct InviteFriendCfg
    {
        
        /// <summary>
        /// 配置索引
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 配置int值
        /// </summary>
        public int inviteCount { get; }
        
        /// <summary>
        /// 配置string值
        /// </summary>
        public string rewards { get; }
        
        internal InviteFriendCfg(int id, int inviteCount, string rewards)
        {
            this.id = id;
            this.inviteCount = inviteCount;
            this.rewards = rewards;
        }
    }
}