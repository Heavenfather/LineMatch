using System.Collections.Generic;

namespace HotfixLogic
{
    /// <summary>
    /// 红点路径定义
    /// </summary>
    public static class RedDotDefine
    {
        /// <summary>
        /// 红点树的根节点
        /// </summary>
        public const string Root = "Root";


        // ---------- 业务红点 ----------
        public const string Card = "Root/Card";
        public const string CardTotalReward = "Root/Card/TotalReward";

        public const string CardStar = "Root/Card/Star";

        public const string CardGoalCard = "Root/Card/GoalCard";

        public const string CardPack = "Root/Card/Pack";
        public const string CardPackNewCard = "Root/Card/Pack/NewCard";
        public const string CardPackReward = "Root/Card/Pack/Reward";


        public const string Rank = "Root/Rank";

        public const string Shop = "Root/Shop";

        public const string Sign = "Root/Sign";
        public const string SignDaily = "Root/Sign/SignDaily";
        public const string SignCumulative = "Root/Sign/Acumulative";

        public const string MonthCard = "Root/MonthCard";
        public const string MonthCardNormal = "Root/MonthCard/Normal";
        public const string MonthCardSuper = "Root/MonthCard/Super";
        public const string TrainMaster = "Root/TrainMaster";

        public const string Gather = "Root/Gather";

        public const string Email = "Root/Gather/Email";
        public const string Invite = "Root/Gather/Invite";
        public const string Puzzle = "Root/Puzzle";

        public const string Task = "Root/Task";
        public const string Treasure = "Root/Treasure";
        public const string Quest = "Root/Quest";



        public readonly static List<string> RedDotsTree = new List<string>
        {
            Root,

#region 卡片红点
            Card,
            CardTotalReward,

            CardGoalCard,

            CardStar,

            CardPack,
            CardPackNewCard,
            CardPackReward,
#endregion

            Rank,

            Shop,

            Sign,
            SignDaily,
            SignCumulative,


            MonthCard,
            MonthCardNormal,
            MonthCardSuper,

            TrainMaster,

            Gather,
            Email,
            Invite,

            Puzzle,
            
            Task,

            Treasure,

            Quest,
        };
    }
}