namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 指令类型
    /// </summary>
    public enum MatchActionType
    {
        Damage, // 扣次数
        Spawn2Other, // 生成其它棋子
        Transform, // 变化下一个状态
        AddScore, // 加分
        Delay,// 延迟
        Shuffle, // 洗牌
    }
}