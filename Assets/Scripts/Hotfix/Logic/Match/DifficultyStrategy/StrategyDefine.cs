namespace HotfixLogic.Match
{
    // 难度调节方向
    public enum DifficultyDirection
    {
        Normal = 0,
        Easier = 1, // 调易
        Harder = 2 // 调难
    }

    // 触发难度计算的上下文数据
    public struct DifficultyTriggerContext
    {
        public int LevelId;
        public int TotalSteps; // 总步数
        public int RemainingSteps; // 剩余步数
        public int ConsecutiveFailures; // 连续失败次数
        public int ReturnUserDays; // 回流未登录天数
    }
}