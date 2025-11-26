using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using DG.Tweening;
using GameConfig;
using GameCore.Singleton;
using Hotfix.Define;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    public class ShuffleElementManager : LazySingleton<ShuffleElementManager>
    {
        private LevelData _levelData;
        private GridSystem _gridSystem;

        public void Initialized(LevelData levelData, GridSystem gridSystem)
        {
            _levelData = levelData;
            _gridSystem = gridSystem;
        }


        /// <summary>
        /// 当前棋盘是否有可消除棋子
        /// </summary>
        /// <returns></returns>
        public bool IsBoardHasPair()
        {
            //全是掉落特殊棋子，不可能没有配对
            if (_levelData != null)
            {
                if (_levelData.dropColor != null)
                {
                    for (int i = 0; i < _levelData.dropColor.Length; i++)
                    {
                        if (ElementSystem.Instance.IsSpecialElement(_levelData.dropColor[i]))
                        {
                            return true;
                        }
                    }
                }
                
                for (int x = 0; x < _levelData.gridCol; x++)
                {
                    for (int y = 0; y < _levelData.gridRow; y++)
                    {
                        if (_levelData.grid[x][y].isWhite)
                            continue;
                        var elements = ElementSystem.Instance.GetGridElements(new Vector2Int(x, y), false);
                        if (elements == null || elements.Count <= 0)
                            continue;
                        //是否有锁住的障碍物
                        bool haveBase = ElementSystem.Instance.TryGetBaseElement(elements, out var baseIndex, true);

                        if (!haveBase)
                            continue;

                        for (int i = 0; i < elements.Count; i++)
                        {
                            //检测到有特殊棋子 就表示有可消除的了
                            if (ElementSystem.Instance.IsSpecialElement(elements[i].Data.ElementType, true) &&
                                !ElementSystem.Instance.HaveOverElementLock(elements,elements[i].Data.ConfigId,out int _))
                            {
                                return true;
                            }

                            if (x < _levelData.gridRow && IsPair(x + 1, y, elements[i].Data))
                            {
                                return true;
                            }

                            if (y < _levelData.gridCol && IsPair(x, y + 1, elements[i].Data))
                            {
                                return true;
                            }
                        }
                    }
                } 
            }
            
            return false;
        }

        /// <summary>
        /// 棋盘洗牌
        /// </summary>
        public void ShuffleBoard(List<ElementBase> shuffleList, bool forceShuffle = false)
        {
            // 记录位置
            Dictionary<Vector2Int, ElementBase> coordsDict = new Dictionary<Vector2Int, ElementBase>();

            // 记录基础元素
            Dictionary<int, List<ElementBase>> elementIdDict = new Dictionary<int, List<ElementBase>>();

            for (int i = 0; i < shuffleList.Count; i++) {
                var element = shuffleList[i];
                if (!coordsDict.ContainsKey(element.Data.GridPos)) {
                    coordsDict.Add(element.Data.GridPos, element);
                }

                if (element.Data.ElementType == ElementType.Normal) {
                    if (!elementIdDict.ContainsKey(element.Data.ConfigId)) {
                        elementIdDict.Add(element.Data.ConfigId, new List<ElementBase>());
                    }
                    elementIdDict[element.Data.ConfigId].Add(element);
                }
            }

            var random = new System.Random();

            // 四方格的坐标
            List<Vector2Int> squareCoords = new List<Vector2Int>();
            var coordList = coordsDict.Keys.OrderBy(n => random.Next()).ToList();
            for (int i = 0; i < coordsDict.Count; i++)
            {
                var coord = coordList[i];

                var leftCoord = new Vector2Int(coord.x - 1, coord.y);
                if (!coordsDict.ContainsKey(leftCoord)) continue;

                var upCoord = new Vector2Int(coord.x, coord.y - 1);
                if (!coordsDict.ContainsKey(upCoord)) continue;

                var leftUpCoord = new Vector2Int(coord.x - 1, coord.y - 1);
                if (!coordsDict.ContainsKey(leftUpCoord)) continue;


                squareCoords.Add(coord);
                squareCoords.Add(leftCoord);
                squareCoords.Add(upCoord);
                squareCoords.Add(leftUpCoord);

                break;
            }

            if (squareCoords.Count != 4) {
                // 棋盘上没有四方格的坐标，直接洗牌
                RandomShuffleBoard(shuffleList, forceShuffle);
                return;
            }


            var elementCoordList = elementIdDict.Keys.OrderBy(n => random.Next()).ToList();
            for (int i = elementCoordList.Count - 1; i >= 0; i--) {
                if (elementIdDict[elementCoordList[i]].Count < 4) {
                    elementIdDict.Remove(elementCoordList[i]);
                    elementCoordList.RemoveAt(i);
                }
            }

            if (elementCoordList.Count <= 0) {
                // 没有四个相同颜色的元素，直接洗牌
                RandomShuffleBoard(shuffleList, forceShuffle);
                return;
            }

            // 四方格的棋子
            var squareList = new List<ElementBase>();
            squareList.AddRange(elementIdDict[elementCoordList[0]].GetRange(0, 4));

            // 交换位置的棋子
            var changeElements = new List<ElementBase>();
            foreach (var coord in squareCoords) {
                // Logger.Debug($"交换四方格位置({coord})");
                changeElements.Add(coordsDict[coord]);
            }

            for (int i = 0; i < squareList.Count; i++) {
                var element1 = squareList[i];
                var element2 = changeElements[i];
                SwapElementEntity(element1, element2);

                shuffleList.Remove(element1);
            }


            var moveElements = new List<ElementBase>();
            moveElements.AddRange(squareList);
            moveElements.AddRange(changeElements);
            ExecuteMoveAnimations(moveElements);

            // 剩余棋子洗牌
            RandomShuffleBoard(shuffleList, forceShuffle);
        }

        private void RandomShuffleBoard(List<ElementBase> shuffleList, bool forceShuffle = false) {
            const int maxShuffleAttempts = 10;
            bool validBoard = false;
            for (int attempt = 0; attempt < maxShuffleAttempts; attempt++)
            {
                // 执行Fisher-Yates洗牌
                FisherYatesShuffle(shuffleList);

                // 检查是否存在配对
                if (!forceShuffle && CheckBoardHasValidPair())
                {
                    validBoard = true;
                    Logger.Info($"第{attempt + 1}次洗牌后找到有效配对");
                    break;
                }
            }

            if (!validBoard && !forceShuffle)
            {
                Logger.Warning("常规洗牌未找到有效配对，尝试修复...");
                AttemptPairFix(shuffleList);

                // 修复后再次检查
                if (!CheckBoardHasValidPair())
                {
                    Logger.Warning("修复未成功，强制创建配对");
                    ForceCreateValidPair();
                }
            }

            ExecuteMoveAnimations(shuffleList);
        }

        /// <summary>
        /// 检测可洗牌的元素是否可以通过洗牌形成配对
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public bool CheckIsCanShuffle(List<ElementBase> elements)
        {
            // 首先检查是否有相邻的元素(不论类型)
            for (int i = 0; i < elements.Count; i++)
            {
                for (int j = i + 1; j < elements.Count; j++)
                {
                    if (IsNeighbor(elements[i].Data.GridPos, elements[j].Data.GridPos))
                    {
                        // 相邻元素存在，再检查是否有相同ConfigId的元素对
                        Dictionary<int, int> configCount = new Dictionary<int, int>();
                        foreach (var element in elements)
                        {
                            if (!configCount.TryAdd(element.Data.ConfigId, 1))
                            {
                                configCount[element.Data.ConfigId]++;
                                if (configCount[element.Data.ConfigId] >= 2)
                                {
                                    return true;
                                }
                            }
                        }

                        break;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 强制转换元素
        /// </summary>
        /// <param name="shuffleList"></param>
        public void ForceChangeElement(List<ElementBase> shuffleList)
        {
            //先看列表里面是否有相邻的棋子，没有的话可以直接当做失败了
            bool hasNeighborPair = false;
            int index = -1;
            for (int i = 0; i < shuffleList.Count - 1; i++)
            {
                for (int j = i + 1; j < shuffleList.Count; j++)
                {
                    if (IsNeighbor(shuffleList[i].Data.GridPos, shuffleList[j].Data.GridPos))
                    {
                        hasNeighborPair = true;
                        index = i;
                        break;
                    }
                }

                if (hasNeighborPair) break;
            }

            if (!hasNeighborPair)
            {
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchNoneMatchToFail);
                return;
            }

            ElementBase baseElement = shuffleList[index];
            int elementId = baseElement.Data.ConfigId;
            foreach (var ele in shuffleList)
            {
                if (ele.Data.UId == baseElement.Data.UId)
                    continue;
                if (IsNeighbor(ele.Data.GridPos, baseElement.Data.GridPos))
                {
                    Logger.Debug($"强制转换({ele.Data.GridPos})元素 {ele.Data.ConfigId}=>{elementId}");
                    var gridItem = _gridSystem.GetGridByCoord(ele.Data.GridPos);
                    var eleData =
                        ElementSystem.Instance.GenElementItemData(elementId, ele.Data.GridPos.x, ele.Data.GridPos.y);
                    var element = ElementSystem.Instance.GenElement(eleData, gridItem.GameObject.transform);
                    element.GameObject.transform.localPosition = Vector3.zero;
                    gridItem.PushElement(element, doDestroy: true);
                    break;
                }
            }
        }

        /// <summary>
        /// 交换两个元素实体
        /// </summary>
        public void SwapElementEntity(ElementBase a, ElementBase b)
        {
            var gridA = _gridSystem.GetGridByCoord(a.Data.GridPos);
            var gridB = _gridSystem.GetGridByCoord(b.Data.GridPos);

            if (gridA == null || gridB == null)
            {
                return;
            }
            
            // 从原位置移除
            gridA.RemoveElement(a);
            gridB.RemoveElement(b);

            // 添加到新位置
            gridA.PushElement(b, false);
            gridB.PushElement(a, false);
        }

        /// <summary>
        /// 收集所有可洗牌的元素
        /// </summary>
        /// <returns></returns>
        public List<ElementBase> CollectShuffleElements()
        {
            List<ElementBase> shuffleList = new();
            var allElements = ElementSystem.Instance.GridElements.Keys;
            foreach (var coord in allElements)
            {
                var elements = ElementSystem.Instance.GetGridElements(coord, false);
                bool result = ElementSystem.Instance.TryGetBaseElement(elements, out var elementIndex, true);

                if (result && elementIndex >= 0)
                {
                    shuffleList.Add(elements[elementIndex]);
                }
            }
            
            return shuffleList;
        }

        /// <summary>
        /// Fisher-Yates洗牌算法实现
        /// </summary>
        private void FisherYatesShuffle(List<ElementBase> list)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                int r = Random.Range(i, count);
                SafeSwapElements(list[i], list[r]);
            }
        }

        /// <summary>
        /// 安全交换元素位置
        /// </summary>
        private void SafeSwapElements(ElementBase a, ElementBase b)
        {
            // 相同类型元素无需交换
            if (a.Data.ConfigId == b.Data.ConfigId) return;

            // 更新网格索引
            SwapElementEntity(a, b);
        }

        /// <summary>
        /// 检查棋盘是否存在有效配对
        /// </summary>
        private bool CheckBoardHasValidPair()
        {
            var allElements = ElementSystem.Instance.GridElements;

            foreach (var pos in allElements.Keys)
            {
                if (!_gridSystem.IsValidPosition(pos.x, pos.y))
                    continue;

                // 检查四个方向的配对
                if (CheckPairAtPosition(pos, Vector2Int.right) ||
                    CheckPairAtPosition(pos, Vector2Int.down) ||
                    CheckPairAtPosition(pos, Vector2Int.left) ||
                    CheckPairAtPosition(pos, Vector2Int.up))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 检查特定方向配对
        /// </summary>
        private bool CheckPairAtPosition(Vector2Int basePos, Vector2Int offset)
        {
            Vector2Int neighborPos = basePos + offset;

            if (!_gridSystem.IsValidPosition(basePos.x, basePos.y) ||
                !_gridSystem.IsValidPosition(neighborPos.x, neighborPos.y))
                return false;

            var baseElements = ElementSystem.Instance.GetGridElements(basePos, true);
            var neighborElements = ElementSystem.Instance.GetGridElements(neighborPos, true);

            if (baseElements == null || neighborElements == null)
                return false;

            bool aResult = ElementSystem.Instance.TryGetBaseElement(baseElements, out var aIndex, true);
            if (!aResult)
                return false;
            bool bResult = ElementSystem.Instance.TryGetBaseElement(neighborElements, out var bIndex, true);
            if (!bResult)
                return false;
            if (baseElements[aIndex].Data.ElementType == ElementType.ColorBall &&
                (neighborElements[bIndex].Data.ElementType == ElementType.Normal ||
                 ElementSystem.Instance.IsSpecialElement(neighborElements[bIndex].Data.ElementType)))
            {
                return true;
            }

            if (neighborElements[bIndex].Data.ElementType == ElementType.ColorBall &&
                (baseElements[aIndex].Data.ElementType == ElementType.Normal ||
                 ElementSystem.Instance.IsSpecialElement(baseElements[aIndex].Data.ElementType)))
            {
                return true;
            }

            if (ElementSystem.Instance.IsSpecialElement(baseElements[aIndex].Data.ElementType, true))
            {
                return true;
            }

            if (ElementSystem.Instance.IsSpecialElement(neighborElements[bIndex].Data.ElementType, true))
            {
                return true;
            }

            return baseElements[aIndex].Data.ConfigId == neighborElements[bIndex].Data.ConfigId;
        }

        /// <summary>
        /// 尝试修复无配对情况
        /// </summary>
        private void AttemptPairFix(List<ElementBase> shuffleList, int maxAttempts = 50)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                // 随机选择两个不同类型元素
                int indexA = Random.Range(0, shuffleList.Count);
                int indexB;
                do
                {
                    indexB = Random.Range(0, shuffleList.Count);
                } while (shuffleList[indexA].Data.ConfigId == shuffleList[indexB].Data.ConfigId);

                // 记录交换前状态
                var elemA = shuffleList[indexA];
                var elemB = shuffleList[indexB];

                // 执行交换
                SafeSwapElements(elemA, elemB);

                if (CheckBoardHasValidPair())
                {
                    Logger.Info($"修复成功！在尝试 {i + 1} 次后找到有效配对");
                    return;
                }

                SwapElementEntity(elemA, elemB);
            }

            Logger.Warning($"修复失败！尝试 {maxAttempts} 次后仍无有效配对");
        }

        /// <summary>
        /// 强制创建有效配对（终极保障）
        /// </summary>
        private void ForceCreateValidPair()
        {
            // 获取所有有效位置
            List<Vector2Int> candidatePositions = _gridSystem.GetAllValidPositions();

            // 查找可能的配对位置
            foreach (var pos in candidatePositions)
            {
                // 尝试四个方向
                if (TryForceCreatePair(pos, Vector2Int.right) ||
                    TryForceCreatePair(pos, Vector2Int.down) ||
                    TryForceCreatePair(pos, Vector2Int.left) ||
                    TryForceCreatePair(pos, Vector2Int.up))
                {
                    return;
                }
            }

            Logger.Error("强制创建配对失败！没有找到合适的位置");
        }

        /// <summary>
        /// 尝试在特定方向强制创建配对
        /// </summary>
        private bool TryForceCreatePair(Vector2Int basePos, Vector2Int offset)
        {
            Vector2Int neighborPos = basePos + offset;

            if (!_gridSystem.IsValidPosition(basePos.x, basePos.y) ||
                !_gridSystem.IsValidPosition(neighborPos.x, neighborPos.y))
                return false;

            var baseElements = ElementSystem.Instance.GetGridElements(basePos, true);
            var neighborElements = ElementSystem.Instance.GetGridElements(neighborPos, true);

            if (baseElements == null || baseElements.Count == 0 ||
                neighborElements == null || neighborElements.Count == 0)
                return false;

            // 选择第一个普通元素作为基准
            ElementBase baseElem = baseElements.FirstOrDefault(e => e.Data.ElementType == ElementType.Normal);
            if (baseElem == null) return false;

            // 在相邻位置找到第一个普通元素
            ElementBase neighborElem = neighborElements.FirstOrDefault(e => e.Data.ElementType == ElementType.Normal);
            if (neighborElem == null) return false;

            // 强制设置为相同配置ID
            // neighborElem.Data.ConfigId = baseElem.Data.ConfigId;
            var gridItem = _gridSystem.GetGridByCoord(neighborElem.Data.GridPos);
            var eleData = ElementSystem.Instance.GenElementItemData(baseElem.Data.ConfigId, gridItem.Data.Coord.x,
                gridItem.Data.Coord.y);
            var element = ElementSystem.Instance.GenElement(eleData, gridItem.GameObject.transform);
            element.GameObject.transform.localPosition = Vector3.zero;
            gridItem.PushElement(element, doDestroy: true);
            Logger.Info($"强制创建配对：位置 {basePos} 和 {neighborPos} 设置为相同元素 {baseElem.Data.ConfigId}");

            return true;
        }

        /// <summary>
        /// 检测是否有两两匹配的普通棋子
        /// </summary>
        /// <returns></returns>
        private bool IsPair(int x, int y, ElementItemData other)
        {
            if (other.ElementType != ElementType.Normal && !ElementSystem.Instance.IsSpecialElement(other.ElementType))
                return false;
            var elements = ElementSystem.Instance.GetGridElements(new Vector2Int(x, y), true);
            if (elements == null || elements.Count <= 0)
                return false;
            bool haveBase = ElementSystem.Instance.TryGetBaseElement(elements, out int index, true);
            if (!haveBase)
                return false;
            //彩球可以连任意的基础棋子和功能棋子
            if (other.ElementType == ElementType.ColorBall && (elements[index].Data.ElementType == ElementType.Normal || ElementSystem.Instance.IsSpecialElement(elements[index].Data.ElementType)))
            {
                return true;
            }
            
            if (elements[index].Data.ElementType == ElementType.ColorBall && 
                (other.ElementType == ElementType.Normal || ElementSystem.Instance.IsSpecialElement(other.ElementType)))
            {
                return true;
            }

            return elements[index].Data.ConfigId == other.ConfigId;
        }

        /// <summary>
        /// 两个坐标是否相邻
        /// </summary>
        /// <param name="selfPos"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private bool IsNeighbor(Vector2Int selfPos, Vector2Int pos)
        {
            return (selfPos.x == pos.x - 1 && selfPos.y == pos.y) ||
                   (selfPos.x == pos.x + 1 && selfPos.y == pos.y) ||
                   (selfPos.x == pos.x && selfPos.y == pos.y - 1) ||
                   (selfPos.x == pos.x && selfPos.y == pos.y + 1);
        }

        /// <summary>
        /// 执行移动动画
        /// </summary>
        private void ExecuteMoveAnimations(List<ElementBase> elements)
        {
            foreach (var element in elements)
            {
                element.DoMove(0, Ease.Linear);
            }
        }
    }
}