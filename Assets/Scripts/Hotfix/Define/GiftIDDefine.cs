namespace Hotfix.Define
{
    public partial class GiftIDDefine
    {
        public const int PayShop = 100001;
        public const int PayLackItemGift = 100002;
        public const int PayLackCoinGift = 100003;


        public const int PayMonthCard = 100101;

        public const int OneYuanGift = 110001;
        public const int OneFirstGift = 110002;
    }


    // 广告场景
    // 广告场景尽量大于10000，后台有特定的广告场景奖励
    // 1：大厅，2：消除，3：寻宝
    public partial class GiftIDDefine
    {
        // 广告位
        public const string AdvUnitId = "adunit-e9c39b21210c189d";

        public const int AdvShop = 10101;

        public const int AdvPuzzleLackItem = 10201;

        public const int Adv_Shop = 1;

        public const int Adv_Sign = 100011;

        public const int Adv_MatchLoseRevive = 200101;
        public const int Adv_MatchWinGoldDouble = 200102;

        public const int Adv_Puzzle_UseAdvBooster = 300001;
    }
}