using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HotfixLogic.Match
{
    public struct SimElement
    {
        public readonly int ConfigId;
        public readonly Vector2Int OriginalPosition; // 棋子在真实棋盘上的原始位置
        public int CurrentSimulatedY; // 棋子在模拟掉落后的当前Y坐标 (可变)

        // 完整的原始元素引用，如果需要更多属性
        public readonly ElementBase OriginalElement;

        public SimElement(ElementBase element, Vector2Int originalPosition, int currentSimulatedY)
        {
            this.ConfigId = element.Data.ConfigId;
            this.OriginalPosition = originalPosition;
            this.CurrentSimulatedY = currentSimulatedY;
            this.OriginalElement = element;
        }


        // 为SimElement重写Equals和GetHashCode，确保它们基于OriginalPosition识别唯一性
        // 否则HashSet在比较SimElement时可能出现问题
        public override bool Equals(object obj)
        {
            if (obj is not SimElement other) return false;
            return ConfigId == other.ConfigId && OriginalPosition == other.OriginalPosition;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ConfigId, OriginalPosition);
        }
    }

    // 定义一个委托，用于外部注入自定义的掉落逻辑
    // 参数: (上方棋子当前位置, 目标掉落空位位置, 完整棋盘状态) -> 返回: 是否可以掉落
    public delegate bool CanDropDelegate(Vector2Int fromPos, Vector2Int toPos,List<SimElement> matchCoords,
        Dictionary<int, List<SimElement>> fullBoard);

    public static partial class MatchTipsUtil
    {
        // 默认的掉落逻辑，如果外部不提供，就使用这个
        private static readonly CanDropDelegate DefaultCanDropLogic = (from, to,matchs, board) => true;

        public static (List<Vector2Int> obstacles, List<Vector2Int> formedRectangleOriginalPos)?
            FindRectanglesAfterObstacleRemoval(int boardWidth,
                int boardHeight,
                CanDropDelegate canDropLogic = null,
                int minMatchLength = 2)
        {
            if (_baseElementPosMap == null || _baseElementPosMap.Count < 4) return null;
            canDropLogic ??= DefaultCanDropLogic;

            // 预处理：将棋盘按列分组，初始化SimElement的当前Y坐标
            var allColumnsInitialState = PreprocessBoardToColumns(_baseElementPosMap, boardWidth);

            // 1. 主循环：遍历所有可能的“目标矩形”列对 (x1, x2)
            for (int x1 = 0; x1 < boardWidth - 1; x1++)
            {
                int x2 = x1 + 1; // 物理上相邻的下一列

                // 确保这两列可能存在于棋盘中（即使目前是空的）
                if (!allColumnsInitialState.ContainsKey(x1)) allColumnsInitialState[x1] = new List<SimElement>();
                if (!allColumnsInitialState.ContainsKey(x2)) allColumnsInitialState[x2] = new List<SimElement>();

                // 2. 定义“搜索窗口”，包含目标列对及其左右邻居
                var searchWindowColumnsInitialState = GetSearchWindow(allColumnsInitialState, x1, x2, boardWidth);

                // 3. 在“搜索窗口”内找出所有可能的消除组合
                var potentialMatches = GetPotentialMatchesInWindow(searchWindowColumnsInitialState, minMatchLength);

                // 优化：如果连消除的可能性都没有，直接跳过当前目标列对
                if (potentialMatches.Count == 0 && (x1 > 0 || x2 < boardWidth - 1)) continue; // 只有在不是边界且没有匹配时才跳过

                // 4. 遍历所有消除可能性
                foreach (var match in potentialMatches)
                {
                    // 5. 创建一个局部的虚拟棋盘用于模拟。
                    // 这里的virtualBoard现在将直接持有可变的SimElement列表。
                    var virtualBoard = CreateVirtualBoard(searchWindowColumnsInitialState);

                    // 获取待消除棋子的原始坐标
                    var obstaclePositions = match.Select(elem => elem.OriginalPosition).ToList();
                    RemoveMatchFromVirtualBoard(virtualBoard, obstaclePositions); // 从虚拟棋盘移除棋子

                    // 6. 只对受影响的列进行掉落模拟
                    var affectedColumns = obstaclePositions.Select(p => p.x).Distinct().ToList();

                    // 统一执行所有受影响列的掉落模拟
                    // 循环修改virtualBoard中的List<SimElement>，更新它们的CurrentSimulatedY
                    foreach (var colX in affectedColumns)
                    {
                        SimulateLocalizedDrop(virtualBoard[colX], colX, boardHeight, canDropLogic, virtualBoard, match);
                    }

                    // 7. 在“目标区域” (x1, x2) 检测是否形成了矩形
                    var rectangle = CheckFor2x2RectangleInPair(virtualBoard, x1, x2, boardHeight);
                    if (rectangle != null)
                    {
                        return (obstaclePositions, rectangle.Select(elem => elem.OriginalPosition).ToList());
                    }
                }
            }

            return null; // 遍历完所有列对，没有找到解法
        }

        /// <summary>
        /// 将棋盘状态转换为按列分组的SimElement列表，并初始化CurrentSimulatedY。
        /// Y=0是顶部，Y值增大表示向下。
        /// </summary>
        private static Dictionary<int, List<SimElement>> PreprocessBoardToColumns(
            Dictionary<Vector2Int, ElementBase> boardState, int boardWidth)
        {
            var columns = new Dictionary<int, List<SimElement>>(boardWidth);
            for (int x = 0; x < boardWidth; x++) // 确保所有列都存在于字典中，即使是空的
            {
                columns[x] = new List<SimElement>();
            }

            foreach (var kvp in boardState)
            {
                int x = kvp.Key.x;
                // 初始化SimElement的CurrentSimulatedY为其原始Y坐标
                columns[x].Add(new SimElement(kvp.Value, kvp.Key, kvp.Key.y));
            }

            // 对每列按初始的Y坐标排序，这对于后续的掉落模拟是必要的
            foreach (var colList in columns.Values)
            {
                colList.Sort((a, b) => a.CurrentSimulatedY.CompareTo(b.CurrentSimulatedY));
            }

            return columns;
        }

        /// <summary>
        /// 获取给定目标列对 (x1, x2) 的搜索窗口内的所有列数据。
        /// 返回的字典包含原始数据的List副本，以备模拟修改。
        /// </summary>
        private static Dictionary<int, List<SimElement>> GetSearchWindow(Dictionary<int, List<SimElement>> allColumns,
            int x1, int x2, int boardWidth)
        {
            var window = new Dictionary<int, List<SimElement>>();
            int x0 = x1 - 1;
            int x3 = x2 + 1;

            // 确保添加到窗口的列是存在的（可能是空的），并且是原始列表的副本
            if (x0 >= 0 && allColumns.TryGetValue(x0, out var col0)) window[x0] = new List<SimElement>(col0);
            else if (x0 >= 0) window[x0] = new List<SimElement>();
            if (allColumns.TryGetValue(x1, out var col1)) window[x1] = new List<SimElement>(col1);
            else window[x1] = new List<SimElement>();
            if (allColumns.TryGetValue(x2, out var col2)) window[x2] = new List<SimElement>(col2);
            else window[x2] = new List<SimElement>();
            if (x3 < boardWidth && allColumns.TryGetValue(x3, out var col3)) window[x3] = new List<SimElement>(col3);
            else if (x3 < boardWidth) window[x3] = new List<SimElement>();

            return window;
        }

        /// <summary>
        /// 从源列中创建虚拟棋盘的副本。
        /// </summary>
        private static Dictionary<int, List<SimElement>> CreateVirtualBoard(
            Dictionary<int, List<SimElement>> sourceColumns)
        {
            var virtualBoard = new Dictionary<int, List<SimElement>>();
            foreach (var kvp in sourceColumns)
            {
                virtualBoard[kvp.Key] = new List<SimElement>(kvp.Value); // 创建副本以进行修改
            }

            return virtualBoard;
        }

        private static List<List<SimElement>> GetPotentialMatchesInWindow(
            Dictionary<int, List<SimElement>> windowColumns, int minMatchLength)
        {
            // 使用HashSet避免重复的匹配组合
            var matches = new HashSet<List<SimElement>>(new ListEqualityComparer<SimElement>());
            var sortedKeys = windowColumns.Keys.ToList();
            sortedKeys.Sort(); // 确保按X轴顺序处理

            // 垂直匹配：在窗口内的每列中寻找
            foreach (var colList in windowColumns.Values)
            {
                FindVerticalMatches(matches, colList, minMatchLength);
            }

            // 水平匹配：在窗口内所有相邻的物理列对中寻找
            for (int i = 0; i < sortedKeys.Count - 1; i++)
            {
                int x_left = sortedKeys[i];
                int x_right = sortedKeys[i + 1];

                if (x_right == x_left + 1) // 确保是物理上的相邻列
                {
                    FindHorizontalMatches(matches, windowColumns[x_left], windowColumns[x_right], minMatchLength);
                }
            }

            return matches.ToList();
        }

        private static void FindVerticalMatches(HashSet<List<SimElement>> matches, List<SimElement> column,
            int minLength)
        {
            if (column.Count < minLength) return;
            // 确保列中的棋子是按Y坐标排序的
            column.Sort((a, b) => a.CurrentSimulatedY.CompareTo(b.CurrentSimulatedY));

            for (int i = 0; i <= column.Count - minLength; i++)
            {
                var potentialMatch = new List<SimElement>();
                potentialMatch.Add(column[i]);
                bool isMatch = true;
                int firstConfigId = column[i].ConfigId;

                // 从当前棋子开始，向下检查连续的同类型棋子
                for (int j = 1; j < minLength; j++)
                {
                    if (i + j >= column.Count)
                    {
                        isMatch = false;
                        break;
                    } // 超出列表范围

                    var nextElement = column[i + j];
                    // 检查ConfigId相同，并且模拟Y坐标必须是连续的
                    if (nextElement.ConfigId != firstConfigId ||
                        nextElement.CurrentSimulatedY != potentialMatch.Last().CurrentSimulatedY + 1)
                    {
                        isMatch = false;
                        break;
                    }

                    potentialMatch.Add(nextElement);
                }

                if (isMatch)
                {
                    potentialMatch.Sort((a, b) =>
                        a.OriginalPosition.y != b.OriginalPosition.y
                            ? a.OriginalPosition.y.CompareTo(b.OriginalPosition.y)
                            : a.OriginalPosition.x.CompareTo(b.OriginalPosition.x));
                    matches.Add(potentialMatch);
                }
            }
        }

        private static void FindHorizontalMatches(HashSet<List<SimElement>> matches, List<SimElement> col1,
            List<SimElement> col2, int minLength)
        {
            // 对于仅两列间的水平匹配，能形成的只有长度为2的匹配。
            if (minLength > 2) return;

            foreach (var e1 in col1)
            {
                foreach (var e2 in col2)
                {
                    // 必须在同一模拟Y轴高度且ConfigId相同
                    if (e1.CurrentSimulatedY == e2.CurrentSimulatedY && e1.ConfigId == e2.ConfigId)
                    {
                        var match = new List<SimElement> { e1, e2 };
                        match.Sort((a, b) =>
                            a.OriginalPosition.y != b.OriginalPosition.y
                                ? a.OriginalPosition.y.CompareTo(b.OriginalPosition.y)
                                : a.OriginalPosition.x.CompareTo(b.OriginalPosition.x));
                        matches.Add(match);
                    }
                }
            }
        }

        private static void RemoveMatchFromVirtualBoard(Dictionary<int, List<SimElement>> virtualBoard,
            List<Vector2Int> positionsToRemove)
        {
            foreach (var pos in positionsToRemove)
            {
                if (virtualBoard.ContainsKey(pos.x))
                    virtualBoard[pos.x].RemoveAll(elem => elem.OriginalPosition == pos);
            }
        }

        /// <summary>
        /// 精确模拟指定列的棋子掉落过程。棋子从上部（低Y）掉落到下部（高Y）填充空位。
        /// 此函数会更新列表内SimElement的CurrentSimulatedY字段，以反映掉落后的绝对Y坐标。
        /// </summary>
        /// <param name="columnElements">待模拟掉落的列要素列表，此列表中的SimElement会被直接修改。</param>
        /// <param name="columnX">当前列的X坐标。</param>
        /// <param name="boardHeight">棋盘高度。</param>
        /// <param name="canDrop">自定义掉落逻辑委托。</param>
        /// <param name="currentFullSimulatedBoardState">整个虚拟棋盘的当前状态，用于canDrop委托查询。</param>
        /// <param name="matchSimElement">模拟即将消除的元素信息，用于canDrop委托查询。</param>
        private static void SimulateLocalizedDrop(
            List<SimElement> columnElements,
            int columnX,
            int boardHeight,
            CanDropDelegate canDrop,
            Dictionary<int, List<SimElement>> currentFullSimulatedBoardState,
            List<SimElement> matchSimElement)
        {
            if (columnElements.Count == 0) return;

            // 确保元素按当前模拟Y坐标从上到下排序，以便正确处理掉落
            columnElements.Sort((a, b) => a.CurrentSimulatedY.CompareTo(b.CurrentSimulatedY));

            bool IsDropAndEmpty(Vector2Int pos)
            {
                //格子是否已经掉落并且已经空置
                for (int i = columnElements.Count - 1; i >= 0; i--)
                {
                    if (columnElements[i].CurrentSimulatedY != columnElements[i].OriginalPosition.y)
                    {
                        if (columnElements[i].OriginalPosition == pos)
                            return true;
                    }
                }

                return false;
            }

            bool changed = true;
            while (changed)
            {
                changed = false;
                // 从下往上遍历棋子，确保下方的棋子先稳定，再考虑上方棋子坠落
                for (int i = columnElements.Count - 1; i >= 0; i--)
                {
                    SimElement currentElement = columnElements[i]; // 获取当前棋子（struct是值类型，这里实际是副本）

                    // 如果棋子已经在棋盘底部或不能再下移，则跳过
                    if (currentElement.CurrentSimulatedY >= boardHeight - 1) continue;

                    int targetY = currentElement.CurrentSimulatedY; // 记录当前棋子能掉落到的最低Y

                    // 尝试逐格下移
                    while (targetY < boardHeight - 1)
                    {
                        int potentialNextY = targetY + 1;
                        Vector2Int potentialDropPos = new Vector2Int(columnX, potentialNextY);

                        // 检查目标位置是否已经被本列中的其他棋子占据
                        bool occupiedInThisColumn = false;
                        for (int j = 0; j < columnElements.Count; j++)
                        {
                            // 排除自身
                            if (j != i && columnElements[j].CurrentSimulatedY == potentialNextY)
                            {
                                occupiedInThisColumn = true;
                                break;
                            }
                        }

                        bool isEmptyByDrop = IsDropAndEmpty(potentialDropPos);

                        // 如果被占据或canDrop委托不允许掉落，则停止下移
                        if ((occupiedInThisColumn || !canDrop(new Vector2Int(columnX, currentElement.CurrentSimulatedY),
                                potentialDropPos,matchSimElement, currentFullSimulatedBoardState)) && !isEmptyByDrop)
                        {
                            break;
                        }

                        targetY = potentialNextY; // 可以掉落到下一个位置
                    }

                    // 如果棋子的Y坐标发生了变化，则更新它
                    if (targetY > currentElement.CurrentSimulatedY)
                    {
                        currentElement.CurrentSimulatedY = targetY;
                        columnElements[i] = currentElement;
                        changed = true;
                    }
                }
            }

            // 模拟完成后，再次排序以确保列表顺序与新的SimulatedY一致，方便后续查找
            columnElements.Sort((a, b) => a.CurrentSimulatedY.CompareTo(b.CurrentSimulatedY));
        }

        /// <summary>
        /// 在给定的虚拟棋盘状态下，查找所有 2x2 的“实心”矩形。
        /// 直接使用SimElement的CurrentSimulatedY字段作为其在模拟后的实际Y坐标。
        /// </summary>
        private static List<SimElement> CheckFor2x2RectangleInPair(
            Dictionary<int, List<SimElement>> virtualBoard,
            int x1, int x2, int boardHeight)
        {
            if (!virtualBoard.ContainsKey(x1) || !virtualBoard.ContainsKey(x2)) return null;

            List<SimElement> col1 = virtualBoard[x1];
            List<SimElement> col2 = virtualBoard[x2];

            // 创建映射以便通过CurrentSimulatedY快速查找棋子
            var col1Map = new Dictionary<int, SimElement>();
            foreach (var elem in col1)
            {
                // 如果同一格有多个棋子，只取最后一个
                col1Map[elem.CurrentSimulatedY] = elem;
            }

            var col2Map = new Dictionary<int, SimElement>();
            foreach (var elem in col2)
            {
                col2Map[elem.CurrentSimulatedY] = elem;
            }

            // 遍历所有可能的模拟Y坐标，作为2x2矩形的左上角Y坐标
            // 2x2矩形需要两个连续的Y坐标，所以只需要遍历到 boardHeight - 2
            for (int simY = 0; simY < boardHeight - 1; simY++)
            {
                int simY_topLeft = simY; // 矩形左上角的模拟Y坐标
                int simY_bottomLeft = simY + 1; // 矩形左下角的模拟Y坐标

                // 检查col1中是否存在这两个棋子
                if (col1Map.TryGetValue(simY_topLeft, out var p1) && col1Map.TryGetValue(simY_bottomLeft, out var p3))
                {
                    // 如果左侧两个棋子ConfigId相同
                    if (p1.ConfigId == p3.ConfigId)
                    {
                        // 检查col2中是否存在对应位置的右侧两个棋子 (p2和p4)
                        if (col2Map.TryGetValue(simY_topLeft, out var p2) && p1.ConfigId == p2.ConfigId &&
                            col2Map.TryGetValue(simY_bottomLeft, out var p4) && p1.ConfigId == p4.ConfigId)
                        {
                            // 找到一个2x2矩形！
                            return new List<SimElement> { p1, p2, p3, p4 };
                        }
                    }
                }
            }

            return null;
        }

        private class ListEqualityComparer<T> : IEqualityComparer<List<T>>
        {
            public bool Equals(List<T> x, List<T> y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
                if (x.Count != y.Count) return false;

                // 为了确保唯一性，我们需要对列表内容进行排序，然后比较Hash值
                // 或者更简单：比较每个元素的OriginalPosition（假定其唯一）
                var x_copy = new List<T>(x);
                var y_copy = new List<T>(y);
                // 这里假定T是SimElement，并使用OriginalPosition进行排序和比较
                if (typeof(T) == typeof(SimElement))
                {
                    x_copy.Sort((a, b) => CompareSimElements((SimElement)(object)a, (SimElement)(object)b));
                    y_copy.Sort((a, b) => CompareSimElements((SimElement)(object)a, (SimElement)(object)b));
                }

                for (int i = 0; i < x_copy.Count; i++)
                {
                    if (!x_copy[i].Equals(y_copy[i])) return false; // 默认的Equals可能不够
                }

                return true;
            }

            public int GetHashCode(List<T> obj)
            {
                if (obj == null) return 0;
                int hash = 17;
                var obj_copy = new List<T>(obj);
                if (typeof(T) == typeof(SimElement))
                {
                    obj_copy.Sort((a, b) => CompareSimElements((SimElement)(object)a, (SimElement)(object)b));
                }

                foreach (var item in obj_copy)
                {
                    hash = hash * 31 + item.GetHashCode(); // SimElement需要重写GetHashCode
                }

                return hash;
            }

            private int CompareSimElements(SimElement a, SimElement b)
            {
                int oyCompare = a.OriginalPosition.y.CompareTo(b.OriginalPosition.y);
                if (oyCompare != 0) return oyCompare;
                return a.OriginalPosition.x.CompareTo(b.OriginalPosition.x);
            }
        }
    }
}