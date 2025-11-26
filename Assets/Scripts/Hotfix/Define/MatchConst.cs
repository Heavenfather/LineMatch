namespace Hotfix.Define
{
    public class MatchConst
    {
        public const string ElementAddressBase = "match/prefab";
        public const string SpritesAddressBase = "match/sprites";

        public const string MatchShowTarget = "MatchShowTarget";
        public const string MatchLockReason = "MatchLockReason";
        public const string MatchLockByGenNew = "MatchLockByGenNew";
        public const string MatchDoneLockReason = "MatchDoneLockReason";
        public const string MatchRandomDiffuseMoveLock = "MatchRandomDiffuseMoveLock";
        public const string MatchLockByUseItem = "MatchLockByUseItem";
        public const string MatchLockByReleaseBomb = "MatchLockByReleaseBomb";
        
        public const string SPINE_IDLE = "idle";
        public const string SPINE_HELLO = "hello";
        public const string SPINE_SUCCESS = "success";
        public const string SPINE_FAIL = "fail";
        public const string SPINE_TRIPLE = "triple";
        public const string SPINE_WARM = "warm";
        
        public const float DropDuration = 0.4f;
        public const float RocketMoveDuration = 0.6f;
        public const float DelElementInterval = 0.03f;
        public const float TrailEmitterDuration = 0.4f;
        public const float ChuanglianCloseDuration = 0.5f;
        public const float BombElementEffDur = 0.23f;
        public const float MatchEndWaitDuration = 0.5f;
        public const float TipsMoveTimes = 3f;
        public const float DropStepInterval = 0.003f;
        public const float EmitterInterval = 0.05f;

        //关卡元素对象池预热
        public const int NormalElementWarmCount = 100;
        public const int NormalElementMaxCount = 200;
        public const int SpecialElementWarmCount = 25;
        public const int SpecialElementMaxCount = 50;
        public const int BlockElementWarmCount = 30;
        public const int BlockElementMaxCount = 100;
        
#if UNITY_WEBGL && !UNITY_EDITOR
        public const int BatchDestroyPerFrameCount = 5;
#else
        public const int BatchDestroyPerFrameCount = 20;
#endif
        
    }

    /// <summary>
    /// 特殊功能棋子id
    /// </summary>
    public enum ElementIdConst : int
    {
        Rocket = 8,
        Bomb = 9,
        ColorBall = 10,
        RocketHorizontal = 11,
        
        SpreadGrass = 115, //草地
        Butterfly = 190,//蝴蝶
        Coin = 280,  //金币
        WishBottle=1001,//许愿瓶
        JumpRobi = 1002,//兔子
    }

    /// <summary>
    /// 消除关卡类型
    /// </summary>
    public enum MatchLevelType
    {
        A,
        B,
        C,
        Editor
    }

    /// <summary>
    /// 消除游戏类型
    /// </summary>
    public enum MatchGameType
    {
        /// <summary>
        /// 普通的消除模式
        /// </summary>
        NormalMatch = 0,
        /// <summary>
        /// towdots的消除模式
        /// </summary>
        TowDots,
    }
}