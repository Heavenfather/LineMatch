using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using GameCore.Localization;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Logic.Match;
using Hotfix.Utils;
using HotfixCore.Adapter;
using HotfixCore.Extensions;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;
using Random = UnityEngine.Random;

namespace HotfixLogic.Match
{
    public partial class GridSystem : MonoBehaviour
    {
        private static readonly Vector2 _gridSize = new Vector2(0.8f, 0.8f);
        public static Vector2 GridSize => _gridSize;
        
        [SerializeField] private Transform gridBoard;
        [SerializeField] Camera MainCamera;
        [SerializeField] private Camera _guideCamera;
        [SerializeField] private Camera _effectCamera;

        [SerializeField] private LayerMask gridItemMask;

        [SerializeField] private GameObject elementPoolRoot;
        [SerializeField] private Background2DScaler background2DScaler;
        [SerializeField] private GameObject _overItemBackground;
        [SerializeField] private GridBoardBackground _gridBoardBackground;
        [SerializeField] private MatchCollectItemController _collectController;
        [SerializeField] private TrailEmitter _trailEmitter;
        [SerializeField] private GameObject _guideFinger;

        [SerializeField] private GridWinStreakBox _winStreakBox;

        [SerializeField] private GridLastStepPrompt _lastStepPrompt;

        private bool _isUsingItemState = false;

        private bool IsUsingItemState
        {
            get=> _isUsingItemState;
            set
            {
                if (value)
                {
                    bool isUsingDice = _curUseItem == (int)ItemDef.EliminateDice;
                    var allItems = ElementSystem.Instance.GridElements;
                    foreach (var elements in allItems.Values)
                    {
                        for (int i = 0; i < elements.Count; i++)
                        {
                            if (elements[i] is BaseElementItem baseElementItem)
                            {
                                if (isUsingDice)
                                    baseElementItem.DoShake(true);
                                else
                                    baseElementItem.DoUseItemFlashIcon(true);
                            }
                        }
                    }

                    if (_tipsTimerId > 0)
                    {
                        G.TimerModule.Stop(_tipsTimerId);
                    }
                    PauseFingerTipsTween();
                }
                else
                {
                    if (_tipsTimerId > 0)
                    {
                        G.TimerModule.Resume(_tipsTimerId);
                    }
                    StopUsingItemTipsTween();
                }
                _isUsingItemState = value;
            }
        }
        private bool _isMatchDone = false;
        // private bool _isStartDrag = false;
        private int _curUseItem;
        private int _vibrationForce = 0;
        private int _boardGridCount = 0;
        private int _remainStep = 0;
        private LevelDifficultyModifyData _difficultyModifyData;
        private LevelData _levelData;
        public LevelData LevelData => _levelData;
        private GridItemData[,] _grid;
        private Dictionary<int, GridItem> _gridItems;
        private Dictionary<Vector2Int, GridItem> _coordToGridItems;
        private List<GridItem> _selectedGrids = new List<GridItem>();
        private List<GridItem> _dragHitGrids = new List<GridItem>();
        private List<GridItem> _currentNeighbor = new List<GridItem>();
        private RaycastHit2D[] _linecastResults = new RaycastHit2D[30];
        private GridItem _currentSelectedGrid;
        private Queue<LevelData> _guideLevels = new Queue<LevelData>();
        private ElementMapDB _elementMap;

        private int _tipsTimerId = -1;
        private int _dropRoundCount = 0;
        private int _globalTipsLevel = 0;
        private bool _haveDoubleTips = false;
        private bool _haveNormalTips = false;
        private bool _haveObstacleTips = false;
        private bool _swap2Normal = false;
        private Vector3 _collectResultCoinPos;
        private MatchGameType _matchGameType;
        private bool _canFlyResultCoin = true;

        private void Awake()
        {
            _matchGameType = MatchManager.Instance.CurrentMatchGameType;
            _elementMap = ConfigMemoryPool.Get<ElementMapDB>();
            _globalTipsLevel = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("MatchLevelTips");
            DOTween.SetTweensCapacity(500, 325);

            if (_guideCamera != null)
            {
                _guideCamera.gameObject.SetVisible(false);
            }

            if (_guideFinger != null)
            {
                SetFingerVisible(false);
            }

            G.UIModule.SetSceneCamera(MainCamera);
            
            G.EventModule.AddEventListener(GameEventDefine.OnMatchLoadingFinish, OnMatchLoadingFinish,this);
            G.EventModule.AddEventListener(GameEventDefine.OnMatchCancelItem, OnMatchCancelItem, this);
            G.EventModule.AddEventListener<EventOneParam<ElementDestroyContext>>(
                GameEventDefine.OnDestroyTargetListElement,
                OnDestroyTargetListElement, this);
            G.EventModule.AddEventListener<EventOneParam<int>>(GameEventDefine.OnGuideFinish, OnGuideFinish, this);
            G.EventModule.AddEventListener<EventOneParam<int>>(GameEventDefine.OnMatchUseItem, OnMatchUseItem, this);
            G.EventModule.AddEventListener<EventOneParam<bool>>(GameEventDefine.OnMatchDoneStateChanged, OnMatchDoneStateChanged, this);
            G.EventModule.AddEventListener<EventOneParam<int>>(GameEventDefine.OnGuideTrigger, OnGuideTriggerNext, this);
            G.EventModule.AddEventListener(GameEventDefine.OnMatchRemainStep, OnMatchRemainStep, this); 
            G.EventModule.AddEventListener<EventOneParam<int>>(GameEventDefine.OnMatchControlStep, OnMatchControlStep, this); 
            G.EventModule.AddEventListener<EventThreeParam<int, Vector3, Vector2>>(GameEventDefine.OnDoMatchStepJudge, OnDoMatchStepJudge, this);
            G.EventModule.AddEventListener(GameEventDefine.OnMatchGuideLevelClickFinish,OnMatchGuideLevelStepComplete,this);
            G.EventModule.AddEventListener<EventOneParam<int>>(GameEventDefine.OnMatchStepModify, OnMatchStepModify,
                this);

            G.EventModule.AddEventListener<EventThreeParam<List<int>, List<int>, bool>>(GameEventDefine.OnMatchUpdateSpecialElements,
                OnMatchUpdateSpecialElements, this);

            G.EventModule.AddEventListener<EventThreeParam<int, Vector3, Vector3>>(GameEventDefine.OnMatchPlayCollectItem,
                OnMatchPlayCollectItem, this);
            
            G.EventModule.AddEventListener<EventOneParam<BoardColorStruck>>(GameEventDefine.OnOkChangeBoardColor,OnOkChangeBoardColor,this);
            G.EventModule.AddEventListener(GameEventDefine.OnMatchUseItemFail, OnMatchUseItemFail, this);
            G.EventModule.AddEventListener<EventTwoParam<int, Vector2Int>>(GameEventDefine.OnMatchUseItemSuccess, OnMatchUseItemSuccess, this);
            G.EventModule.AddEventListener<EventOneParam<int>>(GameEventDefine.OnMatchCollectFinishTarget, OnMatchCollectFinishTarget, this);


            var constDB = ConfigMemoryPool.Get<ConstConfigDB>();
            string devicePlatform = CommonUtil.GetDevicePlatform();
            if (devicePlatform == "ios")
                _vibrationForce = constDB.GetConfigIntVal("VibrationForceIOS");
            else
                _vibrationForce = constDB.GetConfigIntVal("VibrationForce");
        }

        private void OnMatchStepModify(EventOneParam<int> obj)
        {
            _remainStep = obj.Arg;
        }

        private void OnMatchUseItemSuccess(EventTwoParam<int, Vector2Int> obj)
        {
            IsUsingItemState = false;
            var grid = GetGridByCoord(obj.Arg2);
            if(grid == null)
                return;
            OnUseItem(obj.Arg1, grid);
        }

        private void OnMatchCollectFinishTarget(EventOneParam<int> param) {
            var targetId = param.Arg;

            // if (targetId == 129) {
            //     SetBoxElementFinish().Forget();
            // }
        }

        private async UniTask SetBoxElementFinish() {
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));

            var elements = ElementSystem.Instance.GetAllElementsById(130);
            foreach (var element in elements)
            {
                (element as BlockElementItem).LoadBoxSprite(true);
            }
        }

        private void OnMatchUseItemFail()
        {
            IsUsingItemState = false;
        }

        private void OnOkChangeBoardColor(EventOneParam<BoardColorStruck> obj)
        {
            SpriteRenderer bgSp = background2DScaler.gameObject.GetComponent<SpriteRenderer>();
            bgSp.color = obj.Arg.BgColor;
        }

        private void OnMatchDoneStateChanged(EventOneParam<bool> obj)
        {
            _isMatchDone = obj.Arg;
            _guidePathCoords?.Clear();
            _guideFingerTween?.Kill();
            KillOnStepTween();
            
            OnTouchEnd();
        }

        private void OnMatchRemainStep()
        {
            ClearTipsTimer();

            ShowMatchFinish().Forget();
        }

        private void OnMatchControlStep(EventOneParam<int> param) {
            var step = param.Arg;
            if (step == 1)
            {
                _lastStepPrompt.ShowLastStepPrompt();
            }
            else {
                _lastStepPrompt.HideLastStepPrompt();
            }
        }

        private async UniTask ShowMatchFinish() {
            _lastStepPrompt.HideLastStepPrompt();

            G.UIModule.ScreenLock(MatchConst.MatchDoneLockReason, true, 5);
            await UniTask.Delay(TimeSpan.FromSeconds(MatchConst.MatchEndWaitDuration));

            G.EventModule.DispatchEvent(GameEventDefine.OnMatchFinish);
        }

        private void OnDoMatchStepJudge(EventThreeParam<int, Vector3, Vector2> param) {
            OnDoMatchStepJudge(param.Arg1, param.Arg2, param.Arg3).Forget();
        }

        private async UniTask OnDoMatchStepJudge(int remainStep,Vector3 stepTextPos, Vector2 coinScreenPos)
        {
            G.UIModule.ScreenLock(MatchConst.MatchDoneLockReason, true, 7);

            HashSet<Vector2Int> delCoords = new HashSet<Vector2Int>();
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();   
            // 添加超时控制
            CancellationTokenSource timeoutCts = new CancellationTokenSource();
            var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: timeoutCts.Token)
                .ContinueWith(() => {
                    if (tcs.Task.Status != UniTaskStatus.Succeeded) {
                        Logger.Warning("剩余步数检测因其它异常超时，直接触发");
                        tcs.TrySetResult();
                    }
                }).SuppressCancellationThrow();
            try
            {
                if (remainStep > 0)
                {
                    int rocketId = ElementSystem.Instance.GetSpecialElementConfigId(ElementType.Rocket);
                    int rocketHorizontalId =
                        ElementSystem.Instance.GetSpecialElementConfigId(ElementType.RocketHorizontal);
                    int bombId = ElementSystem.Instance.GetSpecialElementConfigId(ElementType.Bomb);

                    List<Vector2Int> filterCoords = new List<Vector2Int>();
                    for (int i = 0; i < remainStep; i++)
                    {
                        Vector2Int randomCoord =
                            ElementSystem.Instance.RandomPickBaseElement(filterCoords, out bool success);
                        if (!IsValidPosition(randomCoord.x, randomCoord.y))
                            continue;
                        if (!success)
                            break;
                        if (!filterCoords.Contains(randomCoord))
                            filterCoords.Add(randomCoord);
                    }

                    if (filterCoords.Count > 0)
                    {
                        List<Vector3> endPositions = new List<Vector3>(filterCoords.Count);
                        for (int i = 0; i < filterCoords.Count; i++)
                        {
                            Vector3 endPos = GetGridPositionByCoord(filterCoords[i].x, filterCoords[i].y);
                            endPositions.Add(endPos);
                        }

                        Vector3 startPos = MainCamera.ScreenToWorldPoint(stepTextPos);
                        _trailEmitter.Emitter(startPos, endPositions, (i) =>
                        {
                            int seed = Random.Range(0, 3);
                            int elementId = bombId;
                            if (seed == 0)
                                elementId = rocketId;
                            else if (seed == 1)
                                elementId = rocketHorizontalId;
                            var itemData =
                                ElementSystem.Instance.GenElementItemData(elementId, filterCoords[i].x,
                                    filterCoords[i].y);
                            var elementItem = ElementSystem.Instance.GenElement(itemData);
                            var gridItem = GetGridByCoord(itemData.GridPos);
                            gridItem.PushElement(elementItem, doDestroy: true);
                            elementItem.GameObject.transform.localPosition = Vector3.zero;
                        }, () => { tcs.TrySetResult(); }, TrailEmitterType.StepTrail);
                    }
                    else
                    {
                        tcs.TrySetResult();
                    }
                }
                else
                {
                    tcs.TrySetResult();
                }
                await tcs.Task;
            }
            finally
            {
                // 取消超时任务
                timeoutCts.Cancel();
                timeoutCts.Dispose();
            }

            ValidateManager.Instance.JudgeAllSpecialElements(ref delCoords);
            if (delCoords.Count <= 0)
            {
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchStepMoveEnd);
                G.UIModule.ScreenLock(MatchConst.MatchDoneLockReason, false);
                return;
            }

            List<UniTask> tasks = new List<UniTask>();
            ElementDestroyContext context = ValidateManager.Instance.Context;
            context.IsCalculateCoinState = MatchManager.Instance.GetLevelStar(MatchManager.Instance.CurLevelID) == 0;
            List<DeleteGridInfo> delInfos = new List<DeleteGridInfo>(context.WillDelCoords);
            _collectResultCoinPos = _effectCamera.ScreenToWorldPoint(coinScreenPos);

            foreach (var coord in delCoords)
            {
                // await UniTask.Delay(TimeSpan.FromSeconds(0.03f));
                tasks.Add(DoElement(coord, context, delInfos, false));
            }

            await UniTask.WhenAll(tasks);
            await StartDropElement(context, false);

            await UniTask.Delay(TimeSpan.FromSeconds(0.4f + 0.8f)); //需要额外等待界面的分数更新完成，否则就会出现结算时的分数和界面的分数不一致，等待时间与分数变化的Tween时间一致

            MatchManager.Instance.TickScoreChange();
            if (MatchManager.Instance.GetLevelStar(MatchManager.Instance.CurLevelID) == 0) {
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchUpdateResultCoin);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.7f)); //进度条
            
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchStepMoveEnd);
            G.UIModule.ScreenLock(MatchConst.MatchDoneLockReason, false);
        }

        private void PlayResultCollectCoin(Vector2 coord, ElementDestroyContext context) {
            if (context.IsCalculateCoinState == false || _collectResultCoinPos == Vector3.zero) return;

            var starPos = GetGridPositionByCoord(coord.x, coord.y);
            var endPos = _collectResultCoinPos;
            _collectController.DoCollectResultCoinFlyTarget(starPos, endPos, () => {
                if (!_canFlyResultCoin) return;

                _canFlyResultCoin = false;

                G.EventModule.DispatchEvent(GameEventDefine.OnMatchBeginCollectResultCoin);
            }).Forget();
        }

        private void OnMatchCancelItem()
        {
            IsUsingItemState = false;
        }

        private void StopUsingItemTipsTween()
        {
            var allItems = ElementSystem.Instance.GridElements;
            if(allItems == null)
                return;
            foreach (var elements in allItems.Values)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i] is BaseElementItem baseElementItem)
                    {
                        baseElementItem.DoShake(false);
                        baseElementItem.DoUseItemFlashIcon(false);
                    }
                }
            }
        }

        private void OnMatchUseItem(EventOneParam<int> obj)
        {
            _curUseItem = obj.Arg;

            IsUsingItemState = true;
        }

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(1))
            {
                Vector2 touchPos = Input.mousePosition;
                var gridItem = GetHitGridItem(touchPos);
                if (gridItem != null)
                {
                    var baseElement = gridItem.GetBaseElementItem();
                    if (baseElement == null)
                    {
                        Logger.Debug("请在有基础棋子的格子上使用该GM");
                        return;
                    }
                    int rocketId = (int)ElementIdConst.Rocket;
                    var data = ElementSystem.Instance.GenElementItemData(rocketId, gridItem.Data.Coord.x,
                        gridItem.Data.Coord.y);
                    var element = ElementSystem.Instance.GenElement(data, gridItem.GameObject.transform);
                    element.GameObject.transform.localPosition = Vector3.zero;
                    gridItem.PushElement(element, doDestroy: true);
                }
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(1))
            {
                Vector2 touchPos = Input.mousePosition;
                var gridItem = GetHitGridItem(touchPos);
                if (gridItem != null)
                {
                    var baseElement = gridItem.GetBaseElementItem();
                    if (baseElement == null)
                    {
                        Logger.Debug("请在有基础棋子的格子上使用该GM");
                        return;
                    }
                    int rocketId = (int)ElementIdConst.Bomb;
                    var data = ElementSystem.Instance.GenElementItemData(rocketId, gridItem.Data.Coord.x,
                        gridItem.Data.Coord.y);
                    var element = ElementSystem.Instance.GenElement(data, gridItem.GameObject.transform);
                    element.GameObject.transform.localPosition = Vector3.zero;
                    gridItem.PushElement(element, doDestroy: true);
                }
            }

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(1))
            {
                Vector2 touchPos = Input.mousePosition;
                var gridItem = GetHitGridItem(touchPos);
                if (gridItem != null)
                {
                    var baseElement = gridItem.GetBaseElementItem();
                    if (baseElement == null)
                    {
                        Logger.Debug("请在有基础棋子的格子上使用该GM");
                        return;
                    }
                    int rocketId = (int)ElementIdConst.ColorBall;
                    var data = ElementSystem.Instance.GenElementItemData(rocketId, gridItem.Data.Coord.x,
                        gridItem.Data.Coord.y);
                    var element = ElementSystem.Instance.GenElement(data, gridItem.GameObject.transform);
                    element.GameObject.transform.localPosition = Vector3.zero;
                    gridItem.PushElement(element, doDestroy: true);
                }
            }
#endif

            if (!G.TouchModule.TouchIsValid()) return;
            ProcessTouchPhase(G.TouchModule.TouchPhase, G.TouchModule.InputPos);
        }

        private void OnDestroy()
        {
            G.EventModule.RemoveEventListener<EventOneParam<ElementDestroyContext>>(
                GameEventDefine.OnDestroyTargetListElement,
                OnDestroyTargetListElement, this);
            G.EventModule.RemoveEventListener(GameEventDefine.OnMatchLoadingFinish, OnMatchLoadingFinish, this);
            G.EventModule.RemoveEventListener(GameEventDefine.OnMatchCancelItem, OnMatchCancelItem, this);
            G.EventModule.RemoveEventListener<EventOneParam<int>>(GameEventDefine.OnMatchUseItem, OnMatchUseItem, this);
            G.EventModule.RemoveEventListener<EventOneParam<int>>(GameEventDefine.OnGuideFinish, OnGuideFinish, this);
            G.EventModule.RemoveEventListener<EventOneParam<bool>>(GameEventDefine.OnMatchDoneStateChanged, OnMatchDoneStateChanged, this);
            G.EventModule.RemoveEventListener<EventOneParam<int>>(GameEventDefine.OnGuideTrigger, OnGuideTriggerNext, this);
            G.EventModule.RemoveEventListener(GameEventDefine.OnMatchRemainStep, OnMatchRemainStep, this);
            G.EventModule.RemoveEventListener<EventThreeParam<int, Vector3, Vector2>>(GameEventDefine.OnDoMatchStepJudge, OnDoMatchStepJudge, this);
            G.EventModule.RemoveEventListener<EventOneParam<BoardColorStruck>>(GameEventDefine.OnOkChangeBoardColor,OnOkChangeBoardColor,this);

            G.EventModule.RemoveEventListener<EventOneParam<int>>(GameEventDefine.OnMatchControlStep, OnMatchControlStep, this); 
            G.EventModule.RemoveEventListener<EventThreeParam<List<int>, List<int>, bool>>(GameEventDefine.OnMatchUpdateSpecialElements,
                OnMatchUpdateSpecialElements, this);
            G.EventModule.RemoveEventListener<EventThreeParam<int, Vector3, Vector3>>(GameEventDefine.OnMatchPlayCollectItem,
                OnMatchPlayCollectItem, this);
            G.EventModule.RemoveEventListener(GameEventDefine.OnMatchUseItemFail, OnMatchUseItemFail, this);
            G.EventModule.RemoveEventListener<EventTwoParam<int,Vector2Int>>(GameEventDefine.OnMatchUseItemSuccess, OnMatchUseItemSuccess, this);
            G.EventModule.RemoveEventListener(GameEventDefine.OnMatchGuideLevelClickFinish,OnMatchGuideLevelStepComplete,this);
            G.EventModule.RemoveEventListener<EventOneParam<int>>(GameEventDefine.OnMatchStepModify, OnMatchStepModify,
                this);

            if (_tipsTimerId > 0)
            {
                G.TimerModule.RemoveTimer(_tipsTimerId);
                _tipsTimerId = -1;
            }
        }

        public async UniTask StartMatch(LevelData levelData,bool isGuide = false,List<LevelData> guideLevels = null,bool isLastGuide = false)
        {
            if (guideLevels is { Count: > 0 })
            {
                isGuide = true;
                _guideLevels = new Queue<LevelData>(guideLevels);
                levelData = _guideLevels.Dequeue();
                _isGuideForceSquare = levelData.id == 5;
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchGuideLevelStartFinish,EventOneParam<LevelData>.Create(levelData));
            }

            bool isLevelGuide = isGuide && !isLastGuide;
            SetExecuteGuideLevel(isLevelGuide);
            
            if (_isExecuteGuideLevel) _guideLevelStep = 1;
                
            
            _levelData = levelData;
            _remainStep = _levelData.stepLimit;
            _lastStepPrompt.SetGridSize(_levelData.gridCol, _levelData.gridRow);
            // _difficultyModifyData = LevelManager.Instance.CalDifficultyChangeRate();
            // if(isGuide || MatchManager.Instance.IsEnterByEditor())
            // {
            //     //关卡编辑器进入不做任何调整
            //     _difficultyModifyData.DifficultyType = LevelDifficultyType.None;
            // }
            MatchTweenUtil.Init(this);
            // LevelManager.Instance.ModifyLevelDropRate(ref _levelData, _difficultyModifyData);
            if (!_isExecuteGuideLevel)
            {
                _guideLevelBg.SetVisible(false);
                background2DScaler.gameObject.SetVisible(true);
                _guideLevelItemBg.SetVisible(false);
                SpriteRenderer bgSp = background2DScaler.gameObject.GetComponent<SpriteRenderer>();
                LevelMapImageDB db = ConfigMemoryPool.Get<LevelMapImageDB>();
                int id = db.GetLevelInPage(levelData.id, levelData.id) + 1;
                LevelMapImage levelMapConfig = db[id];
                ColorUtility.TryParseHtmlString(levelMapConfig.matchBgColor, out Color bgColor);
                bgSp.color = bgColor;
            }
            else
            {
                background2DScaler.gameObject.SetVisible(false);
                _guideLevelBg.SetVisible(true);
                await SetGuideLevelItemBg(levelData.id);
            }

            if (isGuide)
            {
                await Clear();
            }
            
            CenterCamera();

            ShuffleElementManager.Instance.Initialized(levelData, this);
            await ElementSystem.Instance.Initialize(elementPoolRoot.transform, levelData);
            InitializeGrid();
            ValidateManager.Instance.BuildBlockInfos(_grid, this);
            await FillInitialGrids();

            if (_guideLevelBg.gameObject.activeSelf) {
                transform.localPosition = new Vector3(0, 11f, 0);

                transform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutBack).SetDelay(0.5f).OnComplete(() => {
                    _guideLevelItemBg.DOFade(1, 0.3f);
                    if (_levelData.id != 4 && isLastGuide == false) {
                        ShowLevelGuideFinger(_levelData);
                    }
                });

                if (_levelData.id == 4) {
                    var elemnets = ElementSystem.Instance.GridElements[new Vector2Int(2, 1)];
                    ElementBase guideElements = null;
                    if (elemnets.Count > 0) {
                        foreach (var element in elemnets) {
                            guideElements = element;
                            break;
                        }
                    }

                    if (guideElements!= null) {
                        G.UIModule.ScreenLock(MatchConst.MatchLockByGenNew, true, 2.3f);

                        guideElements.GameObject.transform.localPosition = new Vector3(0, 11f, 0);
                        var seq = DOTween.Sequence();
                        seq.AppendInterval(2f);
                        seq.AppendCallback(() => {
                            var location = GetGuideLevelItemBgLocation(4);

                            G.ResourceModule.LoadAssetAsync<Sprite>(location, sp =>
                            {
                                if (sp == null || this.transform == null)
                                {
                                    return;
                                }
                                Vector3 pos = GetGridPositionByCoord(0, 0);
                                _guideLevelItemBg.transform.position =
                                    new Vector3(pos.x - _gridSize.x / 2, pos.y + _gridSize.y / 2, pos.z);
                                _guideLevelItemBg.SetVisible(true);
                                _guideLevelItemBg.sprite = sp;
                            }).Forget();
                        });
                        seq.Append(guideElements.GameObject.transform.DOLocalMove(Vector3.zero, 0.3f));
                        seq.AppendCallback(() => {
                            ShowLevelGuideFinger(_levelData);
                        });
                    }
                }
            }



            if (isGuide == false)
            {
                bool hasPair = ShuffleElementManager.Instance.IsBoardHasPair();
                if (!hasPair)
                {
                    var shuffleList = ShuffleElementManager.Instance.CollectShuffleElements();
                    ShuffleElementManager.Instance.ShuffleBoard(shuffleList);
                }

                DrawSpreadWaterUtil.InitWater();
                DrawSnowLineUtil.InitSnowDict();
                SpreadGrassUtil.UpdateGrassBorder();
                RocketUtil.UpdateRocketIdleEffectVisible();
                MatchLineBlockElementItemUtil.UpdateIdleAnimation();
            }

        }

        public async UniTask Clear()
        {
            _guideFingerTween?.Kill();
            _guideFingerTween = null;
            _guidePathCoords.Clear();
            ClearGuide();
            ClearTipsTimer();
            _collectController.ClearController();
            if (_grid != null)
            {
                foreach (var data in _grid)
                {
                    MemoryPool.Release(data);
                }
            }

            if (_gridItems != null)
            {
                foreach (var gridItem in _gridItems.Values)
                {
                    MemoryPool.Release(gridItem);
                }
            }

            ElementAudioManager.Instance.ReleaseAudio();
            _levelData.Clear();
            _coordToGridItems?.Clear();
            IsUsingItemState = false;

            // _gridBoardBackground.Clear();
            await ElementSystem.Instance.ClearGridElements();
        }
        
        public async UniTask Restart()
        {
            foreach (var data in _grid)
            {
                MemoryPool.Release(data);
            }

            ElementSystem.Instance.Restart();
            _levelData.Clear();
            IsUsingItemState = false;
            _oneStepSequence?.Kill();
            _oneStepSequence = null;
            _guidePathCoords?.Clear();
            ClearGuide();
            StopOrResumeTipsTimer(true);
            
            await ElementSystem.Instance.Initialize(elementPoolRoot.transform, _levelData);
            InitializeGrid();
            ValidateManager.Instance.BuildBlockInfos(_grid, this);

            foreach (var item in _coordToGridItems)
            {
                var data = _grid[item.Key.x, item.Key.y];
                item.Value.ReCreateElement(data);
            }
            _selectedGrids?.Clear();
            _currentNeighbor?.Clear();
            _currentSelectedGrid = null;

            DrawSnowLineUtil.InitSnowDict();
            DrawSpreadWaterUtil.InitWater();

            _lastStepPrompt.HideLastStepPrompt();
        }

        public void DoTrailEmitter(Vector3 startPos,List<Vector3> endPositions,Action<int> onStepComplete, Action onComplete,TrailEmitterType type = TrailEmitterType.Trail)
        {
            _trailEmitter.Emitter(startPos, endPositions, onStepComplete, onComplete, type);
        }
        
        private void CenterCamera()
        {
            const float boardPadding = 0.5f;

            int minX = -1;
            for (int x = 0; x < _levelData.gridCol; x++)
            {
                for (int y = 0; y < _levelData.gridRow; y++)
                {
                    if (!_levelData.grid[x][y].isWhite)
                    {
                        minX = x;
                        break;
                    }
                }

                if (minX >= 0) break;
            }

            int minY = -1;
            for (int y = 0; y < _levelData.gridRow; y++)
            {
                for (int x = 0; x < _levelData.gridCol; x++)
                {
                    if (!_levelData.grid[x][y].isWhite)
                    {
                        minY = y;
                        break;
                    }
                }

                if (minY >= 0) break;
            }


            int maxX = -1;
            for (int x = _levelData.gridCol - 1; x >= 0; x--)
            {
                for (int y = _levelData.gridRow - 1; y >= 0; y--)
                {
                    if (!_levelData.grid[x][y].isWhite)
                    {
                        maxX = x;
                        break;
                    }
                }

                if (maxX >= 0) break;
            }

            int maxY = -1;
            for (int y = _levelData.gridRow - 1; y >= 0; y--)
            {
                for (int x = _levelData.gridCol - 1; x >= 0; x--)
                {
                    if (!_levelData.grid[x][y].isWhite)
                    {
                        maxY = y;
                        break;
                    }
                }

                if (maxY >= 0) break;
            }

            Vector2Int startPos = new Vector2Int(minX, minY);
            Vector2Int endPos = new Vector2Int(maxX, maxY);

            Vector2 centerCoord = new Vector2((startPos.x + endPos.x) / 2.0f, (startPos.y + endPos.y) / 2.0f);
            Vector3 centerPosition = GetGridPositionByCoord(centerCoord.x, centerCoord.y);
            SetCameraPos(new Vector3(centerPosition.x, centerPosition.y + 0.8f, MainCamera.transform.position.z));
            
            if (maxX + 1 >= 8 && !MatchManager.Instance.IsEnterByEditor())
            {
                float screenAspect = MainCamera.aspect;
                float physicalWidth = (endPos.x - startPos.x + 1) * _gridSize.x;
                float physicalHeight = (endPos.y - startPos.y + 1) * _gridSize.y;
                float boardAspect = physicalWidth / physicalHeight;
                float orthographicSize = 0.0f;
                // 判断应该以宽度还是高度为基准
                if (boardAspect > screenAspect)
                {
                    // 棋盘比屏幕更宽 → 以宽度为基准
                    orthographicSize = (physicalWidth / (2f * screenAspect)) + boardPadding;
                }
                else
                {
                    // 棋盘比屏幕更高 → 以高度为基准
                    orthographicSize = (physicalHeight / 2f) + boardPadding;
                }

                MainCamera.orthographicSize = Mathf.Max(orthographicSize, 8.2f);
                _effectCamera.orthographicSize = MainCamera.orthographicSize;
            }
            _guideCamera.transform.position = MainCamera.transform.position;
            _guideCamera.orthographicSize = MainCamera.orthographicSize;

            background2DScaler.ScaleBackground(MainCamera);
            var guideBg = _guideLevelBg.GetComponent<Background2DScaler>();
            if (guideBg != null)
            {
                guideBg.ScaleBackground(MainCamera);
            }
            _overItemBackground.transform.position = background2DScaler.transform.position;
            _overItemBackground.transform.localScale = background2DScaler.transform.localScale;

            if (_isExecuteGuideLevel == false)
                _gridBoardBackground.UpdateGridBg(_levelData, startPos, endPos);
        }

        private void SetCameraPos(Vector3 pos) {
            MainCamera.transform.position = pos;
            _winStreakBox.transform.position = new Vector3(pos.x, pos.y - 5f, -1);
        }

        public (int, int) GetBoardSize()
        {
            return (_levelData.gridCol, _levelData.gridRow);
        }

        public static Vector3 GetGridPositionByCoord(float x, float y)
        {
            return new Vector3(-4 + x * _gridSize.x - 0.8f, 5 - y * _gridSize.y, 0);
        }

        #region 元素操作

        public GridItem GetGridByCoord(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return null;
            return _coordToGridItems.GetValueOrDefault(new Vector2Int(x, y));
        }

        public GridItem GetGridByCoord(Vector2Int coord)
        {
            if (!IsValidPosition(coord.x, coord.y))
                return null;
            return _coordToGridItems.GetValueOrDefault(coord);
        }

        private void OnUseItem(int itemId,GridItem grid)
        {
            if (itemId == (int)ItemDef.EliminateDice)
            {
                UseDice();
            }
            else if (itemId == (int)ItemDef.EliminateHammer)
            {
                UseHammer(grid);
            }
            else if (itemId == (int)ItemDef.EliminateBullet)
            {
                UseBullet(grid, direction: ElementDirection.Up);
            }
            else if (itemId == (int)ItemDef.EliminateArrow)
            {
                UseBullet(grid, direction: ElementDirection.Right);
            }
            else if (itemId == (int)ItemDef.EliminateColored)
            {
                UseColored(grid);
            }
        }

        private bool CheckUseItem(Vector2Int coord)
        {
            if(!IsValidPosition(coord.x, coord.y))
            {
                CommonUtil.ShowCommonTips(LocalizationPool.Get("Match/UseItemPosInvalid"));
                return false;
            }
            
            if (_curUseItem == (int)ItemDef.EliminateDice || _curUseItem == (int)ItemDef.EliminateBullet || _curUseItem == (int)ItemDef.EliminateArrow)
            {
                return true;
            }

            if (_curUseItem == (int)ItemDef.EliminateHammer)
            {
                var elements = ElementSystem.Instance.GetGridElements(coord, false);
                if (elements == null || elements.Count == 0)
                {
                    return false;
                }
                bool bResult = ElementSystem.Instance.TryGetWillDestroyElementItem(elements, out int _);
                return bResult;
            }

            if (_curUseItem == (int)ItemDef.EliminateColored)
            {
                var gridItem = GetGridByCoord(coord);
                if(gridItem == null)
                    return false;
                var baseElement = gridItem.GetBaseElementItem();
                if (baseElement == null)
                {
                    CommonUtil.ShowCommonTips(LocalizationPool.Get("Match/SelectNormalElement"));
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 使用骰子
        /// </summary>
        private void UseDice()
        {
            AudioUtil.PlayUseDice();

            G.UIModule.ScreenLock(MatchConst.MatchDoneLockReason, true, MatchConst.DropDuration);
            
            var shuffleList = ShuffleElementManager.Instance.CollectShuffleElements();
            ShuffleElementManager.Instance.ShuffleBoard(shuffleList, true);
            RefreshMatchTips();
        }

        /// <summary>
        /// 使用锤子
        /// </summary>
        private void UseHammer(GridItem grid)
        {
            var elements = ElementSystem.Instance.GetGridElements(grid.Data.Coord, false);
            if (elements == null || elements.Count == 0)
            {
                return;
            }

            bool bResult = ElementSystem.Instance.TryGetWillDestroyElementItem(elements, out int index);
            if (bResult)
            {
                var element = elements[index];
                GridItem realGrid = null;
                if (element.Data.ElementType == ElementType.FixPosExpand)
                {
                    realGrid = grid;
                }
                else
                {
                   realGrid = GetGridByCoord(element.Data.GridPos);
                }
                AddSelectGrids(realGrid);
                ValidateManager.Instance.Judge(in _selectedGrids, false, (b) => { _selectedGrids.Clear(); },
                    (int)ItemDef.EliminateHammer);
            }
        }

        /// <summary>
        /// 使用子弹
        /// </summary>
        private void UseBullet(GridItem grid, ElementDirection direction)
        {
            int rocketId = ElementSystem.Instance.GetSpecialElementConfigId(direction == ElementDirection.Up
                ? ElementType.Rocket
                : ElementType.RocketHorizontal);
            var rocketItemData = ElementSystem.Instance.GenElementItemData(rocketId, grid.Data.Coord.x, grid.Data.Coord.y);
            var element = ElementSystem.Instance.GenElement(rocketItemData);
            grid.PushElement(element, false, false);
            element.GameObject.transform.localPosition = Vector3.zero;
            AddSelectGrids(grid);
            ValidateManager.Instance.Judge(_selectedGrids, false, (b) => { _selectedGrids.Clear(); },
                (int)ItemDef.EliminateBullet);
        }

        /// <summary>
        /// 使用炫彩冲击
        /// </summary>
        private void UseColored(GridItem grid)
        {
            ElementBase selectBaseElement = grid.GetBaseElementItem();
            if (selectBaseElement == null)
            {
                return;
            }

            int colorBallId = ElementSystem.Instance.GetSpecialElementConfigId(ElementType.ColorBall);
            var colorElementData =
                ElementSystem.Instance.GenElementItemData(colorBallId, grid.Data.Coord.x, grid.Data.Coord.y);
            colorElementData.ColorBallDestroyId = selectBaseElement.Data.ConfigId;
            grid.RemoveBaseElement(true);
            var element = ElementSystem.Instance.GenElement(colorElementData);
            grid.PushElement(element, false, false);
            element.GameObject.transform.localPosition = Vector3.zero;
            AddSelectGrids(grid);
            ValidateManager.Instance.Judge(_selectedGrids, false, (b) => { _selectedGrids.Clear(); },
                (int)ItemDef.EliminateColored);

            AudioUtil.PlayUseColored();
        }

        #endregion

        #region 画背景图

        private void OnMatchPlayCollectItem(EventThreeParam<int, Vector3, Vector3> obj)
        {
            int elementId = obj.Arg1;
            var startPos = obj.Arg2;
            // var endPos = obj.Arg3;
            
            var endPos = _effectCamera.ScreenToWorldPoint(obj.Arg3);
            Action callback = null;
            if (elementId == (int)CollectItemEnum.Coin) {
                callback = () => {
                    AudioUtil.PlayGetCoin();
                };
            } else {
                callback = () => {
                    ElementAudioManager.Instance.PlayCollect(elementId);
                };
            }

            PlayCollectBeginAudio(elementId);

            _collectController.DoCollectItemFlyToTarget(elementId, startPos, endPos, callback);
        }

        private void PlayCollectBeginAudio(int elementId) {
            if (elementId == (int)ElementIdConst.WishBottle) {
                AudioUtil.PlayFlyXuyuanPing();
            } else if (elementId == (int)ElementIdConst.Butterfly) {
                AudioUtil.PlayFlyButterfly();
            }
        }

        private void OnMatchUpdateSpecialElements(EventThreeParam<List<int>, List<int>, bool> param)
        {
            var useBooster = param.Arg1;
            var winstreakBooster = param.Arg2;

            if ((useBooster == null || winstreakBooster == null) || (useBooster.Count <= 0 && winstreakBooster.Count <= 0))
            {
                RefreshMatchTips();
                return;
            }

            List<int> specialElementId;

            bool isResult = param.Arg3;
            if (isResult) {
                UpdateSpecialElementsByResult(ref useBooster);
                if (useBooster.Count <= 0) return;

                specialElementId = useBooster;
            } else {
                specialElementId = new List<int>();
                specialElementId.AddRange(useBooster);
                specialElementId.AddRange(winstreakBooster);
            }

            var coordVecList = ElementSystem.Instance.GetValidCoordsVecs(new List<Vector2Int>());
            
            // 可摆放的位置
            var vecDict = new Dictionary<Vector2Int, bool>();
            foreach (var vec in coordVecList) {
                var elements = ElementSystem.Instance.GetGridElements(vec, true);
                if (elements != null && elements.Count > 0) {
                    if (ElementSystem.Instance.TryGetBaseElement(elements, out var _)) {
                        vecDict[vec] = true;
                    }
                }
            }

            if (vecDict.Count <= 0) return;

            var specialIds = new List<int>();
            var specialElimentPos = new List<Vector2Int>();
            var random = new System.Random();

            // 使用OrderBy和Select方法来打乱列表
            var pushCoordList = vecDict.Keys.OrderBy(n => random.Next()).ToList();

            // 道具数量大于可选坐标数量，则随机
            // 只有一个道具，存随机
            if (specialElementId.Count >= pushCoordList.Count || specialElementId.Count <= 1) {
                for (int i = 0; i < specialElementId.Count; i++) {
                    if (i < pushCoordList.Count) {
                        specialIds.Add(specialElementId[i]);
                        specialElimentPos.Add(pushCoordList[i]);
                    } else {
                        break;
                    }
                }
            } else {
                var configStrArr = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigStrVal("MatchBoosterNeighbor").Split('|');
                var rateArr = new int[configStrArr.Length];
                for (int i = 0; i < configStrArr.Length; i++)
                {
                    rateArr[i] = int.Parse(configStrArr[i]);
                }

                // 根据动态关卡难度调整概率
                // LevelManager.Instance.ModifyGenerateItemRate(ref rateArr, _difficultyModifyData);

                var rateValue = random.Next(0, rateArr.Sum());
                Logger.Debug("随机值：" + rateValue);

                var rateIndex = 0;
                var addValue = 0;
                for (int i = 0; i < rateArr.Length; i++)
                {
                    addValue += rateArr[i];
                    rateIndex = i;
                    if (addValue >= rateValue) break;
                }

                // 相邻的个数
                var neighborCount = rateIndex + 1;
                if (neighborCount > specialElementId.Count) {
                    neighborCount = specialElementId.Count;
                }
                Logger.Debug("相邻个数  至少为：" + neighborCount);
            
                var specialPosList = new List<Vector2Int>();
                for (int i = 0; i < pushCoordList.Count; i++) {
                    var curVec = pushCoordList[i];
                    specialPosList.Add(curVec);

                    if (specialPosList.Count < neighborCount) {
                        var foreachList = MatchTweenUtil.GetNeighborPos(pushCoordList[i]).OrderBy(n => random.Next()).ToList();
                        foreach (var vec in foreachList) {
                            if (vecDict.ContainsKey(vec)) {
                                specialPosList.Add(vec);
                                if (specialPosList.Count >= neighborCount) {
                                    break;
                                }
                            }
                        }
                    }

                    if (specialPosList.Count > neighborCount) {
                        specialPosList.RemoveRange(neighborCount, specialPosList.Count - neighborCount);
                    }

                    if (specialPosList.Count == neighborCount) {
                        // 查找不相邻的棋子

                        // 禁止摆放的坐标
                        var banPosList = new Dictionary<Vector2Int, bool>();
                        foreach (var vec in specialPosList) {
                            banPosList[vec] = true;
                            var neighborPosList = MatchTweenUtil.GetNeighborPos(vec);
                            foreach (var neighborPos in neighborPosList) {
                                if (vecDict.ContainsKey(neighborPos)) {
                                    banPosList[neighborPos] = true;
                                }
                            }
                        }

                        // 查找所有坐标，查找不相邻的位置
                        foreach (var vec in pushCoordList) {
                            if (!banPosList.ContainsKey(vec)) {
                                specialPosList.Add(vec);
                                if (specialPosList.Count >= specialElementId.Count) {
                                    break;
                                }
                            }
                        }
                    }

                    
                    if (specialPosList.Count >= specialElementId.Count) {
                        // 找够了位置，退出
                        break;
                    } else {
                        // 没有找齐继续寻找
                        specialPosList.Clear();
                    }
                }

                if (specialPosList.Count < specialElementId.Count) {
                    // 未找齐，随机
                    specialPosList.Clear();
                    for (int i = 0; i < specialElementId.Count; i++) {
                        if (pushCoordList.Count < i + 1) {
                            specialPosList.Add(pushCoordList[i]);
                        } else {
                            break;
                        }
                    }
                }

                for (int i = 0; i < specialPosList.Count; i++) {
                    if (i < specialElementId.Count) {
                        specialIds.Add(specialElementId[i]);
                        specialElimentPos.Add(specialPosList[i]);
                    } else {
                        break;
                    }
                }
            }

            if (isResult) {
                specialIds = specialIds.OrderBy(n => random.Next()).ToList();
                for (int i = 0; i < specialIds.Count; i++) {
                    Vector2Int vec = specialElimentPos[i]; 

                    var itemData = ElementSystem.Instance.GenElementItemData(specialElementId[i], vec.x, vec.y);
                    var elementItem = ElementSystem.Instance.GenElement(itemData);
                    var grid = GetGridByCoord(vec);
                    grid.PushElement(elementItem, doDestroy: true);
                    elementItem.GameObject.transform.localPosition = Vector3.zero;
                }
            } else {
                var hadUseIdx = new List<int>();
                var posList = new List<Vector3>();
                for (int i = 0; i < specialElimentPos.Count; i++) {
                    Vector2Int vec = specialElimentPos[i];
                    if (useBooster.Contains(specialIds[i])) {
                        var itemData = ElementSystem.Instance.GenElementItemData(specialElementId[i], vec.x, vec.y);
                        var elementItem = ElementSystem.Instance.GenElement(itemData);
                        var grid = GetGridByCoord(vec);
                        grid.PushElement(elementItem, doDestroy: true);
                        elementItem.GameObject.transform.localPosition = Vector3.zero;

                        useBooster.Remove(specialIds[i]);
                        hadUseIdx.Add(i);
                    } else {
                        var pos = GetGridPositionByCoord(vec.x, vec.y);
                        posList.Add(pos);
                    }
                }

                for (int i = hadUseIdx.Count - 1; i >= 0; i--) {
                    specialIds.RemoveAt(hadUseIdx[i]);
                    specialElimentPos.RemoveAt(hadUseIdx[i]);
                }

                MatchManager.Instance.ClearBeginUseElements();
                if (specialIds.Count <= 0) return;

                Action<int, Vector2Int, bool> callback = (specialID, coord, isRefresh) => {
                    var itemData = ElementSystem.Instance.GenElementItemData(specialID, coord.x, coord.y);
                    var elementItem = ElementSystem.Instance.GenElement(itemData);
                    var grid = GetGridByCoord(coord);
                    grid.PushElement(elementItem, doDestroy: true);
                    elementItem.GameObject.transform.localPosition = Vector3.zero;
                    RefreshMatchTips();

                    G.UIModule.ScreenLock(MatchConst.MatchDoneLockReason, false);
                };


                G.UIModule.ScreenLock(MatchConst.MatchDoneLockReason, true, 4);

                _winStreakBox.transform.DOScale(1f, 0.3f).OnComplete(() => {
                    _winStreakBox.gameObject.SetActive(true);
                    _winStreakBox.SetWinStreakItem(specialIds, posList, specialElimentPos, callback);
                });                
            }
        }



        private void UpdateSpecialElementsByResult(ref List<int> specialElementId) {
            if (specialElementId.Count <= 0) return;

            var coordVecList = ElementSystem.Instance.GridElements.Keys.ToList();

            int[] blockHorizontalCount = new int[_levelData.gridRow];
            int[] blockVerticalCount = new int[_levelData.gridCol]; 

            int[] normalHorizontalCount = new int[_levelData.gridRow];
            int[] normalVerticalCount = new int[_levelData.gridCol];
            
            var normalElementDict = new Dictionary<Vector2Int, bool>();
            var blockElementDict = new Dictionary<Vector2Int, bool>();
            foreach (var vec in coordVecList) {
                var elements = ElementSystem.Instance.GetGridElements(vec, true);
                if (elements != null && elements.Count > 0) {
                    if (ElementSystem.Instance.TryGetBaseElement(elements, out var _)) {
                        normalElementDict[vec] = true;

                        normalHorizontalCount[vec.y]++;
                        normalVerticalCount[vec.x]++;
                    }

                    foreach (var element in elements) {
                        var elementType = element.Data.ElementType;
                        if (elementType != ElementType.Normal && !ElementSystem.Instance.IsSpecialElement(elementType))
                        {
                            blockElementDict[vec] = true;

                            blockHorizontalCount[vec.y]++;
                            blockVerticalCount[vec.x]++;
                        }
                    }
                }
            }

            
            // 找出最多的阻碍物的 行 或 列
            var maxCount = 0;
            var idxNum = 0;
            bool isHorizontal = false;
            for (int i = 0; i < blockHorizontalCount.Length; i++) {
                if (blockHorizontalCount[i] > maxCount && normalHorizontalCount[i] > 0) {
                    maxCount = blockHorizontalCount[i];
                    idxNum = i;
                    isHorizontal = true;
                }
            }

            for (int i = 0; i < blockVerticalCount.Length; i++) {
                if (blockVerticalCount[i] > maxCount && normalVerticalCount[i] > 0) {
                    maxCount = blockVerticalCount[i];
                    idxNum = i;
                    isHorizontal = false;
                }
            }


            // 找出最多的阻碍物的 位置
            var blockLinkRange = Vector2Int.zero;
            var foreachNum = isHorizontal ? _levelData.gridCol : _levelData.gridRow;
            var maxBlockCount = 0;
            for (int i = 0; i < foreachNum; i++) {
                var range = Vector2Int.one * i;
                var blockCount = 0;
                var curVec = isHorizontal ? new Vector2Int(i, idxNum) : new Vector2Int(idxNum, i);
                

                while (blockElementDict.ContainsKey(curVec)) {
                    blockCount++;
                    range.y = isHorizontal ? curVec.x : curVec.y;

                    curVec += isHorizontal ? Vector2Int.right : Vector2Int.up;
                }


                if (blockCount > maxBlockCount) {
                    maxBlockCount = blockCount;
                    blockLinkRange = range;
                }
            }


            var findId = 0;
            foreach (var id in specialElementId) {
                if (id == (int)ElementIdConst.Rocket || id == (int)ElementIdConst.Bomb || id == (int)ElementIdConst.RocketHorizontal) {
                    findId = id;
                    break;
                }
            }
            if (findId == 0) return;

            var specialId = findId;

            // 11：水平火箭  8：竖直火箭
            bool isRocket = false;
            if (specialId == (int)ElementIdConst.Rocket || specialId == (int)ElementIdConst.RocketHorizontal) {
                isRocket = true;
                if (isHorizontal) {
                    specialId = (int)ElementIdConst.RocketHorizontal;
                } else {
                    specialId = (int)ElementIdConst.Rocket;
                }
            }

            var specialPos = Vector2Int.zero * -1;
            bool hasFindSpendPos = false;
            var lineCount = isHorizontal ? _levelData.gridCol : _levelData.gridRow;
            for (int i = 0; i < lineCount; i++) {
                var pos = isHorizontal ? new Vector2Int(i, idxNum) : new Vector2Int(idxNum, i);
                if (normalElementDict.ContainsKey(pos)) {
                    specialPos = pos;
                    hasFindSpendPos = true;
                }

                if (hasFindSpendPos && i >= blockLinkRange.x) {
                    break;
                }
            }

            if (!isRocket) {
                bool hasFindBombPos = false;
                // 如果不是火箭，尝试放在旁边，炸掉更多障碍
                for (int i = blockLinkRange.x; i <= blockLinkRange.y; i++) {
                    var pos = isHorizontal ? new Vector2Int(i, idxNum) : new Vector2Int(idxNum, i);

                    var neighborPos = MatchTweenUtil.GetNeighborPos(pos,false);
                    var neighborPos1 = isHorizontal ? neighborPos[0] : neighborPos[1];
                    var neighborPos2 = isHorizontal ? neighborPos[3] : neighborPos[2];

                    if (normalElementDict.ContainsKey(neighborPos1)) {
                        specialPos = neighborPos1;
                        hasFindBombPos = true;
                    }

                    if (normalElementDict.ContainsKey(neighborPos2)) {
                        specialPos = neighborPos2;
                        hasFindBombPos = true;
                    }

                    // 找到障碍物中间的位置就可以退出
                    if (hasFindBombPos && i > blockLinkRange.x && i < blockLinkRange.y) {
                        break;
                    }
                }
            }

            if (specialPos != Vector2Int.zero * -1) {
                var itemData = ElementSystem.Instance.GenElementItemData(specialId, specialPos.x, specialPos.y);
                var elementItem = ElementSystem.Instance.GenElement(itemData);
                var grid = GetGridByCoord(specialPos);
                grid.PushElement(elementItem, doDestroy: true);
                elementItem.GameObject.transform.localPosition = Vector3.zero;

                specialElementId.Remove(findId);
            }
        }

        #endregion
    }
}