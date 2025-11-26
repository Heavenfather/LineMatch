using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using Hotfix.Define;
using HotfixCore.Extensions;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class ColoredRibbonBlockElementItem : BlockElementItem
    {
        private List<GameObject> _overIconList = new List<GameObject>(5);

        protected override void OnInitialized()
        {
            for (int i = 1; i <= 5; i++)
            {
                _overIconList.Add(this.GameObject.transform.Find("Over" + i).gameObject);
            }

            var eff = this.GameObject.transform.Find("Match_eff_sd");
            if (eff != null)
            {
                eff.SetVisible(false);
            }

            int checkLine = -1;
            Transform kuang = this.GameObject.transform.Find("kuang");
            kuang?.SetVisible(false);
            if(Data.Direction == ElementDirection.Up)
                checkLine = Data.GridPos.x;
            else if(Data.Direction == ElementDirection.Right)
                checkLine = Data.GridPos.y;
            if (checkLine != -1)
            {
                (int start, int end) = ElementSystem.Instance.FindColoredRibbonStartEndPos(Data.Direction, checkLine);
                if (end != -1)
                {
                    Vector2 elementSize = GridSystem.GridSize;
                    
                    if (Data.Direction == ElementDirection.Up && start == Data.GridPos.y)
                    {
                        if (kuang != null)
                        {
                            Vector2 size = kuang.GetComponent<SpriteRenderer>().size;
                            kuang.GetComponent<SpriteRenderer>().size =
                                new Vector2(size.x, elementSize.y * (end - start + 1));
                            kuang.SetVisible(true);
                        }
                    }

                    if (Data.Direction == ElementDirection.Right && start == Data.GridPos.x)
                    {
                        if (kuang != null)
                        {
                            Vector2 size = kuang.GetComponent<SpriteRenderer>().size;
                            kuang.GetComponent<SpriteRenderer>().size =
                                new Vector2(elementSize.x * (end - start + 1), size.y);
                            kuang.SetVisible(true);
                        }
                    }
                }
            }

            SetOverIconState();
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            ElementDirection direction = Data.Direction;
            bool result = base.OnDestroy(context);
            SetOverIconState();
            if (result)
            {
                Transform kuang = this.GameObject.transform.Find("kuang");
                kuang?.SetVisible(false);
                
                bool needParseTarget = ElementSystem.Instance.IsNeedParseTarget(Data.ConfigId, out int parseTargetId);
                if (needParseTarget)
                { 
                    context.AddCalAddedCount(parseTargetId, 1);
                }
                //找出相同类型的一起销毁
                if (context.ColoredRibbonWaitDelCoords != null &&
                    context.ColoredRibbonWaitDelCoords.Contains(Data.GridPos))
                {
                    return true;
                }
                UniTaskCompletionSource tcs = new UniTaskCompletionSource();
                context.AddWaitElementDestroyTask(tcs.Task);
                var elementCoords = FindDeleteColoredRibbon(context,direction);
                int totalCount = elementCoords.Count;
                if (totalCount <= 0)
                {
                    tcs.TrySetResult();
                }
                else
                {
                    float startDelay = 0.0f;
                    var seq = DOTween.Sequence();
                    for (int i = 0; i < elementCoords.Count; i++)
                    {
                        var idx = i;
                        float delayTime = startDelay + (i * MatchConst.DelElementInterval);

                        seq.InsertCallback(delayTime, () =>
                        {
                            context.GridSystem.DestroyElementByCoord(context, elementCoords[idx]);
                        });
                    }

                    seq.OnComplete(() =>
                    {
                        tcs.TrySetResult();
                    }).SetAutoKill(true);
                }
            }
            else
            {
                var eff = this.GameObject.transform.Find("Match_eff_sd");
                if (eff != null)
                {
                    eff.SetVisible(false);
                    eff.SetVisible(true);
                    foreach (var particle in eff.GetComponentsInChildren<ParticleSystem>(true))
                    {
                        particle.Play();
                    }
                }
                ElementAudioManager.Instance.Play("ShaDiQiQiu");
            }

            return result;
        }

        private List<Vector2Int> FindDeleteColoredRibbon(ElementDestroyContext context,ElementDirection direction)
        {
            List<Vector2Int> gridCoords = new List<Vector2Int>();
            Vector2Int centerCoord = Data.GridPos;
            int elementId = Data.ConfigId;

            var queue = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            queue.Enqueue(centerCoord);
            visited.Add(centerCoord);
            List<Vector2Int> directionNeighbor = new List<Vector2Int>(4);
            if (direction == ElementDirection.None)
            {
                directionNeighbor.AddRange(ValidateManager.Instance.NeighborDirs);
            }
            else if (direction == ElementDirection.Up)
            {
                directionNeighbor.Add(Vector2Int.up);
                directionNeighbor.Add(Vector2Int.down);
            }
            else if (direction == ElementDirection.Right)
            {
                directionNeighbor.Add(Vector2Int.right);
                directionNeighbor.Add(Vector2Int.left);
            }
            
            while (queue.Count > 0)
            {
                Vector2Int currentPos = queue.Dequeue();
                if (currentPos != centerCoord)
                {
                    var elements = ElementSystem.Instance.GetGridElements(currentPos, true);
                    for (int i = 0; i < elements.Count; i++)
                    {
                        bool haveLock = ElementSystem.Instance.HaveOverElementLock(currentPos, elements[i].Data.ConfigId,out var _);
                        if(haveLock)
                            continue;
                        if (elements[i].Data.ConfigId == elementId)
                        {
                            gridCoords.Add(elements[i].Data.GridPos);
                            context.AddWillDelCoord(elements[i].Data.GridPos, elements[i].Data.EliminateStyle,
                                elements[i].Data.UId, attachCount: elements[i].Data.EliminateCount);
                            context.AddColoredRibbonCoord(elements[i].Data.GridPos);
                            break;
                        }
                    }
                }

                foreach (var dir in directionNeighbor)
                {
                    Vector2Int neighborPos = currentPos + dir;
                    if(!context.GridSystem.IsValidPosition(neighborPos.x, neighborPos.y))
                        continue;
                    if(visited.Contains(neighborPos))
                        continue;
                    var elements = ElementSystem.Instance.GetGridElements(neighborPos, true);
                    if(elements == null || elements.Count <= 0)
                        continue;
                    for (int i = 0; i < elements.Count; i++)
                    {
                        bool haveLock = ElementSystem.Instance.HaveOverElementLock(neighborPos, elements[i].Data.ConfigId,out var _);
                        if(haveLock)
                            continue;
                        if (elements[i].Data.ConfigId == elementId)
                        {
                            queue.Enqueue(neighborPos);
                            visited.Add(neighborPos);
                        }
                    }
                }
            }

            gridCoords.Sort((a, b) =>
                Vector2Int.Distance(a, centerCoord).CompareTo(Vector2Int.Distance(b, centerCoord))
            );

            return gridCoords;
        }

        private void SetOverIconState()
        {
            int remain = Data.EliminateCount;
            for (int i = 0; i < _overIconList.Count; i++)
            {
                _overIconList[i].SetVisible(_overIconList[i].transform.name == $"Over{remain}");
            }
        }

        public override void Clear()
        {
            base.Clear();
            _overIconList.Clear();
        }
    }
}