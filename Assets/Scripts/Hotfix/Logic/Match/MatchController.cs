using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using GameCore.Localization;
using GameCore.Logic;
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Logic.GamePlay;
using Hotfix.Utils;
using HotfixCore.Module;
using HotfixCore.MVC;
using HotfixLogic.Match;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic
{
    [MVCDefine("Match", typeof(MatchData), needShowLoading: true)]
    public class MatchController : BaseController
    {
        public override Type MainView { get; } = typeof(MatchMainWindow);

        private MatchData _matchData => (MatchData)Model;
        private MatchMainWindow _window => Module.GetActiveWindow(typeof(MatchMainWindow)) as MatchMainWindow;

        private int[] _itemsId;

        Dictionary<int, int> _totalMatchCount = new Dictionary<int, int>();
        private int _squareMatchCount = 0;
        private int _useStep = 0;
        private int _lastStep = 0;
        private bool _firstPlay = true;
        private bool _resultFinish = false;
        private bool _isEnterGuideLevel;
        private int _guideLevelAttemptCount = 0;

        protected override async UniTask OnInitialized()
        {
            Dispatcher.AddEventListener(GameEventDefine.OnMatchCloseContinue, OnMatchCloseContinue, this);
            Dispatcher.AddEventListener(GameEventDefine.OnMatchCloseClick, OnMatchClose, this);
            Dispatcher.AddEventListener(GameEventDefine.OnReduceMoveStep, OnReduceMoveStep, this);
            Dispatcher.AddEventListener(GameEventDefine.OnMatchCheckLastStep, OnMatchCheckLastStep, this);
            Dispatcher.AddEventListener<EventOneParam<int>>(GameEventDefine.OnMatchStepMoveEnd, OnMatchStepMoveEnd, this);
            Dispatcher.AddEventListener(GameEventDefine.OnMatchNoneMatchToFail, OnMatchNoneMatchToFail, this);
            Dispatcher.AddEventListener(GameEventDefine.OnMatchRestart, OnMatchRestart, this);
            Dispatcher.AddEventListener(GameEventDefine.OnMatchCancelItem, OnMatchCancelItem, this);
            Dispatcher.AddEventListener(GameEventDefine.OnCheckMatchResult, OnCheckMatchResult, this);
            Dispatcher.AddEventListener<EventTwoParam<int,int>>(GameEventDefine.OnMatchGuideLevelAttemptStep,OnMatchGuideLevelAttemptStep,this);
            Dispatcher.AddEventListener<EventOneParam<int>>(GameEventDefine.OnMatchStepComplete, OnMatchStepComplete, this);
            Dispatcher.AddEventListener<EventOneParam<LevelData>>(GameEventDefine.OnMatchGuideLevelStartFinish, OnMatchGuideLevelStart,
                this);
            Dispatcher.AddEventListener<EventTwoParam<int, Vector2Int>>(GameEventDefine.OnMatchReqUseItem, OnMatchReqUseItem, this);
            Dispatcher.AddEventListener<EventOneParam<Dictionary<int, int>>>(GameEventDefine.OnMatchStepMove,
                OnMatchStepMove, this);
            Dispatcher.AddEventListener<EventTwoParam<int, int>>(GameEventDefine.OnMatchAddTargetNum,
                OnMatchAddTargetNum, this);
            Dispatcher.AddEventListener<EventOneParam<int>>(GameEventDefine.OnMatchUseItem, OnMatchUseItem, this);
            Dispatcher.AddEventListener<EventTwoParam<int, bool>>(GameEventDefine.OnMatchAddStep, OnMatchAddStep, this);
            Dispatcher.AddEventListener<EventOneParam<int>>(GameEventDefine.OnMatchScoreChanged, OnMatchScoreChanged,
                this);
            Dispatcher.AddEventListener<EventOneParam<int>>(GameEventDefine.OnMatchDestroySquare, OnMatchDestroySquare,
                this);
            Dispatcher.AddEventListener(GameEventDefine.OnMatchMidwayClose, OnMatchMidwayClose, this);
            Dispatcher.AddEventListener<EventOneParam<string>>(GameEventDefine.OnMatchTriple, OnMatchTriple, this);
            Dispatcher.AddEventListener(GameEventDefine.OnMatchResultFail, OnMatchResultFail, this);
            Dispatcher.AddEventListener(GameEventDefine.OnOpenMatchGmChangePanel, OnOpenMatchGMChangePanel, this);
            Dispatcher.AddEventListener(GameEventDefine.OnMatchFinish, OnMatchFinish, this);
            Dispatcher.AddEventListener(GameEventDefine.OnMatchUpdateResultCoin, UpdateResultCoin, this);
            Dispatcher.AddEventListener(GameEventDefine.OnMatchGuideLevelAllFinish, OnMatchGuideLevelAllFinish, this);
            Dispatcher.AddEventListener<EventOneParam<int>>(GameEventDefine.OnMatchLinkSquareCount, OnMatchLinkSquareCount, this);
            Dispatcher.AddEventListener<EventOneParam<Color>>(GameEventDefine.OnMatchUpdateLinkColor, OnMatchUpdateLinkColor, this);

            InitData();
            if (_matchData.EnterLevelType == MatchLevelType.C || (_matchData.EnterLevelType == MatchLevelType.Editor &&
                                                                  MatchManager.Instance.CurrentMatchGameType ==
                                                                  MatchGameType.TowDots))
            {
                _itemsId = new int[] { 1108, 1107, 1111, 1130 };
            }
            else
            {
                _itemsId = new int[] { 1110,1108 , 1107, 1111, 1109 };
            }

            await EnterMatch();

            CommonLoading.ShowLoading(LoadingEnum.Match, 1.0f, () => { 
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchLoadingFinish); 

                if (MatchManager.Instance.MaxLevel <= ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("ContinueLevel")) {
                    HotUpdateManager.Instance.Hide();
                }

                // 新手引导不调用连胜道具
                if (!GridSystem.GetExecuteGuideLevel()) {
                    
                    ShowTargetAnim(() => {
                        G.UIModule.ScreenLock(MatchConst.MatchShowTarget, false);

                        GuideManager.Instance.PlayMatchGuide(_matchData);
                        if (!GuideManager.Instance.IsGuiding()) {
                            MatchManager.Instance.GameBeginUseElements();
                        } else {
                            G.UIModule.ScreenLock(MatchConst.MatchLockByGenNew, true, 0.3f);
                        }
                    });
                }
            });

            _window.UpdateLv();

            G.UIModule.CloseAllWithOut(new List<string>() { "HotfixLogic.MatchMainWindow", "HotfixLogic.MainWindow", "HotfixLogic.CommonLoading" });
        }

        private void ShowTargetAnim(Action callback) {
            G.UIModule.ScreenLock(MatchConst.MatchShowTarget, true, 2f);
            _window.ShowTarget(callback: callback);
        }

        private void OnMatchGuideLevelAttemptStep(EventTwoParam<int,int> obj)
        {
            //统计在该引导关卡，尝试的次数
            int step = obj.Arg2;
            int levelId = obj.Arg1;
            int guideId = _matchData.GetGuideIdByGuideLevelId(levelId);
            _guideLevelAttemptCount++;
            Dictionary<string, object> param = new Dictionary<string, object>()
            {
                ["guide_id"] = guideId,
                ["step"] = step,
                ["attempt_counting"] = _guideLevelAttemptCount,
            };
            CommonUtil.LogEvent(LogEventKeyDefine.MatchGuideLevelAttempt, param);
        }

        private void OnMatchGuideLevelStart(EventOneParam<LevelData> obj)
        {
            int step = obj.Arg.id;
            int guideId = _matchData.GetGuideIdByGuideLevelId(step);
            GuideManager.Instance.SendLog(guideId, 1);
        }

        private void OnMatchStepComplete(EventOneParam<int> obj)
        {
            int step = obj.Arg;
            int guideId = _matchData.GetGuideIdByGuideLevelId(step);
            GuideManager.Instance.SendLog(guideId, 3);
            _guideLevelAttemptCount = 0;
        }

        private void OnMatchLinkSquareCount(EventOneParam<int> obj)
        {
            _window.PlayRangeLineAnim(obj.Arg);
        }

        private void OnMatchUpdateLinkColor(EventOneParam<Color> param) {
            _window.OnMatchUpdateLinkColor(param.Arg);
        }

        private void OnMatchGuideLevelAllFinish()
        {
            _isEnterGuideLevel = false;
            string key = _matchData.EnterLevelType == MatchLevelType.A
                ? GamePrefsKey.MatchALevelGuideFinish
                : GamePrefsKey.MatchBLevelGuideFinish;
            PlayerPrefsUtil.SetInt($"{key}_{_matchData.CurrentLevelData.id}", 1);


            // 界面更新
            _window.SetMatchInfoVisible(true,true, () =>
            {
                ShowTargetAnim(() => {
                    G.UIModule.ScreenLock(MatchConst.MatchShowTarget, false);

                    var guideId = 0;

                    if (_matchData.EnterLevelType == MatchLevelType.A)
                    {
                        if (_matchData.CurrentLevelData.id == 1)
                        {
                            guideId = 17;
                            
                        }
                        else if (_matchData.CurrentLevelData.id == 3)
                        {
                            guideId = 18;
                        }
                    }

                    if (_matchData.EnterLevelType == MatchLevelType.B)
                    {
                        if (_matchData.CurrentLevelData.id == 1)
                        {
                            guideId = 1;
                        }
                        if (_matchData.CurrentLevelData.id == 3)
                        {
                            guideId = 4;
                        }

                        if (_matchData.CurrentLevelData.id == 6)
                        {
                            guideId = 13;
                        }
                    }

                    if (guideId != 0) {
                        G.UIModule.ScreenLock(MatchConst.MatchLockByGenNew, true, 0.3f);
                        GuideManager.Instance.StartGuide(guideId).Forget();
                    }
                });
            });
        }

        private void OnMatchReqUseItem(EventTwoParam<int, Vector2Int> obj)
        {
            int useItemId = obj.Arg1;
            if (GuideManager.Instance.IsGuiding())
            {
                GuideManager.Instance.FinishCurrentGuide();
                _window.RefreshItem();
                //新手引导直接送道具，必定会使用成功
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchUseItemSuccess, obj);
                return;
            }

            G.UIModule.ScreenLock(MatchConst.MatchLockByUseItem, true, 5);
            _window.UseMatchItem(useItemId, (result) =>
            {
                if (result)
                {
                    G.EventModule.DispatchEvent(GameEventDefine.OnMatchUseItemSuccess, obj);
                    DifficultyStrategyManager.Instance.SetCurrentUseItemState(true);
                }
                else
                {
                    CommonUtil.ShowCommonTips(LocalizationPool.Get("Common/WeakNet"));
                    G.EventModule.DispatchEvent(GameEventDefine.OnMatchUseItemFail);
                }

                G.UIModule.ScreenLock(MatchConst.MatchLockByUseItem, false);
            });
        }

        private void OnOpenMatchGMChangePanel()
        {
            UniTask.Create(async () =>
            {
                var window = await Module.OpenChildWindow<MatchGMChangeBoard>();
                LevelMapImageDB db = ConfigMemoryPool.Get<LevelMapImageDB>();
                int id = db.GetLevelInPage(MatchManager.Instance.CurLevelID, MatchManager.Instance.MaxLevel);
                LevelMapImage config = db[id + 1];
                if(string.IsNullOrEmpty(ElementSystem.MatchBgColor))
                    ElementSystem.MatchBgColor = config.matchBgColor;
                window.SetInit(ElementSystem.MatchBgColor);
            }).Forget();
        }

        private void OnMatchCloseContinue()
        {
            QuitMatch(false);
        }

        private void OnMatchTriple(EventOneParam<string> obj)
        {
            //火箭连火箭不播
            if (obj.Arg == $"{ElementType.Rocket}-{ElementType.Rocket}")
                return;
            _window.PlaySpine(MatchConst.SPINE_TRIPLE);
        }

        private void OnCheckMatchResult()
        {
            if (_matchData.IsMatchDone)
                return;

            ECheckMatchResult state = _matchData.CheckMatchState();
            if (state == ECheckMatchResult.Success)
            {
                _window.PlaySpine(MatchConst.SPINE_SUCCESS, isLoop: true);
                if (_matchData.MoveStep > 0)
                {
                    //剩余步数计分
                    var difficulty = MatchManager.Instance.GetMatchDifficulty(_matchData.CurrentLevelData.difficulty);
                    LevelDiffScoreDB db = ConfigMemoryPool.Get<LevelDiffScoreDB>();
                    int stepScore = db.CalScore(difficulty, _matchData.MoveStep);
                    Logger.Debug("剩余步数计分：" + stepScore);
                    MatchManager.Instance.AddScore(stepScore);
                }

                //结算切换金币目标
                LevelTargetSystem.Instance.AddTarget((int)ElementIdConst.Coin, 0);

                var baseCoin = MatchManager.Instance.GetBaseCoin(_matchData.CurrentLevelData.difficulty);
                if (MatchManager.Instance.GetLevelStar(MatchManager.Instance.CurLevelID) != 0) {
                    baseCoin = 0;
                }
                _window.SwitchTargetToCoin(baseCoin);
                

                _matchData.IsMatchDone = true;
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchRemainStep);
            }
            else if (state == ECheckMatchResult.Failure)
            {
                _window.PlaySpine(MatchConst.SPINE_FAIL, isLoop: true);
                ShowResultLose();

                _matchData.IsMatchDone = true;
            }
        }

        private void OnMatchCancelItem()
        {
            _window.RefreshItem();
        }

        private void OnMatchRestart()
        {
            if (UserData[0] is MatchLevelType matchType)
            {
                UniTask.Create(async () =>
                {
                    int levelId = MatchManager.Instance.CurLevelID;

                    var levelData = await LevelManager.Instance.GetLevel(matchType, levelId);
                    _matchData.Restart(levelData);
                    InitData();
                    _window.Restart();
                    _window.UpdateLv();
                    await MatchManager.Instance.Restart();
                    G.EventModule.DispatchEvent(GameEventDefine.OnMatchRestartComplete);
                    Module.CloseCurrentWindow();
                }).Forget();
            }
            else
            {
                QuitMatch();
            }
        }

        private void OnMatchAddTargetNum(EventTwoParam<int, int> obj)
        {
            LevelTargetSystem.Instance.AddTargetNum(obj.Arg1, obj.Arg2);
        }

        private void OnMatchNoneMatchToFail()
        {
            ShowResultLose(true);
        }

        private void ShowResultLose(bool mustLose = false)
        {
            var timerID = 0;
            var levelData = _matchData.CurrentLevelData;
            Logger.Debug("Show Match Lose Window!!!");
            var id = timerID;
            timerID = G.TimerModule.AddTimer(() =>
            {
                //棋盘上没有任何可以匹配的棋子，洗牌、强制变换棋子等都没用，就直接当做失败
                UniTask.Create(async () =>
                {
                    var window =
                        await Module.OpenChildWindow<MatchResultLose>(false, mustLose, LevelTargetSystem.Instance.TargetElements);
                    window?.SetLevel(levelData);
                }).Forget();
                G.TimerModule.RemoveTimer(id);
            }, 0.5f);
        }

        private void OnMatchStepMoveEnd(EventOneParam<int> arg)
        {
            _lastStep = arg.Arg;
            _matchData.ReduceMoveStep(arg.Arg);

            //结算上报   上报放在这里有隐患，如果中途退出，数据会丢失 TODO....
            int starCount = MatchManager.Instance.CalStarCountByScore(MatchManager.Instance.TotalScore,
                _matchData.CurrentLevelData.fullScore);

            int coinCount = MatchManager.Instance.GetLevelCoinCount(_matchData.CurrentLevelData.difficulty,
                starCount);

            if (LevelTargetSystem.Instance.TargetElements.TryGetValue((int)ElementIdConst.Coin, out int n))
            {
                Logger.Debug($"结算获取金币:{n}");
                coinCount += n;
            }

            Logger.Debug($"OnMatchStepMoveEnd 结算获取金币:{coinCount}");
            
            RequestGameEnd(true, starCount, coinCount);

            G.UIModule.ShowUIAsync<MatchResultWin>("", _matchData.CurrentLevelData, _lastStep, _firstPlay, coinCount);
        }

        private void UpdateResultCoin() {
            int starCount = MatchManager.Instance.CalStarCountByScore(MatchManager.Instance.TotalScore,
                _matchData.CurrentLevelData.fullScore);

            int coinCount = MatchManager.Instance.GetLevelCoinCount(_matchData.CurrentLevelData.difficulty,
                starCount);

            if (LevelTargetSystem.Instance.TargetElements.TryGetValue((int)ElementIdConst.Coin, out int n))
            {
                Logger.Debug($"结算获取金币:{n}");
                coinCount += n;
            }

            Logger.Debug($"UpdateResultCoin 结算获取金币:{coinCount}");

            var param = EventTwoParam<int, float>.Create(coinCount, 0.3f);
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchSetResultCoin, param);
        }
        
        private void InitData()
        {
            _firstPlay = MatchManager.Instance.GetLevelStar(MatchManager.Instance.CurLevelID) == 0;
            _useStep = 0;
            _lastStep = 0;
            _totalMatchCount.Clear();
            _squareMatchCount = 0;
            _resultFinish = false;
            if (_matchData.IsEnterByEditor())
            {
                MatchManager.Instance.SetLevelType(MatchLevelType.Editor);
            }
        }

        private void RequestGameEnd(bool isWin, int starCount, int coinCount)
        {
            int targetCount = 0;

            
            var targetItemID = G.TargetTaskModule.TargetItemID;
            if (_totalMatchCount.ContainsKey(targetItemID))
            {
                targetCount = _totalMatchCount[targetItemID];
            }
            else if (targetItemID == -1)
            {
                targetCount = _squareMatchCount;
            }

            // 如果目标物是火箭，需要再计算横向火箭
            if (targetItemID == (int)ElementIdConst.Rocket && _totalMatchCount.TryGetValue((int)ElementIdConst.RocketHorizontal, out var value))
            {
                targetCount += value;
            }

            if (!G.TargetTaskModule.TargetTaskIsOpen()) {
                targetCount = 0;
            }

            List<ItemData> items = null;
            if (isWin)
            {
                // 重复挑战不添加目标任务 和 金币
                if (MatchManager.Instance.GetLevelStar(MatchManager.Instance.CurLevelID) != 0)
                {
                    targetCount = 0;
                    coinCount = 0;
                }

                items = new List<ItemData>(10) { new ItemData("star", starCount) };

                if (coinCount > 0) items.Add(new ItemData("coin", coinCount));
                if (IsCanAddUnlockReward(MatchManager.Instance.CurLevelID, out string rewardList))
                {
                    if (!string.IsNullOrEmpty(rewardList))
                    {
                        var rewardItems = CommonUtil.GetItemDatasByStr(rewardList);
                        for (int i = 0; i < rewardItems.Count; i++)
                        {
                            items.Add(rewardItems[i]);
                        }
                    }
                }
            }
            else
            {
                targetCount = 0;
            }

            _window.ReportMatchResult(MatchManager.Instance.CurLevelID, isWin, _useStep, 
                                            _matchData.CurrentLevelData.stepLimit, targetCount, items);
        }

        private bool IsCanAddUnlockReward(int levelId,out string rewardList)
        {
            rewardList = "";
            if (!IsUnlockItemPreLevel(levelId))
                return false;
            int maxLevel = MatchManager.Instance.MaxLevel;
            ConstConfigDB db = ConfigMemoryPool.Get<ConstConfigDB>();
            rewardList = db.GetConfigStrVal("MBeginBoosterUnlockReward");
            return maxLevel == levelId;
        }

        private bool IsUnlockItemPreLevel(int levelId)
        {
            ConstConfigDB db = ConfigMemoryPool.Get<ConstConfigDB>();
            int unlockLv = db.GetConfigIntVal("MBeginBoosterUnlock");
            return unlockLv - 1 == levelId;
        }
        
        private void OnMatchResultFail()
        {
            RequestGameEnd(false, 0, 0);
        }

        private void OnMatchUseItem(EventOneParam<int> obj)
        {
            _window.ShowUseItemTips(obj.Arg);
        }

        private void OnMatchScoreChanged(EventOneParam<int> obj)
        {
            int score = obj.Arg;
            if (score <= 0)
            {
                return;
            }

            _window.RefreshScore(score);
        }

        private void OnReduceMoveStep()
        {
            if (_matchData.IsMatchDone)
                return;

            _useStep++;
            _matchData.ReduceMoveStep(1);
            
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchStepModify,
                EventOneParam<int>.Create(_matchData.MoveStep));
        }

        private void OnMatchCheckLastStep() {
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchControlStep, EventOneParam<int>.Create(_matchData.MoveStep));
        }

        private void OnMatchStepMove(EventOneParam<Dictionary<int, int>> result)
        {
            // if(_matchData.IsMatchDone)
            //     return;
            LevelTargetSystem.Instance.CalculateTarget(result.Arg);

            // if (!_resultFinish)  {
                // ECheckMatchResult state = _matchData.CheckMatchState();
                // if (state == ECheckMatchResult.Actioning || _matchData.MoveStep > 0)
                // {
                    foreach (var kp in result.Arg)
                    {
                        AddMatchCount(kp.Key, kp.Value);
                    }
                // }
            // }
        }

        private void AddMatchCount(int elementId, int count)
        {
            if (_totalMatchCount.ContainsKey(elementId))
            {
                _totalMatchCount[elementId] += count;
            }
            else
            {
                _totalMatchCount.Add(elementId, count);
            }
        }

        private async UniTask EnterMatch()
        {
            //先这样兼容旧的系统，后面全面重构完后，就只有一个入口了
            var matchType = MatchManager.Instance.CurrentMatchGameType;
            if (matchType == MatchGameType.NormalMatch)
            {
                _isEnterGuideLevel = _matchData.TryGetGuideLevel(out var guideLevels);
                List<LevelData> guideLevelsData = null;
                if (_isEnterGuideLevel)
                {
                    guideLevelsData = await LevelManager.Instance.GetGuideLevels(guideLevels);
                    guideLevelsData.Add(_matchData.CurrentLevelData);
                }

                await MatchManager.Instance.Start(_matchData.CurrentLevelData, guideLevelsData);
            }
            else if(matchType == MatchGameType.TowDots)
            {
                await MatchBoot.BootStart(_matchData.CurrentLevelData, MatchServiceType.TowDots, _window);
            }
            InitWindowState();
        }

        private void InitWindowState()
        {
            _window.InitTarget(_matchData);
            _window.InitItems(_itemsId);
            _window.SetMatchInfoVisible(!_isEnterGuideLevel);
        }

        private void OnMatchClose()
        {
            QuitMatch();
        }

        private void OnMatchMidwayClose()
        {
            RequestGameEnd(false, 0, 0);
            QuitMatch();
        }

        private void OnMatchAddStep(EventTwoParam<int, bool> param)
        {
            var advStep = param.Arg1;

            _matchData.AddStep(advStep);
            
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchStepModify,
                EventOneParam<int>.Create(_matchData.MoveStep));
        }

        private void QuitMatch(bool isQuit = true)
        {
            if (!_matchData.IsEnterByEditor())
            {
                if (isQuit)
                {
                    G.SceneModule.SetCurSceneType(SceneType.Main);
                }

                MVCManager.Instance.UnloadModule(MVCEnum.Match.ToString());
                MVCManager.Instance.ActiveModule(MVCEnum.Main.ToString(), true, succ =>
                {
                    if (succ)
                    {
                        if (isQuit) return;

                        RecordWinStreakBooster();
                        MatchManager.Instance.SetMatchLevelID(MatchManager.Instance.CurLevelID);
                        MVCManager.Instance
                            .ActiveModule(MVCEnum.Match.ToString(), true,null, MatchManager.Instance.CurrentMatchLevelType)
                            .Forget();
                        G.SceneModule.SetCurSceneType(SceneType.Match);
                    }
                }).Forget();
                return;
            }

            MVCManager.Instance.UnloadModule(MVCEnum.Match.ToString());
            MVCManager.Instance.ActiveModule(MVCEnum.MatchEditor.ToString(), true).Forget();
        }

        private void RecordWinStreakBooster()
        {
            if (!CheckHasWinStreak()) return;

            int winStreakCount = MatchManager.Instance.GetWinStreakBox();

            var rewardStr = ConfigMemoryPool.Get<streakRewardDB>()[winStreakCount].reward;
            List<int> rewardIDList = new List<int>();
            var db = ConfigMemoryPool.Get<ItemElementDictDB>();
            foreach (var item in rewardStr.Split('|'))
            {
                string itemName = item.Split('*')[0];
                int useCount = int.Parse(item.Split('*')[1]);
                for (int i = 0; i < useCount; i++)
                {
                    rewardIDList.Add(db.GetElementId(itemName));
                }
            }
            MatchManager.Instance.SetWinStreakElements(rewardIDList);
        }

        private bool CheckHasWinStreak()
        {
            var beginLv = MatchManager.Instance.CurLevelID;
            bool isWinStreak = MatchManager.Instance.GetWinStreakBox() > 0;
            bool isOpenWinStreak = beginLv >= ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("WinStreakLV");

            return isWinStreak && isOpenWinStreak;
        }

        private void OnMatchDestroySquare(EventOneParam<int> param)
        {
            _squareMatchCount++;
            if (param.Arg <= 4)
            {
                _window.PlaySpine(MatchConst.SPINE_IDLE);
            }
            else
            {
                _window.PlaySpine(MatchConst.SPINE_HELLO);
            }
        }

        protected override void OnDestroy()
        {
            var matchType = MatchManager.Instance.CurrentMatchGameType;
            if (matchType == MatchGameType.NormalMatch)
                MatchManager.Instance.Quit();
            else if (matchType == MatchGameType.TowDots)
                MatchBoot.BootExit();
            G.UIModule.SetSceneCamera(null);
            DifficultyStrategyManager.Instance.SetCurrentUseItemState(false);
        }

        private void OnMatchFinish() {
            Action callback = () => {
                G.UIModule.ScreenLock(MatchConst.MatchDoneLockReason, true, 0.5f);
                DOVirtual.DelayedCall(0.5f, () => {
                    if (_matchData.MoveStep > 0) {
                        _window.TweenRemainStep(_matchData.MoveStep);
                    }
                    G.EventModule.DispatchEvent(GameEventDefine.OnDoMatchStepJudge,
                        EventThreeParam<int, Vector3, Vector2>.Create(_matchData.MoveStep, _window.GetStepWorldPosition(), _window.GetCoinScreenPosition()));

                }).SetAutoKill(true);
            };
            _resultFinish = true;
            G.UIModule.ShowUIAsync<MatchWinFinish>("", callback);
        }
    }
}