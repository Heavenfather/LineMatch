using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixCore.Extensions;
using HotfixCore.Module;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HotfixLogic.Match
{
    public partial class GridSystem
    {
        private bool _rectangleFlag = false;
        private bool _isLineSquare = false;
        private bool _isTouching = false;
        
        private void ProcessTouchPhase(TouchPhase phase, Vector2 screenPos)
        {
            switch (phase)
            {
                case TouchPhase.Began:
                    OnTouchStart(screenPos);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    OnTouchDrag(screenPos);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    _isTouching = false;
                    OnTouchEnd();
                    break;
            }
        }

        private void OnTouchStart(Vector2 screenPos)
        {
            if (IsPointerOverUI(screenPos)) return;
            
            var gridItem = GetHitGridItem(screenPos);
            if (gridItem != null)
            {
                _isTouching = true;
                var elements = ElementSystem.Instance.GetGridElements(gridItem.Data.Coord, false);
                if (!ElementSystem.Instance.TryGetBaseElement(elements, out int _, true) && !IsUsingItemState)
                    return;

                if (IsUsingItemState)
                {
                    if (CheckUseItem(gridItem.Data.Coord))
                    {
                        G.EventModule.DispatchEvent(GameEventDefine.OnMatchReqUseItem,
                            EventTwoParam<int, Vector2Int>.Create(_curUseItem, gridItem.Data.Coord));
                    }
                }
                else
                {
                    var selectEle = gridItem.DoSelect();
                    if (selectEle != null)
                    {
                        // if (_isGuiding)
                        {
                            GuideManager.Instance.HideGuideLine();
                            ClearGuide();
                        }

                        _currentSelectedGrid = gridItem;
                        AddSelectGrids(gridItem);
                        CommonUtil.DeviceVibration(_vibrationForce, 0.1f);
                        LineController.Instance.SetLineColor(
                            ElementSystem.Instance.GetElementColor(selectEle.Data.ConfigId));
                        if (LineController.Instance.GetUnderLinePointCount() > 0)
                        {
                            LineController.Instance.ClearAllLines();
                        }

                        LineController.Instance.AddUnderPoint(_currentSelectedGrid.GetPosition());
                        LineController.Instance.SetOverLinePoint(1, _currentSelectedGrid.GetPosition());
                    }
                }
            }
        }

        private void OnTouchDrag(Vector2 screenPos)
        {
            if (IsPointerOverUI(screenPos)) return;
            if (_isUsingItemState) return;
            if (_currentSelectedGrid == null)
            {
                _currentSelectedGrid = GetHitGridItem(screenPos);
                if (_currentSelectedGrid == null)
                    return;
                OnTouchStart(screenPos);
                return;
            }

            if (_currentSelectedGrid.GetSelectElement(true) == null)
            {
                return;
            }

            if (!_currentSelectedGrid.GetSelectElement(true).CanSelect())
                return;
            if (_currentSelectedGrid.Data.IsGridLock())
                return;

            Vector2 overLineLastPos = MainCamera.ScreenToWorldPoint(screenPos);

            if (!_rectangleFlag)
            {
                int pointsCount = LineController.Instance.GetOverLinePointCount();
                if (pointsCount <= 0 && _selectedGrids.Count > 0)
                {
                    LineController.Instance.SetOverLinePoint(1, _selectedGrids[^1].GetPosition());
                }
                else
                {
                    LineController.Instance.SetOverLinePoint(2, overLineLastPos);
                }
            }

            if (_selectedGrids.Count >= 2)
            {
                StopOrResumeTipsTimer(true);
            }

            var currentTouchGrid = GetHitGridItem(screenPos);
            if (currentTouchGrid == null)
                return;

            if (_selectedGrids.Count >= 3)
            {
                if (_rectangleFlag)
                {
                    //校验是否回到了最后一个格子，取消方形标签
                    GridItem last = _selectedGrids[^1];
                    if (last.Data.UId == currentTouchGrid.Data.UId)
                    {
                        _rectangleFlag = false;
                        LineController.Instance.RemoveUnderPoint();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                   //校验闭环 使用for循环是因为现在可以在任意位置回连，也可以当做闭环
                    for (int i = 0; i < _selectedGrids.Count - 2; i++)
                    {
                        GridItem gridItem = _selectedGrids[i];
                        bool isLineTo = gridItem.Data.UId == currentTouchGrid.Data.UId;
                        if (isLineTo)
                        {
                            var itemData = gridItem.GetCanLineElementData();
                            if (itemData != null &&
                                !ElementSystem.Instance.IsSpecialElement(itemData.ElementType) &&
                                gridItem.IsNeighbor(_selectedGrids[^1].Data.Coord))
                            {
                                var selectEle = _currentSelectedGrid.GetCanLineElementData();
                                if (selectEle != null)
                                {
                                    int sameId = selectEle.ConfigId;
                                    var allElements = ElementSystem.Instance.GridElements;
                                    foreach (var coord in allElements.Keys)
                                    {
                                        var elements = ElementSystem.Instance.GetGridElements(coord, false);
                                        bool bResult =
                                            ElementSystem.Instance.TryGetBaseElement(elements, out int index);
                                        if (bResult && index >= 0 && elements[index].Data.ConfigId == sameId)
                                        {
                                            if (elements[index] is BaseElementItem baseElementItem)
                                                baseElementItem.PopRectangleEffect();
                                        }
                                    }

                                    if (!_rectangleFlag)
                                    {
                                        CommonUtil.DeviceVibration(_vibrationForce + 1, 0.1f); //形成方格，震动强度提高一档
                                        PlaySquareAudio();
                                    }

                                    _rectangleFlag = true;
                                    LineController.Instance.ClearOverLine();
                                    LineController.Instance.AddUnderPoint(gridItem.GetPosition());
                                    return;
                                }
                            }
                        }
                    }

                }
            }
            
            foreach (var gridItem in _gridItems)
            {
                gridItem.Value.DoDeRectangleEffect();
            }

            bool isLinkAllSpecial = true;
            if (_selectedGrids.Count <= 1)
            {
                isLinkAllSpecial = false;
            }
            else
            {
                for (int i = 0; i < _selectedGrids.Count; i++)
                {
                    var itemData = _selectedGrids[i].GetCanLineElementData();
                    if (itemData != null)
                    {
                        if (!ElementSystem.Instance.IsSpecialElement(itemData.ElementType))
                        {
                            isLinkAllSpecial = false;
                            break;
                        }
                    }
                }
            }

            var currentTouchElement = currentTouchGrid.GetCanLineElementData();
            if (currentTouchElement == null)
                return;
            if (currentTouchElement.ElementType == ElementType.Normal && isLinkAllSpecial) //防止功能棋子连了彩色球，彩色球又去连普通棋子
                return;
            if (_selectedGrids.Contains(currentTouchGrid))
            {
                if (_selectedGrids.Count >= 2)
                {
                    if (_selectedGrids[^2].Data.UId == currentTouchGrid.Data.UId)
                    {
                        _currentSelectedGrid.DoDeselect();
                        LineController.Instance.RemoveUnderPoint();

                        _selectedGrids.Remove(_currentSelectedGrid);
                        PlayLinkAudio();
                        LineController.Instance.SetOverLinePoint(1, _selectedGrids[^1].GetPosition());
                        _currentSelectedGrid = currentTouchGrid;
                    }
                }
            }
            else
            {
                RefDragHitGridItems(_currentSelectedGrid.GetPosition(), overLineLastPos, ref _dragHitGrids);
                for (int i = 0; i < _dragHitGrids.Count; i++)
                {
                    if (_dragHitGrids[i].GetCanLineElementData() == null)
                        continue;
                    if (_currentSelectedGrid.IsNeighbor(_dragHitGrids[i].Data.Coord) &&
                        _currentSelectedGrid.CanMatchTo(_dragHitGrids[i]) &&
                        CheckGuideCanLine(_dragHitGrids[i].Data.Coord))
                    {
                        AddSelectGrids(_dragHitGrids[i]);
                        CommonUtil.DeviceVibration(_vibrationForce, 0.1f);
                        LineController.Instance.AddUnderPoint(_selectedGrids[^1].GetPosition());
                        LineController.Instance.SetOverLinePoint(1, _selectedGrids[^1].GetPosition());

                        _dragHitGrids[i].DoSelect();
                        _dragHitGrids[i].BindLinkGrid(_currentSelectedGrid);

                        _currentSelectedGrid = _dragHitGrids[i];
                    }
                }
            }
        }

        private bool CheckGuideCanLine(Vector2Int coord)
        {
            if (!_isGuiding)
                return true;
            if (_isExecuteGuideLevel)
                return true;
            if (_guidePathCoords == null || _guidePathCoords.Count <= 0)
                return true;
            return _guidePathCoords.Contains(coord);
        }

        private void AddSelectGrids(GridItem gridItem)
        {
            _selectedGrids.Add(gridItem);
            PlayLinkAudio();
        }

        private void PlayLinkAudio() {
            AudioUtil.PlayMatchLink(_selectedGrids.Count - 1);
            // ElementAudioManager.Instance.PlayMatchLink(_selectedGrids.Count - 1);
        }

        private void PlaySquareAudio() {
            // ElementAudioManager.Instance.PlayMatchLink(_selectedGrids.Count);
            AudioUtil.PlayMatchLink(_selectedGrids.Count);
        }

        private void OnTouchEnd()
        {
            StopOrResumeTipsTimer(false);
            _currentSelectedGrid = null;
            if (_selectedGrids == null) return;
            if (_selectedGrids.Count <= 0) return;
            if (_isMatchDone)
            {
                if (_selectedGrids != null)
                {
                    for (int i = 0; i < _selectedGrids.Count; i++)
                    {
                        _selectedGrids[i].DoDeselect();
                    }
                }

                LineController.Instance.ClearAllLines();
                _selectedGrids.Clear();
                return;
            }

            if (_isGuiding)
            {
                if (_guidePathCoords.Count - 1 == 2)
                {
                    var firstGrid = GetGridByCoord(_guidePathCoords[0]);
                    var secondGrid = GetGridByCoord(_guidePathCoords[1]);

                    var firstEle = firstGrid.GetSelectElement(true);
                    var secondEle = secondGrid.GetSelectElement(true);
                    if (firstEle != null && secondEle != null)
                    {
                        if (ElementSystem.Instance.IsSpecialElement(firstEle.Data.ElementType) &&
                            ElementSystem.Instance.IsSpecialElement(secondEle.Data.ElementType))
                        {
                            if (_selectedGrids.Count != 2)
                            {
                                if (_selectedGrids != null)
                                {
                                    for (int i = 0; i < _selectedGrids.Count; i++)
                                    {
                                        _selectedGrids[i].DoDeselect();
                                    }
                                }

                                LineController.Instance.ClearAllLines();
                                _selectedGrids.Clear();
                                return;
                            }
                        }
                    }
                }
            }

            _isLineSquare = _rectangleFlag;
            ValidateManager.Instance.Judge(in _selectedGrids, in _rectangleFlag, b =>
            {
                if (_isGuiding)
                {
                    GuideManager.Instance.ActiveMaskRaycast();
                }

                LineController.Instance.ClearAllLines();

                if (!b)
                {
                    if (_isGuiding)
                    {
                        SetPauseOrRestartGuide(false);
                    }

                    for (int i = 0; i < _selectedGrids.Count; i++)
                    {
                        _selectedGrids[i].DoDeselect();
                    }
                }
                else
                {
                    if (_isGuiding)
                    {
                        GuideManager.Instance.HideTouchGuide();
                        GuideManager.Instance.FinishCurrentGuide();
                        _guideFingerTween?.Kill();
                        _guideFingerTween = null;
                        SetFingerVisible(false);
                    }

                    if (_isExecuteGuideLevel && _isLineSquare && _isGuideForceSquare)
                    {
                        _guideForceSquareCount++;
                    }
                }

                _selectedGrids.Clear();
            });
            _rectangleFlag = false;
        }

        private bool IsPointerOverUI(Vector2 screenPos)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = screenPos;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            // 过滤完全透明的UI
            return results.Any(r =>
                r.gameObject.GetComponent<Graphic>()?.color.a > 0.1f);
        }

        private GridItem GetHitGridItem(Vector2 screenPos)
        {
            Vector3 worldPos = MainCamera.ScreenToWorldPoint(screenPos);
            Collider2D hit = Physics2D.OverlapPoint(worldPos, gridItemMask);
            if (hit != null && hit.TryGetComponent<MatchTouchItem>(out var piece))
            {
                if (_gridItems.TryGetValue(piece.gameObject.GetInstanceID(), out var gridItem))
                {
                    return gridItem;
                }
            }

            return null;
        }

        private void RefDragHitGridItems(Vector2 start, Vector2 end, ref List<GridItem> hitGridItems)
        {
            hitGridItems.Clear();

            int hitCount = Physics2D.LinecastNonAlloc(start, end, _linecastResults, gridItemMask.value);
            for (int i = 0; i < hitCount; i++)
            {
                if (_linecastResults[i].collider != null &&
                    _linecastResults[i].collider.TryGetComponent<MatchTouchItem>(out var piece))
                {
                    if (_gridItems.TryGetValue(piece.gameObject.GetInstanceID(), out var gridItem) &&
                        !hitGridItems.Contains(gridItem))
                    {
                        if (_selectedGrids != null && !_selectedGrids.Contains(gridItem))
                        {
                            hitGridItems.Add(gridItem);
                        }
                    }
                }
            }
        }
    }
}