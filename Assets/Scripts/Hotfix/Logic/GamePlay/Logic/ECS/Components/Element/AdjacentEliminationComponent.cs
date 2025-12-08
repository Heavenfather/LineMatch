namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 旁消组件
    /// 标记元素可以通过旁边格子的消除而被触发消除
    /// </summary>
    public struct AdjacentEliminationComponent
    {
        /// <summary>
        /// 是否只检查上下左右四个方向（false则检查八个方向）
        /// </summary>
        public bool OnlyFourDirections;

        /// <summary>
        /// 本轮是否已经被触发过（避免重复触发）
        /// </summary>
        public bool IsTriggeredThisRound;
    }
}
