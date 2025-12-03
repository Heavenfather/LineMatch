#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using HotfixLogic.Match;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HotfixLogic
{
    /// <summary>
    /// 随机关卡生成器
    /// </summary>
    public static class LevelGenerator
    {
        /// <summary>
        /// 随机生成一个默认关卡
        /// </summary>
        /// <param name="levelId"></param>
        /// <returns></returns>
        public static LevelData GenerateLevelData(int levelId)
        {
            LevelData levelData = new LevelData();
            levelData.gridCol = Random.Range(6, 11);
            levelData.gridRow = Random.Range(6, 11);
            levelData.id = levelId;
            levelData.initColor = new int[] { 1, 2 };
            levelData.initColorRate = new int[] { 5000, 5000 };
            levelData.dropColor = new int[] { 1, 2 };
            levelData.dropColorRate = new int[] { 5000, 5000 };
            levelData.difficulty = 1;
            levelData.target = new TargetElement[1] { new TargetElement() { targetId = 1, targetNum = 10 } };
            levelData.fullScore = 100;
            levelData.stepLimit = 30;
            bool[,] grid = GenerateLevel(levelData.gridCol, levelData.gridRow, levelData.difficulty);
            levelData.grid = new LevelElement[levelData.gridCol][];
            for (int x = 0; x < levelData.gridCol; x++)
            {
                levelData.grid[x] = new LevelElement[levelData.gridRow];
                for (int y = 0; y < levelData.gridRow; y++)
                {
                    levelData.grid[x][y] = new LevelElement() { elements = new List<GridElement>(), isWhite = grid[x, y] };
                }
            }
            levelData.dropFlags = new List<DropFlag>(levelData.gridCol);

            return levelData;
        }

        /// <summary>
        /// 变更关卡格子数据
        /// </summary>
        /// <param name="levelData"></param>
        public static void RefLevelGrid(ref LevelData levelData)
        {
            int wid = levelData.gridCol;
            int hid = levelData.gridRow;
            bool[,] grid = GenerateLevel(levelData.gridCol, levelData.gridRow, levelData.difficulty);
            levelData.grid = new LevelElement[levelData.gridCol][];
            for (int x = 0; x < wid; x++)
            {
                levelData.grid[x] = new LevelElement[hid];
                for (int y = 0; y < hid; y++)
                {
                    levelData.grid[x][y] = new LevelElement() { elements = new List<GridElement>(), isWhite = grid[x, y] };
                }
            }
        }

        /// <summary>
        /// 生成二维数组棋盘
        /// </summary>
        /// <returns>true表示空格</returns>
        static bool[,] GenerateLevel(int width, int height, int difficulty)
        {
            bool[,] board = new bool[width, height];
            switch (difficulty)
            {
                case 1:
                    GenerateRegularBoard(board);
                    break;
                case 2:
                    GenerateRegularWithHoles(board);
                    break;
                case 3:
                    GenerateSplitBoard(board);
                    break;
                case 4:
                    GenerateIrregularShape(board);
                    break;
                case 5:
                    GenerateChaoticBoard(board);
                    break;
            }

            // 后处理验证
            return PostProcess(board, difficulty);
        }

        // 难度1：完全规整
        static void GenerateRegularBoard(bool[,] board)
        {
            // 全填充（无空格）
            return;
        }

        // 难度2：随机分布带孔洞的规整棋盘
        static void GenerateRegularWithHoles(bool[,] board)
        {
            int holes = Mathf.RoundToInt(board.Length * Random.Range(0.05f, 0.15f));
            HashSet<Vector2Int> holePositions = new();

            while (holePositions.Count < holes)
            {
                Vector2Int pos = new(
                    Random.Range(0, board.GetLength(0)),
                    Random.Range(0, board.GetLength(1))
                );

                if (!IsEdgePosition(board, pos))
                    holePositions.Add(pos);
            }

            foreach (var pos in holePositions)
                board[pos.x, pos.y] = true;
        }

        // 难度3：棋盘分割
        enum BarrierType
        {
            Vertical,
            Horizontal
        }

        static void GenerateSplitBoard(bool[,] board)
        {
            int minSplits = 2;
            int maxSplits = 5;
            int splits = Random.Range(minSplits, maxSplits + 1);
            List<RectInt> regions = new List<RectInt> { new RectInt(0, 0, board.GetLength(0), board.GetLength(1)) };

            // 迭代分割算法（保证最少2个区域）
            for (int i = 0; i < splits; i++)
            {
                // 选择最大可分割区域
                RectInt toSplit = regions.OrderByDescending(r => r.width * r.height).First();
                regions.Remove(toSplit);

                // 动态分割方向决策（长宽比>1.5时强制分割长边）
                bool canVertical = toSplit.width >= 6;
                bool canHorizontal = toSplit.height >= 6;
                bool doVertical = canVertical &&
                                  (toSplit.width / (float)toSplit.height > 1.5f ||
                                   (canVertical && Random.value > 0.5f));

                // 执行分割
                if (doVertical && canVertical)
                {
                    int minSplitX = toSplit.xMin + 3;
                    int maxSplitX = toSplit.xMax - 3;
                    if (maxSplitX > minSplitX)
                    {
                        int splitX = Random.Range(minSplitX, maxSplitX);
                        regions.Add(new RectInt(toSplit.x, toSplit.y, splitX - toSplit.x, toSplit.height));
                        regions.Add(new RectInt(splitX + 1, toSplit.y, toSplit.xMax - splitX - 1, toSplit.height));
                        CreateDynamicBarrier(board, splitX, toSplit.y, toSplit.height,
                            BarrierType.Vertical, thickness: Random.Range(1, 3));
                    }
                }
                else if (canHorizontal)
                {
                    int minSplitY = toSplit.yMin + 3;
                    int maxSplitY = toSplit.yMax - 3;
                    if (maxSplitY > minSplitY)
                    {
                        int splitY = Random.Range(minSplitY, maxSplitY);
                        regions.Add(new RectInt(toSplit.x, toSplit.y, toSplit.width, splitY - toSplit.y));
                        regions.Add(new RectInt(toSplit.x, splitY + 1, toSplit.width, toSplit.yMax - splitY - 1));
                        CreateDynamicBarrier(board, splitY, toSplit.x, toSplit.width,
                            BarrierType.Horizontal, thickness: Random.Range(1, 3));
                    }
                }
            }

            // 确保生成至少3个区域（保险机制）
            while (regions.Count < 3)
            {
                RectInt smallest = regions.OrderBy(r => r.width * r.height).First();
                regions.Remove(smallest);
                regions.Add(new RectInt(smallest.x, smallest.y, smallest.width / 2, smallest.height));
                regions.Add(new RectInt(smallest.x + smallest.width / 2, smallest.y, smallest.width / 2,
                    smallest.height));
            }

            // 后处理：连接区域
            ConnectRegions(board, regions);
        }

        // 难度4：规则形状
        static void GenerateIrregularShape(bool[,] board)
        {
            int shapeType = Random.Range(0, 4);
            Vector2 center = new Vector2(
                board.GetLength(0) / 2f - 0.5f,
                board.GetLength(1) / 2f - 0.5f
            );

            // 动态尺寸参数
            float maxRadius = Mathf.Min(center.x, center.y) * Random.Range(0.6f, 0.9f);
            float noiseScale = Random.Range(0.1f, 0.3f);

            for (int x = 0; x < board.GetLength(0); x++)
            {
                for (int y = 0; y < board.GetLength(1); y++)
                {
                    Vector2 pos = new Vector2(x, y);
                    bool isSolid = false;

                    switch (shapeType)
                    {
                        case 0: // 动态菱形
                            isSolid = IsDynamicDiamond(pos, center, maxRadius);
                            break;

                        case 1: // 参数化山形
                            isSolid = IsParametricMountain(pos, center, maxRadius);
                            break;

                        case 2: // 噪声边缘
                            isSolid = IsNoiseEdge(pos, center, maxRadius, noiseScale);
                            break;

                        case 3: // 分形树
                            isSolid = IsFractalTree(pos, center, maxRadius);
                            break;
                    }

                    board[x, y] = !isSolid;
                }
            }

            // 后处理：添加细节
            AddShapeDetails(board, 0.1f);
        }

        // 难度5：混乱棋盘
        static void GenerateChaoticBoard(bool[,] board)
        {
            float density = Random.Range(0.3f, 0.6f);
            for (int x = 0; x < board.GetLength(0); x++)
            {
                for (int y = 0; y < board.GetLength(1); y++)
                {
                    if (Random.value < density && !IsEdgePosition(board, new Vector2Int(x, y)))
                        board[x, y] = true;
                }
            }
        }

        static void CreateDynamicBarrier(bool[,] board, int pos, int start, int length,
            BarrierType type, int thickness)
        {
            // 参数校验
            thickness = Mathf.Clamp(thickness, 1, 3);
            int maxAttempts = 5;

            // 生成带缺口的分割线
            for (int t = 0; t < thickness; t++)
            {
                int currentPos = pos + t;
                if (currentPos >= (type == BarrierType.Vertical ? board.GetLength(0) : board.GetLength(1)))
                    break;

                // 生成缺口模式（必须保证至少一个缺口）
                bool hasGap = false;
                int gapStart = -1;
                int gapLength = 0;

                while (!hasGap && maxAttempts-- > 0)
                {
                    gapStart = Random.Range(start + 2, start + length - 4);
                    gapLength = Random.Range(2, 4);
                    if (gapStart + gapLength < start + length - 1)
                    {
                        hasGap = true;
                        break;
                    }
                }

                // 绘制屏障
                for (int i = start; i < start + length; i++)
                {
                    if (hasGap && i >= gapStart && i < gapStart + gapLength)
                        continue;

                    if (type == BarrierType.Vertical)
                        board[currentPos, i] = true;
                    else
                        board[i, currentPos] = true;
                }
            }
        }

        static void ConnectRegions(bool[,] board, List<RectInt> regions)
        {
            // 确保所有区域连通
            List<Vector2Int> connectionPoints = new List<Vector2Int>();

            // 在每个区域边缘创建连接点
            foreach (var region in regions)
            {
                int connectX = Random.Range(region.x + 1, region.x + region.width - 1);
                int connectY = Random.Range(region.y + 1, region.y + region.height - 1);
                connectionPoints.Add(new Vector2Int(connectX, connectY));
            }

            // 创建连接通道
            for (int i = 0; i < connectionPoints.Count - 1; i++)
            {
                Vector2Int start = connectionPoints[i];
                Vector2Int end = connectionPoints[i + 1];

                // 横向连接
                int dirX = Mathf.Clamp(end.x - start.x, -1, 1);
                for (int x = start.x; x != end.x; x += dirX)
                    board[x, start.y] = false;

                // 纵向连接
                int dirY = Mathf.Clamp(end.y - start.y, -1, 1);
                for (int y = start.y; y != end.y; y += dirY)
                    board[end.x, y] = false;
            }
        }

        // 形状判断方法
        static bool IsDynamicDiamond(Vector2 pos, Vector2 center, float radius)
        {
            float aspect = Random.Range(0.7f, 1.3f);
            Vector2 offset = pos - center;
            return (Mathf.Abs(offset.x) / aspect + Mathf.Abs(offset.y) * aspect) <= radius;
        }

        static bool IsParametricMountain(Vector2 pos, Vector2 center, float scale)
        {
            float angle = Mathf.Atan2(pos.y - center.y, pos.x - center.x);
            float baseRadius = scale * (1 + 0.3f * Mathf.Sin(angle * 3));
            return Vector2.Distance(pos, center) <= baseRadius;
        }

        static bool IsNoiseEdge(Vector2 pos, Vector2 center, float baseRadius, float noiseScale)
        {
            float noise = Mathf.PerlinNoise(pos.x * noiseScale, pos.y * noiseScale);
            float dynamicRadius = baseRadius * (1 + 0.2f * noise);
            return Vector2.Distance(pos, center) <= dynamicRadius;
        }

        // 分形树分支结构
        struct Branch
        {
            public Vector2 start;
            public Vector2 direction;
            public float length;
            public float width;
            public int depth;

            public Branch(Vector2 start, Vector2 direction, float length, float width, int depth)
            {
                this.start = start;
                this.direction = direction;
                this.length = length;
                this.width = width;
                this.depth = depth;
            }
        }

        static bool IsFractalTree(Vector2 pos, Vector2 center, float maxSize)
        {
            // 初始化参数
            float angle = Random.Range(30f, 60f); // 分支角度
            float scaleFactor = Random.Range(0.6f, 0.8f); // 分支缩放比
            int iterations = 4; // 迭代次数

            // 转换为局部坐标
            Vector2 localPos = pos - center;
            Queue<Branch> branches = new Queue<Branch>();

            // 初始主干
            branches.Enqueue(new Branch(
                start: Vector2.zero,
                direction: Vector2.up,
                length: maxSize * 0.8f,
                width: maxSize * 0.3f,
                depth: 0
            ));

            // 迭代生成分支
            while (branches.Count > 0)
            {
                Branch current = branches.Dequeue();

                // 检查当前点是否在主干范围内
                if (IsInBranch(localPos, current))
                    return true;

                if (current.depth < iterations)
                {
                    // 生成左右分支
                    Vector2 leftDir = Quaternion.Euler(0, 0, angle) * current.direction;
                    branches.Enqueue(new Branch(
                        current.start + current.direction * current.length,
                        leftDir.normalized,
                        current.length * scaleFactor,
                        current.width * 0.7f,
                        current.depth + 1
                    ));

                    Vector2 rightDir = Quaternion.Euler(0, 0, -angle) * current.direction;
                    branches.Enqueue(new Branch(
                        current.start + current.direction * current.length,
                        rightDir.normalized,
                        current.length * scaleFactor,
                        current.width * 0.7f,
                        current.depth + 1
                    ));
                }
            }

            return false;
        }

        // 判断点是否在分支路径上
        static bool IsInBranch(Vector2 point, Branch branch)
        {
            Vector2 end = branch.start + branch.direction * branch.length;
            Vector2 lineDir = end - branch.start;
            float t = Mathf.Clamp01(Vector2.Dot(point - branch.start, lineDir) / lineDir.sqrMagnitude);
            Vector2 projection = branch.start + t * lineDir;
            float distance = Vector2.Distance(point, projection);

            // 动态宽度衰减
            float currentWidth = branch.width * Mathf.Pow(0.7f, branch.depth);
            return distance < currentWidth && t > 0 && t < 1;
        }

        static void AddShapeDetails(bool[,] board, float detailProbability)
        {
            // 添加随机孔洞和突出部
            for (int x = 1; x < board.GetLength(0) - 1; x++)
            {
                for (int y = 1; y < board.GetLength(1) - 1; y++)
                {
                    if (Random.value < detailProbability)
                    {
                        // 3x3区域翻转状态
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if (x + dx >= 0 && x + dx < board.GetLength(0) &&
                                    y + dy >= 0 && y + dy < board.GetLength(1))
                                {
                                    board[x + dx, y + dy] = !board[x + dx, y + dy];
                                }
                            }
                        }
                    }
                }
            }
        }

        // 边缘位置检测（防止生成边缘空格）
        static bool IsEdgePosition(bool[,] board, Vector2Int pos)
        {
            return pos.x == 0 || pos.x == board.GetLength(0) - 1 ||
                   pos.y == 0 || pos.y == board.GetLength(1) - 1;
        }

        static bool[,] PostProcess(bool[,] board, int difficulty)
        {
            // 连接性检查（使用泛洪算法）
            if (difficulty <= 3)
            {
                Vector2Int start = FindFirstNonHole(board);
                if (start.x == -1) return board;

                HashSet<Vector2Int> visited = new();
                FloodFill(board, start, visited);

                // 填充未连接区域
                for (int x = 0; x < board.GetLength(0); x++)
                {
                    for (int y = 0; y < board.GetLength(1); y++)
                    {
                        if (!visited.Contains(new Vector2Int(x, y)) && !board[x, y])
                            board[x, y] = true;
                    }
                }
            }

            // 单行/列检测（难度3的特殊处理）
            if (difficulty == 3)
            {
                for (int x = 0; x < board.GetLength(0); x++)
                {
                    if (IsSingleColumn(board, x))
                        FillColumn(board, x, false);
                }

                for (int y = 0; y < board.GetLength(1); y++)
                {
                    if (IsSingleRow(board, y))
                        FillRow(board, y, false);
                }
            }

            return board;
        }

        static void FloodFill(bool[,] board, Vector2Int pos, HashSet<Vector2Int> visited)
        {
            if (pos.x < 0 || pos.x >= board.GetLength(0)) return;
            if (pos.y < 0 || pos.y >= board.GetLength(1)) return;
            if (board[pos.x, pos.y]) return;
            if (visited.Contains(pos)) return;

            visited.Add(pos);

            FloodFill(board, new Vector2Int(pos.x + 1, pos.y), visited);
            FloodFill(board, new Vector2Int(pos.x - 1, pos.y), visited);
            FloodFill(board, new Vector2Int(pos.x, pos.y + 1), visited);
            FloodFill(board, new Vector2Int(pos.x, pos.y - 1), visited);
        }

        static Vector2Int FindFirstNonHole(bool[,] board)
        {
            for (int x = 0; x < board.GetLength(0); x++)
            for (int y = 0; y < board.GetLength(1); y++)
                if (!board[x, y])
                    return new Vector2Int(x, y);
            return new Vector2Int(-1, -1);
        }

        static bool IsSingleColumn(bool[,] board, int x)
        {
            int count = 0;
            for (int y = 0; y < board.GetLength(1); y++)
                if (!board[x, y])
                    count++;
            return count == 1;
        }

        static bool IsSingleRow(bool[,] board, int y)
        {
            int count = 0;
            for (int x = 0; x < board.GetLength(0); x++)
                if (!board[x, y])
                    count++;
            return count == 1;
        }

        static void FillColumn(bool[,] board, int x, bool value)
        {
            for (int y = 0; y < board.GetLength(1); y++)
                board[x, y] = value;
        }

        static void FillRow(bool[,] board, int y, bool value)
        {
            for (int x = 0; x < board.GetLength(0); x++)
                board[x, y] = value;
        }
    }
}
#endif