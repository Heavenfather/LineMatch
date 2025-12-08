namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 标记该元素在本帧即将被消除
    /// </summary>
    public struct EliminatedTag
    {
        /// <summary>
        /// 被消除的次数
        /// </summary>
        public int EliminateCount;

        /// <summary>
        /// 被消除的原因
        /// </summary>
        public EliminateReason Reason;
    }
}