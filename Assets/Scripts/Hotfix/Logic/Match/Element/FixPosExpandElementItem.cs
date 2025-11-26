using System.Collections.Generic;
using GameConfig;
using HotfixCore.Extensions;
using Spine.Unity;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class FixPosExpandElementItem : BlockElementItem
    {
        private Dictionary<Vector2Int, GameObject> _flowerMap = new Dictionary<Vector2Int, GameObject>();

        private Dictionary<Vector2Int, HashSet<Vector2Int>> _flowerSideCoordsMap =
            new Dictionary<Vector2Int, HashSet<Vector2Int>>();

        protected override void OnInitialized()
        {
            Transform flower = GameObject.transform.Find("Icon/Flower");
            flower.name = $"{Data.GridPos.x}-{Data.GridPos.y}";
            _flowerMap.Add(new Vector2Int(Data.GridPos.x, Data.GridPos.y), flower.gameObject);
            Resize(flower);
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            if (context.IsColorBallClearAll)
            {
                bool needParseTarget =
                    ElementSystem.Instance.IsNeedParseTarget(Data.ConfigId, out int parseTargetId);
                if (needParseTarget)
                {
                    for (int i = 0; i < Data.EliminateCount; i++)
                    {
                        context.AddCalAddedCount(parseTargetId, 1);
                    }
                }

                Data.EliminateCount = 0;
            }
            else
            {
                Dictionary<Vector2Int, HashSet<Vector2Int>> tempMap =
                    new Dictionary<Vector2Int, HashSet<Vector2Int>>(_flowerSideCoordsMap);
                foreach (var coord in context.AllDelGridCoord)
                {
                    if (DelFlower(coord,tempMap))
                    {
                        bool needParseTarget =
                            ElementSystem.Instance.IsNeedParseTarget(Data.ConfigId, out int parseTargetId);
                        if (needParseTarget)
                        {
                            context.AddCalAddedCount(parseTargetId, 1);
                        }
                    }
                }
            }

            if (Data.EliminateCount <= 0)
            {
                _flowerSideCoordsMap.Clear();
                PlayEffect();
                return true;
            }

            return false;
        }

        public override void Clear()
        {
            base.Clear();
            foreach (var go in _flowerMap.Values)
            {
                GameObject.Destroy(go);
            }

            _flowerMap.Clear();
        }

        private bool DelFlower(Vector2Int coord,Dictionary<Vector2Int, HashSet<Vector2Int>> temp)
        {
            foreach (var kp in temp)
            {
                if(!_flowerSideCoordsMap.ContainsKey(kp.Key))
                    continue;
                if (kp.Key == coord || kp.Value.Contains(coord))
                {
                    if (_flowerMap.TryGetValue(kp.Key, out var flower))
                    {
                        if (flower.gameObject.GetComponent<Renderer>().enabled)
                        {
                            SkeletonAnimation spine = flower.GetComponent<SkeletonAnimation>();
                            if (spine != null)
                            {
                                spine.AnimationState.SetAnimation(0, "hit", false).Complete += (entry) =>
                                {
                                    flower.GetComponent<Renderer>().enabled = false;
                                    //向日葵消掉的音效
                                    ElementAudioManager.Instance.Play("XiangRiKui");
                                    
                                    if (flower.transform.childCount > 0)
                                    {
                                        flower.transform.GetChild(0).SetVisible(true);
                                    }
                                };
                            }

                            _flowerSideCoordsMap.Remove(kp.Key);
                            --Data.EliminateCount;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void Resize(Transform flowerObject)
        {
            var gridInfos = ElementSystem.Instance.FindCoordHoldGridInfo(Data.GridPos.x, Data.GridPos.y);
            if (gridInfos is { Count: > 0 })
            {
                flowerObject.GetComponent<Renderer>().enabled = true;
                int indexOf = gridInfos.FindIndex(x => x.ElementId == Data.ConfigId);
                if (indexOf >= 0)
                {
                    var info = gridInfos[indexOf];
                    float elementSize = GridSystem.GridSize.x;
                    float offset = elementSize / 2.0f;
                    Transform icon = GameObject.transform.Find("Icon");

                    if (Data.Direction == ElementDirection.Up)
                    {
                        Data.EliminateCount = info.ElementHeight;
                        icon.transform.localPosition = new Vector3(0, offset * -1.0f, 0);
                        for (int i = 1; i < Data.EliminateCount; i++)
                        {
                            GameObject go = GameObject.Instantiate(flowerObject.gameObject, icon);
                            go.name = $"{Data.GridPos.x}-{Data.GridPos.y - i}";
                            go.transform.localPosition = new(flowerObject.localPosition.x,
                                flowerObject.localPosition.y - elementSize * i * -1.0f, 0);
                            _flowerMap.Add(new Vector2Int(Data.GridPos.x, Data.GridPos.y - i), go);
                        }
                    }
                    else if (Data.Direction == ElementDirection.Down)
                    {
                        Data.EliminateCount = info.ElementHeight;
                        icon.transform.localPosition = new Vector3(0, offset, 0);
                        for (int i = 1; i < Data.EliminateCount; i++)
                        {
                            GameObject go = GameObject.Instantiate(flowerObject.gameObject, icon);
                            go.name = $"{Data.GridPos.x}-{Data.GridPos.y + i}";
                            go.transform.localPosition = new(flowerObject.localPosition.x,
                                flowerObject.localPosition.y - elementSize * i * 1.0f, 0);
                            _flowerMap.Add(new Vector2Int(Data.GridPos.x, Data.GridPos.y + i), go);
                        }
                    }
                    else if (Data.Direction == ElementDirection.Left)
                    {
                        Data.EliminateCount = info.ElementWidth;
                        icon.transform.localPosition = new Vector3(offset, 0, 0);
                        for (int i = 1; i < Data.EliminateCount; i++)
                        {
                            GameObject go = GameObject.Instantiate(flowerObject.gameObject, icon);
                            go.name = $"{Data.GridPos.x - i}-{Data.GridPos.y}";
                            go.transform.localPosition = new(elementSize * i * -1.0f + flowerObject.localPosition.x,
                                flowerObject.localPosition.y, 0);
                            _flowerMap.Add(new Vector2Int(Data.GridPos.x - i, Data.GridPos.y), go);
                        }
                    }
                    else if (Data.Direction == ElementDirection.Right)
                    {
                        Data.EliminateCount = info.ElementWidth;
                        icon.transform.localPosition = new Vector3(offset * -1.0f, 0, 0);
                        for (int i = 1; i < Data.EliminateCount; i++)
                        {
                            GameObject go = GameObject.Instantiate(flowerObject.gameObject, icon);
                            go.name = $"{Data.GridPos.x + i}-{Data.GridPos.y}";
                            go.transform.localPosition = new(elementSize * i * 1.0f + flowerObject.localPosition.x,
                                flowerObject.localPosition.y, 0);
                            _flowerMap.Add(new Vector2Int(Data.GridPos.x + i, Data.GridPos.y), go);
                        }
                    }

                    if (Data.Direction == ElementDirection.Up || Data.Direction == ElementDirection.Down)
                    {
                        icon.GetComponent<SpriteRenderer>().size =
                            new Vector2(elementSize, elementSize * Data.EliminateCount);
                    }
                    else if (Data.Direction == ElementDirection.Right || Data.Direction == ElementDirection.Left)
                    {
                        icon.GetComponent<SpriteRenderer>().size =
                            new Vector2(elementSize * Data.EliminateCount, elementSize);
                    }

                    BuildSideMap();
                }
            }
        }

        private void BuildSideMap()
        {
            foreach (var flower in _flowerMap)
            {
                Vector2Int dir = default;
                switch (Data.Direction)
                {
                    case ElementDirection.Up:
                        dir = new Vector2Int(1, 0);
                        break;
                    case ElementDirection.Down:
                        dir = new Vector2Int(-1, 0);
                        break;
                    case ElementDirection.Left:
                        dir = new Vector2Int(0, 1);
                        break;
                    case ElementDirection.Right:
                        dir = new Vector2Int(0, -1);
                        break;
                }

                Vector2Int coord = dir + flower.Key;
                _flowerSideCoordsMap.Add(flower.Key, new HashSet<Vector2Int>() { coord });
            }
        }
    }
}