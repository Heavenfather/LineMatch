using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    public partial class GridSystem
    {
        [SerializeField] private SpriteRenderer _guideLevelBg;
        [SerializeField]
        private SpriteRenderer _guideLevelItemBg;
        
        private List<Vector2Int> _guidePathCoords = new List<Vector2Int>();
        private Sequence _guideFingerTween;
        private bool _isGuiding;
        private bool _isGuideFinish;
        private bool _isExecuteGuideLevel;
        private int _currentGuideElementId;
        private bool _isGuideForceSquare;
        private int _guideForceSquareCount;
        private int _guideLevelStep = 1;
        private static bool _guideLevel;

        private void OnGuideTriggerNext(EventOneParam<int> obj)
        {
            OnGuideStart(obj.Arg);
        }

        private void OnGuideStart(int guideId)
        {
            GuideConfig guideData = GuideManager.Instance.FindGuideData(guideId);
            if (guideData.id <= 0)
                return;
            _isGuiding = true;
            _isGuideFinish = false;
            if (guideData.guideType == GuideType.Weak)
            {
                CheckWeakGuideBlock(guideData);
                return;
            }

            CheckForceGuide(guideData);
        }

        private async void OnGuideFinish(EventOneParam<int> obj)
        {
            var guideID = obj.Arg;

            GuideConfigDB db = ConfigMemoryPool.Get<GuideConfigDB>();
            GuideConfig config = db[guideID];
            string parameter2 = config.guideParameters2;
            if (parameter2.StartsWith("CheckNext"))
            {
                await UniTask.Delay(TimeSpan.FromSeconds(MatchConst.DropDuration));
                if (!MatchGuideUtil.ProcessNextGuide(parameter2))
                    return;
            }

            if (_guideCamera != null)
            {
                _guideCamera.SetVisible(false);
            }

            _isGuideFinish = true;
            if (_isGuiding)
            {
                //恢复
                ResetLayer();
                _isGuiding = false;
            }

            // 障碍物提示走这里
            if (guideID >= 100 && guideID <= 300 || guideID == 1002 || guideID == 1012)
            {
                if (guideID == 1002 || guideID == 1012) {
                    DOVirtual.DelayedCall(MatchConst.DropDuration, () => {
                        MatchManager.Instance.GameBeginUseElements();
                    }).SetAutoKill(true);
                } else {
                    MatchManager.Instance.GameBeginUseElements();
                }
            }
        }

        private void CheckWeakGuideBlock(GuideConfig guideConfig)
        {
            ResetLayer();
            _guidePathCoords.Clear();
            if (string.IsNullOrEmpty(guideConfig.guideParameters))
            {
                return;
            }

            string[] blockItems = guideConfig.guideParameters.Split("|");
            for (int i = 0; i < blockItems.Length; i++)
            {
                if (int.TryParse(blockItems[i], out var guideElementId))
                {
                    if(_guideCamera.gameObject.activeSelf == false && G.UIModule.GetWindow("HotfixLogic.CommonLoading") == null)
                        _guideCamera.SetVisible(true);
                    var allElements = ElementSystem.Instance.GridElements;
                    foreach (var kp in allElements)
                    {
                        for (int j = 0; j < kp.Value.Count; j++)
                        {
                            if (kp.Value[j].Data.ConfigId == guideElementId)
                            {
                                GridItem gridItem = GetGridByCoord(kp.Key);
                                if (gridItem != null)
                                {
                                    foreach (var child in gridItem.GameObject.GetComponentsInChildren<Renderer>(true))
                                    {
                                        child.gameObject.layer = LayerMask.NameToLayer("GuideMaskItem");
                                    }

                                    _guidePathCoords.Add(kp.Key);
                                }

                                break;
                            }
                        }
                    }
                }
                else
                {
                    Logger.Error($"消除弱引导[{guideConfig.id}]参数错误，应该填入障碍物id");
                }
            }
        }

        private void CheckForceGuide(GuideConfig guideData)
        {
            string userParam = guideData.guideParameters;
            if (string.IsNullOrEmpty(userParam))
            {
                OnGuideTouchItem(guideData.id, guideData.guideParameters2);
                return;
            }

            ResetLayer();
            //使用道具
            if (float.TryParse(userParam, out var itemId))
            {
                //暂时都是在UI层引导
            }
            else
            {
                if(_guideCamera.gameObject.activeSelf == false && G.UIModule.GetWindow("HotfixLogic.CommonLoading") == null)
                    _guideCamera.SetVisible(true);
                string[] coords = userParam.Split("|");
                DoFingerTweenPlay(coords);
            }
        }

        private void DoFingerTweenPlay(string[] strs, bool needChangeLayer = true, bool showFinger = true)
        {
            if(strs == null)
                return;
            Vector2Int[] coords = new Vector2Int[strs.Length];
            for (int i = 0; i < strs.Length; i++)
            {
                string[] split = strs[i].Split(",");
                coords[i] = new Vector2Int(int.Parse(split[0]), int.Parse(split[1]));
            }

            DoFingerTweenPlay(coords, needChangeLayer, showFinger:showFinger);
        }
        
        private void DoFingerTweenPlay(Vector2Int[] coords, bool needChangeLayer = true, float delayTime = 0.35f, 
                                            bool isLoop = true, bool showFinger = true, float moveDur = 0.2f, float beginDelay = 0.5f)
        {
            if(_isTouching)
                return;
            int elementId = -1;
            _guidePathCoords.Clear();
            _guideFingerTween?.Kill();
            _selectedGrids?.Clear();
            LineController.Instance.ClearAllLines();
            if (coords == null)
            {
                SetFingerVisible(false);
                return;
            }

            SetFingerVisible(false);

            if (needChangeLayer)
            {
                foreach (var child in _guideFinger.GetComponentsInChildren<Renderer>(true))
                {
                    child.gameObject.layer = LayerMask.NameToLayer("GuideMaskItem");
                }
            } else {
                if (_guideFinger.gameObject.layer == LayerMask.NameToLayer("GuideMaskItem")) {
                    foreach (var child in _guideFinger.GetComponentsInChildren<Renderer>(true))
                    {
                        child.gameObject.layer = LayerMask.NameToLayer("Default");
                    }
                }
            }


            var guideFingerSp = _guideFinger.transform.GetComponent<SpriteRenderer>();
            var fingerSp = _guideFinger.transform.GetChild(0).GetComponent<SpriteRenderer>();

            guideFingerSp.DOFade(0, 0);
            fingerSp.DOFade(0, 0);
    
            var fingerMoveTween = DOTween.Sequence();
            fingerMoveTween.AppendInterval(beginDelay);
            fingerMoveTween.AppendCallback(() => {
                SetFingerVisible(true && showFinger);
            });

            fingerMoveTween.Append(guideFingerSp.DOFade(1, 0.3f));
            fingerMoveTween.Join(fingerSp.DOFade(1, 0.3f));
            fingerMoveTween.AppendCallback(() => {
                SetCoordElementScale(coords[0], moveDur, 1.2f);
            });

            for (int i = 0; i < coords.Length; i++)
            {
                Vector2Int coord = coords[i];
                var gridItem = GetGridByCoord(coord.x, coord.y);
                if (gridItem == null)
                    continue;

                if (needChangeLayer)
                {
                    foreach (var child in gridItem.GameObject.GetComponentsInChildren<Renderer>(true))
                    {
                        child.gameObject.layer = LayerMask.NameToLayer("GuideMaskItem");
                    }
                }

                if (elementId == -1)
                {
                    var ele = gridItem.GetBaseElementItem();
                    if (ele != null)
                        elementId = ele.Data.ConfigId;
                    _currentGuideElementId = elementId;
                }

                var gridPos = gridItem.GetPosition();
                bool addTween = false;
                if (i == 0)
                {
                    _guideFinger.transform.position = gridPos;
                    LineController.Instance.AddUnderPoint(gridPos);
                    if (!isLoop)
                        addTween = true;
                }
                else
                {
                    addTween = true;
                }

                if (addTween)
                {
                    fingerMoveTween.Append(_guideFinger.transform.DOMove(gridPos, moveDur).SetEase(Ease.Linear));
                    fingerMoveTween.AppendCallback(() => {
                        SetCoordElementScale(coord, moveDur, 1.2f);
                        LineController.Instance.AddUnderPoint(gridPos);
                    });
                }

                _guidePathCoords.Add(coord);
            }

            if (needChangeLayer)
                LineController.Instance.SetLineLayer("GuideMaskItem");
            LineController.Instance.SetLineColor(ElementSystem.Instance.GetElementColor(elementId));

            fingerMoveTween.AppendCallback(() => {
                CheckLevelGuideSquare(coords);
            });

            if (isLoop)
            {
                fingerMoveTween.AppendInterval(delayTime);

                fingerMoveTween.Append(guideFingerSp.DOFade(0, 0.2f));
                fingerMoveTween.Join(fingerSp.DOFade(0, 0.2f));
                fingerMoveTween.AppendCallback(() => {
                    OnFingerTweenComplete(0).Forget();
                });

                fingerMoveTween.AppendInterval(0.5f);


                fingerMoveTween.SetLoops(-1, LoopType.Restart).SetAutoKill(false).Pause();
                _guideFingerTween = fingerMoveTween;
            }
            else
            {
                fingerMoveTween.SetEase(Ease.Linear).SetAutoKill(false).Pause();
                _guideFingerTween = DOTween.Sequence().Append(fingerMoveTween).AppendInterval(0.5f).
                    AppendCallback(OnFingerMoveComplete).OnComplete(()=>{
                        _guideFingerTween?.Kill();
                        _guideFingerTween = null;
                        SetFingerVisible(false);
                    });
            }
            _guideFingerTween.Play();
        }

        private void CheckLevelGuideSquare(Vector2Int[] coords) {
            if (coords.Length < 4) return;
            if (!_isExecuteGuideLevel || coords[0] != coords[^1]) return;
            var elements = ElementSystem.Instance.GetGridElements(coords[0], true);
            var elemnttId = 0;
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Data.ElementType == ElementType.Normal)
                {
                    elemnttId = elements[i].Data.ConfigId;
                    break;
                }
            }

            if (elemnttId <= 0) return;
            
            foreach (var elementList in ElementSystem.Instance.GridElements.Values) {
                foreach (var element in elementList)
                {
                    if (element.Data.ConfigId == elemnttId)
                    {
                        (element as BaseElementItem).PlaySelectFlash();
                    }
                }
            }
        }
        
        private async UniTask OnFingerTweenComplete(float delayTime)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delayTime));
            if(_guideFingerTween == null || !_guideFingerTween.IsPlaying())
                return;

            if(_guidePathCoords == null || _guidePathCoords.Count <= 0)
                return;

            for (int j = 0; j < _guidePathCoords.Count; j++)
            {
                SetCoordElementScale(_guidePathCoords[j], 0.1f, 1.0f);
            }

            LineController.Instance.ClearUnderLine();
            LineController.Instance.AddUnderPoint(GetGridPositionByCoord(_guidePathCoords[0].x, _guidePathCoords[0].y));
        }

        private void SetPauseOrRestartGuide(bool isPause)
        {
            if (_guideFingerTween == null)
                return;
            LineController.Instance.ClearUnderLine();
            if (isPause)
            {
                SetFingerVisible(false);
                _guideFinger.transform.GetComponent<SpriteRenderer>().color = Color.white;
                _guideFingerTween.Pause();
                for (int j = 0; j < _guidePathCoords.Count; j++)
                {
                    SetCoordElementScale(_guidePathCoords[j], 0.1f, 1.0f);
                }
            }
            else
            {

                LineController.Instance.SetLineColor(ElementSystem.Instance.GetElementColor(_currentGuideElementId));
                LineController.Instance.AddUnderPoint(GetGridPositionByCoord(_guidePathCoords[0].x,
                    _guidePathCoords[0].y));

                // SetFingerVisible(true);
                _guideFingerTween.Restart();
            }
        }

        private void SetCoordElementScale(Vector2Int gridPos, float dur, float scale)
        {
            var gridItem = GetGridByCoord(gridPos.x, gridPos.y);
            var element = gridItem.GetSelectElement(true);
            if (element is BaseElementItem baseElement)
            {
                baseElement.DoPopScale(scale, dur);
            }
        }

        private void ResetLayer()
        {
            var coords = ElementSystem.Instance.GridElements.Keys;
            foreach (var coord in coords)
            {
                GridItem gridItem = GetGridByCoord(coord);
                if (gridItem != null)
                {
                    foreach (var child in gridItem.GameObject.GetComponentsInChildren<Renderer>(true))
                    {
                        child.gameObject.layer = LayerMask.NameToLayer("Background");
                    }
                }
            }
            LineController.Instance.SetLineLayer("Background");
        }

        private void OnMatchLoadingFinish()
        {
            if (_isGuiding)
            {
                if (_guideCamera != null)
                {
                    _guideCamera.transform.position = MainCamera.transform.position;
                    _guideCamera.orthographicSize = MainCamera.orthographicSize;
                    _guideCamera.SetVisible(true);
                }
            }
        }

        private void ClearGuide()
        {
            // KillOnStepTween();
            if (_oneStepSequence != null && _oneStepSequence.IsPlaying())
            {
                _oneStepSequence.Pause();
                SetFingerVisible(false);
                
                for (int j = 0; j < _guidePathCoords.Count; j++)
                {
                    SetCoordElementScale(_guidePathCoords[j], 0f, 1.0f);
                }
            }
            SetPauseOrRestartGuide(true);
        }

        private void OnGuideTouchItem(int guideId, string parameter)
        {
            if (MatchGuideUtil.CheckTouchItem(parameter, out var coord))
            {
                //更新引导
                string nodePath =
                    $"MacthCanvas/GridBoard/{coord.x}-{coord.y}";
                GuideManager.Instance.UpdateShowNode(guideId, nodePath);
            }
        }

        private void OnMatchGuideLevelStepComplete()
        {
            if (_guideLevels.Count <= 0)
            {
                return;
            }
            if (_guideLevels.Count == 1)
            {
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchGuideLevelAllFinish);
            }

            G.EventModule.DispatchEvent(GameEventDefine.OnMatchStepComplete, EventOneParam<int>.Create(_levelData.id));
            UniTask.Create(async () =>
            {
                var level = _guideLevels.Dequeue();
                await StartMatch(level, true, isLastGuide: _guideLevels.Count <= 0);
                if (_guideLevels.Count > 0)
                    G.EventModule.DispatchEvent(GameEventDefine.OnMatchGuideLevelStartFinish,
                        EventOneParam<LevelData>.Create(level));
            }).Forget();
        }

        private async UniTask<ElementBase[]> GenGuideLevelDropElement(Dictionary<Vector2Int,int> delEleMap)
        {
            List<ElementItemData> elementData = new List<ElementItemData>(delEleMap.Count);
            int index = 0;
            foreach (var (coord, elementId) in delEleMap)
            {
                var data = ElementSystem.Instance.GenElementItemData(elementId, coord.x, -index - 1);
                elementData.Add(data);
                index++;
            }

            ElementBase[] dropElements = await ElementSystem.Instance.BatchGenElements(elementData, elementPoolRoot.transform);
            for (int i = 0; i < dropElements.Length; i++)
            {
                if (dropElements[i] == null)
                    continue;
                if (dropElements[i].GameObject != null)
                    dropElements[i].GameObject.transform.position = GetGridPositionByCoord(dropElements[i].Data.GridPos.x,
                        dropElements[i].Data.GridPos.y);
            }
            return dropElements;
        }

        private void ShowLevelGuideFinger(LevelData level)
        {
            string[] coords = MatchGuideUtil.GetLevelGuideFingerCoords(level.id);
            _isGuiding = true;

            DoFingerTweenPlay(coords, false, showFinger: level.id == 1);

            foreach (var child in _guideFinger.GetComponentsInChildren<Renderer>(true))
            {
                child.gameObject.layer = LayerMask.NameToLayer("Background");
            }
        }

        public static bool GetExecuteGuideLevel() {
            return _guideLevel;
        }

        private void SetExecuteGuideLevel(bool isExecuteGuide) {
            _guideLevel = isExecuteGuide;
            _isExecuteGuideLevel = isExecuteGuide;
        }

        private UniTask SetGuideLevelItemBg(int levelId)
        {
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            var imageId = levelId;
            if (levelId == 4) imageId = 3;

            string location = GetGuideLevelItemBgLocation(imageId);
            _guideLevelItemBg.DOFade(0, 0.3f).OnComplete(() =>
            {
                G.ResourceModule.LoadAssetAsync<Sprite>(location, sp =>
                {
                    if (sp == null || this.transform == null)
                    {
                        tcs.TrySetResult();
                        return;
                    }
                    Vector3 pos = GetGridPositionByCoord(0, 0);
                    _guideLevelItemBg.transform.position =
                        new Vector3(pos.x - _gridSize.x / 2, pos.y + _gridSize.y / 2, pos.z);
                    _guideLevelItemBg.SetVisible(true);
                    _guideLevelItemBg.sprite = sp;
                    tcs.TrySetResult();
                }).Forget();
            });
            return tcs.Task;
        }

        private string GetGuideLevelItemBgLocation(int levelId)
        {
            return $"uitexture/guide/item_base_bg{levelId}";
        }

        /// <summary>
        /// 检测一步消除引导，从fromPos到toPos，能不能让元素调用到topos上
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="matchElements"></param>
        /// <param name="fullboard"></param>
        /// <returns></returns>
        private bool OnGuideCanDropDelegate(Vector2Int fromPos, Vector2Int toPos,List<SimElement> matchElements, Dictionary<int, List<SimElement>> fullboard)
        {
            if(!IsValidPosition(toPos.x, toPos.y))
                return false;

            bool IsMatchSidePos(Vector2Int pos)
            {
                HashSet<Vector2Int> sidePos = new HashSet<Vector2Int>(4);
                for (int i = 0; i < matchElements.Count; i++)
                {
                    sidePos.Add(new Vector2Int(matchElements[i].OriginalPosition.x, matchElements[i].OriginalPosition.y + 1));
                }
                return sidePos.Contains(pos);
            }

            bool IsMatchPos(Vector2Int pos)
            {
                for (int i = 0; i < matchElements.Count; i++)
                {
                    if(matchElements[i].OriginalPosition == pos)
                        return true;
                }

                return false;
            }

            if (fullboard.TryGetValue(toPos.x, out var colElements))
            {
                if(IsMatchPos(toPos))
                    return true;
                if (IsMatchSidePos(toPos))
                {
                    var sideElements = ElementSystem.Instance.GetGridElements(new Vector2Int(toPos.x, toPos.y + 1), false);
                    if (sideElements is { Count: > 0 })
                    {
                        int totalEliCount = 0;
                        for (int i = 0; i < sideElements.Count; i++)
                        {
                            if (sideElements[i].Data.EliminateStyle != EliminateStyle.Side &&
                                sideElements[i].Data.EliminateCount >= 1)
                                return false;
                            totalEliCount += _elementMap.CalculateTotalEliminateCount(sideElements[i].Data.ConfigId);
                        }

                        if (totalEliCount <= 1)
                            return true;
                        else
                            return false;
                    }
                }
                else
                {
                    var toElements = ElementSystem.Instance.GetGridElements(toPos, false);
                    if (toElements is { Count: > 0 })
                    {
                        for (int i = 0; i < toElements.Count; i++)
                        {
                            if (toElements[i].Data.HoldGrid >= 1)
                                return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private void SetFingerVisible(bool visible)
        {
            if (_guideFinger != null && _guideFinger.transform != null)
            {
                _guideFinger.SetVisible(visible);
                _guideFinger.transform.GetComponent<SpriteRenderer>().color = Color.white;
                _guideFinger.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }
}