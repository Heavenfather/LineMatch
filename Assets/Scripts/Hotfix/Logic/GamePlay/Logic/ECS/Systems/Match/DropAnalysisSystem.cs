using System.Collections.Generic;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 掉落分析系统
    /// 分析消除了哪些元素，哪一列需要重新生成和掉落,生产掉落元素数据
    /// </summary>
    public class DropAnalysisSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private IBoard _board;
        private GameStateContext _context;

        private EcsPool<GridCellComponent> _gridPool;
        private EcsPool<ElementComponent> _elePool;
        private EcsPool<ElementPositionComponent> _posPool;
        private EcsPool<ElementRenderComponent> _eleRenderPool;

        private EcsPool<FallAnimationComponent> _fallAnimPool;
        private EcsPool<DropSpawnRequestComponent> _spawnReqPool;
        private EcsFilter _requestFilter;
        private EcsFilter _pendingFilter;

        private List<Vector2Int> _cachedSections;
        private List<int> _cachedSpawnRows;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _board = _context.Board;

            _gridPool = _world.GetPool<GridCellComponent>();
            _elePool = _world.GetPool<ElementComponent>();
            _posPool = _world.GetPool<ElementPositionComponent>();
            _eleRenderPool = _world.GetPool<ElementRenderComponent>();
            _requestFilter = _world.Filter<MatchRequestComponent>().End();
            _pendingFilter = _world.Filter<PendingActionsComponent>().End();
            _fallAnimPool = _world.GetPool<FallAnimationComponent>();
            _spawnReqPool = _world.GetPool<DropSpawnRequestComponent>();

            _cachedSections = new List<Vector2Int>(10);
            _cachedSpawnRows = new List<int>(20);
        }

        public void Run(IEcsSystems systems)
        {
            if (!_board.IsBoardDirty)
                return;
            if(_requestFilter.GetEntitiesCount() > 0)
                return; //还有请求，不要分析
            if(_pendingFilter.GetEntitiesCount() > 0)
                return; //还有请求，不要分析
            bool hasAnyChange = false;

            // 1. 逐列处理
            for (int x = 0; x < _board.Width; x++)
            {
                _cachedSections.Clear();
                _cachedSpawnRows.Clear();

                // 2. 获取分段区间
                CalculateDroppableSections(x);

                // 3. 遍历区间处理掉落
                foreach (var section in _cachedSections)
                {
                    // 在区间内执行双指针掉落，如果有空位，ProcessSectionDrop 会把空行号加到 _cachedSpawnRows
                    ProcessSectionDrop(x, section.x, section.y);
                }

                // 4. 只有当确实需要生成时，才创建请求和分配内存
                if (_cachedSpawnRows.Count > 0)
                {
                    CreateSpawnRequest(x, _cachedSpawnRows);
                    hasAnyChange = true;
                    // Logger.Debug($"掉落分析：生成列{x}请求：{string.Join("-",_cachedSpawnRows)}");
                }
            }

            _board.IsBoardDirty = false;
        }

        /// <summary>
        /// 计算某一列的所有可掉落区间
        /// </summary>
        private void CalculateDroppableSections(int col)
        {
            int startY = 0;
            // 遍历该列所有行
            for (int y = 0; y < _board.Height; y++)
            {
                if (IsFixedObstacle(col, y))
                {
                    // 遇到障碍物，结算当前区间 [startY, y-1]
                    if (y > startY)
                    {
                        _cachedSections.Add(new Vector2Int(startY, y - 1));
                    }

                    // 新起点跳过障碍物
                    startY = y + 1;
                }
            }

            // 结算最后一个区间
            if (startY < _board.Height)
            {
                _cachedSections.Add(new Vector2Int(startY, _board.Height - 1));
            }
        }

        private bool IsFixedObstacle(int x, int y)
        {
            if (!_board.IsValid(x, y)) return true; // 边界外视为障碍

            int gridEnt = _board[x, y];
            ref var grid = ref _gridPool.Get(gridEnt);

            // 只要格子上有不可移动的元素，就视为阻挡掉落的障碍
            if (!grid.IsBlank && grid.StackedEntityIds != null)
            {
                foreach (var id in grid.StackedEntityIds)
                {
                    if (_elePool.Has(id))
                    {
                        ref var ele = ref _elePool.Get(id);
                        // 如果存在不可移动 且 不是即将销毁的棋子
                        if (!ele.IsMovable && ele.HoldGrid >= 1 && ele.LogicState != ElementLogicalState.Dying)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 创建掉落生成请求实体，给 DropElementSpawnSystem 进行消费
        /// </summary>
        /// <param name="col"></param>
        /// <param name="rows"></param>
        private void CreateSpawnRequest(int col, List<int> rows)
        {
            int entity = _world.NewEntity();
            ref var req = ref _spawnReqPool.Add(entity);
            req.Column = col;
            req.TargetRows = new List<int>(rows);
        }

        /// <summary>
        /// 处理区间内的掉落逻辑，将产生的空位行号填入
        /// </summary>
        private void ProcessSectionDrop(int col, int startY, int bottomY)
        {
            int writeY = bottomY; // 写入指针从底部开始

            // 双指针扫描：从底部向上部扫描
            for (int readY = bottomY; readY >= startY; readY--)
            {
                int movableEntity = GetMovableEntity(col, readY);

                if (movableEntity != -1) // 读到了有效棋子
                {
                    if (readY != writeY)
                    {
                        // 发生掉落：把 readY 的棋子 拉到 writeY
                        MoveElementLogic(movableEntity, col, readY, col, writeY);
                    }
                    // 填满了一个位置，写入指针向上移
                    writeY--;
                }
            }

            // 处理完后，[startY, writeY] 的部分就是空的，需要生成填充
            for (int y = writeY; y >= startY; y--)
            {
                _cachedSpawnRows.Add(y);
            }
        }

        private int GetMovableEntity(int x, int y)
        {
            if (!_board.IsValid(x, y)) return -1;
            int gridEnt = _board[x, y];
            ref var grid = ref _gridPool.Get(gridEnt);

            if (grid.IsBlank || grid.StackedEntityIds == null) return -1;

            foreach (var id in grid.StackedEntityIds)
            {
                if (_elePool.Has(id))
                {
                    ref var ele = ref _elePool.Get(id);
                    // 必须是可移动的
                    if (ele.IsMovable && ele.LogicState != ElementLogicalState.Dying)
                    {
                        return id;
                    }
                }
            }

            return -1;
        }

        private void MoveElementLogic(int entity, int oldX, int oldY, int newX, int newY)
        {
            // 1. 修改 Position 组件
            ref var pos = ref _posPool.Get(entity);
            pos.X = newX;
            pos.Y = newY;

            // 2. 修改 Grid 引用 移旧补新
            RemoveFromGrid(oldX, oldY, entity);
            AddToGrid(newX, newY, entity);

            // 3. 标记动画
            if (!_fallAnimPool.Has(entity))
            {
                ref var anim = ref _fallAnimPool.Add(entity);
                anim.FromGrid = new Vector2Int(oldX, oldY);
                anim.ToGrid = new Vector2Int(newX, newY);
            }
            else
            {
                ref var anim = ref _fallAnimPool.Get(entity);
                anim.ToGrid = new Vector2Int(newX, newY);
            }
        }


        private void RemoveFromGrid(int x, int y, int entityId)
        {
            int gridEntity = _board[x, y];
            ref var grid = ref _gridPool.Get(gridEntity);
            if (grid.StackedEntityIds != null)
            {
                grid.StackedEntityIds.Remove(entityId);
            }
        }

        private void AddToGrid(int x, int y, int entityId)
        {
            int gridEntity = _board[x, y];
            ref var grid = ref _gridPool.Get(gridEntity);
            if (grid.StackedEntityIds == null) grid.StackedEntityIds = new List<int>();
            grid.StackedEntityIds.Add(entityId);
            ref var renderCom = ref _eleRenderPool.Get(entityId);
            if (renderCom.ViewInstance != null)
            {
                var gridViewInstance = _board.GetGridInstance(x, y);
                renderCom.ViewInstance.transform.SetParent(gridViewInstance?.transform);
            }
        }
    }
}