namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 消除请求类型 不同的类型有不同的 IMatchRule 注意添加
    /// </summary>
    public enum MatchRequestType
    {
        None,
        PlayerLine, // 普通连线
        PlayerSquare, // 方块闭环
        UseItem, // 使用道具
        PostDropCheck, // 掉落后的自动检测
        Rocket, //单个火箭
        Bomb, //单个炸弹
        RocketAndRocket, // 火箭连火箭
        RocketAndBomb, // 火箭连炸
        RocketAndColorBall, // 火箭连彩球
        BombAndBomb, // 炸连炸
        BombAndColorBall, // 炸连彩球
        ColorBallAndColorBall, // 彩球连彩球
        ColorBallAndNormal, //彩球连普通棋子
        TowDotsStarBomb, // TowDots模式的星爆点连普通棋子
        TowDotsSearchDot, //TowDots模式的搜寻点连普通棋子
        TowDotsHorizontalDotLine, //TowDots模式的横向点连普通棋子
        TowDotsBombLineNormal, //TowDots模式的炸弹连普通棋子
        TowDotsColorBallLineNormal, //TowDots模式的彩球连普通棋子
        //TowDots的组合技，需求还不明确，TODO...
    }
}