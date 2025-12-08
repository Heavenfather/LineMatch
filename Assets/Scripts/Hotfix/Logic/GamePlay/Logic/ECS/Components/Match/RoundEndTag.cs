namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 回合结束标签
    /// </summary>
    public struct RoundEndTag
    {
        /// <summary>
        /// 触发这个回合开始的请求类型
        /// PlayerLine/PlayerSquare/UseItem
        /// </summary>
        public MatchRequestType RoundStartType;
    }
}