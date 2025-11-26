using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using GameCore.LitJson;
using GameCore.Localization;
using GameCore.Settings;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixCore.Extensions;
using HotfixCore.Module;
using HotfixCore.MVC;
using HotfixLogic.Match;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = GameCore.Log.Logger;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

namespace HotfixLogic
{
    [Window(UILayer.UI, "uiprefab/match/matchmainwindow")]
    public partial class MatchMainWindow : UIWindow
    {
        private class TargetElementObject
        {
            private int _targetId;
            private int _remainNum;
            public int TargetId => _targetId;
            public int RemainNum => _remainNum;
            public GameObject GameObject => _gameObject;

            private GameObject _gameObject;
            private GameObject _finish;
            private GameObject _effObj;
            private Image _icon;
            private TextMeshProUGUI _num;
            private ElementMap _config;
            private Tween _numTween;

            public TargetElementObject(GameObject gameObject, int elementId, int num)
            {
                _targetId = elementId;
                _remainNum = num;
                _gameObject = gameObject;
                var db = ConfigMemoryPool.Get<ElementMapDB>();
                _config = db[elementId];
                _icon = _gameObject.transform.Find("icon").GetComponent<Image>();
                _num = _gameObject.transform.Find("num").GetComponent<TextMeshProUGUI>();
                _finish = gameObject.transform.Find("finish")?.gameObject;
                _finish?.SetVisible(false);

                _effObj = gameObject.transform.Find("starEff")?.gameObject;
                _effObj?.SetActive(false);
                UpdateNum(_remainNum);

                if (_config.elementType != ElementType.Normal)
                {
                    _icon.transform.localScale = Vector3.one * 0.7f;
                }
                else
                {
                    _icon.transform.localScale = Vector3.one * 0.8f;
                }
            }

            public void InitView()
            {
                string location = MatchManager.Instance.GetElementIconLocation(_targetId);
                G.ResourceModule.LoadAssetAsync<Sprite>(location,
                    (sp) =>
                    {
                        _icon.sprite = sp;

                        ElementMapDB elementDB = ConfigMemoryPool.Get<ElementMapDB>();
                        ref readonly ElementMap config = ref elementDB[_targetId];
                        if (config.elementType == ElementType.Normal)
                        {
                            _icon.color = ConfigMemoryPool.Get<LevelMapImageDB>()
                                .GetBaseElementColor(MatchManager.Instance.CurLevelID, MatchManager.Instance.MaxLevel,
                                    _targetId);
                        }
                    }).Forget();
            }

            public void UpdateNum(int remain)
            {
                _remainNum = remain;
                if (remain <= 0)
                {
                    bool isFinish = LevelTargetSystem.Instance.IsTargetFinish(_targetId);

                    if (_finish != null)
                    {
                        var curFinish = _finish.activeSelf;
                        if (!curFinish && isFinish)
                        {
                            AudioUtil.PlaySound("audio/match/collect_finish");
                        }
                    }


                    _num.text = isFinish ? "" : "0";
                    _finish.SetVisible(isFinish);


                    if (_targetId == 280) {
                        _effObj?.SetActive(true);
                    } else {
                        _effObj?.SetActive(isFinish);
                    }
                    
                    return;
                }

                _num.text = remain.ToString();
                _finish.SetVisible(false);
            }

            public int GetNum()
            {
                return int.TryParse(_num.text, out int num) ? num : 0;
            }

            public void DoNumTween(int to)
            {
                if (_numTween != null)
                {
                    _numTween.Kill(true);
                }
                
                _numTween = DOTween.To(() => _remainNum, x => _remainNum = x, to, 0.5f).OnUpdate(() =>
                {
                    _num.text = _remainNum.ToString();
                }).SetDelay(0.15f).OnComplete(() =>
                {
                    UpdateNum(to);
                });
            }

            public void Destroy()
            {
                GameObject.Destroy(_gameObject);
            }
        }

        private const float TweenDuration = 0.5f;
        private List<TargetElementObject> _targetObjects = null;
        private TargetElementObject _coinTargetObject = null;
        private List<MatchItem> _matchItems;
        private Dictionary<int, int> _matchItemUseCount;
        private Dictionary<string, SkeletonGraphic> _ipSpineMap;
        private MatchData _data;
        private List<GameObject> _starList;
        private int[] _starScore = new int[3];
        private int _currentShowScore;
        private Tween _scoreTween;
        private Tween _progressTween;
        private Tween _tipsPopTween;
        private Tween _coinCollectTween;
        private string _currentPlaySpineName;
        private string _currentPlaySpineAniName;
        private int _playIdleTimes;
        private int _playIdleMaxTimes;
        private ElementMapDB _elementDB;

        // 金币兑换道具ID
        private Dictionary<int, int> _coinExchangeIDs = new Dictionary<int, int>();
        private Dictionary<int, int> _useItems = new Dictionary<int, int>();

        // 金币复活次数
        private int _coinReviveCount = 0;

        // 广告复活次数
        private int _advReviveCount = 0;

        // 游戏消耗的时长
        private int _useTime;
        private long _startTime;

        private int _level3TimerId;

        private int _reviveUseCoin = 0;
        private bool _isCalculateCoinState = false;
        private bool _isResetGuide6Tips = false;
        private Sequence _tipsTopSeq;
        private Sequence _tipsBottomSeq;

        private Color _normalTextColor = new Color(0.1725f, 0.1058f, 0.0274f, 1f);
        private Color _warningTextColor = new Color(0.9058f, 0.3254f, 0.3254f, 1f);

        private List<Tween> _lastStepTween = new List<Tween>();

        private bool _isShowTargetAnim = false;

        private bool _lastAnimShow;

        private int _targetCoinCount;

        public override void OnCreate()
        {
            _elementDB = ConfigMemoryPool.Get<ElementMapDB>();
            _starList = new List<GameObject>(3)
            {
                go_star1.transform.Find("finish").gameObject,
                go_star2.transform.Find("finish").gameObject,
                go_star3.transform.Find("finish").gameObject
            };

            btn_gmColor.AddClick(() => { G.EventModule.DispatchEvent(GameEventDefine.OnOpenMatchGmChangePanel); });
            btn_gmColor.SetVisible(GameSettings.Instance.IsShowDebugConsole());
            btn_guideLevelFinish.AddClick(OnGuideLevelFinishClick);

            _matchItems = new();
            _ipSpineMap = new Dictionary<string, SkeletonGraphic>(6)
            {
                [MatchConst.SPINE_SUCCESS] = spine_success,
                [MatchConst.SPINE_FAIL] = spine_fail,
                [MatchConst.SPINE_HELLO] = spine_hello,
                [MatchConst.SPINE_IDLE] = spine_idle,
                [MatchConst.SPINE_TRIPLE] = spine_triple,
                [MatchConst.SPINE_WARM] = spine_warm,
            };
            _playIdleTimes = 0;
            _playIdleMaxTimes = 3;
            _isCalculateCoinState = false;
            ResetSpineToIdle();

            ResetReportData();
            text_topTips.text = "";
            // text_bottomTips.text = "";
            btn_guideLevelFinish.SetVisible(false);

            if (CommonUtil.IsWechatMiniGame()) {
                var anchoredPosition = go_top.GetComponent<RectTransform>().anchoredPosition;
                anchoredPosition.y -= 30;
                go_top.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
            }

            go_beginTarget.SetActive(false);
        }

        public override void AddListeners()
        {
            AddListeners<EventTwoParam<int, Vector3>>(GameEventDefine.OnMatchElementMoveToTarget,
                OnMatchElementMoveToTarget, this);
            AddListeners<EventOneParam<bool>>(GameEventDefine.OnMatchTargetChangedNum, RefreshTarget, this);

            AddListeners<EventOneParam<int>>(GameEventDefine.OnGuideTrigger, OnGuideTrigger, this);
            AddListeners<EventOneParam<int>>(GameEventDefine.OnMatchAddStepUseCoin, OnMatchAddStepUseCoin, this);

            AddListeners<EventTwoParam<int, bool>>(GameEventDefine.OnMatchAddStep, OnMatchAddStep, this);

            AddListeners(GameEventDefine.OnGameApplicationFocus, OnGameApplicationFocus, this);
            AddListeners(GameEventDefine.OnNotGameApplicationFocus, OnNotGameApplicationFocus, this);
            AddListeners<EventOneParam<int>>(GameEventDefine.OnMatchGuideLevelStepFinish, OnMatchGuideLevelStepFinish, this);
            AddListeners<EventOneParam<int>>(GameEventDefine.OnMatchSquareLineStep, OnMatchSquareLineStep, this);

            AddListeners<EventOneParam<LevelData>>(GameEventDefine.OnMatchGuideLevelStartFinish, OnMatchGuideLevelStart,
                this);
            AddListeners<EventOneParam<int>>(GameEventDefine.OnExchangeCoinShopSucc, OnExchangeCoinShopSucc);
            AddListeners(GameEventDefine.OnMatchAddResultCoin, OnMatchAddResultCoin, this);
            AddListeners<EventTwoParam<int, float>>(GameEventDefine.OnMatchSetResultCoin, OnMatchSetResultCoin, this);
            
            // AddListeners<EventOneParam<int>>(GameEventDefine.OnMatchCollectItemFlyComplete,
            //     OnMatchCollectItemFlyComplete, this);

            AddListeners<EventOneParam<bool>>(GameEventDefine.OnMatchShowLastStepPrompt, OnMatchShowLastStepPrompt, this);

            AddListeners(GameEventDefine.OnMatchBeginCollectResultCoin, OnMatchBeginCollectResultCoin, this);
        }
            

        private void OnMatchSquareLineStep(EventOneParam<int> obj)
        {
            int step = obj.Arg;
            string topKey = "Match/GuideLevel5_1";
            if (step == 2)
            {
                topKey = "Match/GuideLevel5_3";
            }

            KillTipsSeq();

            text_topTips.text = LocalizationPool.Get(topKey);
            
            _tipsTopSeq = DOTween.Sequence(text_topTips.DOFade(0, 0.3f)).AppendInterval(0.5f)
                .Append(text_topTips.DOFade(1, 0.3f));

            if (step == 1) {
                text_bottomTips.text = LocalizationPool.Get("Match/GuideLevel5_2");
                _tipsTopSeq.Join(text_bottomTips.DOFade(1, 0.3f));
            }


            go_bottomTip.SetVisible(false);
        }

        private void OnMatchGuideLevelStepFinish(EventOneParam<int> arg)
        {
            if(_isResetGuide6Tips)
                return;
            _isResetGuide6Tips = true;

            KillTipsSeq();

            if (arg.Arg == 6)
            {
                _tipsTopSeq = DOTween.Sequence(text_topTips.DOFade(0, 0.3f)).AppendInterval(0.5f)
                    .Append(text_topTips.DOFade(1, 0.3f).OnStart(() =>
                    {
                        text_topTips.text = LocalizationPool.Get("Match/GuideLevel6_1");
                    }));
            }
            if (arg.Arg == 4)
            {
                text_centerTips.text = LocalizationPool.Get("Match/GuideLevel4_2");
                DOTween.Sequence(text_centerTips.DOFade(0, 0.1f))
                    .Append(text_centerTips.DOFade(1, 0.3f));
                text_topTips.text = "";
            }
            else if (arg.Arg == 5)
            {
                text_centerTips.text = LocalizationPool.Get("Match/GuideLevel5_5");
                DOTween.Sequence(text_centerTips.DOFade(0, 0.1f))
                    .Append(text_centerTips.DOFade(1, 0.3f));
                text_topTips.text = "";
            }
        }

        private void OnMatchGuideLevelStart(EventOneParam<LevelData> arg)
        {
            var levelData = arg.Arg;
            string lanKey = $"Match/GuideLevel{levelData.id}";

            KillTipsSeq();

            if (levelData.id == 1) {
                MatchGuideFirstLevel();
            } else if (levelData.id == 4) {
                MatchGuideStep4();
            } else {
                _tipsTopSeq = DOTween.Sequence();
                _tipsTopSeq.Append(text_topTips.DOFade(0, 0.3f));
                _tipsTopSeq.AppendInterval(0.5f);
                _tipsTopSeq.Append(text_topTips.DOFade(1, 0.3f).OnStart(() =>
                    {
                        text_topTips.text = LocalizationPool.Get(lanKey);
                    }));
                
                btn_guideLevelFinish.SetVisible(levelData.id == 3);
                // go_bottomTip.SetVisible(levelData.id == 5);
                if (levelData.id == 3)
                {
                    _level3TimerId = AddTimer(OnCountingLevel3Timer, 2, false);
                }
            }
        }

        private void MatchGuideStep4() {
            text_topTips.text = LocalizationPool.Get("Match/GuideLevel4");
            text_bottomTips.text = LocalizationPool.Get("Match/GuideLevel4_1");

            // var beginDelayTime = 2.0f;
            _tipsTopSeq = DOTween.Sequence();
            // _tipsTopSeq.AppendInterval(beginDelayTime);
            _tipsTopSeq.Append(text_topTips.DOFade(1, 0.3f));
            _tipsTopSeq.AppendInterval(2f);
            _tipsTopSeq.Append(text_bottomTips.DOFade(1, 0.3f));
        }

        private void MatchGuideFirstLevel() {
            text_topTips.text = LocalizationPool.Get("Match/GuideLevel1");
            text_bottomTips.text = LocalizationPool.Get("Match/GuideLevel1_1");

            var beginDelayTime = 2.0f;
            _tipsTopSeq = DOTween.Sequence();
            _tipsTopSeq.AppendInterval(beginDelayTime);
            _tipsTopSeq.Append(text_topTips.DOFade(1, 0.3f));
            _tipsTopSeq.AppendInterval(2f);
            _tipsTopSeq.Append(text_bottomTips.DOFade(1, 0.3f));
        }

        private void KillTipsSeq() {
            _tipsTopSeq?.Kill();
            _tipsTopSeq = null;
            text_topTips.alpha = 0;

            _tipsBottomSeq?.Kill();
            _tipsBottomSeq = null;
            text_bottomTips.alpha = 0;
        }

        private void OnCountingLevel3Timer()
        {
            OnGuideLevelFinishClick();
        }

        private void OnGuideTrigger(EventOneParam<int> obj)
        {
            RefreshItem();
        }

        private void OnGuideLevelFinishClick()
        {
            if (_level3TimerId > 0)
            {
                RemoveTimer(_level3TimerId);
                _level3TimerId = -1;
            }
            btn_guideLevelFinish.SetVisible(false);
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchGuideLevelClickFinish);
        }

        private void OnMatchElementMoveToTarget(EventTwoParam<int, Vector3> obj)
        {
            int elementId = obj.Arg1;

            var targetObject = FindTargetObject(elementId);
            if (targetObject == null)
                return;

            bool isFinish = LevelTargetSystem.Instance.IsTargetFinish(elementId);
            if (isFinish && ConfigMemoryPool.Get<ElementMapDB>().IsCircleElement(elementId))
                return;

            var screenPoint = G.UIModule.GetUIScreenPos(targetObject.transform.position);

            var param = EventThreeParam<int, Vector3, Vector3>.Create(elementId, obj.Arg2, screenPoint);
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchPlayCollectItem, param);
        }

        public void InitTarget(MatchData data)
        {
            _data = data;
            InitStarScore();
            CreateTarget();
            RefreshStep();

            SetLeftText();
        }

        public void InitItems(int[] itemsId)
        {
            Logger.Debug("MatchMainWindow.InitItems");
            _matchItems.Add(widget_matchItem1);
            _matchItems.Add(widget_matchItem2);
            _matchItems.Add(widget_matchItem3);
            _matchItems.Add(widget_matchItem4);
            _matchItems.Add(widget_matchItem5);

            _matchItemUseCount = new Dictionary<int, int>();

            for (int i = 0; i < itemsId.Length; i++)
            {
                if (i >= _matchItems.Count) return;
                var item = _matchItems[i];
                item.gameObject.SetActive(true);
                item.Init(itemsId[i]);
                _matchItemUseCount.Add(itemsId[i], 0);
            }
        }

        public void Restart()
        {
            _isShowTargetAnim = false;
            _lastAnimShow = false;

            _currentShowScore = 0;
            _isCalculateCoinState = false;
            PlaySpine(MatchConst.SPINE_IDLE);
            InitStarScore();
            RefreshStep();
            ResetItem();

            ResetReportData();
        }

        public void ResetItem()
        {
            for (int i = 0; i < _matchItems.Count; i++)
            {
                _matchItems[i].ResetUseCount();
            }

            foreach (var key in _matchItemUseCount.Keys.ToList())
            {
                _matchItemUseCount[key] = 0;
            }

            RefreshItem();
        }

        public void ShowUseItemTips(int itemId)
        {
            LevelMapImageDB db = ConfigMemoryPool.Get<LevelMapImageDB>();
            int id = Mathf.Max(1, db.GetLevelInPage(MatchManager.Instance.CurLevelID, MatchManager.Instance.MaxLevel));
            LevelMapImage config = db[id + 1];

            var textColor = Color.white;
            if (config.lastLine == MatchColorScheme.Warm) {
                textColor = Color.white * 0.3215f;
                textColor.a = 1;
            }


            text_popTips.color = textColor;
            text_popTips.text = LocalizationPool.Get($"Match/UseItem{itemId}");
            _tipsPopTween = DOTween
                .To((f) => { text_popTips.transform.localScale = new Vector3(f, f, f); }, 0.8f, 1.0f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo);
            for (int i = 0; i < _matchItems.Count; i++)
            {
                _matchItems[i].SetGrayOrLight(itemId);
            }

            ResetMatchItemsPos();
        }

        public void UseMatchItem(int itemId, Action<bool> callback)
        {
            if (_matchItemUseCount[itemId] == 0 && G.GameItemModule.CheckHasBuffByItemId(itemId))
            {
                _matchItemUseCount[itemId]++;
                var item = _matchItems.Find(i => i.ItemId == itemId);
                if (item != null) item.UpdateUseCount();
                RefreshItem();

                RecordUseItem(itemId);
                callback?.Invoke(true);
            }
            else
            {
                G.GameItemModule.UseItem(itemId, 1, (result) =>
                {
                    if (result)
                    {
                        _matchItemUseCount[itemId]++;
                        RecordUseItem(itemId);
                    }

                    RefreshItem();
                    callback?.Invoke(result);
                }, false);
            }
        }

        public void RefreshItem()
        {
            for (int i = 0; i < _matchItems.Count; i++)
            {
                _matchItems[i].RefreshNum();
                _matchItems[i].SetGrayOrLight(-1);
            }

            text_popTips.text = "";
            _tipsPopTween?.Kill();

            CheckLastStepAnim();
        }

        private void RefreshTarget(EventOneParam<bool> arg)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            for (int i = 0; i < _targetObjects.Count; i++)
            {
                //收集的元素，数量需要逐步变更
                if (!arg.Arg && db.IsCollectElement(_targetObjects[i].TargetId))
                {
                    TweenRefreshTarget(_targetObjects[i]);
                    continue;
                }
                if(LevelTargetSystem.Instance.TargetElements.ContainsKey(_targetObjects[i].TargetId))
                {
                    int remain = Mathf.Max(0, LevelTargetSystem.Instance.TargetElements[_targetObjects[i].TargetId]);
                    _targetObjects[i].UpdateNum(remain);
                }
            }
        }

        private void TweenRefreshTarget(TargetElementObject targetObject)
        {
            int targetId = targetObject.TargetId;
            if (LevelTargetSystem.Instance.TargetElements.TryGetValue(targetId, out int remain))
            {
                remain = Mathf.Max(0, remain);
                targetObject.DoNumTween(remain);
            }
        }

        public void TweenRemainStep(int remain)
        {
            if (remain > 0)
            {
                float duration = remain * 0.09f;
                DOTween.To((v) => { text_step.text = $"{Mathf.Floor(v)}"; }, remain, 0, duration).SetAutoKill();
            }
        }

        public Vector3 GetStepWorldPosition()
        {
            Vector3 pos = G.UIModule.GetCompScreenPosition(text_step.GetComponent<RectTransform>());
            return pos;
        }

        public void SetMatchInfoVisible(bool visible, bool animate = false, Action callback = null)
        {
            if (visible)
            {
                if (animate)
                {
                    RectTransform topRect = go_top.GetComponent<RectTransform>();
                    CanvasGroup topCanvasGroup = go_top.GetComponent<CanvasGroup>();
                    topCanvasGroup.alpha = 0;
                    RectTransform bottomRect = go_bottom.GetComponent<RectTransform>();
                    CanvasGroup bottomCanvasGroup = go_bottom.GetComponent<CanvasGroup>();
                    bottomCanvasGroup.alpha = 0;
                    Vector2 topOri = new Vector2(topRect.anchoredPosition.x, topRect.anchoredPosition.y);
                    Vector2 bottomOri = new Vector2(bottomRect.anchoredPosition.x, bottomRect.anchoredPosition.y);
                    topRect.anchoredPosition = new Vector2(topRect.anchoredPosition.x, topRect.anchoredPosition.y + 100);
                    bottomRect.anchoredPosition = new Vector2(bottomRect.anchoredPosition.x,
                        bottomRect.anchoredPosition.y - 100);
                    go_top.SetVisible(true);
                    go_bottom.SetVisible(true);

                    const float dur = 0.3f;
                    G.UIModule.ScreenLock("MatchInfoVisible", true);
                    DOTween.Sequence()
                        .Append(topRect.DOAnchorPos(topOri, dur))
                        .Append(bottomRect.DOAnchorPos(bottomOri, dur))
                        .Append(topCanvasGroup.DOFade(1, dur))
                        .Append(bottomCanvasGroup.DOFade(1, dur)).OnComplete(() =>
                        {
                            callback?.Invoke();
                            G.UIModule.ScreenLock("MatchInfoVisible", false);
                        });
                }
                else
                {
                    go_top.SetVisible(true);
                    go_bottom.SetVisible(true);
                }
            }
            else
            {
                go_top.SetVisible(false);
                go_bottom.SetVisible(false);
            }

            text_topTips.SetVisible(!visible);
            // text_bottomTips.SetVisible(!visible);
            text_centerTips.SetVisible(!visible);
        }

        public void RefreshScore(int score)
        {
            _scoreTween?.Kill(true);
            _progressTween?.Kill(true);

            text_score.gameObject.transform.DOScale(1.2f, 0.1f);
            _scoreTween = DOTween
                .To(value => { text_score.text = $"{Mathf.Floor(value)}"; }, _currentShowScore,
                    _currentShowScore + score, TweenDuration).OnComplete(() =>
                {
                    _currentShowScore += score;

                    ShowStar(_starList[0], _currentShowScore >= _starScore[0]);
                    ShowStar(_starList[1], _currentShowScore >= _starScore[1]);
                    ShowStar(_starList[2], _currentShowScore >= _starScore[2]);
                    text_score.gameObject.transform.DOScale(1.0f, 0.1f);
                });



            float targetPercent = Math.Min((_currentShowScore + score) * 1.0f / _data.CurrentLevelData.fullScore, 1.0f);
            float curPercent = img_starProgress.rectTransform.sizeDelta.x / 612.0f;
            if (curPercent < 1.0f) {
                SetProgressEffVisible(true);
            }

            _progressTween = img_starProgress.rectTransform.DOSizeDelta(new Vector2(targetPercent * 612, 20), TweenDuration).OnComplete(() =>
            {
                SetProgressEffVisible(false);
            });
        }

        private void ResetProgress() {
            img_starProgress.rectTransform.sizeDelta = new Vector2(0, 20);
        }

        private void SetProgressEffVisible(bool visible)
        {
            go_effProgress.SetActive(visible);
            go_effProgressFrame.SetActive(visible);
        }

        public void ShowStar(GameObject star, bool isShow)
        {


            if (!isShow || star.gameObject.activeSelf) return;

            var starParent = star.transform.parent;

            var animator = starParent.GetComponent<Animator>();
            animator.enabled = true;
            animator.Play("ani_ef_cx_00010");

            var effObj1 = star.transform.Find("Match_eff_xingbao");
            var effObj2 = star.transform.Find("Match_eff_jiaxing");

            var seq = DOTween.Sequence();
            seq.AppendInterval(0.24f);
            seq.AppendCallback(() => {
                if (effObj1 != null) effObj1.gameObject.SetActive(true);
            });
            seq.AppendInterval(0.76f);
            seq.AppendCallback(() => {
                if (effObj2 != null) effObj2.gameObject.SetActive(true);
            });
            seq.AppendInterval(1f);
            seq.AppendCallback(() => {
                if (effObj1 != null) effObj1.gameObject.SetActive(false);
                if (effObj2 != null) effObj1.gameObject.SetActive(false);
                animator.enabled = false;
                animator.Play("");
            });

            star.SetActive(true);

            AudioUtil.PlaySound("audio/match/collect_star");
        }

        public void RefreshStep()
        {
            text_step.text = _data.MoveStep > 0 ? _data.MoveStep.ToString() : "0";

            if (_data.MoveStep <= 5 && _data.MoveStep > 0)
            {
                text_step.color = _warningTextColor;
                if (!go_stepEff.activeSelf)
                {
                    go_stepEff.gameObject.SetActive(true);
                }
            }
            else
            {
                text_step.color = _normalTextColor;
                go_stepEff.gameObject.SetActive(false);
            }

            if (_data.MoveStep == 5 || _data.MoveStep == 2) {
                PlaySpine(MatchConst.SPINE_WARM);
                    AudioUtil.PlaySound("audio/common/com_11");
            }
                
        }

        private GameObject FindTargetObject(int elementId)
        {
            if (_isCalculateCoinState)
            {
                return go_coinCell;
            }

            foreach (var targetObject in _targetObjects)
            {
                if (targetObject.TargetId == elementId)
                    return targetObject.GameObject;
            }

            return null;
        }

        private void CreateTarget()
        {
            var targets = LevelTargetSystem.Instance.TargetElements;
            _targetObjects = new List<TargetElementObject>(targets.Count);
            foreach (var target in targets)
            {
                GameObject go = GameObject.Instantiate(go_targetCell, go_targetRoot.transform);
                go.SetVisible(true);
                TargetElementObject targetObject = new TargetElementObject(go, target.Key, target.Value);
                targetObject.InitView();
                _targetObjects.Add(targetObject);
            }

            if (!LevelManager.Instance.IsCoinLevel) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(go_targetRoot.transform as RectTransform);

                foreach (var target in _targetObjects) {
                    target.GameObject.SetActive(false);
                }
            }
        }

        private void SetLeftText()
        {
            var targets = LevelTargetSystem.Instance.TargetElements;
            foreach (var target in targets)
            {
                if (target.Key == (int)ElementIdConst.Coin)
                {
                    text_left.text = LocalizationPool.Get("Common/Reward");
                }
                else
                {
                    text_left.text = LocalizationPool.Get("Common/Target");
                }
            }
        }

        private void InitStarScore()
        {
            int fullScore = _data.CurrentLevelData.fullScore;
            _starScore[0] = Mathf.CeilToInt(fullScore * 0.3f);
            _starScore[1] = Mathf.CeilToInt(fullScore * 0.6f);
            _starScore[2] = fullScore;
            for (int i = 0; i < _starList.Count; i++)
            {
                _starList[i].SetVisible(false);
            }

            text_score.text = "0";
            ResetProgress();
        }

        public override void OnDestroy()
        {
            if (_targetObjects != null)
            {
                for (int i = 0; i < _targetObjects.Count; i++)
                {
                    _targetObjects[i].Destroy();
                }

                _targetObjects.Clear();
                _targetObjects = null;
            }

            _coinTargetObject = null;

            _scoreTween?.Kill();
            _progressTween?.Kill();
        }

        public void UpdateLv()
        {
            text_lv.text = $"Lv:{MatchManager.Instance.CurLevelID}";
            // text_lv.text = $"Lv:{_data.CurrentLevelData.id}";
        }

        /// <summary>
        /// 切换目标为金币
        /// </summary>
        public void SwitchTargetToCoin(int baseCoin)
        {
            _isCalculateCoinState = true;
            // go_targetRoot.SetVisible(false);
            if (_coinTargetObject == null)
            {
                foreach (var target in _targetObjects)
                {
                    if (target.TargetId == (int)ElementIdConst.Coin)
                    {
                        _coinTargetObject = target;
                        break;
                    }
                    else
                    {
                        target.GameObject.SetVisible(false);
                    }
                }

                if (_coinTargetObject == null)
                {
                    go_coinCell.SetVisible(true);
                    _coinTargetObject = new TargetElementObject(go_coinCell, (int)ElementIdConst.Coin, 0);
                    _coinTargetObject.UpdateNum(baseCoin);
                }
                else
                {
                    var num = _coinTargetObject.GetNum();
                    _coinTargetObject.UpdateNum(num + baseCoin);
                }
            }

            text_left.text = LocalizationPool.Get("Common/Reward");
        }

        public Vector2 GetCoinScreenPosition()
        {
            var screenPos = G.UIModule.GetUIScreenPos(go_coinCell.transform.position);
            return screenPos;
        }

        public void PlaySpine(string spineName, string animationName = "Touch", bool isLoop = false)
        {
            if (_currentPlaySpineName == spineName && _currentPlaySpineAniName == animationName) return;
            _currentPlaySpineName = spineName;
            _currentPlaySpineAniName = animationName;
            foreach (var spine in _ipSpineMap)
            {
                if (spine.Key == spineName)
                {
                    spine.Value.gameObject.SetVisible(true);
                    spine.Value.AnimationState.SetAnimation(0, animationName, isLoop);
                    spine.Value.AnimationState.Complete -= OnOnceSpinePlayComplete;
                    spine.Value.AnimationState.Complete += OnOnceSpinePlayComplete;
                }
                else
                {
                    spine.Value.gameObject.SetVisible(false);
                }
            }
        }

        private void OnOnceSpinePlayComplete(TrackEntry trackentry)
        {
            if (!trackentry.Loop)
                ResetSpineToIdle();
        }

        private void ResetSpineToIdle()
        {
            foreach (var spine in _ipSpineMap)
            {
                if (spine.Key == MatchConst.SPINE_IDLE)
                {
                    spine.Value.AnimationState.SetAnimation(0, "Idle", true);
                    spine.Value.AnimationState.Complete -= OnIdleSpineComplete;
                    spine.Value.AnimationState.Complete += OnIdleSpineComplete;
                    spine.Value.gameObject.SetVisible(true);
                    _currentPlaySpineName = MatchConst.SPINE_IDLE;
                    _currentPlaySpineAniName = "Idle";
                }
                else
                {
                    spine.Value.gameObject.SetVisible(false);
                }
            }
        }

        private void OnIdleSpineComplete(TrackEntry trackentry)
        {
            _playIdleTimes++;
            if (_playIdleTimes >= _playIdleMaxTimes)
            {
                _playIdleTimes = 0;
                _playIdleMaxTimes = Random.Range(2, 4);
                RandomPlayIdle2();
            }
        }

        private void RandomPlayIdle2()
        {
            PlaySpine(MatchConst.SPINE_IDLE, "Idle2");
        }

        private void ResetReportData()
        {
            _advReviveCount = 0;

            _coinReviveCount = 0;

            _startTime = CommonUtil.GetNowTime();
            _useTime = 0;

            _coinExchangeIDs.Clear();
            _useItems.Clear();

            _reviveUseCoin = 0;
        }

        private void OnMatchAddStep(EventTwoParam<int, bool> param)
        {
            var isAdv = param.Arg2;

            if (isAdv)
            {
                _advReviveCount++;
            }
            else
            {
                _coinReviveCount++;
            }
        }

        private void OnGameApplicationFocus()
        {
            // 回到前台，重置游戏开始时间
            _startTime = CommonUtil.GetNowTime();
        }

        private void OnNotGameApplicationFocus()
        {
            // 退到后台，记录游戏消耗时间
            _useTime += (int)(CommonUtil.GetNowTime() - _startTime);
        }

        private void OnExchangeCoinShopSucc(EventOneParam<int> param)
        {
            if (!_coinExchangeIDs.ContainsKey(param.Arg))
            {
                _coinExchangeIDs.Add(param.Arg, 0);
            }

            _coinExchangeIDs[param.Arg]++;
        }

        private void RecordUseItem(int itemId)
        {
            if (!_useItems.ContainsKey(itemId))
            {
                _useItems.Add(itemId, 0);
            }

            _useItems[itemId]++;
        }

        public void ReportMatchResult(int lvID, bool isWin, int useStep, int configStep, int targetNum = 0,
            List<ItemData> rewardItems = null)
        {
            targetNum = isWin ? targetNum : 0;

            var totalScore = isWin ? MatchManager.Instance.TotalScore : 0;

            var useItemsStr = "";
            if (_useItems.Count > 0)
            {
                useItemsStr = JsonMapper.ToJson(_useItems);
            }

            var coinExchangeStr = "";
            if (_coinExchangeIDs.Count > 0)
            {
                coinExchangeStr = JsonMapper.ToJson(_coinExchangeIDs);
            }

            var useTime = CommonUtil.GetNowTime() - _startTime + _useTime;
            string genAndUseItem = TaskManager.Instance.GetCalculateTaskJson();

            int lvDifficulty = LevelManager.Instance.GetLevelDifficulty(lvID);
            var behaviourValue = LevelManager.Instance.BehaviourValue;
            var groupValue = LevelManager.Instance.GroupValue;

            G.HttpModule.ReportLevelGameEnd(lvID, isWin, useStep, configStep, (int)useTime, lvDifficulty,
                behaviourValue, groupValue, targetNum,
                totalScore, _advReviveCount, _coinReviveCount, _reviveUseCoin, useItemsStr, coinExchangeStr,
                rewardItems, item_generate_and_use: genAndUseItem);
        }

        private void OnMatchAddStepUseCoin(EventOneParam<int> param)
        {
            _reviveUseCoin += param.Arg;
        }

        private void OnMatchAddResultCoin()
        {
            _targetCoinCount++;
        }

        private void OnMatchSetResultCoin(EventTwoParam<int, float> param)
        {
            if (param.Arg2 > 0)
            {
                // _targetCoinCount = param.Arg1;

                var targetCoinNum = param.Arg1;
                var curCoinNum = _coinTargetObject.GetNum();
                if (_coinCollectTween != null)
                {
                    _coinCollectTween.Kill();
                    _coinCollectTween = null;
                }

                _coinCollectTween = DOTween.To(() => curCoinNum, x => curCoinNum = x, targetCoinNum, param.Arg2)
                    .OnUpdate(() => { _coinTargetObject.UpdateNum(curCoinNum); })
                    .OnComplete(() => { _coinCollectTween = null; });
            }
            else
            {
                _coinTargetObject.UpdateNum(param.Arg1);
            }
        }

        // private void AddResultCoin(int num = 1)
        // {
        //     if (_coinTargetObject == null) return;

        //     var curNum = _coinTargetObject.GetNum();
        //     _coinTargetObject.UpdateNum(curNum + num);
        // }

        private void OnMatchBeginCollectResultCoin() {
            var curCoinNum = _coinTargetObject.GetNum();
            var baseCoinNum = curCoinNum;
            _coinCollectTween = DOTween.To(() => curCoinNum, x => curCoinNum = x, baseCoinNum + _targetCoinCount, 0.5f)
                    .OnUpdate(() => { 
                        if (_coinTargetObject.GetNum() < curCoinNum) {
                            _coinTargetObject.UpdateNum(curCoinNum); 
                        }
                    })
                    .OnComplete(() => { _coinCollectTween = null; });
        }

        private void OnMatchShowLastStepPrompt(EventOneParam<bool> param) {
            _lastAnimShow = param.Arg;
            CheckLastStepAnim();
        }

        private void CheckLastStepAnim() {
            if (_lastAnimShow) {
                PlayMatchItemLastStepAnim();
            } else {
                ResetMatchItemsPos();
            }
        }

        private void ResetMatchItemsPos() {
            KillStepTween();
            for (int i = 0; i < _matchItems.Count; i++) {
                var item = _matchItems[i];

                var obj = item.GetIconWidgetObj();
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;
            }
        }

        private void PlayMatchItemLastStepAnim() {
            KillStepTween();

            var scaleDuration = 0.3f;

            for (int i = 0; i < _matchItems.Count; i++) {
                var item = _matchItems[i];

                var obj = item.GetIconWidgetObj();
                obj.transform.localPosition = Vector3.zero;

                var seq = DOTween.Sequence();

                seq.AppendInterval(i * scaleDuration * 2);
                seq.Append(obj.transform.DOLocalMoveY(30, scaleDuration));
                seq.Join(obj.transform.DOScaleY(1.2f, scaleDuration));
                seq.Append(obj.transform.DOLocalMoveY(0, scaleDuration));
                seq.Join(obj.transform.DOScaleY(0.8f, scaleDuration));
                seq.Append(obj.transform.DOScaleY(1f, scaleDuration));
                seq.AppendInterval((_matchItems.Count - i) * scaleDuration * 2);

                seq.SetLoops(-1);

                _lastStepTween.Add(seq);
            }
        }


        private void KillStepTween() {
            foreach (var tween in _lastStepTween) {
                tween.Kill();
            }
            _lastStepTween.Clear();
        }

        public void ShowTarget(float delayTime = 0, Action callback = null) {
            if (LevelManager.Instance.IsCoinLevel) {
                // 金币关直接返回
                callback?.Invoke();
                return;
            }

            if (_isShowTargetAnim) return;
            _isShowTargetAnim = true;

            var targetPosList = new List<Vector3>();
            foreach (var target in _targetObjects) {
                targetPosList.Add(target.GameObject.transform.position);
            }

            go_beginTarget.SetActive(true);
            canvasGroup_beginTarget.alpha = 0;

            var rectTransform = canvasGroup_beginTarget.transform as RectTransform;
            var targetImgPos = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(targetImgPos.x, targetImgPos.y + 100);


            var seq = DOTween.Sequence();
            seq.AppendInterval(delayTime);
            seq.Append(canvasGroup_beginTarget.DOFade(1, 0.3f));
            seq.Join(rectTransform.DOAnchorPos(targetImgPos, 0.3f));
            seq.AppendCallback(() => {
                for (int i = 0; i < targetPosList.Count; i++) {
                    var target = _targetObjects[i];
                    target.GameObject.SetActive(true);
                    target.GameObject.transform.SetParent(go_targetLayout.transform);

                    target.GameObject.transform.localScale = Vector3.zero;
                    target.GameObject.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
                }
            });
            seq.AppendInterval(0.5f);
            seq.AppendCallback(() => {
                canvasGroup_beginTarget.DOFade(0, 0.3f);

                TargetElementObject lastTarget;
                for (int i = 0; i < targetPosList.Count; i++) {
                    var target = _targetObjects[i];
                    var pos = targetPosList[i];
                    lastTarget = target;

                    target.GameObject.transform.SetParent(go_flyTarget.transform);
                    target.GameObject.transform.DOScale(1f, 0.3f).SetDelay(i * 0.1f);
                    target.GameObject.transform.DOMove(pos, 0.3f).SetDelay(i * 0.1f).OnComplete(() => {
                        target.GameObject.transform.SetParent(go_targetRoot.transform);

                        if (lastTarget == target) {
                            go_beginTarget.SetActive(false);
                            callback?.Invoke();
                        }
                    });
                    
                }
            });



        }
    }
}