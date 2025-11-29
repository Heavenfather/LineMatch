using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using GameCore.Localization;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Logic.Match;
using Hotfix.Utils;
using HotfixCore.Extensions;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;
using Random = UnityEngine.Random;

namespace HotfixLogic.Match
{
    public partial class GridSystem
    {
        private HashSet<Vector2Int> _destroyGrids = new HashSet<Vector2Int>(100); //存储已销毁的格子
        private Dictionary<int, int> _xEmptyCoord = new Dictionary<int, int>(15); //在x方向上销毁的数量
        private List<ElementItemData> _genElements = new List<ElementItemData>(100);
        private Dictionary<int, List<ElementBase>> _needMoveElementsMap = new Dictionary<int, List<ElementBase>>(15);

        private void OnDestroyTargetListElement(EventOneParam<ElementDestroyContext> arg)
        {
            var context = arg.Arg;
            OnDestroyTargetListElement(context).Forget();
        }

        private async UniTask OnDestroyTargetListElement(ElementDestroyContext context)
        {
            ClearTipsTimer();

            MatchTweenUtil.PlayCreateSpecialElementAnim(context);

            // 彩球连特殊棋子 飞行动画
            G.UIModule.ScreenLock(MatchConst.MatchLockReason, true, 15);
            if (context.BallLineSpecialItems is { Count: > 0 })
            {
                await DoLineBallAndSpecialItem(context);
                context.BallLineSpecialItems.Clear();
            }

            await DoDelElement(context);
            G.UIModule.ScreenLock(MatchConst.MatchLockReason, false);
            JumpCollectElementItemUtil.CheckJump(_destroyGrids, this);
            SpreadGrassUtil.UpdateGrassBorder();

            if (context.NewWaterCoords is { Count: > 0 })
            {
                await DrawSpreadWaterUtil.DoFlowWater(context);
            }

            if (context.UsingItemId < 0 && !_isExecuteGuideLevel)
                G.EventModule.DispatchEvent(GameEventDefine.OnReduceMoveStep);

            ProcessInfectFireElement(context);

            await UniTask.Yield();
            StartDropElement(context).Forget();
        }

        private async UniTask DoDelElement(ElementDestroyContext context)
        {
            List<DeleteGridInfo> delInfos = new List<DeleteGridInfo>(context.WillDelCoords);
            List<DeleteGridInfo> holdCheckInfos = new List<DeleteGridInfo>(context.WillDelCoords);

            if (context.DoubleColorBallCoords is { Count: > 0 })
            {
                // 双彩球
                await DoDoubleBallDelElement(context, delInfos);
                context.DoubleColorBallCoords.Clear();
            }
            else if (context.DoubleBombCoords is { Count: > 0 })
            {
                // 双炸弹
                await DoDoubleBombDelElement(context, delInfos);
                context.DoubleBombCoords.Clear();
            }
            else if (context.IsColorBallLineBomb)
            {
                // 炸弹连彩球  爆炸动画
                await DoBallLineBombDelElement(context, delInfos);
                context.IsColorBallLineBomb = false;
            }
            else if (context.IsColorBallLineRocket)
            {
                // 火箭连彩球 爆炸动画
                await DoRocketColorDelElement(context, delInfos);
                context.IsColorBallLineRocket = false;
            }
            else if (context.IsRocketAndRocket)
            {
                // 火箭连火箭
                await DoRocketAndRocketDelElement(context, delInfos);
                context.IsRocketAndRocket = false;
            }
            else if (context.IsRocketAndBomb)
            {
                // 火箭连炸弹
                await DoMultiRocketDelElement(context, delInfos);
                context.IsRocketAndBomb = false;
            }
            else if (context.IsColorBallLineNormal)
            {
                //彩球连普通元素
                await DoDelBallElement(context, context.LinkSpecialBallCoord, delInfos);
                context.IsColorBallLineNormal = false;
            }
            else
            {
                bool hasSpecialBomb = false;
                Vector2Int startBombCoord = default;
                foreach (var info in holdCheckInfos)
                {
                    int configId = info.ElementConfigIds.First();
                    if (ElementSystem.Instance.IsBombElement(configId))
                    {
                        hasSpecialBomb = true;
                        startBombCoord = info.Coord;
                        break;
                    }
                }

                if (hasSpecialBomb)
                {
                    await DoElement(startBombCoord, context, delInfos, false);
                }
                else
                {
                    foreach (var info in holdCheckInfos)
                    {
                        DestroyElementByCoord(context, info.Coord);
                        delInfos.Remove(info);
                    }
                }
            }

            if (context.WaitElementDestroyTasks is { Count: > 0 })
            {
                await UniTask.WhenAll(context.WaitElementDestroyTasks);
            }

            if (delInfos.Count > 0)
            {
                if (context.WaitElementDestroyTasks is { Count: > 0 })
                {
                    await UniTask.WhenAll(context.WaitElementDestroyTasks);
                }

                for (int i = 0; i < delInfos.Count; i++)
                {
                    DestroyElementByCoord(context, delInfos[i].Coord);
                }

                delInfos.Clear();
            }

            if (context.WaitElementDestroyTasks is { Count: > 0 })
            {
                await UniTask.WhenAll(context.WaitElementDestroyTasks);
                context.WaitElementDestroyTasks.Clear();
            }
        }

        public UniTask DoElement(Vector2Int coord, ElementDestroyContext context, List<DeleteGridInfo> delInfos,
            bool delNormal)
        {
            int index = delInfos.FindIndex(x => x.Coord == coord);
            if (index < 0)
                return UniTask.CompletedTask;
            var elements = ElementSystem.Instance.GetGridElements(coord, true);
            if (elements is { Count: > 0 })
            {
                if (!delInfos[index].DelStyle.Contains(elements[^1].Data.EliminateStyle))
                    return UniTask.CompletedTask;
                if (elements[^1].Data.ElementType == ElementType.Bomb)
                {
                    PlayResultCollectCoin(coord, context);
                    return DoBombDelElement(context, coord, delInfos);
                }

                if (elements[^1].Data.ElementType == ElementType.Rocket ||
                    elements[^1].Data.ElementType == ElementType.RocketHorizontal)
                {
                    PlayResultCollectCoin(coord, context);
                    return DoRocketDelElement(context, coord, elements[^1].Data.Direction, delInfos);
                }

                if (elements[^1].Data.ElementType == ElementType.ColorBall)
                {
                    PlayResultCollectCoin(coord, context);
                    return DoDelBallElement(context, coord, delInfos);
                }

                if (elements[^1].Data.ElementType == ElementType.BombBlock)
                {
                    return DoBombBlockDelElement(context, coord, delInfos);
                }

                if (delNormal)
                {
                    DestroyElementByCoord(context, coord);
                    delInfos.RemoveAt(index);
                }
            }

            return UniTask.CompletedTask;
        }

        private UniTask DoBombDelElement(ElementDestroyContext context, Vector2Int coord,
            List<DeleteGridInfo> delGridInfos)
        {
            var neighborPos = ValidateManager.Instance.GetBombCoords(coord);
            List<Vector2Int> neighborElementPos = new List<Vector2Int>(neighborPos.Count);
            for (int i = 0; i < neighborPos.Count; i++)
            {
                if (!IsValidPosition(neighborPos[i].x, neighborPos[i].y))
                    continue;
                if (context.SpecialElementDelFilterCoords != null &&
                    context.SpecialElementDelFilterCoords.Contains(neighborPos[i]))
                    continue;
                neighborElementPos.Add(neighborPos[i]);
                context.AddSpecialElementDelFilterCoord(neighborPos[i]);
            }

            if (neighborElementPos.Count <= 0)
            {
                int index = delGridInfos.FindIndex(x => x.Coord == coord);
                if (index >= 0)
                    delGridInfos.RemoveAt(index);
                DestroyElementByCoord(context, coord);
                return UniTask.CompletedTask;
            }

            UniTaskCompletionSource cts = new UniTaskCompletionSource();
            var seq = DOTween.Sequence();
            seq.AppendCallback(() =>
            {
                int index = delGridInfos.FindIndex(x => x.Coord == coord);
                if (index >= 0)
                    delGridInfos.RemoveAt(index);
                DestroyElementByCoord(context, coord);

                foreach (var pos in neighborElementPos)
                {
                    context.AddWaitElementDestroyTask(DoElement(pos, context, delGridInfos, true));
                }
            });
            seq.AppendInterval(MatchConst.BombElementEffDur);
            seq.SetAutoKill().OnComplete(() =>
            {
                cts.TrySetResult();
            });
            return cts.Task;
        }

        private UniTask DoBombBlockDelElement(ElementDestroyContext context, Vector2Int coord,
            List<DeleteGridInfo> delGridInfos)
        {
            var neighborPos = MatchTweenUtil.GetEightNeighborPos(coord);
            List<Vector2Int> neighborElementPos = new List<Vector2Int>(neighborPos.Count);
            for (int i = 0; i < neighborPos.Count; i++)
            {
                if (!IsValidPosition(neighborPos[i].x, neighborPos[i].y))
                    continue;
                if (context.SpecialElementDelFilterCoords != null &&
                    context.SpecialElementDelFilterCoords.Contains(neighborPos[i]))
                    continue;
                neighborElementPos.Add(neighborPos[i]);
                context.AddSpecialElementDelFilterCoord(neighborPos[i]);
                ValidateManager.Instance.AddDelGridElement(neighborPos[i], context, coord);
            }

            if (neighborElementPos.Count <= 0)
            {
                int index = delGridInfos.FindIndex(x => x.Coord == coord);
                if (index >= 0)
                    delGridInfos.RemoveAt(index);
                DestroyElementByCoord(context, coord);
                return UniTask.CompletedTask;
            }

            UniTaskCompletionSource cts = new UniTaskCompletionSource();
            var seq = DOTween.Sequence();
            seq.AppendCallback(() =>
            {
                int index = delGridInfos.FindIndex(x => x.Coord == coord);
                if (index >= 0)
                    delGridInfos.RemoveAt(index);
                DestroyElementByCoord(context, coord);

                List<DeleteGridInfo> currentInfos = new List<DeleteGridInfo>(context.WillDelCoords);
                foreach (var pos in neighborElementPos)
                {
                    context.AddWaitElementDestroyTask(DoElement(pos, context, currentInfos, true));
                }
            });
            seq.AppendInterval(MatchConst.BombElementEffDur);
            seq.SetAutoKill(true).OnComplete(() => { cts.TrySetResult(); });
            return cts.Task;
        }

        private async UniTask DoMultiRocketDelElement(ElementDestroyContext context, List<DeleteGridInfo> delGridInfos)
        {
            // 第一个找到的火箭就是  组合火箭
            RockElementItem firstRocket = null;
            BombElementItem firstBomb = null;
            for (int i = 0; i < delGridInfos.Count; i++)
            {
                var gridInfo = delGridInfos[i];
                var elements = ElementSystem.Instance.GetGridElements(gridInfo.Coord, true);
                bool result = ElementSystem.Instance.TryGetBaseElement(elements, out int index, true);
                if (result)
                {
                    if (firstRocket == null && elements[index] is RockElementItem rockItem)
                        firstRocket = rockItem;
                    if (firstBomb == null && elements[index] is BombElementItem bombItem)
                        firstBomb = bombItem;
                }

                if (firstRocket != null && firstBomb != null)
                    break;
            }

            if (firstRocket == null)
                return;
            if (firstBomb == null)
                return;

            UniTaskCompletionSource cts = new UniTaskCompletionSource();
            MatchTweenUtil.DoBombAndRocket(firstRocket, firstBomb, async () =>
            {
                var rocketPos = firstRocket.Data.GridPos;
                //三横三竖的火箭
                var rocketPosList = new Vector2Int[]
                {
                    new Vector2Int(rocketPos.x, rocketPos.y - 1),
                    new Vector2Int(rocketPos.x, rocketPos.y),
                    new Vector2Int(rocketPos.x, rocketPos.y + 1),
                    new Vector2Int(rocketPos.x - 1, rocketPos.y),
                    new Vector2Int(rocketPos.x, rocketPos.y),
                    new Vector2Int(rocketPos.x + 1, rocketPos.y),
                };

                for (int i = 0; i < rocketPosList.Length; i++)
                {
                    var delPos = rocketPosList[i];
                    var direction = i < 3 ? ElementDirection.Right : ElementDirection.Up;
                    if (!(direction == firstRocket.Data.Direction && delPos == firstRocket.Data.GridPos))
                    {
                        int rocketId = direction == ElementDirection.Up
                            ? (int)ElementIdConst.Rocket
                            : (int)ElementIdConst.RocketHorizontal;
                        GameObject moveTarget = ElementObjectPool.Instance.Spawn($"Element-{rocketId}");
                        moveTarget.transform.position =
                            direction == ElementDirection.Up
                                ? GetGridPositionByCoord(delPos.x, 12)
                                : GetGridPositionByCoord(-2, delPos.y);
                        MatchTweenUtil.DoRocketTargetMove(moveTarget, delPos, direction,
                            () => { ElementObjectPool.Instance.Recycle(moveTarget); });
                    }

                    DoRocketDelElement(context, delPos, direction, delGridInfos).Forget();
                }

                await UniTask.Delay(TimeSpan.FromSeconds(MatchConst.RocketMoveDuration));
                int bombIndex = delGridInfos.FindIndex(x => x.Coord == firstBomb.Data.GridPos);
                if (bombIndex >= 0)
                    delGridInfos.RemoveAt(bombIndex);
                DestroyElementByCoord(context, firstBomb.Data.GridPos);

                cts.TrySetResult();
            });

            await cts.Task;
        }

        private UniTask DoRocketDelElement(ElementDestroyContext context, Vector2Int rocketCoord,
            ElementDirection direction, List<DeleteGridInfo> delGridInfos, Vector2Int? linkRockCoord = null)
        {
            if (direction == ElementDirection.None || direction == ElementDirection.Left ||
                direction == ElementDirection.Down)
                return UniTask.CompletedTask;
            int index = delGridInfos.FindIndex(x => x.Coord == rocketCoord);
            if (index >= 0)
                delGridInfos.RemoveAt(index);

            UniTaskCompletionSource cts = new UniTaskCompletionSource();
            MatchTweenUtil.DoRocketTweenMove(rocketCoord, null, linkRockCoord);
            DestroyElementByCoord(context, rocketCoord);

            Vector2Int doRocketCoord = rocketCoord;
            if (linkRockCoord.HasValue)
                doRocketCoord = linkRockCoord.Value;
            var delPosList =
                MatchTweenUtil.GetRocketDelPos(doRocketCoord, direction, context.SpecialElementDelFilterCoords);
            List<Vector2Int> realDelPos = new List<Vector2Int>(delPosList.Count);
            for (int i = 0; i < delPosList.Count; i++)
            {
                var elements = ElementSystem.Instance.GetGridElements(delPosList[i], false);
                if (elements != null && elements.Count > 0 && !realDelPos.Contains(elements[^1].Data.GridPos))
                {
                    realDelPos.Add(elements[^1].Data.GridPos);
                    context.AddSpecialElementDelFilterCoord(elements[^1].Data.GridPos);
                }
            }

            if (realDelPos.Count > 0)
            {
                float startDelay = 0.0f;
                var seq = DOTween.Sequence();
                float interval = MatchConst.RocketMoveDuration / ((realDelPos.Count + 3) * 1.0f);
                for (int i = 0; i < realDelPos.Count; i++)
                {
                    var idx = i;
                    float delayTime = startDelay + (i * interval);
                    seq.InsertCallback(delayTime, () =>
                    {
                        var pos = realDelPos[idx];
                        context.AddWaitElementDestroyTask(DoElement(pos, context, delGridInfos, true));
                    });
                }

                seq.OnComplete(() => { cts.TrySetResult(); });
            }
            else
            {
                cts.TrySetResult();
            }

            return cts.Task;
        }

        public void DestroyElementByCoord(ElementDestroyContext context, Vector2Int coord)
        {
            var grid = GetGridByCoord(coord);
            if (grid != null && grid.Data.Elements is { Count: > 0 })
            {
                PlayCollectBlockTarget(grid);
                if (grid.DestroyElement(context))
                {
                    _destroyGrids.Add(grid.Data.Coord);
                }
                else
                {
                    if (context.ExtraDelGridCoords is { Count: > 0 })
                    {
                        foreach (var extraCoord in context.ExtraDelGridCoords)
                        {
                            _destroyGrids.Add(extraCoord);
                        }

                        context.ExtraDelGridCoords.Clear();
                    }
                }

                var topElement = grid.Data.GetTopElement();
                if (topElement != null && topElement.EliminateCount == 0 &&
                    topElement.ElementType != ElementType.ColorBlockPlus)
                {
                    if (grid.DestroyElement(context))
                    {
                        _destroyGrids.Add(grid.Data.Coord);
                    }
                }
            }
        }

        private UniTask DoDelBallElement(ElementDestroyContext context, Vector2Int ballCoord,
            List<DeleteGridInfo> delGridInfos)
        {
            var elements = ElementSystem.Instance.GetGridElements(ballCoord, true);
            bool result = ElementSystem.Instance.TryGetBaseElement(elements, out int ballIndex, true);
            if (!result)
                return UniTask.CompletedTask;
            ColorBallElementItem colorBallElement = elements[ballIndex] as ColorBallElementItem;
            if (colorBallElement == null)
                return UniTask.CompletedTask;

            // int index = delGridInfos.FindIndex(x => x.Coord == colorBallElement.Data.GridPos);
            // if (index >= 0)
            //     delGridInfos.RemoveAt(index);
            if (context.UsingItemId > 0)
            {
                DestroyElementByCoord(context, colorBallElement.Data.GridPos);
                return UniTask.CompletedTask;
            }

            int curMaxId = -1;
            if (colorBallElement.Data.ColorBallDestroyId > 0)
            {
                curMaxId = colorBallElement.Data.ColorBallDestroyId;
            }
            else
            {
                curMaxId = ElementSystem.Instance.RandomPickBoardBaseElementId(context.ColorBallFilterIds);
                context.AddColorBallFilterId(curMaxId);
                ValidateManager.Instance.AttachColorBall(context, curMaxId, colorBallElement.Data.GridPos,
                    colorBallElement.Data.UId);
            }

            if (curMaxId <= 0)
            {
                DestroyElementByCoord(context, colorBallElement.Data.GridPos);
                return UniTask.CompletedTask;
            }

            List<ElementBase> delElements =
                ElementSystem.Instance.GetAllElementsById(curMaxId, context.SpecialElementDelFilterCoords);
            if (delElements.Count <= 0)
            {
                DestroyElementByCoord(context, colorBallElement.Data.GridPos);
                return UniTask.CompletedTask;
            }

            colorBallElement.PlayBallAnimation("Idle02");
            var ballPos = colorBallElement.GameObject.transform.position;

            List<Vector3> endPositions = new List<Vector3>(delElements.Count);
            for (int i = 0; i < delElements.Count; i++)
            {
                endPositions.Add(delElements[i].GameObject.transform.position);
                context.AddSpecialElementDelFilterCoord(delElements[i].Data.GridPos);
            }

            UniTaskCompletionSource cts = new UniTaskCompletionSource();
            _trailEmitter.Emitter(ballPos, endPositions, (i) =>
            {
                AudioUtil.PlayMatchEliminate();
                delElements[i].PlayShock();
            }, () =>
            {
                DestroyElementByCoord(context, ballCoord);
                foreach (var element in delElements)
                {
                    DestroyElementByCoord(context, element.Data.GridPos);
                    int index = delGridInfos.FindIndex(x => x.Coord == element.Data.GridPos);
                    if (index >= 0)
                        delGridInfos.RemoveAt(index);
                }
                delElements.Clear();
                // delGridInfos.Clear();

                cts.TrySetResult();
            });

            return cts.Task;
        }

        private async UniTask DoRocketAndRocketDelElement(ElementDestroyContext context,
            List<DeleteGridInfo> delGridInfos)
        {
            List<ElementItemData> rocketCoords = context.RocketAndRocket;
            if (rocketCoords == null || rocketCoords.Count <= 0)
                return;
            DoRocketDelElement(context, rocketCoords[0].GridPos, rocketCoords[0].Direction, delGridInfos).Forget();
            DoRocketDelElement(context, rocketCoords[1].GridPos, rocketCoords[1].Direction, delGridInfos,
                linkRockCoord: rocketCoords[0].GridPos).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(MatchConst.RocketMoveDuration));
        }

        private async UniTask DoRocketColorDelElement(ElementDestroyContext context, List<DeleteGridInfo> delGridInfos)
        {
            var rocketList = new List<RockElementItem>();
            // 优先创建火箭字典
            foreach (var gridInfo in delGridInfos)
            {
                var rocketCoord = gridInfo.Coord;
                var elements = ElementSystem.Instance.GetGridElements(rocketCoord, true);
                RockElementItem rocketItem = null;
                if (elements != null && elements.Count > 0)
                {
                    foreach (var element in elements)
                    {
                        if (element is RockElementItem rocket)
                        {
                            rocketItem = rocket;
                            break;
                        }
                    }
                }

                if (rocketItem != null)
                {
                    rocketList.Add(rocketItem);
                }
            }

            rocketList.Shuffle();
            const float perDelayTime = 0.03f;
            List<UniTask> uniTasks = new List<UniTask>();
            foreach (var rocketItem in rocketList)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(perDelayTime));
                uniTasks.Add(DoRocketDelElement(context, rocketItem.Data.GridPos, rocketItem.Data.Direction,
                    delGridInfos));
            }

            await UniTask.WhenAll(uniTasks);
            delGridInfos.Clear();
        }

        private void ProcessInfectFireElement(ElementDestroyContext context)
        {
            //当前消除的元素有消除过火元素，就不能再扩散
            if (context.HasDestroyFireElement || !ElementSystem.Instance.HasFireElement(out var fireElements))
                return;
            List<ElementBase> validFireElements = new List<ElementBase>(fireElements.Count);
            foreach (var element in fireElements)
            {
                foreach (var dir in ValidateManager.Instance.NeighborDirs)
                {
                    Vector2Int checkCoord = element.Data.GridPos + dir;
                    if (ElementSystem.Instance.HasBaseElement(checkCoord))
                    {
                        validFireElements.Add(element);
                        break;
                    }
                }
            }

            if (validFireElements.Count == 0)
                return;

            int randomIndex = Random.Range(0, validFireElements.Count);
            var selectFire = validFireElements[randomIndex];
            List<Vector2Int> validTargets = new List<Vector2Int>(8);
            foreach (var dir in ValidateManager.Instance.NeighborDirs)
            {
                Vector2Int checkCoord = selectFire.Data.GridPos + dir;
                if (ElementSystem.Instance.HasBaseElement(checkCoord))
                    validTargets.Add(checkCoord);
            }

            Vector2Int infectTarget = validTargets[Random.Range(0, validTargets.Count)];
            var gridItem = GetGridByCoord(infectTarget);
            gridItem.RemoveBaseElement(true);
            var data = ElementSystem.Instance.GenElementItemData(selectFire.Data.ConfigId, infectTarget.x,
                infectTarget.y);
            var genElement = ElementSystem.Instance.GenElement(data, gridItem.GameObject.transform);
            genElement.GameObject.transform.localPosition = Vector3.zero;
            gridItem.PushElement(genElement, false);
        }

        private async UniTask StartDropElement(ElementDestroyContext context, bool updateScore = true)
        {
            ClearContainer();

            bool isGuideDrop = false;
            if (_isExecuteGuideLevel)
            {
                G.UIModule.ScreenLock(MatchConst.MatchLockReason, false);

                G.EventModule.DispatchEvent(GameEventDefine.OnMatchGuideLevelAttemptStep,EventTwoParam<int,int>.Create(_levelData.id,_guideLevelStep));
                if (_levelData.id == 4)
                {
                    _destroyGrids.Clear();

                    if (_guideLevelStep < 2)
                    {
                        isGuideDrop = true;
                        _xEmptyCoord.Add(1, 1);
                        _guideLevelStep++;

                        string[] coords = new[] { "0,1", "1,1" };;
                        DoFingerTweenPlay(coords, false, false);
                    }
                    else
                    {

                        G.EventModule.DispatchEvent(GameEventDefine.OnMatchGuideLevelStepFinish,
                            EventOneParam<int>.Create(4));
                        _guideLevelItemBg.SetVisible(false);
                        UniTask.Create(async () =>
                        {
                            context.CalAddedDelTargets?.Clear();
                            await Clear();
                            await UniTask.Delay(TimeSpan.FromSeconds(2f));
                            OnMatchGuideLevelStepComplete();
                        }).Forget();
                        return;
                    }
                }
                else if (_levelData.id == 5)
                {
                    RefreshMatchTips(true);

                    var dropDict = new Dictionary<Vector2Int,int>();
                    Vector2Int[] coords = null;
                    isGuideDrop = true;

                    if (!_isLineSquare)
                    {
                        
                        var newElements = await GenGuideLevelDropElement(context.DestroyedElements);
                        ProcessNewElementDrop(newElements);
                        _destroyGrids.Clear();
                    }
                    else
                    {
                        _guideLevelStep++;
                        G.EventModule.DispatchEvent(GameEventDefine.OnMatchSquareLineStep,EventOneParam<int>.Create(_guideForceSquareCount));
                        if (_guideForceSquareCount == 1 || _guideForceSquareCount == 2) {

                            foreach (var delCoord in _destroyGrids)
                            {
                                _xEmptyCoord.TryAdd(delCoord.x, 0);
                                _xEmptyCoord[delCoord.x]++;
                            }

                            if (_guideForceSquareCount == 1) {
                                dropDict.Add(new Vector2Int(0, 0), 2);
                                dropDict.Add(new Vector2Int(2, 0), 2);
                                dropDict.Add(new Vector2Int(3, 0), 3);
                                dropDict.Add(new Vector2Int(5, 0), 3);
                                dropDict.Add(new Vector2Int(2, 1), 3);
                                dropDict.Add(new Vector2Int(3, 1), 3);
                            } else if (_guideForceSquareCount == 2) {
                                dropDict.Add(new Vector2Int(1, 3), 2);
                                dropDict.Add(new Vector2Int(5, 2), 2);
                                dropDict.Add(new Vector2Int(1, 2), 4);
                                dropDict.Add(new Vector2Int(0, 2), 2);
                                dropDict.Add(new Vector2Int(3, 1), 4);
                                dropDict.Add(new Vector2Int(0, 0), 2);
                                dropDict.Add(new Vector2Int(2, 0), 4);
                                dropDict.Add(new Vector2Int(3, 0), 2);
                                dropDict.Add(new Vector2Int(5, 0), 4);
                            }

                            var newElements = await GenGuideLevelDropElement(dropDict);
                            ProcessBoardDrop(context);
                            ProcessNewElementDrop(newElements);
                            _destroyGrids.Clear();
                        } else if (_guideForceSquareCount >= 3) {
                            G.EventModule.DispatchEvent(GameEventDefine.OnMatchGuideLevelStepFinish,EventOneParam<int>.Create(5));
                            _guideLevelItemBg.SetVisible(false);
                            UniTask.Create(async () =>
                            {
                                context.CalAddedDelTargets?.Clear();
                                await Clear();
                                await UniTask.Delay(TimeSpan.FromSeconds(2f));
                                OnMatchGuideLevelStepComplete();
                            }).Forget();
                            _destroyGrids.Clear();
                            return;
                        }
                    }

                    if (_guideForceSquareCount == 0) {
                        coords = new Vector2Int[] {
                            new Vector2Int(2, 0),
                            new Vector2Int(3, 0),
                            new Vector2Int(3, 1),
                            new Vector2Int(2, 1),
                            new Vector2Int(2, 0)
                        };
                    } else if (_guideForceSquareCount == 1) {
                        coords = new Vector2Int[] {
                            new Vector2Int(0, 2),
                            new Vector2Int(1, 2),
                            new Vector2Int(1, 3),
                            new Vector2Int(0, 3),
                            new Vector2Int(0, 2)
                        };
                    } else if (_guideForceSquareCount == 2) {
                        coords = new Vector2Int[] {
                            new Vector2Int(4, 2),
                            new Vector2Int(5, 2),
                            new Vector2Int(5, 3),
                            new Vector2Int(4, 3),
                            new Vector2Int(4, 2)
                        };
                    }
                    if (coords!= null) DoFingerTweenPlay(coords, false, showFinger:false);
                }
                else if (_levelData.id == 6)
                {
                    //是否还有障碍物
                    bool haveBlock = false;
                    foreach (var elements in ElementSystem.Instance.GridElements.Values)
                    {
                        for (int i = 0; i < elements.Count; i++)
                        {
                            if (elements[i].Data.ElementType == ElementType.Block)
                            {
                                haveBlock = true;
                                break;
                            }
                        }
                        if(haveBlock)
                            break;
                    }

                    if (!haveBlock)
                    {
                        isGuideDrop = true;
                        _destroyGrids.Clear();
                        _guideLevelItemBg.SetVisible(false);
                        Vector2Int blockKey = default;
                        foreach (var item in context.DestroyedElements)
                        {
                            if (_elementMap[item.Value].elementType != ElementType.Normal)
                            {
                                blockKey = item.Key;
                                break;
                            }
                        }

                        context.DestroyedElements[blockKey] = _levelData.initColor[0];
                        G.EventModule.DispatchEvent(GameEventDefine.OnMatchGuideLevelStepFinish,EventOneParam<int>.Create(6));
                        UniTask.Create(async () =>
                        {
                            context.CalAddedDelTargets?.Clear();
                            await UniTask.Delay(TimeSpan.FromSeconds(0.8f));
                            await Clear();
                            await UniTask.Delay(TimeSpan.FromSeconds(1.2f));
                            OnMatchGuideLevelStepComplete();
                        }).Forget();
                        // return;
                    }
                    else
                    {
                        G.EventModule.DispatchEvent(GameEventDefine.OnMatchGuideLevelStepFinish,EventOneParam<int>.Create(6));
                    }
                }
                else if (_levelData.id == 2)
                {
                    isGuideDrop = true;
                    int delCount = context.DestroyedElements.Count;
                    if (delCount != 7)
                    {
                        var newElements = await GenGuideLevelDropElement(context.DestroyedElements);
                        ProcessNewElementDrop(newElements);
                        _destroyGrids.Clear();

                        ShowLevelGuideFinger(_levelData);
                    }
                    else
                    {
                        context.CalAddedDelTargets?.Clear();
                        OnMatchGuideLevelStepComplete();
                        return;
                    }
                }
                else
                {
                    context.CalAddedDelTargets?.Clear();
                    OnMatchGuideLevelStepComplete();
                    return;
                }
            }
            
            ++_dropRoundCount;
            if (!isGuideDrop)
            {
                G.UIModule.ScreenLock(MatchConst.MatchLockByGenNew, true);
                //设置动态难度调整数值
                var difficultyContext = DifficultyStrategyManager.Instance.CreateContext(in _levelData, _remainStep);
                DifficultyStrategyManager.Instance.CalculateDifficultyStrategies(difficultyContext);
                
                var newElements = await BuildDropElement(context);
                ProcessBoardDrop(context);
                ProcessNewElementDrop(newElements);
                G.UIModule.ScreenLock(MatchConst.MatchLockByGenNew, false);
            }
            else
            {
                if (_levelData.id == 4 || _levelData.id == 6)
                {
                    var newElements = await GenGuideLevelDropElement(context.DestroyedElements);
                    ProcessBoardDrop(context);
                    ProcessNewElementDrop(newElements);
                }
            }

            int index = 0;
            Dictionary<int, List<ElementBase>> currentMoveMap =
                new Dictionary<int, List<ElementBase>>(_needMoveElementsMap);
            foreach (var kvp in currentMoveMap)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(MatchConst.DropStepInterval * index));
                index++;
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    kvp.Value[i].DoMove();
                }
            }

            if (isGuideDrop)
                return;
            
            context.WillDelCoords.Clear();
            //掉落完成，检查是否有掉落物元素
            await UniTask.Delay(TimeSpan.FromSeconds(MatchConst.DropDuration));

            if (context.IsAutoReleaseBomb)
            {
                context.IsAutoReleaseBomb = false;
                await DoAutoReleaseBomb(context);
            }
            await CheckAfterDropElement(context, updateScore);

            RocketUtil.UpdateRocketIdleEffectVisible();
            MatchLineBlockElementItemUtil.UpdateIdleAnimation();
            //检测棋盘上是否还存在配对棋子
            bool hasPair = ShuffleElementManager.Instance.IsBoardHasPair();
            if (!hasPair)
            {
                var shuffleList = ShuffleElementManager.Instance.CollectShuffleElements();
                bool canShuffle = ShuffleElementManager.Instance.CheckIsCanShuffle(shuffleList);
                if (!canShuffle)
                {
                    Logger.Info("当前棋盘无任何有效配对，跳过洗牌，强制改变元素创建配对");
                    CommonUtil.ShowCommonTips(LocalizationPool.Get("Match/ForceChanged"));
                    ShuffleElementManager.Instance.ForceChangeElement(shuffleList);
                    return;
                }

                CommonUtil.ShowCommonTips(LocalizationPool.Get("Match/NotPair"));
                await UniTask.Delay(TimeSpan.FromSeconds(0.8f));
                ShuffleElementManager.Instance.ShuffleBoard(shuffleList);
            }

            _dropRoundCount = 0;
            ElementAudioManager.Instance.ClearPlaying();
            if (_isGuideFinish) {
                _isGuideFinish = false;
            }

            if (!_isGuideFinish) MatchManager.Instance.GameBeginUseElements();

            G.EventModule.DispatchEvent(GameEventDefine.OnMatchCheckLastStep);
        }

        private void ClearContainer()
        {
            _xEmptyCoord.Clear();
            _genElements.Clear();
            _needMoveElementsMap.Clear();
        }

        private void ProcessBoardDrop(ElementDestroyContext context)
        {
            bool IsFirstLockDel(Vector2Int coord)
            {
                if (context.DestroyedElements == null)
                    return false;
                if (context.DestroyedElements.TryGetValue(coord, out var elementId))
                {
                    ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
                    var config = db[elementId];
                    if (config.elementType == ElementType.Lock || config.elementType == ElementType.TargetBlock)
                        return true;
                }

                if (MatchManager.Instance.CurrentMatchLevelType == MatchLevelType.C ||
                    MatchManager.Instance.CurrentMatchGameType == MatchGameType.TowDots)
                    return false;
                if (context.GenSpecialInfos is { Count: > 0 })
                {
                    if (context.GenSpecialInfos.FindIndex(info => info.Coord == coord) >= 0)
                        return true;

                    GridItem gridItem = GetGridByCoord(coord);
                    var baseEle = gridItem.GetSelectElement(true);
                    if (ElementSystem.Instance.IsSpecialElement(baseEle.Data.ElementType))
                    {
                        for (int i = 0; i < context.GenSpecialInfos.Count; i++)
                        {
                            if (gridItem.IsNeighbor(context.GenSpecialInfos[i].Coord))
                                return true;
                        }
                    }
                }

                return false;
            }

            //先挪动旧棋子
            foreach (var x in _xEmptyCoord.Keys)
            {
                Queue<int> availablePositions = new Queue<int>();
                for (int y = _levelData.gridRow - 1; y >= 0; y--)
                {
                    var gridItem = GetGridByCoord(x, y);
                    if (gridItem == null)
                    {
                        continue;
                    }

                    if (gridItem.Data.IsGridLock())
                    {
                        availablePositions.Clear();
                        continue;
                    }

                    var baseEle = gridItem.GetSelectElement(true);
                    if (baseEle != null && !IsFirstLockDel(gridItem.Data.Coord) && baseEle.CanMove())
                    {
                        while (availablePositions.Count > 0 && availablePositions.Peek() <= y)
                        {
                            availablePositions.Dequeue();
                        }

                        if (availablePositions.Count > 0)
                        {
                            int targetY = availablePositions.Dequeue();
                            var targetGrid = GetGridByCoord(x, targetY);
                            targetGrid.PushElement(baseEle);
                            // _needMoveElements.Add(baseEle);
                            if (!_needMoveElementsMap.ContainsKey(baseEle.Data.GridPos.x))
                                _needMoveElementsMap.Add(baseEle.Data.GridPos.x, new List<ElementBase>() { baseEle });
                            else
                                _needMoveElementsMap[baseEle.Data.GridPos.x].Add(baseEle);
                            gridItem.RemoveBaseElement();
                        }
                    }

                    if (gridItem.Data.IsNeedFillBaseElement())
                        availablePositions.Enqueue(y);
                }
            }

            context.GenSpecialInfos?.Clear();
        }

        private void ProcessNewElementDrop(ElementBase[] newElements)
        {
            //再处理新棋子
            for (int i = 0; i < newElements.Length; i++)
            {
                if (newElements[i] == null)
                    continue;
                int x = newElements[i].Data.GridPos.x;
                for (int y = _levelData.gridRow - 1; y >= 0; y--)
                {
                    var gridItem = GetGridByCoord(x, y);
                    if (gridItem == null)
                        continue;
                    if (gridItem.Data.IsNeedFillBaseElement())
                    {
                        gridItem.PushElement(newElements[i]);
                        // _needMoveElements.Add(newElements[i]);
                        if (!_needMoveElementsMap.ContainsKey(newElements[i].Data.GridPos.x))
                            _needMoveElementsMap.Add(newElements[i].Data.GridPos.x,
                                new List<ElementBase>() { newElements[i] });
                        else
                            _needMoveElementsMap[newElements[i].Data.GridPos.x].Add(newElements[i]);
                        break;
                    }
                }
            }
        }

        private async UniTask<ElementBase[]> BuildDropElement(ElementDestroyContext context)
        {
            foreach (var delCoord in _destroyGrids)
            {
                _xEmptyCoord.TryAdd(delCoord.x, 0);
                _xEmptyCoord[delCoord.x]++;

                if (context.DestroyedElements.TryGetValue(delCoord, out int destroyedId))
                {
                    //占多格的障碍物，消除后会影响另外一个坐标的格子
                    if (ElementSystem.Instance.IsHoldMulGridElement(destroyedId, out var dir, out var holdCount,
                            delCoord))
                    {
                        if (dir == ElementDirection.Right)
                        {
                            for (int j = 1; j < holdCount; j++)
                            {
                                _xEmptyCoord.TryAdd(delCoord.x + j, 0);
                            }
                        }
                        else if (dir == ElementDirection.Left)
                        {
                            for (int j = 1; j < holdCount; j++)
                            {
                                _xEmptyCoord.TryAdd(delCoord.x - j, 0);
                            }
                        }
                    }
                }
            }

            //补空
            Dictionary<int, HashSet<Vector2Int>> tempEmptyCoord = new Dictionary<int, HashSet<Vector2Int>>();
            foreach (var kp in _xEmptyCoord)
            {
                for (int y = 0; y < _levelData.gridRow; y++)
                {
                    if (_grid[kp.Key, y].IsNeedFillBaseElement() && !_destroyGrids.Contains(new Vector2Int(kp.Key, y)))
                    {
                        if (!tempEmptyCoord.ContainsKey(kp.Key))
                            tempEmptyCoord.Add(kp.Key, new HashSet<Vector2Int>());
                        tempEmptyCoord[kp.Key].Add(_grid[kp.Key, y].Coord);
                    }
                }
            }

            foreach (var kp in tempEmptyCoord)
            {
                _xEmptyCoord[kp.Key] += kp.Value.Count;
            }

            //生成上方棋子
            Dictionary<int, int> checkTargets = new Dictionary<int, int>(LevelTargetSystem.Instance.TargetElements);
            if (context.CalAddedDelTargets != null)
            {
                foreach (var kvp in context.CalAddedDelTargets)
                {
                    if (checkTargets.ContainsKey(kvp.Key))
                        checkTargets[kvp.Key] -= kvp.Value;
                }
            }

            foreach (var kp in _xEmptyCoord)
            {
                int count = kp.Value;
                int[] finalDropId = null;
                int[] finalDropRate = null;
                if (context.FilterElementId > 0)
                {
                    int filterIndex = -1;
                    for (int i = 0; i < _levelData.dropColor.Length; i++)
                    {
                        if (_levelData.dropColor[i] == context.FilterElementId)
                        {
                            filterIndex = i;
                            break;
                        }
                    }

                    if (filterIndex >= 0)
                    {
                        int finalDropCount = _levelData.dropColor.Length - 1;
                        //将这个过滤元素的概率平均分到其它棋子身上
                        int filterAvRate = _levelData.dropColorRate[filterIndex] / finalDropCount;
                        finalDropId = new int[finalDropCount];
                        finalDropRate = new int[finalDropCount];
                        int index = 0;
                        for (int i = 0; i < _levelData.dropColor.Length; i++)
                        {
                            if (_levelData.dropColor[i] == context.FilterElementId)
                            {
                                continue;
                            }

                            finalDropId[index] = _levelData.dropColor[i];
                            finalDropRate[index] = _levelData.dropColorRate[i] + filterAvRate;
                            index++;
                        }
                    }
                    else
                    {
                        finalDropId = _levelData.dropColor;
                        finalDropRate = _levelData.dropColorRate;
                    }
                }
                else
                {
                    finalDropId = _levelData.dropColor;
                    finalDropRate = _levelData.dropColorRate;
                }
                
                // 应用策略1，修改掉落概率
                DifficultyStrategyManager.Instance.ModifyLevelDropRate(ref finalDropRate);

                int[] dropIds =
                    ElementSystem.Instance.PickElementDynamicAdjustment(finalDropId, finalDropRate,
                        count, kp.Key, false, context.CollectDropId, checkTargets);
                for (int i = 0; i < count; i++)
                {
                    var ele = ElementSystem.Instance.GenElementItemData(dropIds[i], kp.Key, -i - 1);
                    _genElements.Add(ele);
                }
            }

            List<ElementItemData> normalElements = new List<ElementItemData>(_genElements.Count);
            for (int i = 0; i < _genElements.Count; i++)
            {
                if (_genElements[i].ElementType == ElementType.Normal)
                    normalElements.Add(_genElements[i]);
            }

            DifficultyStrategyManager.Instance.ApplyAdvancedDropStrategies(this, normalElements, _destroyGrids);
            _destroyGrids.Clear();
            
            var newElements = await ElementSystem.Instance.BatchGenElements(_genElements, elementPoolRoot.transform);
            for (int i = 0; i < newElements.Length; i++)
            {
                if (newElements[i] == null)
                    continue;
                if (newElements[i].GameObject != null)
                    newElements[i].GameObject.transform.position = GetGridPositionByCoord(newElements[i].Data.GridPos.x,
                        newElements[i].Data.GridPos.y);
            }

            return newElements;
        }

        private async UniTask CheckAfterDropElement(ElementDestroyContext context, bool updateScore = true)
        {
            var allElements = ElementSystem.Instance.GridElements;
            bool haveBombPanel = context.BombPanelElementCoord != null && context.BombPanelElementCoord.Count > 0;
            if (haveBombPanel)
            {
                Vector2Int bombCoord = context.BombPanelElementCoord[0];
                ValidateManager.Instance.BombBoardPanel(context, context.BombPanelElementCoord);
                context.AddWillDelCoord(bombCoord, EliminateStyle.Side, -1);
                DestroyElementByCoord(context, bombCoord);
                GridItem bombGrid = GetGridByCoord(bombCoord);
                var bombEffect =
                    MatchEffectManager.Instance.Get(MatchEffectType.BlockBombPanel, bombGrid?.GameObject.transform);
                if (bombEffect != null)
                {
                    MatchEffectManager.Instance.PlayObjectEffect(bombEffect);
                    float duration = bombEffect.GetParticleSystemLength();
                    UniTask.Create(async () =>
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(duration));
                        MatchEffectManager.Instance.Recycle(bombEffect);
                    }).Forget();
                }
            }

            List<ElementBase> needAfterChangeStateElements = new List<ElementBase>();
            HashSet<int> collectedUId = new HashSet<int>();
            foreach (var kp in allElements)
            {
                if (context.BombPanelElementCoord != null && context.BombPanelElementCoord.Contains(kp.Key))
                    continue;
                int y = FindLastNotEmptyY(kp.Key.x);
                foreach (var element in kp.Value)
                {
                    if (element.Data.EliminateCount == 0)
                    {
                        context.AddWillDelCoord(kp.Key, element.Data.EliminateStyle, element.Data.UId);
                        collectedUId.Add(element.Data.UId);
                    }

                    //掉落收集
                    if (element is CollectElementItem && element.Data.GridPos.y == y &&
                        !ElementSystem.Instance.HaveOverElementLock(kp.Value, element.Data.ConfigId, out var _))
                    {
                        context.AddWillDelCoord(kp.Key, element.Data.EliminateStyle, element.Data.UId);
                        context.CollectDropId = element.Data.ConfigId;
                        collectedUId.Add(element.Data.UId);
                    }

                    if (element is TargetBlockElementItem targetElement)
                    {
                        int targetId = targetElement.GetTargetId();
                        if (context.CalAddedDelTargets.ContainsKey(targetId) &&
                            context.CalAddedDelTargets[targetId] > 0)
                        {
                            context.AddWillDelCoord(kp.Key, EliminateStyle.Target, targetElement.Data.UId);
                            collectedUId.Add(targetElement.Data.UId);
                        }
                    }

                    if (_dropRoundCount == 1 && context.GenSquareElementId > 0 && element.Data.EliminateStyle == EliminateStyle.Square)
                    {
                        context.AddWillDelCoord(kp.Key, element.Data.EliminateStyle, element.Data.UId);
                        collectedUId.Add(element.Data.UId);
                    }
                    if (!haveBombPanel)
                    {
                        //随机扩散元素
                        if (context.RandomDiffuseWaitCoords.Contains(element.Data.GridPos))
                        {
                            context.AddWillDelCoord(element.Data.GridPos, element.Data.EliminateStyle,
                                element.Data.UId);
                            collectedUId.Add(element.Data.UId);
                        }

                        if (_dropRoundCount == 1 && (element is ColoredLightBlockElementItem || element is AdjustCollectElementItem))
                        {
                            needAfterChangeStateElements.Add(element);
                            collectedUId.Add(element.Data.UId);
                        }
                    }
                }
            }

            bool hasDrop = false;
            if (context.WillDelCoords.Count > 0)
            {
                hasDrop = true;
                var sortedMap = MatchTweenUtil.SortedDeleteInfoToMap(context.WillDelCoords, collectedUId);
                foreach (var (_, coords) in sortedMap)
                {
                    for (int i = 0; i < coords.Count; i++)
                    {
                        var gridItem = GetGridByCoord(coords[i]);
                        if (gridItem.DestroyElement(context))
                        {
                            _destroyGrids.Add(coords[i]);
                        }

                        int index = context.WillDelCoords.FindIndex(x => x.Coord == coords[i]);
                        if (index >= 0)
                            context.WillDelCoords.RemoveAt(index);
                    }

                    await UniTask.Yield();
                }

                if (context.WaitElementDestroyTasks is { Count: > 0 })
                {
                    await UniTask.WhenAll(context.WaitElementDestroyTasks);
                    context.WaitElementDestroyTasks.Clear();
                }

                if (!haveBombPanel)
                    context.RandomDiffuseWaitCoords.Clear();
                await StartDropElement(context);

                context.CollectDropId = -1;
            }

            if (needAfterChangeStateElements.Count > 0)
            {
                int lightColorId = ElementSystem.Instance.RandomLightBlockColorId();
                for (int i = 0; i < needAfterChangeStateElements.Count; i++)
                {
                    if (needAfterChangeStateElements[i] is ColoredLightBlockElementItem lightBlockElementItem)
                        lightBlockElementItem.UpdateColor(lightColorId);
                    if (needAfterChangeStateElements[i] is AdjustCollectElementItem adjustElementItem)
                        adjustElementItem.UpdateElementState();
                }
            }

            if(_isExecuteGuideLevel)
            {
                context.CalAddedDelTargets?.Clear();
                return;
            }
            SendMatchResult(context.CalAddedDelTargets);

            if (updateScore) MatchManager.Instance.TickScoreChange();

            if (!hasDrop)
            {
                RefreshMatchTips();
                G.EventModule.DispatchEvent(GameEventDefine.OnCheckMatchResult);
            }
        }

        public int FindLastNotEmptyY(int x)
        {
            int minY = -1;
            for (int y = _levelData.gridRow - 1; y >= 0; --y)
            {
                if (!_grid[x, y].IsWhite && minY < y)
                {
                    minY = y;
                }
            }

            return minY;
        }

        public int FindLastNotEmptyX(int y)
        {
            int minX = -1;
            for (int x = _levelData.gridCol - 1; x >= 0; --x)
            {
                if (!_grid[x, y].IsWhite && minX < x)
                {
                    minX = x;
                }
            }

            return minX;
        }

        private UniTask DoLineBallAndSpecialItem(ElementDestroyContext context)
        {
            var ballCoord = context.LinkSpecialBallCoord;
            var elements = ElementSystem.Instance.GetGridElements(ballCoord, true);
            if (elements == null || elements.Count <= 0) return UniTask.CompletedTask;

            var ballElement = elements.Find(n => n is ColorBallElementItem) as ColorBallElementItem;
            if (ballElement == null) return UniTask.CompletedTask;

            var specialDatas = new List<ElementItemData>(context.BallLineSpecialItems);
            int total = specialDatas.Count;

            void PushSpecialItem(int index)
            {
                if (index < 0 || index >= total)
                    return;

                var itemData = specialDatas[index];
                var elementItem = ElementSystem.Instance.GenElement(itemData, ballElement.GameObject.transform);
                var gridItem = GetGridByCoord(itemData.GridPos);
                gridItem.PushElement(elementItem, doDestroy: true);
                elementItem.GameObject.transform.localPosition = Vector3.zero;
                elementItem.PlayShock();
            }

            if (total == 1)
            {
                PushSpecialItem(0);
                return UniTask.CompletedTask;
            }
            
            ballElement.PlayBallAnimation("Idle02");
            ballElement.DoPopScale();
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();

            List<Vector3> endPositions = new List<Vector3>(specialDatas.Count);
            for (int i = 0; i < specialDatas.Count; i++)
            {
                var pos = GetGridPositionByCoord(specialDatas[i].GridPos.x, specialDatas[i].GridPos.y);
                endPositions.Add(pos);
            }

            _trailEmitter.Emitter(ballElement.GameObject.transform.position, endPositions, PushSpecialItem,
                () =>
                {
                    tcs.TrySetResult();
                });

            return tcs.Task;
        }

        private async UniTask DoBallLineBombDelElement(ElementDestroyContext context, List<DeleteGridInfo> delGridInfos)
        {
            var bombList = GetBombList();
            bombList.Shuffle();
            const float perDelay = 0.03f;
            List<UniTask> tasks = new List<UniTask>();
            foreach (var bombItem in bombList)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(perDelay));
                tasks.Add(DoBombDelElement(context, bombItem.Data.GridPos, delGridInfos));
            }

            await UniTask.WhenAll(tasks);
            delGridInfos.Clear();
        }

        private async UniTask DoAutoReleaseBomb(ElementDestroyContext context)
        {
            var bombList = GetBombList();
            foreach (var elementItem in bombList)
            {
                ValidateManager.Instance.AttachBomb(context, elementItem.Data.GridPos, elementItem.Data.UId);
            }
            bombList.Shuffle();
            const float perDelay = 0.03f;
            List<UniTask> tasks = new List<UniTask>();
            List<DeleteGridInfo> delGridInfos = new List<DeleteGridInfo>(context.WillDelCoords);
            G.UIModule.ScreenLock(MatchConst.MatchLockByReleaseBomb, true);
            foreach (var bombItem in bombList)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(perDelay));
                tasks.Add(DoBombDelElement(context, bombItem.Data.GridPos, delGridInfos));
            }
            await UniTask.WhenAll(tasks);
            StartDropElement(context).Forget();
            
            G.UIModule.ScreenLock(MatchConst.MatchLockByReleaseBomb, false);
        }

        private List<BombElementItem> GetBombList()
        {
            var bombList = new List<BombElementItem>();
            var coords = ElementSystem.Instance.GridElements.Keys;
            foreach (var bombCoord in coords)
            {
                var elements = ElementSystem.Instance.GetGridElements(bombCoord, true);
                bool result = ElementSystem.Instance.TryGetBaseElement(elements, out int index, true);
                if (result)
                {
                    if (elements[index] is BombElementItem bombItem)
                    {
                        bombList.Add(bombItem);
                    }
                }
            }

            return bombList;
        }

        private UniTask DoDoubleBombDelElement(ElementDestroyContext context, List<DeleteGridInfo> delGridInfos)
        {
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            List<Vector2Int> doubleBombCoords = new List<Vector2Int>(context.DoubleBombCoords);
            MatchTweenUtil.DoDoubleBombDelElement(doubleBombCoords, (b) =>
            {
                if (!b)
                {
                    tcs.TrySetResult();
                    return;
                }

                if (doubleBombCoords.Count >= 2)
                {
                    int firstIndex = delGridInfos.FindIndex(x => x.Coord == doubleBombCoords[0]);
                    if (firstIndex >= 0)
                        delGridInfos.RemoveAt(firstIndex);
                    int secondIndex = delGridInfos.FindIndex(x => x.Coord == doubleBombCoords[1]);
                    if (secondIndex >= 0)
                        delGridInfos.RemoveAt(secondIndex);
                    DestroyElementByCoord(context, doubleBombCoords[0]);
                    DestroyElementByCoord(context, doubleBombCoords[1]);
                }

                UniTask.Create(async () =>
                {
                    var map = MatchTweenUtil.SortedDeleteInfoToMap(delGridInfos);
                    foreach (var (configId, coords) in map)
                    {
                        for (int i = 0; i < coords.Count; i++)
                        {
                            context.AddWaitElementDestroyTask(DoElement(coords[i], context, delGridInfos, true));
                        }

                        await UniTask.DelayFrame(1);
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(MatchConst.BombElementEffDur));
                    tcs.TrySetResult();
                }).Forget();
            });

            return tcs.Task;
        }

        private UniTask DoDoubleBallDelElement(ElementDestroyContext context, List<DeleteGridInfo> delGridInfos)
        {
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            SpriteRenderer sp = _overItemBackground.GetComponent<SpriteRenderer>();
            Sequence blackBgSeq = DOTween.Sequence();
            blackBgSeq.Append(sp.DOFade(0.45f, 0.1f));
            blackBgSeq.AppendInterval(2.7f);
            blackBgSeq.Append(sp.DOFade(0, 1.2f));
            List<Vector2Int> doubleBallCoords = new List<Vector2Int>(context.DoubleColorBallCoords);
            MatchTweenUtil.DoDoubleBallDelElement(doubleBallCoords, async success =>
            {
                if (!success)
                {
                    tcs.TrySetResult();
                    return;
                }

                if (delGridInfos == null)
                {
                    tcs.TrySetResult();
                    return;
                }

                int firstIndex = delGridInfos.FindIndex(x => x.Coord == doubleBallCoords[0]);
                if (firstIndex >= 0)
                    delGridInfos.RemoveAt(firstIndex);
                int secondIndex = delGridInfos.FindIndex(x => x.Coord == doubleBallCoords[1]);
                if (secondIndex >= 0)
                    delGridInfos.RemoveAt(secondIndex);
                DestroyElementByCoord(context, doubleBallCoords[0]);
                DestroyElementByCoord(context, doubleBallCoords[1]);
                var map = MatchTweenUtil.SortedDeleteInfoToMap(delGridInfos);
                foreach (var (configId, coords) in map)
                {
                    for (int i = 0; i < coords.Count; i++)
                    {
                        DestroyElementByCoord(context, coords[i]);
                    }

                    await UniTask.DelayFrame(1);
                }

                delGridInfos.Clear(); //全屏消除直接清空
                tcs.TrySetResult();
            });
            return tcs.Task;
        }

        private void PlayCollectBlockTarget(GridItem grid)
        {
            if (grid == null) return;

            var baseElement = grid.GetBaseElementItem();
            if (baseElement == null) return;

            if (ElementSystem.Instance.IsTargetBlockTarget(baseElement.Data.ConfigId))
            {
                var blockElements = ElementSystem.Instance.GetTargetBlockElements(baseElement.Data.ConfigId);
                if (blockElements.Count > 0)
                {
                    foreach (var blockElement in blockElements)
                    {
                        if (blockElement.GetCollectCount() > 0)
                        {
                            var targetPos = blockElement.GetTextNumPosition();
                            blockElement.AddCollectCount();
                            _collectController.DoCollectItemFlyToTarget(baseElement.Data.ConfigId,
                                baseElement.GameObject.transform.position, targetPos,
                                () => { AudioUtil.PlayCHuanglianShouji(); }, 0);
                            break;
                        }
                    }
                }
            }
        }
        
        private void SendMatchResult(Dictionary<int, int> matchElements)
        {
            if (matchElements == null || matchElements.Count == 0)
                return;
            //统计结果
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchStepMove,
                EventOneParam<Dictionary<int, int>>.Create(matchElements));
            matchElements.Clear();

            _collectController.ResetData();
        }
    }
}