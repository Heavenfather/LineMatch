namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 元素节点当前状态
    /// </summary>
    public enum NodeState
    {
        /// <summary>
        /// 节点未被使用
        /// </summary>
        UnUse,
        /// <summary>
        /// 节点空闲状态
        /// </summary>
        Idle,
        /// <summary>
        /// 节点已被选中
        /// </summary>
        Selected,
        /// <summary>
        /// 节点正在匹配
        /// </summary>
        Matching,
        /// <summary>
        /// 节点正在被消除
        /// </summary>
        Destroying,
        /// <summary>
        /// 节点已被释放
        /// </summary>
        Release,
    }
}