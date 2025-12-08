namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 消除原因
    /// </summary>
    public enum EliminateReason
    {
        /// <summary>
        /// 直接受到伤害
        /// </summary>
        Damage,

        /// <summary>
        /// 通过旁消触发
        /// </summary>
        Side,
        
        /// <summary>
        /// 通过替换触发
        /// </summary>
        Replace,
        
        /// <summary>
        /// 通过收集触发
        /// </summary>
        Collect,
    }
}