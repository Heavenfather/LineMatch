using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    public class ValidateSquare : IValidate
    {
        public const int GenConditionCount = 3;
        public const int GenColoBallConditionCount = 4;
        private int _matchElementId;
        private int _matchCount;

        public void Validate(ElementDestroyContext context, List<GridItem> gridItems, Action<bool> callback)
        {
            int count = gridItems.Count;
            var baseEle = gridItems[0].GetSelectElement(true);
            _matchElementId = baseEle.Data.ConfigId;
            context.AddCurrentEffectId(_matchElementId);
            context.FilterElementId = _matchElementId;
            var allElements = ElementSystem.Instance.GridElements;

            _matchCount = 0;

            //收集同类型元素
            foreach (var coord in allElements.Keys)
            {
                var elements = ElementSystem.Instance.GetGridElements(coord, false);
                bool result = ElementSystem.Instance.TryGetBaseElement(elements, out int index);

                if (result && index >= 0 && elements[index].Data.ConfigId == _matchElementId)
                {
                    _matchCount++;
                    context.AddEffGridCoord(coord);
                    context.AddWillDelCoord(coord, EliminateStyle.Match, elements[index].Data.UId);
                }
            }

            var effBlockCoords = ValidateManager.Instance.FindEffBlockCoords(context.EffGridCoords, context);
            if (effBlockCoords.Count > 0)
            {
                foreach (var coord in effBlockCoords)
                {
                    var sideElements = ElementSystem.Instance.GetGridElements(coord, true);
                    if (sideElements is { Count: > 0 })
                    {
                        context.AddEffGridCoord(coord);
                        context.AddWillDelCoord(coord, EliminateStyle.Side, sideElements[^1].Data.UId);
                    }
                }
            }

            if (ElementSystem.Instance.IsSpecialElement(baseEle.Data.ElementType))
            {
                CheckSpecialElement(context, gridItems);
            }
            else
            {
                CheckGenSpecialElement(context, gridItems);
            }

            context.GenSquareElementId = _matchElementId;
            G.EventModule.DispatchEvent(GameEventDefine.OnDestroyTargetListElement,
                EventOneParam<ElementDestroyContext>.Create(context));
            callback?.Invoke(true);

            G.EventModule.DispatchEvent(GameEventDefine.OnMatchDestroySquare,
                EventOneParam<int>.Create(count));
        }

        private void CheckSpecialElement(ElementDestroyContext context, List<GridItem> gridItems)
        {
            var element = gridItems[0].GetSelectElement(true);
            if (element == null)
                return;

            //按照配置id排序 最终消除效果只取级别最高的两个 级别：彩球>炸弹>火箭
            if (gridItems.Count > 2)
            {
                gridItems.Sort((a, b) =>
                {
                    var aElement = a.GetSelectElement(true);
                    var bElement = b.GetSelectElement(true);

                    if (aElement.Data.Priority > bElement.Data.Priority)
                        return -1;
                    if (aElement.Data.Priority < bElement.Data.Priority)
                        return 1;
                    return 0;
                });
                foreach (var item in gridItems)
                {
                    var ele = item.GetSelectElement(true);
                    context.AddCurrentEffectId(ele.Data.ConfigId);
                }
            }

            var firstElement = gridItems[0].GetSelectElement(true);
            var secondElement = gridItems[1].GetSelectElement(true);
            ValidateManager.Instance.InvokeComposeElement(context, firstElement, secondElement);
        }

        private void CheckGenSpecialElement(ElementDestroyContext context, List<GridItem> items)
        {
            var db = ConfigMemoryPool.Get<BlockDiffScoreDB>();

            List<GridItem> closedLoop = GetFinalClosedLoopItems(items);
    
            // 如果没有形成闭环，或者闭环的周长过小，则只计算基础分数。
            if (closedLoop == null || closedLoop.Count < 4)
            {
                int score = db.CalScore(_matchElementId, _matchCount, OneTakeScoreType.FourRect);
                MatchManager.Instance.AddScore(score);
                return;
            }
            
            int minX = closedLoop[0].Data.Coord.x;
            int minY = closedLoop[0].Data.Coord.y;
            int maxX = closedLoop[0].Data.Coord.x;
            int maxY = closedLoop[0].Data.Coord.y;

            for (int i = 1; i < closedLoop.Count; i++)
            {
                var coord = closedLoop[i].Data.Coord;
                minX = Mathf.Min(minX, coord.x);
                minY = Mathf.Min(minY, coord.y);
                maxX = Mathf.Max(maxX, coord.x);
                maxY = Mathf.Max(maxY, coord.y);
            }

            MatchGameType matchGameType = MatchManager.Instance.CurrentMatchGameType;
            if (matchGameType == MatchGameType.TowDots)
            {
                var enclosedItems = GetEnclosedGridItems(context,closedLoop, minY, maxY);
                if (enclosedItems is { Count: > 0 })
                {
                    for (int i = 0; i < enclosedItems.Count; i++)
                    {
                        context.AddSpecialElement(ElementType.Bomb, enclosedItems[i]);
                    }

                    context.IsAutoReleaseBomb = true;
                }
                int score = db.CalScore(_matchElementId, _matchCount, OneTakeScoreType.FourRect);
                MatchManager.Instance.AddScore(score);
                return;
            }
    
            int width = maxX - minX + 1; // 包围盒宽度 (列数)
            int height = maxY - minY + 1; // 包围盒高度 (行数)
            Vector2Int genCoord = closedLoop[0].Data.Coord; 
            
            var ruleDb = ConfigMemoryPool.Get<SpecialEleRuleDB>();
            var matchRule = ruleDb.Match(width, height);
            if (matchRule.HasValue == false)
            {
                int score = db.CalScore(_matchElementId, _matchCount, OneTakeScoreType.FourRect);
                MatchManager.Instance.AddScore(score);
                return;
            }
            var rule = matchRule.Value;
            ElementType finalType = rule.resultElement;
            //特殊处理火箭方向
            if(rule.resultElement == ElementType.Rocket)
                finalType = ruleDb.GetFinalRocketType(width, height, rule);
            int addScore = db.CalScore(_matchElementId, _matchCount, rule.scoreType);
            MatchManager.Instance.AddScore(addScore);
            context.AddSpecialElement(finalType, genCoord);
            TaskManager.Instance.AddTaskCalculate((TaskTag)rule.taskTag);
        }

        private List<GridItem> GetFinalClosedLoopItems(List<GridItem> items)
        {
            if (items == null || items.Count < 4) // 至少需要4个格子才能形成闭环
                return null;
    
            // 获取最后一个格子（当前点）
            GridItem lastGrid = items[^1];
            Vector2Int lastCoord = lastGrid.Data.Coord;
    
            // 排除最后两个格子是因为最后两个格子总是相邻的（连线路径）
            for (int i = 0; i < items.Count - 2; i++)
            {
                GridItem grid = items[i];
        
                // 检查当前格子是否与最后一个格子是邻居
                if (grid.IsNeighbor(lastCoord))
                {
                    // 找到闭环，生成闭环格子数组
                    List<GridItem> closedLoopGrids = new List<GridItem>();
            
                    // 添加从回连点到最后一个格子的所有格子
                    for (int j = i; j < items.Count; j++)
                    {
                        closedLoopGrids.Add(items[j]);
                    }
            
                    return closedLoopGrids;
                }
            }
    
            return null; // 未找到闭环
        }

        private List<Vector2Int> GetEnclosedGridItems(ElementDestroyContext context,List<GridItem> closedLoopGrids, int minY, int maxY)
        {
            var enclosedItems = new List<Vector2Int>();
            if (closedLoopGrids == null || closedLoopGrids.Count < 4)
            {
                return enclosedItems;
            }

            var boundaryCoords = new HashSet<Vector2Int>(closedLoopGrids.Count);
            for (int i = 0; i < closedLoopGrids.Count; i++)
            {
                boundaryCoords.Add(closedLoopGrids[i].Data.Coord);
            }

            //逐行扫描
            for (int y = minY; y <= maxY; y++)
            {
                var intersectionsX = new List<int>();

                // 计算当前扫描线与多边形边的交点
                for (int i = 0; i < closedLoopGrids.Count; i++)
                {
                    Vector2Int p1 = closedLoopGrids[i].Data.Coord;
                    Vector2Int p2 = closedLoopGrids[(i + 1) % closedLoopGrids.Count].Data.Coord;

                    if (p1.y <= y && p2.y > y && p1.y != p2.y)
                    {
                        float t = (float)(y - p1.y) / (p2.y - p1.y);
                        float ix = p1.x + t * (p2.x - p1.x);
                        intersectionsX.Add(Mathf.RoundToInt(ix));
                    }
                    else if (p2.y <= y && p1.y > y && p1.y != p2.y)
                    {
                        float t = (float)(y - p2.y) / (p1.y - p2.y);
                        float ix = p2.x + t * (p1.x - p2.x);
                        intersectionsX.Add(Mathf.RoundToInt(ix));
                    }
                    else if (p1.y == p2.y && p1.y == y)
                    {
                        int minX = Mathf.Min(p1.x, p2.x);
                        int maxX = Mathf.Max(p1.x, p2.x);
                        for (int x = minX; x <= maxX; x++)
                        {
                            Vector2Int pt = new Vector2Int(x, y);
                            boundaryCoords.Add(pt); // 标记水平段作为边界点
                        }
                    }
                }
                
                intersectionsX.Sort();
                for (int k = 0; k < intersectionsX.Count; k += 2)
                {
                    if (k + 1 >= intersectionsX.Count) break;
    
                    int startX = intersectionsX[k];
                    int endX = intersectionsX[k + 1];

                    for (int x = startX; x < endX; ++x)
                    {
                        Vector2Int pt = new Vector2Int(x, y);
                        if (!boundaryCoords.Contains(pt))
                        {
                            var elements = ElementSystem.Instance.GetGridElements(pt, false);
                            if(elements == null || elements.Count == 0)
                                continue;
                            if (ElementSystem.Instance.TryGetBaseElement(elements, out var index))
                            {
                                context.AddCalAddedCount(elements[index].Data.ConfigId, 1);
                                enclosedItems.Add(pt);
                            }
                        }
                    }
                }
            }

            return enclosedItems;
        }
    }
}