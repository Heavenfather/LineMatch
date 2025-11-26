using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using Hotfix.Define;
using HotfixCore.Extensions;
using HotfixCore.MemoryPool;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.Match
{
    public static class MatchTweenUtil
    {
        private static GridSystem _gridSystem;

        public static void Init(GridSystem gridSystem)
        {
            _gridSystem = gridSystem;
        }

        /// <summary>
        /// 火箭移动
        /// </summary>
        public static void DoRocketTweenMove(Vector2Int coord, Action callback, Vector2Int? linkRocketCoord = null)
        {
            var elements = ElementSystem.Instance.GetGridElements(coord, true);
            GameObject moveTarget = null;
            bool result = ElementSystem.Instance.TryGetBaseElement(elements, out int rocketIndex, true);
            ElementBase element = null;
            ElementDirection direction = ElementDirection.None;
            if (result && elements[rocketIndex] is RockElementItem)
            {
                element = elements[rocketIndex];
                direction = element.Data.Direction;
                moveTarget = elements[rocketIndex].GameObject;
            }
            else
            {
                return;
            }

            if (direction == ElementDirection.Up || direction == ElementDirection.Right)
            {
                DoRocketTargetMove(moveTarget, new Vector2Int(coord.x, coord.y), direction, () =>
                {
                    callback?.Invoke();
                }, linkRocketCoord);
            }
        }

        public static void DoRocketTargetMove(GameObject moveTarget, Vector2Int coord, ElementDirection direction,
            Action callback, Vector2Int? linkRocketCoord = null)
        {
            MatchManager.Instance.PlayRockerAudio();
            if (linkRocketCoord.HasValue)
            {
                coord = linkRocketCoord.Value;
            }

            Vector3 startPos = Vector3.zero;
            Vector3 endPos = Vector3.zero;
            if (direction == ElementDirection.Up)
            {
                startPos = GridSystem.GetGridPositionByCoord(coord.x, 12);
                endPos = GridSystem.GetGridPositionByCoord(coord.x, -5);
            }
            else if (direction == ElementDirection.Right)
            {
                startPos = GridSystem.GetGridPositionByCoord(-2, coord.y);
                endPos = GridSystem.GetGridPositionByCoord(15, coord.y);
            }
            else
            {
                return;
            }

            moveTarget.transform.position = startPos;
            Renderer icon = moveTarget.transform.Find("Icon")?.GetComponent<Renderer>();
            if (icon != null)
            {
                icon.sortingLayerName = "OverLine";
                icon.sortingOrder = 1;
            }

            var effectIdle = moveTarget.transform.Find("eff_idle")?.gameObject;
            if (effectIdle != null)
                effectIdle.SetVisible(false);
            var effectTrail = moveTarget.transform.Find("eff_trail")?.gameObject;
            if (effectTrail != null)
                effectTrail.SetVisible(true);
            moveTarget.transform.DOMove(endPos, MatchConst.RocketMoveDuration).OnComplete(
                () => { callback?.Invoke(); });
        }

        /// <summary>
        /// 获取火箭类型移动的格子坐标
        /// </summary>
        /// <returns></returns>
        public static List<Vector2Int> GetRocketDelPos(Vector2Int rocketCoord, ElementDirection direction,
            HashSet<Vector2Int> filterList = null)
        {
            var delPosList = new List<Vector2Int>();

            var gridPos = new Vector2Int(rocketCoord.x, 0);
            var moveDir = Vector2Int.up;
            if (direction == ElementDirection.Right || direction == ElementDirection.Left)
            {
                moveDir = Vector2Int.right;
                gridPos = new Vector2Int(0, rocketCoord.y);
            }

            delPosList.Add(gridPos);

            while (true)
            {
                gridPos += moveDir;
                if (!_gridSystem.IsLimitPosition(gridPos.x, gridPos.y))
                    break;
                // if (_gridSystem.IsWhitePos(gridPos.x, gridPos.y)) continue;
                if (filterList != null && filterList.Contains(gridPos)) continue;

                if (direction == ElementDirection.Right)
                {
                    delPosList.Add(gridPos);
                }
                else if (direction == ElementDirection.Left)
                {
                    delPosList.Insert(0, gridPos);
                }
                else if (direction == ElementDirection.Down)
                {
                    delPosList.Add(gridPos);
                }
                else if (direction == ElementDirection.Up)
                {
                    delPosList.Insert(0, gridPos);
                }
            }

            return delPosList;
        }

        /// <summary>
        /// 双彩球
        /// </summary>
        /// <param name="colorBallCoordList"></param>
        /// <param name="complete"></param>
        public static void DoDoubleBallDelElement(List<Vector2Int> colorBallCoordList, Action<bool> complete)
        {
            if (colorBallCoordList.Count < 2)
            {
                complete?.Invoke(false);
                return;
            }

            ColorBallElementItem firstBallItem = null;
            ColorBallElementItem secondBallItem = null;

            for (int i = 0; i < 2; i++)
            {
                var bombCoord = colorBallCoordList[i];
                var elements = ElementSystem.Instance.GetGridElements(bombCoord, true);
                if (elements != null && elements.Count > 0)
                {
                    foreach (var element in elements)
                    {
                        if (element is ColorBallElementItem bomb)
                        {
                            if (i == 0)
                            {
                                firstBallItem = bomb;
                            }
                            else
                            {
                                secondBallItem = bomb;
                            }

                            break;
                        }
                    }
                }
            }

            if (firstBallItem == null || secondBallItem == null)
            {
                complete?.Invoke(false);
                return;
            }
            secondBallItem.GameObject.SetVisible(false);
            firstBallItem.GameObject.SetVisible(false);

            GridItem gridItem = _gridSystem.GetGridByCoord(firstBallItem.Data.GridPos);
            var swapEffect = MatchEffectManager.Instance.Get(MatchEffectType.DoubleBallBombPanel, gridItem.GameObject.transform);
            var di = swapEffect.transform.Find("Match_eff_cq_jh_bd");
            MatchEffectManager.Instance.PlayObjectEffect(di?.gameObject);
            Animator animator = swapEffect.GetComponent<Animator>();
            if (animator != null)
            {
                animator.PlayUIAnimation("ani_wl_00001", (b) =>
                {
                    if(di != null)
                        di.SetVisible(false);
                    var bombEff = swapEffect.transform.Find("Match_eff_baodian_ty");
                    float duration = bombEff.GetParticleSystemLength();
                    MatchEffectManager.Instance.PlayObjectEffect(bombEff.gameObject);
                    UniTask.Create(async () =>
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(duration));
                        bombEff.SetVisible(false);
                        MatchEffectManager.Instance.Recycle(swapEffect);
                    }).Forget();
                    complete?.Invoke(b);
                });
            }
            else
            {
                complete?.Invoke(false);
            }
        }

        public static void DoBombAndRocket(RockElementItem rocketItem, BombElementItem bombItem, Action callback)
        {
            if (rocketItem == null || bombItem == null)
            {
                callback?.Invoke();
                return;
            }
            Vector2Int bombCoord = bombItem.Data.GridPos;
            Vector2Int rocketCoord = rocketItem.Data.GridPos;

            bool isVertical = bombCoord.x != rocketCoord.x;
            Vector3[] positions = new Vector3[4];
            if (isVertical)
            {
                positions[0] = new Vector3(-0.25f, 0, 0);
                positions[1] = new Vector3(0, 0, -1);
                positions[2] = new Vector3(0.25f, 0, 0);
                positions[3] = new Vector3(0, 0, 1);
            }
            else
            {
                positions[0] = new Vector3(0, 0.25f, 0);
                positions[1] = new Vector3(0, 0, -1);
                positions[2] = new Vector3(0, -0.25f, 0);
                positions[3] = new Vector3(0, 0, 1);
            }

            float beginScale = 1.1f;
            float beginCircleDur = 0.08f;
            float biggerScale = 1.2f;
            float accelerationFactor = 0.6f; // 加速因子,数值越小，加速越快
            Ease moveEase = Ease.Flash;
            var sequence = DOTween.Sequence();
            rocketItem.ResetSortingLayer("OverLine", 2);
            bombItem.ResetSortingLayer("OverLine", 2);

            Transform firstBall = rocketItem.GameObject.transform;
            Transform secondBall = bombItem.GameObject.transform;

            void AddCycleToSequence(Sequence seq, float stepDuration)
            {
                // 第一步
                seq.Append(firstBall.DOLocalMove(positions[1], stepDuration).SetEase(moveEase));
                seq.Join(firstBall.DOScale(biggerScale, stepDuration));
                seq.Join(secondBall.DOLocalMove(positions[3], stepDuration).SetEase(moveEase));
                seq.Join(secondBall.DOScale(1.0f, stepDuration));

                // 第二步
                seq.Append(secondBall.DOLocalMove(positions[0], stepDuration).SetEase(moveEase));
                seq.Join(secondBall.DOScale(beginScale, stepDuration));
                seq.Join(firstBall.DOLocalMove(positions[2], stepDuration).SetEase(moveEase));
                seq.Join(firstBall.DOScale(beginScale, stepDuration));

                // 第三步
                seq.Append(secondBall.DOLocalMove(positions[1], stepDuration).SetEase(moveEase));
                seq.Join(secondBall.DOScale(biggerScale, stepDuration));
                seq.Join(firstBall.DOLocalMove(positions[3], stepDuration).SetEase(moveEase));
                seq.Join(firstBall.DOScale(1.0f, stepDuration));

                // 第四步
                seq.Append(firstBall.DOLocalMove(positions[0], stepDuration).SetEase(moveEase));
                seq.Join(firstBall.DOScale(beginScale, stepDuration));
                seq.Join(secondBall.DOLocalMove(positions[2], stepDuration).SetEase(moveEase));
                seq.Join(secondBall.DOScale(beginScale, stepDuration));
            }
            
            float currentDuration = beginCircleDur;
            for (int i = 0; i < 2; i++)
            {
                AddCycleToSequence(sequence, currentDuration);
                currentDuration *= accelerationFactor;
            }
            
            sequence.OnComplete(() =>
            {
                callback?.Invoke();
            });
        }

        /// <summary>
        /// 双炸弹
        /// </summary>
        public static void DoDoubleBombDelElement(List<Vector2Int> bombCoordList, Action<bool> complete)
        {
            if (bombCoordList.Count < 2)
            {
                complete?.Invoke(false);
                return;
            }

            BombElementItem firstBombItem = null;
            BombElementItem secondBombItem = null;

            for (int i = 0; i < 2; i++)
            {
                var bombCoord = bombCoordList[i];
                var elements = ElementSystem.Instance.GetGridElements(bombCoord, true);
                if (elements != null && elements.Count > 0)
                {
                    foreach (var element in elements)
                    {
                        if (element is BombElementItem bomb)
                        {
                            if (i == 0)
                            {
                                firstBombItem = bomb;
                            }
                            else
                            {
                                secondBombItem = bomb;
                            }

                            break;
                        }
                    }
                }
            }

            if (firstBombItem == null || secondBombItem == null)
            {
                complete?.Invoke(false);
                return;
            }

            var bombPos = new Vector3(0, 0, -0.1f);
            firstBombItem.GameObject.transform.localPosition = bombPos;
            firstBombItem.ResetSortingLayer("OverLine",2);
            firstBombItem.SwitchToBigIcon();
            secondBombItem.GameObject.SetActive(false);

            Transform tweenTarget = firstBombItem.GameObject.transform.Find("IconBig");
            tweenTarget.localScale = Vector3.one * 0.2f;

            System.Random random = new System.Random();

            var beginScaleTime = 0.3f;
            var bombAwaitTime = 0.05f;
            var awaitCount = 12;

            var seqScale = DOTween.Sequence();
            seqScale.Append(tweenTarget.DOScale(0.4f, beginScaleTime))
                .OnComplete(() => { firstBombItem.PlayShock(); });
            seqScale.Join(tweenTarget.DOLocalRotate(new Vector3(0, 0, random.Next(-15, 16)), 0.1f));
            seqScale.Join(tweenTarget.DOLocalRotate(new Vector3(0, 0, random.Next(-15, 16)), 0.1f).SetDelay(0.1f));
            seqScale.Join(tweenTarget.DOLocalRotate(new Vector3(0, 0, random.Next(-15, 16)), 0.1f).SetDelay(0.2f));


            for (int i = 0; i < awaitCount; i++)
            {
                double randomX = random.NextDouble() * 0.15 - 0.075;
                double randomY = random.NextDouble() * 0.15 - 0.075;
                var pos = new Vector3((float)randomX, (float)randomY, -0.1f);
                seqScale.Append(tweenTarget.DOLocalMove(pos, bombAwaitTime));

                seqScale.Join(tweenTarget.DOLocalRotate(new Vector3(0, 0, random.Next(-5, 6)),
                    bombAwaitTime));

                float randomScale = Mathf.Clamp((float)random.NextDouble() * 0.4f, 0.3f, 0.45f);
                Vector3 scale = new Vector3(randomScale, randomScale, 1f);
                seqScale.Join(tweenTarget.DOScale(scale, bombAwaitTime));
            }

            seqScale.Append(tweenTarget.DOScale(Vector3.one * 0.4f, bombAwaitTime));
            seqScale.Join(tweenTarget.DOLocalRotate(Vector3.zero, bombAwaitTime));
            seqScale.Join(tweenTarget.DOLocalMove(Vector3.zero, bombAwaitTime));

            seqScale.Append(tweenTarget.DOScale(new Vector3(0.4f, 0.45f, 0.4f), bombAwaitTime * 2))
                .OnComplete(() =>
                {
                    // firstBombItem.PlayDoubleBallEffect();
                    complete?.Invoke(true);
                });
        }

        /// <summary>
        /// 合成功能棋子
        /// </summary>
        /// <param name="context"></param>
        public static void PlayCreateSpecialElementAnim(ElementDestroyContext context)
        {
            if (!context.IsCombineSpecialElement) return;

            foreach (var info in context.GenSpecialInfos)
            {
                if (context.TryGetSpecialElement(info.Coord, out var itemData))
                {
                    var grid = _gridSystem.GetGridByCoord(info.Coord);
                    var element = ElementSystem.Instance.GenElement(itemData, grid.GameObject.transform);
                    element.GameObject.transform.localPosition = Vector3.zero;
                    grid.PushElement(element, doDestroy: true);
                }
            }
        }


        // 获取四个方向的位置，顺序为：上左右下
        public static List<Vector2Int> GetNeighborPos(Vector2Int gridPos, bool checkValid = true)
        {
            List<Vector2Int> neighbor = new List<Vector2Int>();

            for (int i = 0; i < ValidateManager.Instance.NeighborDirs.Length; i++)
            {
                var dir = gridPos + ValidateManager.Instance.NeighborDirs[i];
                if (checkValid)
                {
                    if (!_gridSystem.IsValidPosition(dir.x, dir.y))
                    {
                        continue;
                    }
                }

                neighbor.Add(dir);
            }

            return neighbor;
        }

        //获取八方向位置
        public static List<Vector2Int> GetEightNeighborPos(Vector2Int gridPos)
        {
            List<Vector2Int> neighbor = new List<Vector2Int>();
            var dirs = ValidateManager.Instance.EightNeighborDirs;
            for (int i = 0; i < dirs.Length; i++)
            {
                Vector2Int pos = gridPos + dirs[i];
                neighbor.Add(pos);
            }

            return neighbor;
        }

        public static Dictionary<int, List<Vector2Int>> SortedDeleteInfoToMap(List<DeleteGridInfo> oriDelInfos)
        {
            Dictionary<int, List<Vector2Int>> delInfosMap = new Dictionary<int, List<Vector2Int>>(20);

            bool IsContainCoord(Vector2Int coord)
            {
                foreach (var coords in delInfosMap.Values)
                {
                    if(coords.Contains(coord))
                        return true;
                }

                return false;
            }

            for (int i = 0; i < oriDelInfos.Count; i++)
            {
                var delInfo = oriDelInfos[i];
                if (delInfo.ElementConfigIds != null)
                {
                    foreach (var configId in delInfo.ElementConfigIds)
                    {
                        if(IsContainCoord(delInfo.Coord))
                            continue;
                        if (!delInfosMap.ContainsKey(configId))
                        {
                            delInfosMap[configId] = new List<Vector2Int>(30);
                        }

                        delInfosMap[configId].Add(delInfo.Coord);
                    }
                }
            }

            return delInfosMap;
        }
    }
}