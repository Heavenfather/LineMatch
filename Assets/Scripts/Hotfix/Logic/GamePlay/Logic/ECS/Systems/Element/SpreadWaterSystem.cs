using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using HotfixLogic.Match;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 蔓延水系统
    /// 处理水的蔓延逻辑：当含水格子旁边消除棋子时，水会蔓延到相邻格子
    /// </summary>
    public class SpreadWaterSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private GameStateContext _context;
        private IBoard _board;
        private IElementFactoryService _elementFactory;
        private IMatchService _matchService;

        private EcsFilter _spreadFilter;
        private EcsPool<SpreadWaterSpreadComponent> _spreadPool;
        private EcsPool<SpreadWaterComponent> _waterPool;
        private EcsPool<ElementComponent> _elementPool;
        private EcsPool<GridCellComponent> _gridCellPool;
        private EcsPool<VisualBusyComponent> _busyPool;

        // BFS搜索用的数据结构
        private Queue<Vector2Int> _searchQueue = new Queue<Vector2Int>(100);
        private HashSet<Vector2Int> _visited = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> _infectCoords = new HashSet<Vector2Int>();

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _board = _context.Board;
            _elementFactory = MatchBoot.Container.Resolve<IElementFactoryService>();
            _matchService = _context.ServiceFactory.GetService(_context.CurrentMatchType);

            _spreadFilter = _world.Filter<SpreadWaterSpreadComponent>().End();
            _spreadPool = _world.GetPool<SpreadWaterSpreadComponent>();
            _waterPool = _world.GetPool<SpreadWaterComponent>();
            _elementPool = _world.GetPool<ElementComponent>();
            _gridCellPool = _world.GetPool<GridCellComponent>();
            _busyPool = _world.GetPool<VisualBusyComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _spreadFilter)
            {
                ref var spread = ref _spreadPool.Get(entity);

                if (!spread.IsProcessed)
                {
                    // 标记为忙碌，阻止掉落
                    if (!_busyPool.Has(entity))
                    {
                        _busyPool.Add(entity);
                    }

                    ProcessWaterSpread(entity, ref spread);
                    spread.IsProcessed = true;
                }
            }
        }

        /// <summary>
        /// 处理水的蔓延
        /// </summary>
        private void ProcessWaterSpread(int entity, ref SpreadWaterSpreadComponent spread)
        {
            if (spread.EliminatedCoords == null || spread.EliminatedCoords.Count == 0)
            {
                if (_busyPool.Has(entity))
                    _busyPool.Del(entity);
                return;
            }

            // 1. 初始化蔓延目标
            _infectCoords.Clear();
            InitializeInfection(spread.EliminatedCoords);

            // 2. BFS扩散查找所有可蔓延的格子
            SpreadInfection(spread.EliminatedCoords);

            // 3. 保存蔓延目标
            spread.SpreadTargets = new HashSet<Vector2Int>(_infectCoords);

            if (_infectCoords.Count == 0)
            {
                if (_busyPool.Has(entity))
                    _busyPool.Del(entity);
                return;
            }

            // 4. 创建水元素到目标格子
            CreateWaterElements(spread.WaterConfigId, _infectCoords);

            // 5. 添加分数
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            int baseScore = db[spread.WaterConfigId].score;
            MatchManager.Instance.AddScore(baseScore * _infectCoords.Count);

            LevelTargetSystem.Instance.ResumeTarget(spread.WaterConfigId, _infectCoords.Count);
        }

        /// <summary>
        /// 初始化蔓延目标（第一层）
        /// /// </summary>
        private void InitializeInfection(List<Vector2Int> eliminatedCoords)
        {
            _searchQueue.Clear();
            _visited.Clear();

            var dirs = MatchPosUtil.NeighborDirs;

            // 遍历所有被消除的格子
            foreach (var coord in eliminatedCoords)
            {
                // 如果该格子不能被蔓延，跳过
                if (!CanInfect(coord))
                    continue;

                // 检查该格子的四个方向是否有水
                bool hasWaterNearby = false;
                foreach (var dir in dirs)
                {
                    Vector2Int neighborCoord = coord + dir;

                    // 如果邻居格子有水，标记为可蔓延
                    if (HasWater(neighborCoord))
                    {
                        hasWaterNearby = true;
                        break;
                    }
                }

                // 如果旁边有水，将该格子加入蔓延目标
                if (hasWaterNearby)

                    _searchQueue.Enqueue(coord);
                _infectCoords.Add(coord);
                _visited.Add(coord);
            }
        }

        /// <summary>
        /// BFS扩散蔓延
        /// </summary>
        private void SpreadInfection(List<Vector2Int> eliminatedCoords)
        {
            int safetyCounter = 0;
            const int maxOperations = 100;

            var dirs = MatchPosUtil.NeighborDirs;
            while (_searchQueue.Count > 0 && safetyCounter++ < maxOperations)
            {
                Vector2Int current = _searchQueue.Dequeue();

                // 检查四个方向
                foreach (var dir in dirs)
                {
                    Vector2Int neighbor = current + dir;

                    // 如果邻居在消除列表中，且可以被蔓延，且未访问过
                    if (eliminatedCoords.Contains(neighbor) && CanInfect(neighbor) && _visited.Add(neighbor))
                    {
                        _infectCoords.Add(neighbor);
                        _searchQueue.Enqueue(neighbor);
                    }
                }
            }

            if (safetyCounter >= maxOperations)
            {
                Logger.Warning("SpreadWaterSystem: BFS reached max operations limit!");
            }
        }

        /// <summary>
        /// 检查格子是否有水
        /// </summary>
        private bool HasWater(Vector2Int coord)
        {
            if (!_board.TryGetGridEntity(coord.x, coord.y, out int gridEntity))
                return false;

            ref var gridCell = ref _gridCellPool.Get(gridEntity);
            if (gridCell.IsBlank || gridCell.StackedEntityIds == null)
                return false;

            foreach (var entityId in gridCell.StackedEntityIds)
            {
                if (_waterPool.Has(entityId))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判断格子是否可以被蔓延
        /// </summary>
        private bool CanInfect(Vector2Int coord)
        {
            // 1. 检查坐标是否有效
            if (!_board.TryGetGridEntity(coord.x, coord.y, out int gridEntity))
                return false;

            ref var gridCell = ref _gridCellPool.Get(gridEntity);

            // 2. 空白格子不能蔓延
            if (gridCell.IsBlank)
                return false;

            // 3. 检查格子上的元素
            if (gridCell.StackedEntityIds == null || gridCell.StackedEntityIds.Count == 0)
                return true; // 空格子可以蔓延

            foreach (var entityId in gridCell.StackedEntityIds)
            {
                if (!_elementPool.Has(entityId))
                    continue;

                ref var element = ref _elementPool.Get(entityId);

                // 4. 如果已经有水，不能再蔓延
                if (_waterPool.Has(entityId))
                    return false;

                // 5. 只能蔓延到Normal棋子或特殊功能棋子
                if (element.Type != ElementType.Normal && !_matchService.IsSpecialElement(element.ConfigId))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 创建水元素到目标格子
        /// </summary>
        private void CreateWaterElements(int waterConfigId, HashSet<Vector2Int> targetCoords)
        {
            foreach (var coord in targetCoords)
            {
                if (!_board.TryGetGridEntity(coord.x, coord.y, out int gridEntity))
                    continue;

                ref var gridCell = ref _gridCellPool.Get(gridEntity);

                // 检查是否已经有水
                bool hasWater = false;
                if (gridCell.StackedEntityIds != null)
                {
                    foreach (var entityId in gridCell.StackedEntityIds)
                    {
                        if (_waterPool.Has(entityId))
                        {
                            hasWater = true;
                            break;
                        }
                    }
                }

                if (hasWater)
                    continue;

                // 创建水元素
                int waterEntity = _elementFactory.CreateElementEntity(
                    _context,
                    _matchService,
                    waterConfigId,
                    ElementBuildSource.Dynamic,
                    coord.x, coord.y,
                    1, 1
                );

                // 添加到格子
                if (gridCell.StackedEntityIds == null)
                {
                    gridCell.StackedEntityIds = new List<int>();
                }

                gridCell.StackedEntityIds.Insert(0, waterEntity); //放在最底层
            }
        }
    }
}