namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 元素在棋盘上的状态
    /// </summary>
    public enum ElementState
    {
        /// <summary>
        /// 元素空闲状态
        /// </summary>
        Idle,

        /// <summary>
        /// 元素选中状态
        /// </summary>
        Selected,

        /// <summary>
        /// 元素匹配状态
        /// </summary>
        Matching,

        /// <summary>
        /// 元素移动状态
        /// </summary>
        Moving,
        
        /// <summary>
        /// 元素已被释放
        /// </summary>
        Release,
    }
}