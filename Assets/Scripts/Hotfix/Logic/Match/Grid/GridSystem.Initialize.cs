using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameConfig;
using HotfixCore.MemoryPool;
using UnityEngine;

namespace HotfixLogic.Match
{
    public partial class GridSystem
    {
        /// <summary>
        /// 初始化网格数据
        /// </summary>
        private void InitializeGrid()
        {
            if (_levelData == null) return;
            _grid = new GridItemData[_levelData.gridCol, _levelData.gridRow];

            // 设置动态难度调整参数
            var context =
                DifficultyStrategyManager.Instance.CreateContext(in this._levelData, this._levelData.stepLimit);
            DifficultyStrategyManager.Instance.CalculateDifficultyStrategies(context);
            
            _boardGridCount = 0;
            int index = 0;
            for (int x = 0; x < _levelData.gridCol; x++)
            {
                for (int y = 0; y < _levelData.gridRow; y++)
                {
                    ++index;
                    _grid[x, y] = BuildInitGridData(x, y, index);
                }
            }
        }

        /// <summary>
        /// 获取所有有效可操作位置（非障碍物+有可操作元素）
        /// </summary>
        public List<Vector2Int> GetAllValidPositions()
        {
            List<Vector2Int> validPositions = new List<Vector2Int>();

            // 并行遍历提高大棋盘效率
            Parallel.For(0, _levelData.gridCol, x =>
            {
                for (int y = 0; y < _levelData.gridRow; y++)
                {
                    if (IsValidPosition(x, y) && IsOperationalTile(x, y))
                    {
                        lock (validPositions)
                        {
                            validPositions.Add(new Vector2Int(x, y));
                        }
                    }
                }
            });

            return validPositions.OrderBy(p => p.x).ThenBy(p => p.y).ToList();
        }

        /// <summary>
        /// 判断是否为可操作棋子（非障碍物+非特殊类型）
        /// </summary>
        private bool IsOperationalTile(int x, int y)
        {
            ElementItemData topElement = _grid[x, y].GetTopElement();
            return topElement != null && topElement.ElementType == ElementType.Normal;
        }

        public bool IsValidPosition(int x, int y)
        {
            // 基础范围检查
            if (x < 0 || x >= _levelData.gridCol) return false;
            if (y < 0 || y >= _levelData.gridRow) return false;
            if (_grid[x, y].IsWhite) return false;
            return true;
        }

        public bool IsLimitPosition(int x, int y)
        {
            if (x < 0 || x >= _levelData.gridCol) return false;
            if (y < 0 || y >= _levelData.gridRow) return false;
            return true;
        }

        public bool IsWhitePos(int x, int y)
        {
            return _grid[x, y].IsWhite;
        }

        /// <summary>
        /// 生成格子数据
        /// </summary>
        /// <returns></returns>
        private GridItemData BuildInitGridData(int x, int y, int index)
        {
            var element = _levelData.grid[x][y];
            var gridData = MemoryPool.Acquire<GridItemData>();
            gridData.UId = index;
            gridData.Coord = new Vector2Int(x, y);
            gridData.IsWhite = element.isWhite;
            if (!gridData.IsWhite)
                _boardGridCount++;

            bool needBaseElement = !element.isWhite && IsCanBuildBottomElement(x, y);
            ElementItemData baseElement = null;
            if (needBaseElement)
            {
                int[] currentRates = _levelData.initColorRate;
                
                // 动态调整难度策略3 --- 调整开局独立棋子的概率
                float modifier = DifficultyStrategyManager.Instance.GetIndependentRatioModifier();
                if (modifier != 0)
                {
                    int[] tempRates = (int[])_levelData.initColorRate.Clone();
                    //寻找已生成的邻居棋子
                    HashSet<int> neighborIds = new HashSet<int>();
                    // 因为棋盘的生成是从上往下从左往右生成 所以需要通过获取左边和上边
                    //左边
                    if(x > 0 && _grid[x - 1, y] != null && _grid[x - 1, y].GetTopElement() != null &&
                       _grid[x - 1, y].GetTopElement().ElementType == ElementType.Normal)
                        neighborIds.Add(_grid[x - 1, y].GetTopElement().ConfigId);
                    //上边
                    if (y > 0 && _grid[x, y - 1] != null && _grid[x, y - 1].GetTopElement() != null &&
                        _grid[x, y - 1].GetTopElement().ElementType == ElementType.Normal)
                        neighborIds.Add(_grid[x, y - 1].GetTopElement().ConfigId);

                    if (neighborIds.Count > 0)
                    {
                        // 将 modifier (如 -0.15) 转换为权重的增加
                        // 简单算法：给邻居颜色的权重增加 15% - 30%
                        int totalRate = 0;
                        foreach(var r in tempRates) totalRate += r;
                
                        // 比如增加总权重给邻居颜色
                        int bonus = Mathf.Abs((int)(totalRate * modifier));
                
                        for (int i = 0; i < _levelData.initColor.Length; i++)
                        {
                            if (neighborIds.Contains(_levelData.initColor[i]))
                            {
                                tempRates[i] += bonus;
                            }
                        }
                        currentRates = tempRates; // 使用修改后的概率
                    }
                }
                
                int[] colorId = ElementSystem.Instance.PickElementDynamicAdjustment(
                    _levelData.initColor, currentRates, 1, x, true);
                baseElement = ElementSystem.Instance.GenElementItemData(colorId[0], x, y);
            }

            var eleInfos = _levelData.FindCoordHoldGridInfo(x, y);
            if (eleInfos == null || eleInfos.Count == 0)
            {
                //没有配置任何的棋子 随机从基础棋子里面取
                if (needBaseElement)
                {
                    gridData.PushElement(baseElement);
                }
            }
            else
            {
                //配了棋子
                // 预处理所有元素配置
                var overlayElements = new List<ElementItemData>();
                var bottomElements = new List<ElementItemData>();
                var normalElements = new List<ElementItemData>();
                foreach (var info in eleInfos)
                {
                    var ele = ElementSystem.Instance.GenElementItemData(info.ElementId, x, y);
                    if (ele.HoldGrid > 0 && ele.HoldGrid < 1)
                    {
                        overlayElements.Add(ele); //半透明类型元素
                    }
                    else if (ele.HoldGrid == 0)
                    {
                        bottomElements.Add(ele); //底板类型元素
                    }
                    else
                    {
                        if (info.StartCoord.x == x && info.StartCoord.y == y)
                        {
                            normalElements.Add(ele);
                        }

                        needBaseElement = false;
                    }
                }

                //先添加底板类型元素
                foreach (var ele in bottomElements)
                {
                    gridData.PushElement(ele);
                }

                if (needBaseElement)
                {
                    gridData.PushElement(baseElement);
                }

                foreach (var ele in normalElements) gridData.PushElement(ele);

                //再添加覆盖类型的元素
                foreach (var ele in overlayElements) gridData.PushElement(ele);
            }

            return gridData;
        }

        /// <summary>
        /// 是否可以初始化基础元素到底板上
        /// </summary>
        private bool IsCanBuildBottomElement(int coordX, int coordY)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            for (int y = _levelData.gridRow - 1; y >= 0; y--)
            {
                if (y > coordY)
                    continue;
                var l = _levelData.FindCoordHoldGridInfo(coordX, y);
                if (l == null)
                    continue;
                //暂时先注释掉，这个需求还是太奇怪了
                // for (int i = 0; i < l.Count; i++)
                // {
                //     if (db[l[i].ElementId].elementType == ElementType.Block)
                //         return false;
                // }
            }

            return true;
        }

        /// <summary>
        /// 生成格子实体对象
        /// </summary>
        private UniTask FillInitialGrids()
        {
            //初始化填充网格
            UniTaskCompletionSource taskCompletionSource = new UniTaskCompletionSource();
            _gridItems ??= new Dictionary<int, GridItem>();
            _coordToGridItems ??= new Dictionary<Vector2Int, GridItem>();
            _gridItems.Clear();
            _coordToGridItems.Clear();
            for (int x = 0; x < _levelData.gridCol; x++)
            {
                for (int y = 0; y < _levelData.gridRow; y++)
                {
                    GridItem gridItem = MemoryPool.Acquire<GridItem>();
                    if (_grid[x, y].IsWhite)
                        continue;
                    gridItem.CreateSelf(_grid[x, y], gridBoard, GetGridPositionByCoord(x, y),
                            () =>
                            {
                                _gridItems.Add(gridItem.GameObject.gameObject.GetInstanceID(), gridItem);
                                _coordToGridItems.Add(gridItem.Data.Coord, gridItem);
                                if (_gridItems.Count == _boardGridCount)
                                {
                                    taskCompletionSource.TrySetResult();
                                }
                            })
                        .Forget();
                }
            }

            return taskCompletionSource.Task;
        }
    }
}