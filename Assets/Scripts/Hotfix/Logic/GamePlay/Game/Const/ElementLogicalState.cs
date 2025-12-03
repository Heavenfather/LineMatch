namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 棋子的逻辑状态管理
    /// </summary>
    public enum ElementLogicalState
    {
        Idle, // 静止可交互
        Matching, // 正在判定/准备消除
        Acting, // 正在播放表现，逻辑锁定
        Falling, // 正在掉落
        Dying, // 已确认为死亡，等待回收
        Freeze, // 冻结，不可进行任何操作
    }
}