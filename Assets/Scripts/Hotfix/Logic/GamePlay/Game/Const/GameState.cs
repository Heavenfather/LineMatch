namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 游戏整体状态
    /// </summary>
    public enum GameState
    {
        None,
        /// 游戏初始化
        Initialize,

        /// 游戏开始
        Start,

        /// 游戏暂停
        Pause,
        
        /// 游戏重新开始
        Restart,
        
        // 游戏结算
        Settlement,

        /// 游戏结束
        End,
    }
}