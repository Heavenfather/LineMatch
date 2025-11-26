namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 普通棋子的闪图处理方式
    /// </summary>
    public enum NormalFlashIconAniType
    {
        None,
        /// <summary>
        /// 被选中时的闪烁效果
        /// </summary>
        SelectedFlash,
        /// <summary>
        /// 使用道具时的闪烁效果
        /// </summary>
        UseItemFlash,
    }
}