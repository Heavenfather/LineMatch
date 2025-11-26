using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameConfig;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    public enum MatchTipsType : int
    {
        None = -1,
        ColorBall = 0,
        Bomb = 1,
        Rocket = 2,
        Square = 3
    }

    public static partial class MatchTipsUtil
    {
        private static Dictionary<Vector2Int, ElementBase> _baseElementPosMap;
        private static Dictionary<int, HashSet<Vector2Int>> _baseElementIdMap;

        private static List<ElementBase> _specialElements;

        // private static bool[,] _visited;
        private static BitArray _visitedBits;
        private static int _gridWidth; // 需要记录网格宽度用于索引计算
        private static List<Vector2Int> _currentPath;
        private static int _currentConfigId;
        private const int MAXDEPTH = 16;

        //棋盘检测优先级顺序，横竖数量4>3>3>2
        private static readonly int[] _priorityMinCounts = new[]
        {
            ValidateSquare.GenColoBallConditionCount, ValidateSquare.GenConditionCount,
            ValidateSquare.GenConditionCount, 2
        };

        // 最少需要的数量要求
        private static readonly int[] _priorityNeedMinCounts = new[]
        {
            12, 8, 6, 4
        };

        //两个方向是否都需要同时满足要求的数量
        private static readonly bool[] _priorityBothDirections = { true, true, false, true };

        /// <summary>
        /// 掉落完成/初始化完成后 更新棋盘上的元素分组
        /// </summary>
        public static void RefreshElementGroup()
        {
            _baseElementPosMap ??= new Dictionary<Vector2Int, ElementBase>();
            _baseElementPosMap.Clear();
            _baseElementIdMap ??= new Dictionary<int, HashSet<Vector2Int>>();
            _baseElementIdMap.Clear();
            _specialElements ??= new List<ElementBase>();
            _specialElements.Clear();

            var allElements = ElementSystem.Instance.GridElements;
            foreach (var elements in allElements.Values)
            {
                bool bResult = ElementSystem.Instance.TryGetBaseElement(elements, out int index, true);
                if (bResult)
                {
                    if (elements[index].Data.ElementType == ElementType.Normal)
                    {
                        _baseElementPosMap.TryAdd(elements[index].Data.GridPos, elements[index]);
                        if (!_baseElementIdMap.ContainsKey(elements[index].Data.ConfigId))
                            _baseElementIdMap.TryAdd(elements[index].Data.ConfigId, new HashSet<Vector2Int>());
                        _baseElementIdMap[elements[index].Data.ConfigId].Add(elements[index].Data.GridPos);
                    }

                    if (ElementSystem.Instance.IsSpecialElement(elements[index].Data.ElementType))
                    {
                        _specialElements.Add(elements[index]);
                    }
                }
            }

            if (_specialElements.Count > 0)
            {
                //按照彩球>炸弹>火箭的排序，优先推荐在彩球旁边形成的闭环
                _specialElements.Sort((a, b) =>
                {
                    if (a.Data.Priority > b.Data.Priority)
                        return -1;
                    if (a.Data.Priority < b.Data.Priority)
                        return 1;
                    return 0;
                });
            }
        }

        /// <summary>
        /// 获取功能棋子推荐连线的提示（按照优先级）
        /// </summary>
        /// <param name="tipsList">功能棋子列表</param>
        /// <returns></returns>
        public static bool TryGetSpecialTipsList(out List<Vector2Int> tipsList)
        {
            tipsList = null;

            if (_specialElements == null || _specialElements.Count == 0)
                return false;

            // 按照优先级顺序检查相邻对
            // 1. 彩球+彩球 (最高优先级)
            var colorBallPairs = FindSpecialElementPairs(ElementType.ColorBall, ElementType.ColorBall);
            if (colorBallPairs.Count >= 2)
            {
                tipsList = colorBallPairs;
                return true;
            }

            // 2. 彩球+炸弹 和 彩球+火箭 (同一优先级)
            var colorBallBombPairs = FindSpecialElementPairs(ElementType.ColorBall, ElementType.Bomb);
            var colorBallRocketPairs = FindSpecialElementPairs(ElementType.ColorBall, ElementType.Rocket);
            var colorBallRocketHorizontalPairs =
                FindSpecialElementPairs(ElementType.ColorBall, ElementType.RocketHorizontal);

            // 合并彩球+炸弹和彩球+火箭的结果
            var colorBallWithExplosivePairs = new List<Vector2Int>();
            colorBallWithExplosivePairs.AddRange(colorBallBombPairs);
            colorBallWithExplosivePairs.AddRange(colorBallRocketPairs);
            colorBallWithExplosivePairs.AddRange(colorBallRocketHorizontalPairs);

            if (colorBallWithExplosivePairs.Count >= 2)
            {
                tipsList = colorBallWithExplosivePairs;
                return true;
            }

            // 3. 炸弹+炸弹
            var bombPairs = FindSpecialElementPairs(ElementType.Bomb, ElementType.Bomb);
            if (bombPairs.Count >= 2)
            {
                tipsList = bombPairs;
                return true;
            }

            // 4. 炸弹+火箭
            var bombRocketPairs = FindSpecialElementPairs(ElementType.Bomb, ElementType.Rocket);
            var bombRocketHorizontalPairs = FindSpecialElementPairs(ElementType.Bomb, ElementType.RocketHorizontal);

            // 合并炸弹+火箭的结果
            var bombWithRocketPairs = new List<Vector2Int>();
            bombWithRocketPairs.AddRange(bombRocketPairs);
            bombWithRocketPairs.AddRange(bombRocketHorizontalPairs);

            if (bombWithRocketPairs.Count >= 2)
            {
                tipsList = bombWithRocketPairs;
                return true;
            }

            // 5. 火箭+火箭 (最低优先级)
            var rocketPairs = FindSpecialElementPairs(ElementType.Rocket, ElementType.Rocket);
            var rocketHorizontalPairs =
                FindSpecialElementPairs(ElementType.RocketHorizontal, ElementType.RocketHorizontal);
            var rocketMixedPairs = FindSpecialElementPairs(ElementType.Rocket, ElementType.RocketHorizontal);

            // 合并所有火箭组合的结果
            var allRocketPairs = new List<Vector2Int>();
            allRocketPairs.AddRange(rocketPairs);
            allRocketPairs.AddRange(rocketHorizontalPairs);
            allRocketPairs.AddRange(rocketMixedPairs);

            if (allRocketPairs.Count >= 2)
            {
                tipsList = allRocketPairs;
                return true;
            }

            // 如果没有找到任何符合优先级的相邻对，返回任意相邻的特殊棋子
            return TryGetAnySpecialTipsList(out tipsList);
        }

        /// <summary>
        /// 获取普通棋子推荐连线的提示
        /// </summary>
        /// <param name="tipsList">推荐目标的列表数组</param>
        /// <param name="priority">优先级</param>
        /// <returns>是否有推荐目标</returns>
        public static bool TryGetNormalTipsList(ref List<List<Vector2Int>> tipsList, out int priority)
        {
            priority = 0;
            //优先推荐有功能棋子旁边的闭环
            var dirs = ValidateManager.Instance.NeighborDirs;
            if (_specialElements.Count > 0)
            {
                for (int i = 0; i < _specialElements.Count; i++)
                {
                    //找特殊棋子的八个方向
                    foreach (var dir in dirs)
                    {
                        Vector2Int startPos = dir + _specialElements[i].Data.GridPos;
                        if (IsValidGrid(startPos))
                        {
                            //比较形成矩形的和不规则形状的，哪个获取到的优先级高就返回哪个
                            (List<(int configId, List<Vector2Int> loop)> rectangleList, int rectanglePriority) = FindRectangularLoops(3, startPos);
                            PickBestRectangle(rectangleList, rectanglePriority, ref tipsList, ref priority);
                            if (tipsList is { Count: > 0 })
                            {
                                priority = rectanglePriority;
                                return true;
                            }
                        }
                    }
                }
            }

            //没有功能棋子，找任意目标
            (List<(int configId, List<Vector2Int> loop)> rectangleList2, int rectanglePriority2) = FindRectangularLoops();
            PickBestRectangle(rectangleList2, rectanglePriority2, ref tipsList, ref priority);
            if (tipsList is { Count: > 0 })
            {
                priority = rectanglePriority2;
                return true;
            }
            return false;
        }

        private static void PickBestRectangle(List<(int configId, List<Vector2Int> loop)> rectangleList,
            int rectanglePriority, ref List<List<Vector2Int>> tipsList, ref int priority)
        {
            var targets = LevelTargetSystem.Instance.TargetElements;
            if (rectangleList is { Count: > 0 })
            {
                foreach (var rectangle in rectangleList)
                {
                    if (targets.ContainsKey(rectangle.configId) &&
                        !LevelTargetSystem.Instance.IsTargetFinish(rectangle.configId))
                    {
                        tipsList.Add(rectangle.loop);
                        priority = rectanglePriority;
                    }
                }
                if(tipsList.Count > 0)
                    return;
                foreach (var rectangle in rectangleList)
                {
                    tipsList.Add(rectangle.loop);
                }
                priority = rectanglePriority;
            }
        }

        /// <summary>
        /// 获取功能棋子推荐连线的提示
        /// </summary>
        /// <param name="tipsList">功能棋子列表</param>
        /// <returns></returns>
        private static bool TryGetAnySpecialTipsList(out List<Vector2Int> tipsList)
        {
            tipsList = null;
            if (_specialElements.Count > 0)
            {
                tipsList = new List<Vector2Int>(2);
                for (int i = 0; i < _specialElements.Count; i++)
                {
                    foreach (var dir in ValidateManager.Instance.NeighborDirs)
                    {
                        Vector2Int pos = dir + _specialElements[i].Data.GridPos;
                        if (_specialElements.FindIndex(x => x.Data.GridPos == pos) >= 0)
                        {
                            tipsList.Add(_specialElements[i].Data.GridPos);
                            tipsList.Add(pos);
                            break;
                        }
                    }

                    if (tipsList.Count >= 2)
                    {
                        break;
                    }
                }
            }

            return tipsList != null && tipsList.Count > 0;
        }

        /// <summary>
        /// 查找特定类型的相邻特殊棋子对
        /// </summary>
        /// <param name="type1">第一种棋子类型</param>
        /// <param name="type2">第二种棋子类型</param>
        /// <returns>相邻的棋子位置列表</returns>
        private static List<Vector2Int> FindSpecialElementPairs(ElementType type1, ElementType type2)
        {
            var pairs = new List<Vector2Int>();

            for (int i = 0; i < _specialElements.Count; i++)
            {
                var element1 = _specialElements[i];

                // 检查第一个元素是否符合类型1
                if (element1.Data.ElementType != type1)
                    continue;

                // 检查四个方向的相邻位置
                foreach (var dir in ValidateManager.Instance.NeighborDirs)
                {
                    Vector2Int neighborPos = element1.Data.GridPos + dir;

                    // 查找相邻位置是否有符合类型2的特殊棋子
                    var neighborElement = _specialElements.Find(x =>
                        x.Data.GridPos == neighborPos &&
                        (x.Data.ElementType == type2 ||
                         (type2 == ElementType.Rocket && x.Data.ElementType == ElementType.RocketHorizontal) ||
                         (type2 == ElementType.RocketHorizontal && x.Data.ElementType == ElementType.Rocket)));

                    if (neighborElement != null)
                    {
                        pairs.Add(element1.Data.GridPos);
                        pairs.Add(neighborPos);
                        return pairs; // 找到一对就返回
                    }
                }
            }

            return pairs;
        }

        private static (List<(int configId, List<Vector2Int> loop)>, int) FindRectangularLoops(int priorityCount = 4,
            Vector2Int? startPos = null)
        {
            // 按优先级顺序查找矩形闭环
            for (int priority = 0; priority < priorityCount; priority++)
            {
                bool bothDirections = _priorityBothDirections[priority];
                int minCount = _priorityMinCounts[priority];

                // 用于存储当前优先级找到的闭环，每个ConfigId只存一个
                Dictionary<int, List<Vector2Int>> configLoops = new Dictionary<int, List<Vector2Int>>();
                // 用于去重，避免同一矩形被多次检测
                HashSet<(int, int, int, int)> seenRects = new HashSet<(int, int, int, int)>();

                // 按ConfigId分组处理
                foreach (var group in _baseElementIdMap)
                {
                    int configId = group.Key;
                    var positions = group.Value;

                    // 如果这个ConfigId已经找到闭环，跳过
                    if (configLoops.ContainsKey(configId)) continue;

                    // 使用空间分区优化：按行列分组
                    Dictionary<int, HashSet<int>> xMap = new Dictionary<int, HashSet<int>>();
                    Dictionary<int, HashSet<int>> yMap = new Dictionary<int, HashSet<int>>();

                    foreach (var pos in positions)
                    {
                        if (!xMap.ContainsKey(pos.x)) xMap[pos.x] = new HashSet<int>();
                        if (!yMap.ContainsKey(pos.y)) yMap[pos.y] = new HashSet<int>();
                        xMap[pos.x].Add(pos.y);
                        yMap[pos.y].Add(pos.x);
                    }

                    // 只检查有足够点数的行列
                    var validXs = xMap.Where(kv => kv.Value.Count >= 2).Select(kv => kv.Key).ToList();

                    // 按矩形大小排序，优先找大的矩形
                    validXs.Sort((a, b) => Math.Abs(a - b).CompareTo(Math.Abs(a - b)));

                    bool foundLoop = false;

                    for (int i = 0; i < validXs.Count && !foundLoop; i++)
                    {
                        int x1 = validXs[i];
                        var ySet1 = xMap[x1];

                        for (int j = i + 1; j < validXs.Count && !foundLoop; j++)
                        {
                            int x2 = validXs[j];
                            var ySet2 = xMap[x2];

                            // 快速交集计算：找出两个x坐标共有的y坐标
                            var commonYs = ySet1.Intersect(ySet2).ToList();
                            if (commonYs.Count < 2) continue;

                            // 按y坐标差值排序，优先找大的矩形
                            commonYs.Sort((a, b) => Math.Abs(a - b).CompareTo(Math.Abs(a - b)));

                            for (int k = 0; k < commonYs.Count && !foundLoop; k++)
                            {
                                for (int l = k + 1; l < commonYs.Count && !foundLoop; l++)
                                {
                                    int y1 = commonYs[k];
                                    int y2 = commonYs[l];

                                    Vector2Int p1 = new Vector2Int(x1, y1);
                                    Vector2Int p2 = new Vector2Int(x2, y2);

                                    // 检查矩形完整性
                                    if (IsRectangularComplete(p1, p2, configId))
                                    {
                                        // 检查矩形尺寸优先级
                                        int width = Math.Abs(x2 - x1) + 1;
                                        int height = Math.Abs(y2 - y1) + 1;

                                        if (CheckRectangularPriority(width, height, bothDirections, minCount))
                                        {
                                            // 去重检查
                                            var rectKey = (Math.Min(x1, x2), Math.Min(y1, y2),
                                                Math.Max(x1, x2), Math.Max(y1, y2));
                                            if (seenRects.Contains(rectKey)) continue;
                                            seenRects.Add(rectKey);

                                            // 检查起始位置约束
                                            if (startPos.HasValue &&
                                                !IsPositionOnRectangularBoundary(startPos.Value, p1, p2))
                                            {
                                                continue;
                                            }

                                            var boundary = GetRectangularBoundary(p1, p2);
                                            configLoops[configId] = boundary;
                                            foundLoop = true;
                                            break; // 找到这个ConfigId的一个闭环就跳出
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // 如果当前优先级找到闭环，立即返回（按优先级从高到低）
                if (configLoops.Count > 0)
                {
                    // 转换为需要的返回格式
                    var result = configLoops.Select(kv => (kv.Key, kv.Value)).ToList();
                    return (result, priority);
                }
            }

            return (null, -1);
        }
        
        /// <summary>
        /// 检查位置是否在矩形边界上
        /// </summary>
        private static bool IsPositionOnRectangularBoundary(Vector2Int position, Vector2Int p1, Vector2Int p2) 
        { 
            int minX = Math.Min(p1.x, p2.x); 
            int maxX = Math.Max(p1.x, p2.x); 
            int minY = Math.Min(p1.y, p2.y); 
            int maxY = Math.Max(p1.y, p2.y); 

            // 检查是否在上边界或下边界
            if ((position.y == minY || position.y == maxY) && position.x >= minX && position.x <= maxX) 
                return true; 

            // 检查是否在左边界或右边界
            if ((position.x == minX || position.x == maxX) && position.y >= minY && position.y <= maxY) 
                return true; 

            return false; 
        } 
        
        /// <summary>
        /// 检查矩形是否满足优先级条件
        /// </summary>
        private static bool CheckRectangularPriority(int width, int height, bool bothDirections, int minCount) 
        { 
            if (bothDirections) 
            { 
                // 对于优先级4，需要确保两个方向都等于2
                if (minCount == 2) 
                { 
                    return width == 2 && height == 2; 
                } 

                if (minCount == 3) 
                { 
                    //炸弹优先
                    return width == ValidateSquare.GenConditionCount && height == ValidateSquare.GenConditionCount; 
                } 

                return width >= minCount && height >= minCount; 
            } 

            return width >= minCount || height >= minCount; 
        } 
        
        /// <summary>
        /// 优化的矩形完整性检查：使用预计算的行列信息
        /// </summary>
        private static bool IsRectangularComplete(Vector2Int p1, Vector2Int p2, int configId)
        {
            int minX = Math.Min(p1.x, p2.x);
            int maxX = Math.Max(p1.x, p2.x);
            int minY = Math.Min(p1.y, p2.y);
            int maxY = Math.Max(p1.y, p2.y);

            // 快速检查四个角点
            if (!_baseElementPosMap.ContainsKey(new Vector2Int(minX, minY)) ||
                !_baseElementPosMap.ContainsKey(new Vector2Int(minX, maxY)) ||
                !_baseElementPosMap.ContainsKey(new Vector2Int(maxX, minY)) ||
                !_baseElementPosMap.ContainsKey(new Vector2Int(maxX, maxY)))
            {
                return false;
            }

            // 检查边界完整性
            return CheckRectangularBoundaryIntegrity(minX, maxX, minY, maxY, configId);
        }

        /// <summary>
        /// 边界完整性检查
        /// </summary>
        private static bool CheckRectangularBoundaryIntegrity(int minX, int maxX, int minY, int maxY, int configId)
        {
            // 批量检查边界点
            for (int x = minX; x <= maxX; x++)
            {
                // 上边界
                if (!CheckPosition(x, maxY, configId)) return false;
                // 下边界  
                if (!CheckPosition(x, minY, configId)) return false;
            }

            for (int y = minY + 1; y < maxY; y++)
            {
                // 左边界
                if (!CheckPosition(minX, y, configId)) return false;
                // 右边界
                if (!CheckPosition(maxX, y, configId)) return false;
            }

            return true;
        }

        /// <summary>
        /// 优化的位置检查
        /// </summary>
        private static bool CheckPosition(int x, int y, int configId)
        {
            var pos = new Vector2Int(x, y);
            return _baseElementPosMap.TryGetValue(pos, out var element) &&
                   element.Data.ConfigId == configId;
        }

        /// <summary>
        /// 优化的矩形边界生成
        /// </summary>
        private static List<Vector2Int> GetRectangularBoundary(Vector2Int p1, Vector2Int p2)
        {
            int minX = Math.Min(p1.x, p2.x);
            int maxX = Math.Max(p1.x, p2.x);
            int minY = Math.Min(p1.y, p2.y);
            int maxY = Math.Max(p1.y, p2.y);

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;
            int totalPoints = 2 * (width + height) - 4; // 去除重复角点

            var boundary = new List<Vector2Int>(totalPoints);

            // 预分配容量，避免动态扩容
            boundary.Capacity = totalPoints;

            // 上边界（从左到右）
            for (int x = minX; x <= maxX; x++)
            {
                boundary.Add(new Vector2Int(x, maxY));
            }

            // 右边界（从上到下，排除右上角）
            for (int y = maxY - 1; y >= minY; y--)
            {
                boundary.Add(new Vector2Int(maxX, y));
            }

            // 下边界（从右到左，排除右下角）
            for (int x = maxX - 1; x >= minX; x--)
            {
                boundary.Add(new Vector2Int(x, minY));
            }

            // 左边界（从下到上，排除左下角和左上角）
            for (int y = minY + 1; y < maxY; y++)
            {
                boundary.Add(new Vector2Int(minX, y));
            }

            return boundary;
        }

        /// <summary>
        /// 从指定位置开始找闭环(可以是不规则形状的)
        /// </summary>
        /// <returns></returns>
        private static (List<Vector2Int>, int) FindArbitraryShapeLoopFromPos(Vector2Int startPos, int gridX, int gridY)
        {
            //检查指定位置是否有棋子
            if (_baseElementPosMap.TryGetValue(startPos, out var element))
            {
                _currentConfigId = element.Data.ConfigId;
                // _visited = new bool[gridX, gridY];
                _gridWidth = gridX;
                if (_visitedBits == null || _visitedBits.Length != gridX * gridY)
                    _visitedBits = new BitArray(gridX * gridY);
                else
                    _visitedBits.SetAll(false); // 清空所有位

                //优先级只遍历前3个，不包含2x2的矩形，在功能棋子旁边形成2x2的闭环没意义
                for (int priority = 0; priority < 3; priority++)
                {
                    bool bothDirections = _priorityBothDirections[priority];
                    int minCount = _priorityMinCounts[priority];

                    //重置访问数据
                    _visitedBits.SetAll(false);

                    _currentPath ??= new List<Vector2Int>();
                    _currentPath.Clear();

                    if (FindLoopFromPos(startPos, startPos, 0, bothDirections, minCount, gridX, gridY))
                    {
                        return (_currentPath, priority);
                    }
                }
            }

            return (null, -1);
        }

        /// <summary>
        /// 从任意位置开始找闭环
        /// </summary>
        /// <returns></returns>
        private static (List<Vector2Int>, int) FinsBestLoop(int gridX, int gridY)
        {
            // _visited = new bool[gridX, gridY];
            _gridWidth = gridX;
            if (_visitedBits == null || _visitedBits.Length != gridX * gridY)
                _visitedBits = new BitArray(gridX * gridY);
            else
                _visitedBits.SetAll(false); // 清空所有位

            //按照4个优先级遍历
            for (int priority = 0; priority < 4; priority++)
            {
                bool bothDirections = _priorityBothDirections[priority];
                int minCount = _priorityMinCounts[priority];

                //按照基础元素分组寻找
                // 使用更高效的数据结构存储组信息
                int groupCount = _baseElementIdMap.Count;
                if (groupCount == 0) continue;

                // 预分配数组，避免频繁分配
                var groupConfigIds = new int[groupCount];
                var groupPositions = new HashSet<Vector2Int>[groupCount];
                var groupSizes = new int[groupCount];

                // 提取组信息并排序（按大小降序）
                int idx = 0;
                foreach (var pair in _baseElementIdMap)
                {
                    groupConfigIds[idx] = pair.Key;
                    groupPositions[idx] = pair.Value;
                    groupSizes[idx] = pair.Value.Count;
                    idx++;
                }

                // 手动排序：按组大小降序
                for (int i = 0; i < groupCount - 1; i++)
                {
                    for (int j = i + 1; j < groupCount; j++)
                    {
                        if (groupSizes[j] > groupSizes[i])
                        {
                            // 交换所有相关数据
                            (groupConfigIds[i], groupConfigIds[j]) = (groupConfigIds[j], groupConfigIds[i]);
                            (groupPositions[i], groupPositions[j]) = (groupPositions[j], groupPositions[i]);
                            (groupSizes[i], groupSizes[j]) = (groupSizes[j], groupSizes[i]);
                        }
                    }
                }

                // 遍历排序后的组
                for (int i = 0; i < groupCount; i++)
                {
                    _currentConfigId = groupConfigIds[i];
                    var positions = groupPositions[i];

                    // 如果该组的棋子数量不足以形成闭环，直接跳过
                    if (positions.Count < _priorityNeedMinCounts[priority])
                        continue;
                    foreach (var startPos in positions)
                    {
                        _currentPath ??= new List<Vector2Int>();
                        _currentPath.Clear();

                        if (FindLoopFromPos(startPos, startPos, 0, bothDirections, minCount, gridX, gridY))
                        {
                            return (_currentPath, priority);
                        }
                    }
                }
            }

            return (null, -1);
        }

        /// <summary>
        /// DFS算法找出任意可能形成闭环的路径
        /// </summary>
        /// <param name="currentPos">当前寻找坐标</param>
        /// <param name="startPos">开始坐标</param>
        /// <param name="depth">循环深度</param>
        /// <param name="bothDirections">横竖是否需要同时满足</param>
        /// <param name="minCount">最低需要满足的横竖数量</param>
        /// <param name="gridX">列数</param>
        /// <param name="gridY">行数</param>
        /// <returns></returns>
        private static bool FindLoopFromPos(Vector2Int currentPos, Vector2Int startPos, int depth, bool bothDirections,
            int minCount, int gridX, int gridY)
        {
            return false;
            // 深度限制 - 提前检查以避免不必要的处理
            if (depth >= MAXDEPTH || depth > gridX * gridY)
                return false;

            // 检查是否回到起点且深度足够形成闭环
            if (depth > 3 && currentPos == startPos)
            {
                if (CheckPriorityCondition(_currentPath, bothDirections, minCount))
                    return true;
                return false;
            }

            // 边界检查和访问检查
            int currentIndex = currentPos.x + currentPos.y * _gridWidth;
            if (currentPos.x < 0 || currentPos.x >= gridX || currentPos.y < 0 || currentPos.y >= gridY ||
                _visitedBits[currentIndex])
                return false;

            // 只找相同类型的元素
            if (!_baseElementPosMap.TryGetValue(currentPos, out var element) ||
                element.Data.ConfigId != _currentConfigId)
                return false;

            // 标记访问
            _visitedBits[currentIndex] = true;
            _currentPath.Add(currentPos);

            // 优先检查更有可能形成闭环的方向
            var dirs = ValidateManager.Instance.NeighborDirs;
            foreach (var dir in dirs)
            {
                Vector2Int nextPos = dir + currentPos;
                // 使用更高效的网格有效性检查
                if (nextPos.x >= 0 && nextPos.x < gridX && nextPos.y >= 0 && nextPos.y < gridY)
                {
                    if (FindLoopFromPos(nextPos, startPos, depth + 1, bothDirections, minCount, gridX, gridY))
                        return true;
                }
            }

            // 回溯
            _visitedBits[currentIndex] = false;
            _currentPath.RemoveAt(_currentPath.Count - 1);
            return false;
        }

        /// <summary>
        /// 检测寻找的路径是否符合配置条件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bothDirections"></param>
        /// <param name="minCount"></param>
        /// <returns></returns>
        private static bool CheckPriorityCondition(List<Vector2Int> path, bool bothDirections, int minCount)
        {
            if (path.Count < 4)
                return false; //至少需要4个才会有闭环

            //找出行列相差最大值,这里与形成功能棋子的矩形算法一致
            int maxX = -1;
            int maxY = -1;
            int minX = 99;
            int minY = 99;
            for (int i = 0; i < path.Count; i++)
            {
                if (maxX < path[i].x)
                    maxX = path[i].x;
                if (maxY < path[i].y)
                    maxY = path[i].y;
                if (minX > path[i].x)
                    minX = path[i].x;
                if (minY > path[i].y)
                    minY = path[i].y;
            }

            int colCount = maxX - minX + 1; //坐标系从0开始，需要+1
            int rowCount = maxY - minY + 1;

            if (bothDirections)
            {
                if (minCount == 2)
                {
                    return rowCount == 2 && colCount == 2;
                }

                if (minCount == 3)
                {
                    //炸弹优先
                    return rowCount == ValidateSquare.GenConditionCount && colCount == ValidateSquare.GenConditionCount;
                }

                return colCount >= minCount && rowCount >= minCount;
            }

            return colCount >= minCount || rowCount >= minCount;
        }

        private static bool IsValidGrid(Vector2Int pos)
        {
            return _baseElementPosMap.ContainsKey(pos);
        }
    }
}