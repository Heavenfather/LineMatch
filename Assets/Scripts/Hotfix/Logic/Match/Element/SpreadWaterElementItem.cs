using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    public class SpreadWaterElementItem : ElementBase
    {
        private HashSet<Vector2Int> _infectCoords = new HashSet<Vector2Int>();
        private List<DeleteGridInfo> _willDelCoords;
        private Queue<Vector2Int> _searchQueue = new Queue<Vector2Int>(100);
        private HashSet<Vector2Int> _visited = new HashSet<Vector2Int>();
        private ElementMapDB _db => ConfigMemoryPool.Get<ElementMapDB>();
        private GridWaterLine _waterLine;
        private GridWaterShadow _waterShadow;

        protected override void OnInitialized()
        {
            _waterLine = GameObject.transform.Find("WaterLine").GetComponent<GridWaterLine>();
            _waterLine.gameObject.SetActive(false);
            _waterLine.ResetLine();

            _waterShadow = GameObject.transform.Find("WaterShadow").GetComponent<GridWaterShadow>();
            _waterShadow.SetGridPos(Data.GridPos);
            _waterShadow.HideAllShadow();
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            _infectCoords.Clear();
            _willDelCoords = context.WillDelCoords;
            if (_willDelCoords != null && _willDelCoords.Count > 0)
            {
                InitializeInfection(context);
                SpreadInfection(context);
            }

            int baseScore = _db[Data.ConfigId].score;
            MatchManager.Instance.AddScore(baseScore * _infectCoords.Count);
            context.AddInfectElement(Data.ConfigId, _infectCoords);
            ProcessInfectElement(context);
            return false;
        }

        public override bool CanMove()
        {
            return false;
        }

        private void InitializeInfection(ElementDestroyContext context)
        {
            _searchQueue.Clear();
            _visited.Clear();

            //第一次先初始化传染目标
            foreach (var oriCoord in _willDelCoords)
            {
                if (!oriCoord.DelStyle.Contains(EliminateStyle.Side))
                    continue;
                if(!HaveWater(oriCoord.Coord))
                    continue;
                foreach (var dir in ValidateManager.Instance.NeighborDirs)
                {
                    Vector2Int coord = oriCoord.Coord + dir;
                    if (_willDelCoords.FindIndex(x => x.Coord == coord) >= 0 && CanInfect(coord, context))
                    {
                        _searchQueue.Enqueue(coord);
                        _visited.Add(coord);
                        _infectCoords.Add(coord);
                    }
                }
            }
        }

        private bool HaveWater(Vector2Int coord)
        {
            var elements = ElementSystem.Instance.GetGridElements(coord, true);
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Data.ElementType == ElementType.SpreadWater)
                    return true;
            }

            return false;
        }

        private void SpreadInfection(ElementDestroyContext context)
        {
            int safetyCounter = 0;
            const int maxOperations = 1000;

            while (_searchQueue.Count > 0 && safetyCounter++ < maxOperations)
            {
                Vector2Int current = _searchQueue.Dequeue();
                //四方向检测，持续找出会被传染的目标
                foreach (var dir in ValidateManager.Instance.NeighborDirs)
                {
                    Vector2Int neighbor = current + dir;

                    if (_willDelCoords.FindIndex(x => x.Coord == neighbor) >= 0 && CanInfect(neighbor, context) &&
                        _visited.Add(neighbor))
                    {
                        _infectCoords.Add(neighbor);
                        _searchQueue.Enqueue(neighbor);
                    }
                }
            }
        }

        private bool CanInfect(Vector2Int coord, ElementDestroyContext context)
        {
            if (!context.GridSystem.IsValidPosition(coord.x, coord.y))
                return false;
            var elements = ElementSystem.Instance.GetGridElements(coord, false);
            if (elements != null && elements.Count > 0)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if ((elements[i].Data.ElementType != ElementType.Normal &&
                         !ElementSystem.Instance.IsSpecialElement(elements[i].Data.ElementType)) ||
                        elements[i].Data.ElementType == ElementType.SpreadWater)
                        return false;
                }
            }

            return true;
        }

        private void ProcessInfectElement(ElementDestroyContext context)
        {
            Dictionary<int, HashSet<Vector2Int>> infectMap = context.InfectElements;
            if (infectMap is not { Count: > 0 })
                return;
                
            //传染元素到指定的格子上
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            foreach (var infectInfo in infectMap)
            {
                int elementId = infectInfo.Key;
                HashSet<Vector2Int> infectCoords = infectInfo.Value;
                foreach (var coord in infectCoords)
                {
                    var grid = context.GridSystem.GetGridByCoord(coord);
                    if (grid == null)
                        continue;
                    if (grid.Data.IsWhite)
                        continue;
                    ref readonly ElementMap config = ref db[elementId];
                    if (config.elementType == ElementType.SpreadWater)
                    {
                        InfectWater(context, elementId, coord, grid);
                    }
                }
            }
        }

        private void InfectWater(ElementDestroyContext context, int elementId, Vector2Int coord, GridItem grid)
        {
            if (!context.GridSystem.IsValidPosition(coord.x, coord.y))
                return;

            var elements = ElementSystem.Instance.GetGridElements(coord, true);
            if (elements != null && elements.Count > 0)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Data.ElementType == ElementType.SpreadWater)
                        return;
                }
            }

            var data = ElementSystem.Instance.GenElementItemData(elementId, coord.x, coord.y);
            var genElement = ElementSystem.Instance.GenElement(data, grid.GameObject.transform);
            grid.PushElement(genElement, false);
            genElement.GameObject.transform.localPosition = Vector3.zero;
            (genElement as SpreadWaterElementItem).SetIconVisible(false);

            context.AddCalAddedCount(elementId, 1);
            context.AddWaterCoord(coord);
        }

        public void FlowWater(ElementDirection dir, bool immediately = false)
        {
            var curPos = GameObject.transform.position;
            var flowPos = curPos;
            switch (dir)
            {
                case ElementDirection.Left:
                    flowPos.x -= 0.8f;
                    break;
                case ElementDirection.Right:
                    flowPos.x += 0.8f;
                    break;
                case ElementDirection.Up:
                    flowPos.y -= 0.8f;
                    break;
                case ElementDirection.Down:
                    flowPos.y += 0.8f;
                    break;
            }

            _waterLine.gameObject.SetActive(true);
            _waterLine.SetMovePosition(dir, flowPos, immediately);
            _waterShadow.SetWaterFlow(dir);
        }

        public void SetIconVisible(bool visible)
        {
            GameObject.transform.Find("Icon").gameObject.SetActive(visible);
        }

        public void UpdateWaterShadow(List<Vector2Int> gridCoords)
        {
            _waterShadow.UpdateShadow(gridCoords);
        }
    }

    public class DrawSpreadWaterUtil
    {
        private static Dictionary<Vector2Int, SpreadWaterElementItem> _waterDict =
            new Dictionary<Vector2Int, SpreadWaterElementItem>();

        private static Dictionary<SpreadWaterElementItem, List<SpreadWaterElementItem>> _waterLinkDict =
            new Dictionary<SpreadWaterElementItem, List<SpreadWaterElementItem>>();

        public static void InitWater()
        {
            Clear();

            var waterList = ElementSystem.Instance.GetAllTargetElements(ElementType.SpreadWater);
            foreach (var water in waterList)
            {
                _waterDict.Add(water.Data.GridPos, (SpreadWaterElementItem)water);
            }

            if (waterList.Count > 0)
            {
                foreach (var water in waterList)
                {
                    var waterElement = water as SpreadWaterElementItem;
                    WaterLinkNeighbour(waterElement);
                }
            }

            UpdateWaterShadow();
        }

        public static async UniTask DoFlowWater(ElementDestroyContext context)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

            var newWaterCoords = new List<Vector2Int>(context.NewWaterCoords);
            var newWaterDict = new Dictionary<Vector2Int, SpreadWaterElementItem>();
            var flowObjList = new List<List<SpreadWaterElementItem>>();

            var newWaterCount = 0;
            while (true)
            {
                // 流水动画的item，既流水的发起位置
                List<SpreadWaterElementItem> waterFlowList = new List<SpreadWaterElementItem>();

                newWaterCount = newWaterCoords.Count;

                for (int i = newWaterCoords.Count - 1; i >= 0; i--)
                {
                    var neighborPos = GetNeighborPos(newWaterCoords[i]);
                    foreach (var pos in neighborPos)
                    {
                        var isFind = false;

                        if (_waterDict.ContainsKey(pos) || newWaterDict.ContainsKey(pos))
                        {
                            var elements = ElementSystem.Instance.GetGridElements(newWaterCoords[i], true);
                            if (elements != null && elements.Count > 0)
                            {
                                foreach (var element in elements)
                                {
                                    if (element is SpreadWaterElementItem waterItem)
                                    {
                                        
                                        waterFlowList.Add(waterItem);

                                        // 记录流水动画的item
                                        newWaterDict.Add(newWaterCoords[i], waterItem);
                                        newWaterCoords.RemoveAt(i);
                                        isFind = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (isFind) break;
                    }
                }

                if (newWaterCount == newWaterCoords.Count)
                {
                    Logger.Debug("未找到可连接的水");
                    foreach (var pos in newWaterCoords)
                    {
                        var elements = ElementSystem.Instance.GetGridElements(pos, true);
                        if (elements != null && elements.Count > 0)
                        {
                            foreach (var element in elements)
                            {
                                if (element is SpreadWaterElementItem waterItem)
                                {
                                    waterFlowList.Add(waterItem);
                                    newWaterDict.Add(pos, waterItem);
                                    break;
                                }
                            }
                        }
                    }

                    // 没有找到水流的位置，应该是出问题了，直接跳出
                    flowObjList.Add(waterFlowList);
                    break;
                }

                if (waterFlowList.Count > 0)
                {
                    flowObjList.Add(waterFlowList);
                }

                if (newWaterCoords.Count == 0) break;
            }

            foreach (var waterElements in flowObjList)
            {

                foreach (var waterElement in waterElements)
                {
                    var waterItem = waterElement;
                    var waterGridPos = waterItem.Data.GridPos;
                    var neighborPos = GetNeighborPos(waterGridPos);
                    for (int i = 0; i < neighborPos.Count; i++)
                    {
                        var pos = neighborPos[i];
                        var dir = ElementDirection.None;
                        if (i == 0)
                        {
                            dir = ElementDirection.Down;
                        }
                        else if (i == 1)
                        {
                            dir = ElementDirection.Right;
                        }
                        else if (i == 2)
                        {
                            dir = ElementDirection.Left;
                        }
                        else if (i == 3)
                        {
                            dir = ElementDirection.Up;
                        }

                        if (_waterDict.ContainsKey(pos))
                        {
                            var flowItem = _waterDict[pos];
                            flowItem.FlowWater(dir);

                            _waterDict[waterGridPos] = waterItem;
                            BindWaterFlowDict(flowItem, waterItem);
                            break;
                        }
                    }

                    // 有可能会找不到
                    if (!_waterDict.ContainsKey(waterGridPos)) {
                        _waterDict[waterGridPos] = waterItem;
                    }
                }

                

                await UniTask.Delay(TimeSpan.FromSeconds(0.23f));

                foreach (var waterElement in waterElements)
                {
                    WaterLinkNeighbour(waterElement, true); 
                }
            }

            foreach (var waterPos in context.NewWaterCoords)
            {
                var elements = ElementSystem.Instance.GetGridElements(waterPos, true);
                if (elements != null && elements.Count > 0)
                {
                    foreach (var element in elements)
                    {
                        if (element is SpreadWaterElementItem waterItem)
                        {
                            waterItem.SetIconVisible(true);
                            break;
                        }
                    }
                }
            }

            UpdateWaterShadow();
        }

        public static void Clear()
        {
            _waterDict.Clear();
            _waterLinkDict.Clear();
        }

        private static void UpdateWaterShadow()
        {
            foreach (var waterPos in _waterDict.Keys)
            {
                var neighborPos = GetNeighborPos(waterPos);
                var waterPosList = new List<Vector2Int>();
                foreach (var pos in neighborPos)
                {
                    if (_waterDict.ContainsKey(pos))
                    {
                        waterPosList.Add(pos);
                    }
                }

                var waterElement = _waterDict[waterPos];
                waterElement.UpdateWaterShadow(waterPosList);
            }
        }

        private static void WaterLinkNeighbour(SpreadWaterElementItem waterElement, bool isFlow = false)
        {
            var neighborPos = GetNeighborPos(waterElement.Data.GridPos);

            var upPos = neighborPos[0];
            if (_waterDict.ContainsKey(upPos) && !WaterIsLine(waterElement, _waterDict[upPos]))
            {
                waterElement.FlowWater(ElementDirection.Up, isFlow);
                BindWaterFlowDict(waterElement, _waterDict[upPos]);
            }

            var leftPos = neighborPos[1];
            if (_waterDict.ContainsKey(leftPos) && !WaterIsLine(waterElement, _waterDict[leftPos]))
            {
                waterElement.FlowWater(ElementDirection.Left, isFlow);
                BindWaterFlowDict(waterElement, _waterDict[leftPos]);
            }


            var rightPos = neighborPos[2];
            if (_waterDict.ContainsKey(rightPos) && !WaterIsLine(waterElement, _waterDict[rightPos]))
            {
                waterElement.FlowWater(ElementDirection.Right, isFlow);
                BindWaterFlowDict(waterElement, _waterDict[rightPos]);
            }

            var downPos = neighborPos[3];
            if (_waterDict.ContainsKey(downPos) && !WaterIsLine(waterElement, _waterDict[downPos]))
            {
                waterElement.FlowWater(ElementDirection.Down, isFlow);
                BindWaterFlowDict(waterElement, _waterDict[downPos]);
            }
        }

        private static void BindWaterFlowDict(SpreadWaterElementItem water, SpreadWaterElementItem dirElement)
        {
            if (!_waterLinkDict.ContainsKey(water))
            {
                _waterLinkDict.Add(water, new List<SpreadWaterElementItem>());
            }

            if (!_waterLinkDict[water].Contains(dirElement))
            {
                _waterLinkDict[water].Add(dirElement);
            }
        }

        private static bool WaterIsLine(SpreadWaterElementItem water1, SpreadWaterElementItem water2)
        {
            if (water1 == null || water2 == null) return false;

            if (!_waterLinkDict.ContainsKey(water2)) return false;

            return _waterLinkDict[water2].Contains(water1);
        }
        
        

        // 获取四个方向的位置，顺序为：上左右下
        private static List<Vector2Int> GetNeighborPos(Vector2Int gridPos)
        {
            List<Vector2Int> neighbor = new List<Vector2Int>();

            for (int i = 0; i < ValidateManager.Instance.NeighborDirs.Length; i++)
            {
                var dir = gridPos + ValidateManager.Instance.NeighborDirs[i];
                neighbor.Add(dir);
            }

            return neighbor;
        }
    }
}