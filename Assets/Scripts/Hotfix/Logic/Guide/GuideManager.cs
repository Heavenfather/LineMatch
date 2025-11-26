using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameConfig;
using GameCore.Singleton;
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixCore.Module;
using HotfixCore.MVC;
using HotfixLogic.Match;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic
{
    public class GuideManager : MonoSingleton<GuideManager>
    {
        private int _currentGuideId = -1;
        private bool _isGuiding;
        private Dictionary<string, Func<GuideConfig, BaseModel, bool>> _bindConditionFunc;
        private GuideConfigDB _guideDB;
        private List<int> _waitCheckGuideList;

        /// <summary>
        /// 当前引导Id
        /// </summary>
#if UNITY_EDITOR
        [Sirenix.OdinInspector.ShowInInspector,Sirenix.OdinInspector.LabelText("当前引导Id")]
#endif
        public int CurrentGuideId => _currentGuideId;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/删除引导数据")]
        public static void ClearGuide()
        {
            var configDb = ConfigMemoryPool.Get<GuideConfigDB>();
            for (int i = 0; i < configDb.All.Length; i++)
            {
                PlayerPrefsUtil.SetInt($"{GamePrefsKey.GUIDEFINISHKEY}_{configDb.All[i].id}", 0);
            }
        }

        [Sirenix.OdinInspector.LabelText("引导Id"),Sirenix.OdinInspector.InlineButton("OnTestStartGuide","开始")]
        public int TestGuideId;

        private void OnTestStartGuide()
        {
            var configDb = ConfigMemoryPool.Get<GuideConfigDB>();
            if (!configDb.IsContain(TestGuideId))
            {
                Logger.Error($"不存在的引导Id:{TestGuideId}");
                return;
            }
            PlayerPrefsUtil.SetInt($"{GamePrefsKey.GUIDEFINISHKEY}_{TestGuideId}", 0);
            StartGuide(TestGuideId).Forget();
        }
#endif
        
        protected override void OnAwake()
        {
            G.EventModule.AddEventListener<EventOneParam<string>>(GameEventDefine.OnWindowClose, OnWindowClose, this);
            G.EventModule.AddEventListener<EventOneParam<string>>(GameEventDefine.OnWindowOpen, OnWindowOpen, this);
            G.EventModule.AddEventListener<EventOneParam<string>>(GameEventDefine.OnGuideForceButtonClick, OnForceButtonClick, this);
            G.EventModule.AddEventListener(GameEventDefine.OnPuzzleUpdateScaleFinish, OnPuzzleUpdateScaleFinish, this);
            G.EventModule.AddEventListener(GameEventDefine.OnPuzzleStartTargetShowFinish, OnPuzzleStartTargetShowFinish, this);
            G.EventModule.AddEventListener<EventOneParam<MainPageType>>(GameEventDefine.OnMainPageTransFinish, OnMainPageTransFinish, this);
            G.EventModule.AddEventListener(GameEventDefine.OnPuzzleQuit, CheckCardGuide, this);

            _guideDB = ConfigMemoryPool.Get<GuideConfigDB>();
            _currentGuideId = -1;
            _bindConditionFunc = new Dictionary<string, Func<GuideConfig, BaseModel, bool>>()
            {
                // [MVCEnum.Match.ToString()] = OnEnterMatch,
                // [MVCEnum.Main.ToString()] = OnEnterMain,
                [MVCEnum.Puzzle.ToString()] = OnEnterPuzzle,
            };
            _waitCheckGuideList = new List<int>();
            for (int i = 0; i < _guideDB.All.Length; i++)
            {
                if (!IsGuideFinish(_guideDB.All[i].id))
                {
                    _waitCheckGuideList.Add(_guideDB.All[i].id);
                }
            }
        }

        public void CheckAndExecuteGuide(string mvcName, BaseModel model)
        {
            if (!_guideDB.HasTriggerModule(mvcName))
                return;
            
            for (int i = 0; i < _waitCheckGuideList.Count; i++)
            {
                int guideId = _waitCheckGuideList[i];
                var  guideData = GetGuideConfig(guideId);

                if (guideData.triggerModule != mvcName)
                    continue;

                if (IsGuideFinish(guideId))
                {
                    _waitCheckGuideList.Remove(guideId);
                    continue;
                }

                string currentType = G.SceneModule.CurSceneType.ToString();
                if(currentType != guideData.triggerModule)
                    continue;

                if (_bindConditionFunc.ContainsKey(mvcName))
                {
                    if (_bindConditionFunc[mvcName](guideData, model))
                    {
                        StartGuide(guideId).Forget();
                        break;
                    }
                }
            }
        }

        public void PlayMatchGuide(BaseModel model) {
            var mvcName = MVCEnum.Match.ToString();
            if (!_guideDB.HasTriggerModule(mvcName))
                return;
            
            for (int i = 0; i < _waitCheckGuideList.Count; i++)
            {
                int guideId = _waitCheckGuideList[i];
                var  guideData = GetGuideConfig(guideId);

                if (guideData.triggerModule != mvcName)
                    continue;

                if (IsGuideFinish(guideId))
                {
                    _waitCheckGuideList.Remove(guideId);
                    continue;
                }

                string currentType = G.SceneModule.CurSceneType.ToString();
                if(currentType != guideData.triggerModule)
                    continue;

                if (OnEnterMatch(guideData, model))
                {
                    StartGuide(guideId).Forget();
                    break;
                }
            }
        }

        public bool CheckMainGuide() {
            var mvcName = "Main";

            if (!_guideDB.HasTriggerModule(mvcName))
                return false;
            
            for (int i = 0; i < _waitCheckGuideList.Count; i++)
            {
                int guideId = _waitCheckGuideList[i];

                var  guideData = GetGuideConfig(guideId);
                if (guideData.triggerModule != mvcName)
                    continue;

                if (IsGuideFinish(guideId))
                {
                    _waitCheckGuideList.Remove(guideId);
                    continue;
                }

                string currentType = G.SceneModule.CurSceneType.ToString();
                if(currentType != guideData.triggerModule)
                    continue;
                
                if (OnEnterMain(guideData, null)) {
                    return true;
                }
            }
            
            return false;
        }

        private GuideConfig GetGuideConfig(int guideId) {
            ref readonly GuideConfig guideData = ref _guideDB[guideId];
            return guideData;
        }


        // 主界面的引导单独做
        public void PlayMainGuid() {
            var mvcName = "Main";

            if (!_guideDB.HasTriggerModule(mvcName))
                return;
            
            for (int i = 0; i < _waitCheckGuideList.Count; i++)
            {
                int guideId = _waitCheckGuideList[i];

                var  guideData = GetGuideConfig(guideId);
                if (guideData.triggerModule != mvcName)
                    continue;

                if (IsGuideFinish(guideId))
                {
                    _waitCheckGuideList.Remove(guideId);
                    continue;
                }

                string currentType = G.SceneModule.CurSceneType.ToString();
                if(currentType != guideData.triggerModule)
                    continue;
                
                if (OnEnterMain(guideData, null)) {
                    StartGuide(guideId).Forget();
                }
            }
        }

        public async UniTask<bool> StartGuide(int guideId)
        {
            var guideData = FindGuideData(guideId);

            if (IsGuideFinish(guideId))
            {
                if (guideData is { nextId: > 0 } && !IsGuideFinish(guideData.nextId))
                {
                    EnterNextGuide(guideData.nextId, false);
                }
                return false;
            }

            if (!string.IsNullOrEmpty(guideData.nodePath))
            {
                float time = 0;
                while (true)
                {
                    if (GameObject.Find(guideData.nodePath) != null)
                    {
                        break;
                    }
                    time += Time.deltaTime;
                    if (time > 2)
                    {
                        Logger.Debug($"{guideId} 引导目标节点不存在:{guideData.nodePath}");
                        CloseGuide();
                        return false;
                    }
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                }
            }

            if (guideData.id > 0)
            {
                _currentGuideId = guideData.id;
                _isGuiding = true;
                // SendLog(guideId, 1);
                G.UIModule.ShowUIAsync<GuideMainWindow>("", guideData);
                G.EventModule.DispatchEvent(GameEventDefine.OnGuideTrigger,
                    EventOneParam<int>.Create(guideData.id));

                return true;
            }

            Logger.Debug($"无法找到引导数据:{guideId}");
            return false;
        }

        public void ForceCloseGuide()
        {
            bool bResult = G.UIModule.HasWindow<GuideMainWindow>();
            if (bResult)
            {
                G.UIModule.CloseUI<GuideMainWindow>();
                if (_currentGuideId > 0)
                {
                    FinishGuide(_currentGuideId, true);
                }
            }
        }
        
        /// <summary>
        /// 动态更新引导节点路径
        /// </summary>
        /// <param name="guideId"></param>
        /// <param name="nodePath"></param>
        public void UpdateShowNode(int guideId, string nodePath)
        { 
            G.UIModule.GetUIAsync<GuideMainWindow>(window =>
            {
                if(window == null)
                    return;
                ref readonly GuideConfig guideData = ref _guideDB[guideId];
                window.RefreshHole(new HoleConfig(guideData.id, guideData.holeData, nodePath,nodePath, guideData.holeShape));
                window.SetFingerTouch(nodePath);
            });
        }

        public void HideTouchGuide()
        {
            G.UIModule.GetUIAsync<GuideMainWindow>(window =>
            {
                if (window == null)
                {
                    return;
                }

                window.HideFingerGuide();
            });
        }

        public void HideGuideLine()
        {
            G.UIModule.GetUIAsync<GuideMainWindow>(window =>
            {
                if (window == null)
                {
                    return;
                }

                window.HideLine();
            });
        }

        public void ActiveMaskRaycast()
        {
            G.UIModule.GetUIAsync<GuideMainWindow>(window =>
            {
                if (window == null)
                {
                    return;
                }

                window.ActiveMaskRaycast();
            });
        }

        public void FinishGuide(int guideId,bool skipNext = false)
        {
            if (IsGuideFinish(guideId))
                return;

            PlayerPrefsUtil.SetInt($"{GamePrefsKey.GUIDEFINISHKEY}_{guideId}", 1);

            SendLog(guideId, 3);
            _waitCheckGuideList.Remove(guideId);
            var guideData = FindGuideData(guideId);
            if (!string.IsNullOrEmpty(guideData.rewardId))
            {
                G.HttpModule.ReqGetGuideReward(guideData.rewardId, (msg, code) => { });
            }
            if (!skipNext && guideData.nextId > 0)
            {
                EnterNextGuide(guideData.nextId, true);
            }
            else
            {
                _isGuiding = false;
                CloseGuide();
                G.EventModule.DispatchEvent(GameEventDefine.OnGuideFinish, EventOneParam<int>.Create(guideId));
            }

            if (guideData.finishLock > 0)
            {
                FinishLockTime(guideData.finishLock).Forget();
            }
        }

        public void FinishCurrentGuide()
        {
            if (_currentGuideId <= 0)
                return;
                
            if (IsGuideFinish(_currentGuideId))
                return;
            
            FinishGuide(_currentGuideId);
        }

        public bool IsGuiding(int guideId = 0)
        {
            if(guideId != 0)
                return _currentGuideId == guideId;
            return _isGuiding;
        }

        public void CloseGuide()
        {
            _currentGuideId = -1;
            G.UIModule.CloseUI<GuideMainWindow>();
        }

        public bool IsGuideFinish(int guideId)
        {
            int state = PlayerPrefsUtil.GetInt($"{GamePrefsKey.GUIDEFINISHKEY}_{guideId}", 0);
            if (state == 0)
            {
                return false;
            }

            List<int> nextIds = new List<int>();
            _guideDB.RefNextGuideList(guideId, ref nextIds);
            for (int i = 0; i < nextIds.Count; i++)
            {
                int nextGuideState = PlayerPrefsUtil.GetInt($"{GamePrefsKey.GUIDEFINISHKEY}_{nextIds[i]}", 0);
                if (nextGuideState == 0)
                    return false;
            }

            return true;
        }

        public GuideConfig FindGuideData(int guideId)
        {
            return _guideDB[guideId];
        }

        /// <summary>
        /// 上报引导打点到中台
        /// </summary>
        /// <param name="guideId">引导id</param>
        /// <param name="state">步骤 1-开始 2-触发 3-完成</param>
        public void SendLog(int guideId,int state)
        {
            //上报事件
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("guide_id", guideId);
            data.Add("guide_step", state);
            CommonUtil.LogEvent(LogEventKeyDefine.GuideFinish, data);
        }

        private void OnForceButtonClick(EventOneParam<string> obj)
        {
            if(!IsGuiding())
                return;
            string nodePath = obj.Arg;
            var guideData = FindGuideData(_currentGuideId);
            if (guideData.nodePath == nodePath)
            {
                FinishCurrentGuide();
            }
        }
        
        private async UniTask FinishLockTime(float time)
        {
            G.UIModule.ScreenLock("GuideFinishLock", true);
            await UniTask.Delay(TimeSpan.FromSeconds(time));
            G.UIModule.ScreenLock("GuideFinishLock", false);
        }
        
        private void EnterNextGuide(int nextId, bool needDelay)
        {
            var guideData = FindGuideData(nextId);
            if (guideData.id <= 0)
                return;
            if (needDelay && guideData.delayNext > 0)
            {
                G.UIModule.ScreenLock("GuideDelayLock", true);
                UniTask.Create(async () =>
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(guideData.delayNext));
                    await StartGuide(guideData.id);

                    G.UIModule.ScreenLock("GuideDelayLock", false);
                }).Forget();
            }
            else
            {
                StartGuide(guideData.id).Forget();
            }
        }

        private void OnWindowClose(EventOneParam<string> obj)
        {
            if (_currentGuideId > 0)
            {
                ref readonly GuideConfig currentConfig = ref _guideDB[_currentGuideId];
                if (currentConfig.triggerModule == "Main")
                {
                    var topWindow = G.UIModule.GetTopWindow((int)UILayer.UI);
                    if (topWindow.Equals("HotfixLogic.MainWindow"))
                        StartGuide(_currentGuideId).Forget();
                }

                if (currentConfig.triggerModule == "Puzzle")
                {
                    StartGuide(_currentGuideId).Forget();
                }
            }
            else
            {
                var checkList = _guideDB.CloseWindowGuides;
                for (int i = 0; i < checkList.Count; i++)
                {
                    if(IsGuideFinish(checkList[i]))
                        continue;

                    var guideData = FindGuideData(checkList[i]);
                    if (!string.IsNullOrEmpty(guideData.triggerParameters))
                    {
                        string[] parameters = guideData.triggerParameters.Split("|");
                        if ($"HotfixLogic.{parameters[0]}" == obj.Arg)
                        {

                            if (parameters.Length > 1) {
                                if (parameters[1] == "Card")
                                {
                                    var topWindow = G.UIModule.GetTopWindow((int)UILayer.UI);
                                    if (!topWindow.Equals("HotfixLogic.MainWindow"))
                                    {
                                        _currentGuideId = checkList[i];
                                        return;
                                    }
                                } else if (parameters[1] == "GoldCard") {
                                    if (!G.CardModule.HasNewGoldCard) continue;
                                    var topWindow = G.UIModule.GetTopWindow((int)UILayer.UI);
                                    if (!topWindow.Equals("HotfixLogic.MainWindow"))
                                    {
                                        _currentGuideId = checkList[i];
                                        return;
                                    } else {
                                        
                                    }
                                }
                            }
                            StartGuide(checkList[i]).Forget();
                            return;
                        }
                    }
                }
            }
        }

        private void OnWindowOpen(EventOneParam<string> obj)
        {
            var checkList = _guideDB.OpenWindowGuides;
            for (int i = 0; i < checkList.Count; i++)
            {
                if(IsGuideFinish(checkList[i]))
                    continue;
                var guideData = FindGuideData(checkList[i]);
                if (!string.IsNullOrEmpty(guideData.triggerParameters))
                {
                    string[] parameters = guideData.triggerParameters.Split("|");
                    if ($"HotfixLogic.{parameters[0]}" == obj.Arg)
                    {
                        if (parameters.Length > 1) {
                            if (parameters[1] == "Match")
                            {
                                int curId = MatchManager.Instance.MaxLevel;
                                if (int.TryParse(guideData.guideParameters, out var levelId))
                                {
                                    if(curId == levelId)
                                    {
                                        StartGuide(checkList[i]).Forget();
                                        return;
                                    }
                                }
                            } else if (parameters[1] == "NewCard") {
                                var newCardCount = G.CardModule.GetCardCount(10101);
                                if (newCardCount > 0) {
                                    StartGuide(checkList[i]).Forget();
                                    return;
                                }
                            } else if (parameters[1] == "NewGoldCard") {
                                if (G.CardModule.HasNewGoldCard) {
                                    StartGuide(checkList[i]).Forget();
                                    return;
                                }
                            }
                        } else {
                            StartGuide(checkList[i]).Forget();
                            return;
                        }
                    }
                }
            }
        }

        private void OnPuzzleUpdateScaleFinish()
        {
            
        }

        private void OnPuzzleStartTargetShowFinish() {
            CheckPuzzleUseLoupe();
        }

        private bool CheckPuzzleUseLoupe() {
            
            return false;
        }
        
        private void OnMainPageTransFinish(EventOneParam<MainPageType> obj)
        {
            
        }

        private void CheckCardGuide() {
            if (_currentGuideId > 0 && _currentGuideId == 1030) {
                StartGuide(_currentGuideId).Forget();
            }
        }
        
        #region 触发条件函数

        private bool OnEnterMatch(GuideConfig config, BaseModel model)
        {
            if (model is MatchData data)
            {
                if (data.IsEnterByEditor())
                {
                    return false;
                }

                // 写死20关是金币关的引导
                if (data.CurrentLevelData.id == 20 && config.triggerParameters == "Coin")
                {
                    if (LevelManager.Instance.IsCoinLevel)
                    {
                        return true;
                    }

                    return false;
                }

                // 条件格式 A关/B关|关卡id
                string[] condition = config.triggerParameters.Split('|');
                MatchLevelType enterType = MatchManager.Instance.CurrentMatchLevelType;
                if (enterType.ToString().ToLower() == condition[0].ToLower() && int.TryParse(condition[1], out int levelId))
                {
                    int maxId = MatchManager.Instance.MaxLevel;
                    if (levelId < maxId)
                        return false;
                    if (config.guideType == GuideType.Force)
                    {
                        if (int.TryParse(config.guideParameters, out int itemId))
                        {
                            //引导使用道具是免费的
                            bool bResult = levelId == data.CurrentLevelData.id;
                            return bResult;
                        }
                    }

                    return levelId == data.CurrentLevelData.id;
                }
            }

            return false;
        }

        private bool OnEnterMain(GuideConfig config, BaseModel model)
        {
            if (!string.IsNullOrEmpty(config.triggerParameters))
            {
                string[] parameters = config.triggerParameters.Split('|');
                if (parameters[0] == "Puzzle")
                {
                    //引导点击寻宝页签
                    int currentLevel = MatchManager.Instance.CurLevelID;
                    int levelId = ConfigMemoryPool.Get<PuzzleUnlockDB>().All[0].unlockLevel;
                    bool bResult = currentLevel >= levelId;
                    //当有Main不是顶部窗口时，先记录下当前引导，等待顶部窗口关闭
                    var topWindow = G.UIModule.GetTopWindow();
                    if (bResult && !topWindow.Equals("HotfixLogic.MainWindow"))
                    {
                        _currentGuideId = config.id;
                        return false;
                    }

                    return bResult;
                }

                if (parameters[0] == "Match")
                {
                    if (int.TryParse(parameters[1], out int levelId))
                    {
                        int currentLevel = MatchManager.Instance.CurLevelID;
                        bool bResult = currentLevel == levelId;
                        var topWindow = G.UIModule.GetTopWindow();
                        if (bResult && !topWindow.Equals("HotfixLogic.MainWindow"))
                        {
                            _currentGuideId = config.id;
                            return false;
                        }
                        return bResult;
                    }
                }

                if (parameters[0] == "MatchTask")
                {
                    if (!G.TargetTaskModule.TargetTaskLvOpen()) return false;

                    if (int.TryParse(parameters[1], out int guideId1))
                    {
                        if (int.TryParse(parameters[2], out int guideId2))
                        {
                            bool bResult = IsGuideFinish(guideId1) && IsGuideFinish(guideId2);
                            var topWindow = G.UIModule.GetTopWindow();
                            string mianWinName = "HotfixLogic.MainWindow";
                            if (bResult && !topWindow.Equals(mianWinName))
                            {
                                _currentGuideId = config.id;
                                return false;
                            }

                        }
                    }
                }
            }

            return false;
        }


        private bool OnEnterPuzzle(GuideConfig config, BaseModel model)
        {
            if (_currentGuideId > 0)
                return false;
            if (string.IsNullOrEmpty(config.triggerParameters))
            {
                return false;
            }

            if (int.TryParse(config.triggerParameters, out int mapId))
            {

            }

            return false;
        }

        #endregion
    }
}