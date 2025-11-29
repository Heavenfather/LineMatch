using System.Collections.Generic;
using DG.Tweening;
using HotfixCore.Extensions;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    public partial class GridSystem
    {
        private List<List<Vector2Int>> _elementTipsList = null;
        private List<Vector2Int> _obstaclesTips = null;
        private List<Vector2Int> _formedRectangleTips = null;
        private MatchTipsType _tipsType;
        private ColorBallTipsTrail _colorBallTipsTrail;
        private RocketTipsTrail _rocketTipsTrail;
        private BombTipsTrail _bombTipsTrail;
        private List<Vector2Int> _specialElementTipsList = null;
        private Sequence _oneStepSequence = null;

        private int _normalTipsSwap = 0;

        private void RefreshMatchTips(bool forceUpdate = false)
        {
            if (!forceUpdate)
            {
                if (_isMatchDone)
                {
                    ClearTipsTimer();
                    return;
                }

                if (_isGuiding)
                    return;
            }

            _haveDoubleTips = false;
            _haveNormalTips = false;
            _swap2Normal = false;
            _haveObstacleTips = false;
            _tipsType = MatchTipsType.None;
            _normalTipsSwap = 0;
            _elementTipsList ??= new List<List<Vector2Int>>();
            _elementTipsList.Clear();
            ClearTipsTimer();
            MatchTipsUtil.RefreshElementGroup();
            if (MatchTipsUtil.TryGetSpecialTipsList(out _specialElementTipsList))
            {
                _haveDoubleTips = true;
                // Stopwatch stopwatch = new Stopwatch();
                // stopwatch.Start();
                if (MatchTipsUtil.TryGetNormalTipsList(ref _elementTipsList, out int priority))
                {
                    _haveNormalTips = true;
                    _tipsType = (MatchTipsType)priority;
                    // Logger.Debug($"RefreshMatchTips total times :{stopwatch.Elapsed.TotalMilliseconds} ms");
                }

                // else
                    _tipsTimerId = G.TimerModule.AddTimer(OnElementTipsUpdate, GetTipsTimerDuration(), true);
            }
            else
            {
                // Stopwatch stopwatch = new Stopwatch();
                // stopwatch.Start();
                if (MatchTipsUtil.TryGetNormalTipsList(ref _elementTipsList, out int priority))
                {
                    _tipsType = (MatchTipsType)priority;
                    _haveNormalTips = true;
                    // if (_levelData.id <= _globalTipsLevel)
                    //     PlayFingerPathTips();
                    // else
                        _tipsTimerId = G.TimerModule.AddTimer(OnElementTipsUpdate, GetTipsTimerDuration(), true);
                }
                else
                {
                    // Stopwatch stopwatch = new Stopwatch();
                    // stopwatch.Start();
                    if (_remainStep >= 2)
                    {
                        var result = MatchTipsUtil.FindRectanglesAfterObstacleRemoval(_levelData.gridCol,
                            _levelData.gridRow, OnGuideCanDropDelegate);
                        if (result != null)
                        {
                            _haveObstacleTips = true;
                            _obstaclesTips = result.Value.obstacles;
                            _formedRectangleTips = result.Value.formedRectangleOriginalPos;
                            _tipsTimerId = G.TimerModule.AddTimer(OnElementTipsUpdate, GetTipsTimerDuration(), true);

                            // PlayOneStepMatchTips(_obstaclesTips, _formedRectangleTips);
                        }
                    }
                    // Logger.Debug($"FindRectanglesAfterObstacleRemoval total times :{stopwatch.Elapsed.TotalMilliseconds} ms");
                }
            }
        }

        private void OnElementTipsUpdate()
        {
            if(_isTouching)
                return;
            if (_isGuiding && _isExecuteGuideLevel == false)
                return;
            if (IsUsingItemState)
                return;
            if (_haveDoubleTips == false && _haveNormalTips == false && _haveObstacleTips == false)
                return;
            if (_haveDoubleTips && _haveNormalTips)
            {
                if (_swap2Normal)
                {
                    if (_levelData.id <= _globalTipsLevel)
                        PlayFingerPathTips();
                    else
                        PlayMatchNormalTips();

                    _normalTipsSwap++;
                    if(_normalTipsSwap >= _elementTipsList.Count)
                        _normalTipsSwap = 0;
                }
                else
                {
                    PlayMatchDoubleTips();
                }

                _swap2Normal = !_swap2Normal;
            }
            else if (_haveDoubleTips)
            {
                PlayMatchDoubleTips();
            }
            else if (_haveNormalTips)
            {
                if (_levelData.id <= _globalTipsLevel)
                    PlayFingerPathTips();
                else
                    PlayMatchNormalTips();

                _normalTipsSwap++;
                if(_normalTipsSwap >= _elementTipsList.Count)
                    _normalTipsSwap = 0;
            }
            else if (_haveObstacleTips)
            {
                if (_oneStepSequence == null || !_oneStepSequence.IsPlaying())
                {
                    PlayOneStepMatchTips(_obstaclesTips, _formedRectangleTips);
                }
                // PlayOneStepMatchTips(_obstaclesTips,_formedRectangleTips);
            }
        }

        private void PlayMatchDoubleTips()
        {
            if(_specialElementTipsList == null || _specialElementTipsList.Count <= 0)
                return;
            var elements1 = ElementSystem.Instance.GetGridElements(_specialElementTipsList[0], true);
            var elements2 = ElementSystem.Instance.GetGridElements(_specialElementTipsList[1], true);
            if (ElementSystem.Instance.TryGetBaseElement(elements1, out int index1,true))
            {
                if (ElementSystem.Instance.TryGetBaseElement(elements2, out int index2,true))
                {
                    var element1 = elements1[index1];
                    var element2 = elements2[index2];
                    //校验方向
                    if (element1.Data.GridPos.x == element2.Data.GridPos.x)
                    {
                        // 垂直方向
                        if (element1.Data.GridPos.y > element2.Data.GridPos.y)
                        {
                            (element1 as BaseElementItem)?.DoTipsTween(0.05f, false);
                            (element2 as BaseElementItem)?.DoTipsTween(-0.05f, false);
                        }
                        else
                        {
                            (element1 as BaseElementItem)?.DoTipsTween(-0.05f, false);
                            (element2 as BaseElementItem)?.DoTipsTween(0.05f, false);
                        }
                    }
                    else if (element1.Data.GridPos.y == element2.Data.GridPos.y)
                    {
                        // 水平方向
                        if (element1.Data.GridPos.x > element2.Data.GridPos.x)
                        {
                            (element1 as BaseElementItem)?.DoTipsTween(-0.05f, true);
                            (element2 as BaseElementItem)?.DoTipsTween(0.05f, true);
                        }
                        else
                        {
                            (element1 as BaseElementItem)?.DoTipsTween(0.05f, true);
                            (element2 as BaseElementItem)?.DoTipsTween(-0.05f, true);
                        }
                    }
                }
            }
        }

        private void PlayMatchNormalTips()
        {
            if (_elementTipsList == null || _elementTipsList.Count <= 0)
                return;
            var l = _elementTipsList[_normalTipsSwap % _elementTipsList.Count];
            for (int i = 0; i < l.Count; i++)
            {
                var grid = GetGridByCoord(l[i]);
                if (grid == null)
                    continue;
                var baseElement = grid.GetBaseElementItem();
                if (baseElement != null)
                {
                    baseElement.PlaySelectFlash(2.0f, 1.2f, 3.0f);
                }
            }

            List<Vector3> positions = new List<Vector3>(l.Count);
            for (int i = 0; i < l.Count; i++)
            {
                positions.Add(GetGridPositionByCoord(l[i].x, l[i].y));
            }

            positions.Add(GetGridPositionByCoord(l[0].x, l[0].y));
            if (_tipsType == MatchTipsType.ColorBall)
            {
                GameObject trail = MatchEffectManager.Instance.Get(MatchEffectType.ColorBallTipsTrail);
                MatchEffectManager.Instance.PlayObjectEffect(trail);
                _colorBallTipsTrail = trail.GetComponent<ColorBallTipsTrail>();
                if (_colorBallTipsTrail != null)
                    _colorBallTipsTrail.DrawTrailRender(positions);
            }
            else if (_tipsType == MatchTipsType.Rocket)
            {
                GameObject trail = MatchEffectManager.Instance.Get(MatchEffectType.RocketTipsTrail);
                _rocketTipsTrail = trail.GetComponent<RocketTipsTrail>();
                MatchEffectManager.Instance.PlayObjectEffect(trail);
                if (_rocketTipsTrail != null)
                    _rocketTipsTrail.PlayTrail(positions);
            }
            else if (_tipsType == MatchTipsType.Bomb)
            {
                GameObject trail = MatchEffectManager.Instance.Get(MatchEffectType.BombTipsTrail);
                _bombTipsTrail = trail.GetComponent<BombTipsTrail>();
                MatchEffectManager.Instance.PlayObjectEffect(trail);
                if (_bombTipsTrail != null)
                    _bombTipsTrail.DrawTrail(positions);
            }
        }

        private void PlayFingerPathTips()
        {
            if (_elementTipsList == null || _elementTipsList.Count <= 0)
                return;
            if(_guideFingerTween != null && _guideFingerTween.IsPlaying())
                return;

            var l = _elementTipsList[_normalTipsSwap % _elementTipsList.Count];
            Vector2Int[] coords = new Vector2Int[l.Count + 1];
            for (int i = 0; i < l.Count; i++)
            {
                coords[i] = l[i];
            }

            coords[^1] = l[0];
            DoFingerTweenPlay(coords, false, 0.2f, false);
        }

        private void PlayOneStepMatchTips(List<Vector2Int> lineList, List<Vector2Int> targetList)
        {
            if(lineList == null || lineList.Count <= 0)
                return;
            if(targetList == null || targetList.Count <= 0)
                return;
            _oneStepSequence?.Kill();
            _oneStepSequence = null;
            
            _guideFinger.transform.position = GetGridPositionByCoord(lineList[0].x, lineList[0].y);
            SetFingerVisible(true);
            int elementId = -1;
            var fingerMoveTween = DOTween.Sequence();
            for (int i = 0; i < lineList.Count; i++)
            {
                Vector2Int coord = lineList[i];
                var gridItem = GetGridByCoord(coord.x, coord.y);
                if (gridItem == null)
                    continue;
                if (elementId == -1)
                {
                    var ele = gridItem.GetBaseElementItem();
                    if (ele != null)
                        elementId = ele.Data.ConfigId;
                }

                var gridPos = gridItem.GetPosition();
                fingerMoveTween.Append(_guideFinger.transform.DOMove(gridPos, 0.3f).SetEase(Ease.Linear)
                    .OnComplete(
                        () =>
                        {
                            SetCoordElementScale(coord, 0.3f, 1.2f);
                            LineController.Instance.AddUnderPoint(gridPos);
                        }));
                _guidePathCoords.Add(coord);
            }
            LineController.Instance.SetLineColor(ElementSystem.Instance.GetElementColor(elementId));
            fingerMoveTween.SetEase(Ease.Linear).SetAutoKill(false);

            _oneStepSequence = DOTween.Sequence();
            _oneStepSequence.Append(fingerMoveTween);


            _oneStepSequence.AppendCallback(() =>
                {
                    for (int i = 0; i < targetList.Count; i++)
                    {
                        var grid = GetGridByCoord(targetList[i]);
                        if (grid == null)
                            continue;
                        var baseElement = grid.GetBaseElementItem();
                        if (baseElement != null)
                        {
                            baseElement.PlaySelectFlash(2.0f, 1.2f, 3.0f);
                        }
                    }
                }).AppendInterval(0.7f).
                AppendCallback(OnFingerMoveComplete).AppendInterval(GetTipsTimerDuration()).
                AppendCallback(() =>
                {
                    SetFingerVisible(true);
                }).SetLoops(-1);
            
        }

        private void KillOnStepTween() {
            if (_oneStepSequence != null) {
                _oneStepSequence.Kill();
                _oneStepSequence = null;
                SetFingerVisible(false);
            }
        }

        private void PauseFingerTipsTween()
        {
            if (_guideFingerTween != null && _guideFingerTween.IsPlaying())
            {
                _guideFingerTween.Pause();
                SetFingerVisible(false);
                LineController.Instance.ClearUnderLine();
                for (int i = 0; i < _guidePathCoords.Count; i++)
                {
                    SetCoordElementScale(_guidePathCoords[i], 0.1f, 1.0f);
                }
            }
        }

        private void OnFingerMoveComplete()
        {
            if(this.transform == null)
                return;
            if(_guidePathCoords == null || _guidePathCoords.Count == 0)
                return;

            SetFingerVisible(false);
            LineController.Instance.ClearUnderLine();
            for (int i = 0; i < _guidePathCoords.Count; i++)
            {
                SetCoordElementScale(_guidePathCoords[i], 0.1f, 1.0f);
            }
            
        }

        private float GetTipsTimerDuration()
        {
            if (_levelData.id >= _globalTipsLevel)
                return 5f;
            return 3f;
        }

        private void ClearTipsTimer()
        {
            if (_tipsTimerId > 0)
            {
                G.TimerModule.RemoveTimer(_tipsTimerId);
                _tipsTimerId = -1;
                foreach (var elements in ElementSystem.Instance.GridElements.Values)
                {
                    for (int i = 0; i < elements.Count; i++)
                    {
                        if (elements[i] is BaseElementItem baseElementItem)
                        {
                            baseElementItem.StopShock();
                            baseElementItem.StopTipsTween();
                        }
                    }
                }
            }

            KillOnStepTween(); 
            if (_colorBallTipsTrail != null)
            {
                _colorBallTipsTrail.StopTrail();
                _colorBallTipsTrail = null;
            }

            if (_rocketTipsTrail != null)
            {
                _rocketTipsTrail.StopTrail();
                _rocketTipsTrail = null;
            }

            if (_bombTipsTrail != null)
            {
                _bombTipsTrail.StopTrail();
                _bombTipsTrail = null;
            }
            
            _guidePathCoords.Clear();
            _guideFingerTween?.Kill();
        }

        private void StopOrResumeTipsTimer(bool isStop,bool isRemove = false)
        {
            if (_tipsTimerId > 0)
            {
                if (isStop)
                {
                    if (isRemove)
                        G.TimerModule.RemoveTimer(_tipsTimerId);
                    else
                        G.TimerModule.Stop(_tipsTimerId);
                    if (_colorBallTipsTrail != null)
                    {
                        _colorBallTipsTrail.StopTrail();
                        _colorBallTipsTrail = null;
                    }

                    if (_rocketTipsTrail != null)
                    {
                        _rocketTipsTrail.StopTrail();
                        _rocketTipsTrail = null;
                    }
                    if (_bombTipsTrail != null)
                    {
                        _bombTipsTrail.StopTrail();
                        _bombTipsTrail = null;
                    }

                    _guideFingerTween?.Kill();
                    _guideFingerTween = null;
                    SetFingerVisible(false);
                }
                else
                {
                    G.TimerModule.Resume(_tipsTimerId);
                }
            }
        }
    }
}