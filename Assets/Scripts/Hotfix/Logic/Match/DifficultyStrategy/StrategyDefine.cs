namespace HotfixLogic.Match
{
    // 定义调控策略类型
    public enum DifficultyStrategyType
    {
        None = 0,
        ColorRegularity = 1, // 策略1: 棋子规整度
        NeighborConsistency = 2, // 策略2: 相邻棋子同色概率
        IndependentRatio = 3, // 策略3: 开局不连通独立棋子分布
        SquareFormation = 4 // 策略4: 掉落形成方格
    }

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