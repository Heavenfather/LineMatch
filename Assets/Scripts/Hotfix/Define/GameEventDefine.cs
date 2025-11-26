using HotfixCore.Module;

namespace Hotfix.Define
{
    /// <summary>
    /// 游戏事件id定义
    /// </summary>
    public class GameEventDefine
    {
        #region 游戏框架

        public static readonly int OnGameApplicationQuit = RuntimeId.ToRuntimeId("OnGameApplicationQuit");
        public static readonly int OnGameApplicationPause = RuntimeId.ToRuntimeId("OnGameApplicationPause");
        public static readonly int OnGameApplicationFocus = RuntimeId.ToRuntimeId("OnGameApplicationFocus");
        public static readonly int OnNotGameApplicationFocus = RuntimeId.ToRuntimeId("OnNotGameApplicationFocus");
        public static readonly int OnGameLanguageChanged = RuntimeId.ToRuntimeId("OnGameLanguageChanged");
        public static readonly int OnGuideTrigger = RuntimeId.ToRuntimeId("OnGuideTriggerNext");
        public static readonly int OnGuideFinish = RuntimeId.ToRuntimeId("OnGuideFinish");
        public static readonly int OnWindowClose = RuntimeId.ToRuntimeId("OnWindowClose");
        public static readonly int OnWindowOpen = RuntimeId.ToRuntimeId("OnWindowOpen");
        #endregion

        #region 消除
        public static readonly int OnDestroyTargetListElement = RuntimeId.ToRuntimeId("OnDestroyTargetListElement");
        public static readonly int OnMatchStepMove = RuntimeId.ToRuntimeId("OnMatchStepMove");
        public static readonly int OnMatchCloseContinue = RuntimeId.ToRuntimeId("OnMatchCloseContinue");
        public static readonly int OnMatchCloseClick = RuntimeId.ToRuntimeId("OnMatchCloseClick");
        public static readonly int OnMatchUpdateLevel = RuntimeId.ToRuntimeId("OnMatchUpdateLevel");
        public static readonly int OnMatchAddStep = RuntimeId.ToRuntimeId("OnMatchAddStep");
        public static readonly int OnMatchAddStepUseCoin = RuntimeId.ToRuntimeId("OnMatchAddStepUseCoin");
        public static readonly int OnReduceMoveStep = RuntimeId.ToRuntimeId("OnReduceMoveStep");
        public static readonly int OnMatchGmEnter = RuntimeId.ToRuntimeId("OnMatchGmEnter");
        public static readonly int OnMatchScoreChanged = RuntimeId.ToRuntimeId("OnMatchScoreChanged");
        public static readonly int OnMatchUseItem = RuntimeId.ToRuntimeId("OnMatchUseItem");
        public static readonly int OnMatchCancelItem = RuntimeId.ToRuntimeId("OnMatchCancelItem");
        public static readonly int OnMatchUseItemSuccess = RuntimeId.ToRuntimeId("OnMatchUseItemSuccess");
        public static readonly int OnMatchUseItemFail = RuntimeId.ToRuntimeId("OnMatchUseItemFail");
        public static readonly int OnMatchReqUseItem = RuntimeId.ToRuntimeId("OnMatchReqUseItem");
        public static readonly int OnMatchRemainStep = RuntimeId.ToRuntimeId("OnMatchRemainStep"); 
        public static readonly int OnMatchControlStep = RuntimeId.ToRuntimeId("OnMatchControlStep"); 
        public static readonly int OnMatchGuideLevelAllFinish = RuntimeId.ToRuntimeId("OnMatchGuideLevelAllFinish");
        public static readonly int OnDoMatchStepJudge = RuntimeId.ToRuntimeId("OnDoMatchStepJudge");
        public static readonly int OnMatchStepMoveEnd = RuntimeId.ToRuntimeId("OnMatchStepMoveEnd");
        public static readonly int OnMatchDestroySquare = RuntimeId.ToRuntimeId("OnMatchDestroySquare");
        public static readonly int OnMatchElementMoveToTarget = RuntimeId.ToRuntimeId("OnMatchElementMoveToTarget");
        public static readonly int OnMatchUpdateSpecialElements = RuntimeId.ToRuntimeId("OnMatchUpdateSpecialElements");
        public static readonly int OnMatchNoneMatchToFail = RuntimeId.ToRuntimeId("OnMatchNoneMatchToFail");
        public static readonly int OnMatchAddTargetNum = RuntimeId.ToRuntimeId("OnMatchAddTargetNum");
        public static readonly int OnMatchRestart = RuntimeId.ToRuntimeId("OnMatchRestart");
        public static readonly int OnMatchRestartComplete = RuntimeId.ToRuntimeId("OnMatchRestartComplete");
        public static readonly int OnCheckMatchResult = RuntimeId.ToRuntimeId("OnCheckMatchResult");
        public static readonly int OnMatchDoneStateChanged = RuntimeId.ToRuntimeId("OnMatchDoneStateChanged");
        public static readonly int OnMatchPlayCollectItem = RuntimeId.ToRuntimeId("OnMatchPlayCollectItem");
        public static readonly int OnMatchBegin = RuntimeId.ToRuntimeId("OnMatchBegin");
        public static readonly int OnMatchLoadingFinish = RuntimeId.ToRuntimeId("OnMatchLoadingFinish");
        public static readonly int OnMatchMidwayClose = RuntimeId.ToRuntimeId("OnMatchMidwayClose");
        public static readonly int OnMatchTriple = RuntimeId.ToRuntimeId("OnMatchTriple");
        public static readonly int OnMatchGetLevelDetailFinish = RuntimeId.ToRuntimeId("OnMatchGetLevelDetailFinish");
        public static readonly int OnOpenMatchGmChangePanel = RuntimeId.ToRuntimeId("OnOpenMatchGMChangePanel");
        public static readonly int OnMatchResultFail = RuntimeId.ToRuntimeId("OnMatchResultFail");
        public static readonly int OnOkChangeBoardColor = RuntimeId.ToRuntimeId("OnOkChangeBoardColor");
        public static readonly int OnMatchTargetChangedNum = RuntimeId.ToRuntimeId("OnMatchTargetChangedNum");
        public static readonly int OnMatchResultAddCoinAndStar = RuntimeId.ToRuntimeId("OnMatchResultAddCoinAndStar");
        public static readonly int OnMatchFinish = RuntimeId.ToRuntimeId("OnMatchFinish");
        public static readonly int OnMatchBeginCollectResultCoin = RuntimeId.ToRuntimeId("OnMatchBeginCollectResultCoin");
        public static readonly int OnMatchAddResultCoin = RuntimeId.ToRuntimeId("OnMatchAddResultCoin");
        public static readonly int OnMatchSetResultCoin = RuntimeId.ToRuntimeId("OnMatchSetResultCoin");
        public static readonly int OnMatchUpdateResultCoin = RuntimeId.ToRuntimeId("OnMatchUpdateResultCoin");
        public static readonly int OnMatchUpdateMaxLevel = RuntimeId.ToRuntimeId("OnMatchUpdateMaxLevel");
        public static readonly int OnMatchCollectItemFlyComplete = RuntimeId.ToRuntimeId("OnMatchCollectItemFlyComplete");
        public static readonly int OnMatchGuideLevelStartFinish = RuntimeId.ToRuntimeId("OnMatchGuideLevelStartFinish");
        public static readonly int OnMatchGuideLevelClickFinish = RuntimeId.ToRuntimeId("OnMatchGuideLevelClickFinish");
        public static readonly int OnMatchGuideLevelStepFinish = RuntimeId.ToRuntimeId("OnMatchGuideLevelStepFinish");
        public static readonly int OnMatchGuideLevelAttemptStep = RuntimeId.ToRuntimeId("OnMatchGuideLevelAttemptStep");
        public static readonly int OnMatchStepComplete = RuntimeId.ToRuntimeId("OnMatchStepComplete");
        public static readonly int OnMatchSquareLineStep = RuntimeId.ToRuntimeId("OnMatchSquareLineStep");
        public static readonly int OnMatchShowLastStepPrompt = RuntimeId.ToRuntimeId("OnMatchShowLastStepPrompt");
        public static readonly int OnMatchStepModify = RuntimeId.ToRuntimeId("OnMatchStepModify");
        public static readonly int OnMatchCheckLastStep = RuntimeId.ToRuntimeId("OnMatchCheckLastStep");
        public static readonly int OnMatchContinueBack = RuntimeId.ToRuntimeId("OnMatchContinueBack");
        #endregion

        #region 寻宝
        public static readonly int OnPuzzleLoadWidget = RuntimeId.ToRuntimeId("OnPuzzleLoadWidget");
        public static readonly int OnPuzzleUpdateNextLevel = RuntimeId.ToRuntimeId("OnPuzzleUpdateNextLevel");
        public static readonly int OnPuzzleShowFindTips = RuntimeId.ToRuntimeId("OnPuzzleShowFindTips");
        public static readonly int OnPuzzleFindItem = RuntimeId.ToRuntimeId("OnPuzzleFindItem");
        public static readonly int OnPuzzleUpdateArea = RuntimeId.ToRuntimeId("OnPuzzleUpdateArea");
        public static readonly int OnPuzzleUpdateScale = RuntimeId.ToRuntimeId("OnPuzzleUpdateScale");
        public static readonly int OnPuzzleUpdateScaleFinish = RuntimeId.ToRuntimeId("OnPuzzleUpdateScaleFinish");
        public static readonly int OnPuzzleUseBooster = RuntimeId.ToRuntimeId("OnPuzzleUseBooster");
        public static readonly int OnPuzzlePointerTarget = RuntimeId.ToRuntimeId("OnPuzzlePointerTarget");
        public static readonly int OnPuzzleCircleTarget = RuntimeId.ToRuntimeId("OnPuzzleCircleTarget");
        public static readonly int OnPuzzleLoupeFocusItem = RuntimeId.ToRuntimeId("OnPuzzleLoupeFocusItem");
        public static readonly int OnPuzzleLoupeFinish = RuntimeId.ToRuntimeId("OnPuzzleLoupeFinish");
        public static readonly int OnPuzzleLevelFinish = RuntimeId.ToRuntimeId("OnPuzzleLevelFinish");
        public static readonly int OnPuzzleTargetTween = RuntimeId.ToRuntimeId("OnPuzzleTargetTween");
        public static readonly int OnPuzzleStart = RuntimeId.ToRuntimeId("OnPuzzleStart");
        public static readonly int OnPuzzleUpdateLevel = RuntimeId.ToRuntimeId("OnPuzzleUpdateLevel"); 
        public static readonly int OnPuzzleGMShowAudio = RuntimeId.ToRuntimeId("OnPuzzleGMShowAudio"); 
        public static readonly int OnPuzzleLoadCfgFinish = RuntimeId.ToRuntimeId("OnPuzzleLoadCfgFinish"); 
        public static readonly int OnPuzzleShowSpineAreaLine = RuntimeId.ToRuntimeId("OnPuzzleShowSpineAreaLine"); 
        public static readonly int OnPuzzleQuit = RuntimeId.ToRuntimeId("OnPuzzleQuit"); 
        public static readonly int OnPuzzleUpdateRedot = RuntimeId.ToRuntimeId("OnPuzzleUpdateRedot"); 
        public static readonly int OnPuzzleCollectItem = RuntimeId.ToRuntimeId("OnPuzzleCollectItem"); 
        public static readonly int OnPuzzleHidePointer = RuntimeId.ToRuntimeId("OnPuzzleHidePointer"); 
        public static readonly int OnPuzzleCollectItemBindHead = RuntimeId.ToRuntimeId("OnPuzzleCollectItemBindHead"); 
        public static readonly int OnPuzzleStartTargetShowFinish = RuntimeId.ToRuntimeId("OnPuzzleStartTargetShowFinish"); 
        public static readonly int OnPuzzleShowTarget = RuntimeId.ToRuntimeId("OnPuzzleShowTarget"); 
        public static readonly int OnPuzzleShowVacuum = RuntimeId.ToRuntimeId("OnPuzzleShowVacuum"); 
        #endregion


        #region 主界面
        public static readonly int OnMainTouchPageBtn = RuntimeId.ToRuntimeId("OnMainTouchPageBtn");
        public static readonly int OnMainSelectPage = RuntimeId.ToRuntimeId("OnMainSelectPage");
        public static readonly int OnMainPageTransFinish = RuntimeId.ToRuntimeId("OnMainPageTransFinish");
        public static readonly int OnMainReActive = RuntimeId.ToRuntimeId("OnMainReActive");
        public static readonly int OnMainUpdatePointPos = RuntimeId.ToRuntimeId("OnMainUpdatePointPos");
        public static readonly int OnMainUpdateTrainMaster = RuntimeId.ToRuntimeId("OnMainUpdateTrainMaster");
        public static readonly int OnMainActivityBtnTimeOutZero = RuntimeId.ToRuntimeId("OnMainActivityBtnTimeOutZero");
        public static readonly int OnMainUpdateInviteFriend = RuntimeId.ToRuntimeId("OnMainUpdateInviteFriend");
        public static readonly int OnMainShowCoinFly = RuntimeId.ToRuntimeId("OnMainShowCoinFly");
        public static readonly int OnMainUpdatePropertyWidgetState = RuntimeId.ToRuntimeId("OnMainUpdatePropertyWidgetState");
        public static readonly int OnMainEnterLobbyFlyItemFinish = RuntimeId.ToRuntimeId("OnMainEnterLobbyFlyItemFinish");
        public static readonly int OnMainUpdateTreasure = RuntimeId.ToRuntimeId("OnMainUpdateTreasure");
        public static readonly int OnMainFlyResultStarFinish = RuntimeId.ToRuntimeId("OnMainFlyResultStarFinish");
        #endregion

        #region 活动
        public static readonly int OnTargetTaskUpdate = RuntimeId.ToRuntimeId("OnTargetTaskUpdate");
        public static readonly int OnTargetTaskTimeout = RuntimeId.ToRuntimeId("OnTargetTaskTimeout");
        public static readonly int OnTargetAddCount = RuntimeId.ToRuntimeId("OnTargetAddCount");
        public static readonly int OnTargetTaskFlyFinish = RuntimeId.ToRuntimeId("OnTargetTaskFlyFinish");
        #endregion

        public static readonly int OnUpdteGameItem = RuntimeId.ToRuntimeId("OnUpdteGameItem");
        public static readonly int OnUpdateLiveTime = RuntimeId.ToRuntimeId("OnUpdateLiveTime");
        public static readonly int OnUpdateBuffTime = RuntimeId.ToRuntimeId("OnUpdateBuffTime");
        public static readonly int OnGetAdRewardSucc = RuntimeId.ToRuntimeId("OnGetAdRewardSucc");
        public static readonly int OnExchangeCoinShopSucc = RuntimeId.ToRuntimeId("OnExchangeCoinShopSucc");

        public static readonly int OnLoginFinish = RuntimeId.ToRuntimeId("OnLoginFinish");

        public static readonly int OnRankTimeout = RuntimeId.ToRuntimeId("OnRankTimeout");

        #region 集卡
        public static readonly int OnCardUpdate = RuntimeId.ToRuntimeId("OnCardUpdate");
        public static readonly int OnCardTimeOut = RuntimeId.ToRuntimeId("OnCardTimeOut");
        public static readonly int OnCardPackState = RuntimeId.ToRuntimeId("OnCardPackState");
        public static readonly int OnCardStarState = RuntimeId.ToRuntimeId("OnCardStarState");
        public static readonly int OnCardTotalState = RuntimeId.ToRuntimeId("OnCardTotalState");
        public static readonly int OnCardSelectCollectCard = RuntimeId.ToRuntimeId("OnCardSelectCollectCard");
        #endregion

        #region 签到

        public static readonly int OnSignReqGetReward = RuntimeId.ToRuntimeId("OnSignReqGetReward");
        public static readonly int OnSignDrawClick = RuntimeId.ToRuntimeId("OnSignDrawClick");
        public static readonly int OnSignAcumulativeCellClick = RuntimeId.ToRuntimeId("OnSignAcumulativeCellClick");
        
        #endregion

        #region 修改信息
        public static readonly int OnUserInfoChangeNickName = RuntimeId.ToRuntimeId("OnUserInfoChangeNickName");
        public static readonly int OnUserInfoChangeHead = RuntimeId.ToRuntimeId("OnUserInfoChangeHead");
        public static readonly int OnUserInfoChangeHeadFrame = RuntimeId.ToRuntimeId("OnUserInfoChangeHeadFrame");
        public static readonly int OnUserInfoChangeMedal = RuntimeId.ToRuntimeId("OnUserInfoChangeMedal");
        public static readonly int OnUserInfoChangeNameColor = RuntimeId.ToRuntimeId("OnUserInfoChangeNameColor");
        public static readonly int OnUserInfoBuyItem = RuntimeId.ToRuntimeId("OnUserInfoBuyItem");
        #endregion

        #region 任务

        public static readonly int OnSevenTaskToggleChange = RuntimeId.ToRuntimeId("OnSevenTaskToggleChange");
        public static readonly int OnTaskProgressClick = RuntimeId.ToRuntimeId("OnTaskProgressClick");
        public static readonly int OnDailyTaskDayTimeout = RuntimeId.ToRuntimeId("OnDailyTaskDayTimeout");
        public static readonly int OnTaskCellClick = RuntimeId.ToRuntimeId("OnTaskCellClick");

        #endregion
        
        public static readonly int OnUpdateAdvCount = RuntimeId.ToRuntimeId("OnUpdateAdvCount");
        public static readonly int OnShopBuyProductFinish = RuntimeId.ToRuntimeId("OnShopBuyProductFinish");
        public static readonly int OnShopUpdateProductData = RuntimeId.ToRuntimeId("OnShopUpdateProductData");
        public static readonly int OnUpdateLimitGiftTime = RuntimeId.ToRuntimeId("OnUpdateLimitGiftTime");



        public static readonly int OnMonthCardUpdateData = RuntimeId.ToRuntimeId("OnMonthCardUpdateData");

        public static readonly int OnEamilGetReward = RuntimeId.ToRuntimeId("OnEamilGetReward");
        public static readonly int OnEamilUpdateData = RuntimeId.ToRuntimeId("OnEamilUpdateData");

        public static readonly int OnCommonFlyRewardItem = RuntimeId.ToRuntimeId("OnCommonFlyRewardItem");
        public static readonly int OnCommonUpdateRewardTips = RuntimeId.ToRuntimeId("OnCommonUpdateRewardTips");



        public static readonly int OnNoticeSelectItem = RuntimeId.ToRuntimeId("OnNoticeSelectItem");

        public static readonly int OnTrainMasterClose = RuntimeId.ToRuntimeId("OnTrainMasterClose");

        public static readonly int OnIapBuyLackItemFinish = RuntimeId.ToRuntimeId("OnIapBuyLackItemFinish");
        public static readonly int OnNoticeUpdateData = RuntimeId.ToRuntimeId("OnNoticeUpdateData");


        public static readonly int OnTreasureGetReward = RuntimeId.ToRuntimeId("OnTreasureGetReward");
        public static readonly int OnGuideForceButtonClick = RuntimeId.ToRuntimeId("OnGuideForceButtonClick");
        public static readonly int OnGuideShowGuide = RuntimeId.ToRuntimeId("OnGuideShowGuide");
        public static readonly int OnGuideUsePuzzleLoupe = RuntimeId.ToRuntimeId("OnGuideUsePuzzleLoupe");
        public static readonly int OnMainUpdateQuest = RuntimeId.ToRuntimeId("OnMainUpdateQuest");
        public static readonly int OnQuestFeedbackSucc = RuntimeId.ToRuntimeId("OnQuestFeedbackSucc");

        public static readonly int OnABConfigInitFinish = RuntimeId.ToRuntimeId("OnABConfigInitFinish");
        public static readonly int OnSwitchDataInitFinish = RuntimeId.ToRuntimeId("OnSwitchDataInitFinish");
    }
}