using System.Collections.Generic;
using GameConfig;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public class BoardInitSystem : IEcsInitSystem
    {
        private EcsWorld _world;
        private EcsPool<GridCellComponent> _gridCellPool;
        private EcsPool<BoardTag> _boardTagPool;
        private GameStateContext _context;
        private IElementFactoryService _elementFactory;
        private IMatchService _matchService;

        public void Init(IEcsSystems systems)
        {
            _context = systems.GetShared<GameStateContext>();
            _world = systems.GetWorld();
            _gridCellPool = _world.GetPool<GridCellComponent>();
            _boardTagPool = _world.GetPool<BoardTag>();
            _matchService = _context.ServiceFactory.GetService(_context.CurrentMatchType);
            _elementFactory = _context.ServiceFactory.GetElementFactoryService();
            _elementFactory.SetSpawnStrategy(_matchService.GetSpawnStrategy());

            LevelData levelData = _context.CurrentLevel;
            _context.Board.Initialize(levelData);

            // 1. 先创建所有格子的实体 (容器)
            CreateGridEntities(levelData);

            // 2. 再填充格子内的棋子 (内容)
            FillElements(levelData);
            
            CalculateGlobalDropQuotas();
            AnalyzeTargetColumns();
        }

        private void CreateGridEntities(LevelData levelData)
        {
            for (int x = 0; x < levelData.gridCol; x++)
            {
                for (int y = 0; y < levelData.gridRow; y++)
                {
                    int gridEntity = _world.NewEntity();
                    _boardTagPool.Add(gridEntity);

                    ref var gridComp = ref _gridCellPool.Add(gridEntity);
                    gridComp.Position = new Vector2Int(x, y);
                    gridComp.WorldPosition = MatchPosUtil.CalculateWorldPosition(x, y, 1, 1, ElementDirection.None);
                    gridComp.IsBlank = levelData.grid[x][y].isWhite;
                    gridComp.StackedEntityIds = new List<int>();

                    // 注册到 Board 查找表
                    _context.Board.RegisterGridEntity(x, y, gridEntity, gridComp.IsBlank);
                }
            }
        }

        private void FillElements(LevelData levelData)
        {
            // 遍历所有格子
            for (int x = 0; x < levelData.gridCol; x++)
            {
                for (int y = 0; y < levelData.gridRow; y++)
                {
                    if (levelData.grid[x][y].isWhite)
                        continue;
                    // 1. 获取该位置的配置信息
                    var holdInfos = levelData.FindCoordHoldGridInfo(x, y);
                    // 2. 处理固定配置的元素
                    if (holdInfos != null && holdInfos.Count > 0)
                    {
                        foreach (var info in holdInfos)
                        {
                            // 只有当当前遍历的 (x,y) 是这个元素的“起始位置”时，才创建实体
                            if (info.StartCoord.x == x && info.StartCoord.y == y)
                            {
                                int width = Mathf.Max(info.ElementWidth, 1);
                                int height = Mathf.Max(info.ElementHeight, 1);

                                // 创建实体
                                int entity = _elementFactory.CreateElementEntity(_context, _matchService,
                                    info.ElementId,ElementBuildSource.Config,
                                    x, y, width, height);

                                // 将实体ID填入它占据的所有格子中
                                FillElementEntityToGrids(entity, x, y, width, height);
                            }
                        }
                    }

                    // 3. 生成基础棋子
                    if (IsCellEmpty(x, y))
                    {
                        int configId = levelData.initColor[Random.Range(0, levelData.initColor.Length)];
                        int entity = _elementFactory.CreateElementEntity(_context, _matchService, configId,ElementBuildSource.Config, x, y, 1, 1);
                        FillElementEntityToGrids(entity, x, y, 1, 1);
                        
                        // 标记为随机生成的棋子
                        var randomTagPool = _world.GetPool<RandomGeneratedTag>();
                        randomTagPool.Add(entity);
                    }
                }
            }
        }

        /// <summary>
        /// 将实体ID注册到它覆盖的所有格子上
        /// </summary>
        private void FillElementEntityToGrids(int entityId, int startX, int startY, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    // start这里在上一步已经填充了，这里只需要补充它占据的其它格子
                    int cx = startX + i;
                    int cy = startY + j;

                    // 获取 (cx, cy) 处的格子组件
                    var gridEntity = _context.Board[cx, cy];
                    ref var gridComp = ref _gridCellPool.Get(gridEntity);
                    gridComp.StackedEntityIds.Add(entityId);
                }
            }
        }

        private bool IsCellEmpty(int x, int y)
        {
            int gridEntity = _context.Board[x, y];
            ref var gridComp = ref _gridCellPool.Get(gridEntity);
            //1.格子是空白的，当然不能填充任何棋子
            if (gridComp.IsBlank) return false;

            //2.格子上没有任何棋子，自然是可以填充的
            if (gridComp.StackedEntityIds.Count == 0) return true;

            //3.如何有东西，那就检查是否可以阻挡棋子生成的
            var elementPool = _world.GetPool<ElementComponent>();
            foreach (var entity in gridComp.StackedEntityIds)
            {
                ref var elementComp = ref elementPool.Get(entity);
                if (_matchService.IsBlockingBaseElement(elementComp.ConfigId))
                    return false;
            }

            //默认会填充
            return true;
        }

        /// <summary>
        /// 计算掉落目标和棋盘上已配置的棋子缺口
        /// </summary>
        private void CalculateGlobalDropQuotas()
        {
            LevelData level = _context.CurrentLevel;
            var quotas = _context.MatchStateContext.GlobalDropQuotas;
            quotas.Clear();
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();

            // 1. 遍历所有收集目标
            if (level.target == null) return;

            foreach (var target in level.target)
            {
                int targetId = target.targetId;
                int totalNeeded = target.targetNum;

                // 2. 统计棋盘上已经存在的数量
                int existingCount = 0;

                foreach (var rowData in level.grid)
                {
                    if (rowData == null) continue;
                    foreach (var cell in rowData)
                    {
                        if (cell == null || cell.elements == null) continue;
                        foreach (var ele in cell.elements)
                        {
                            // 循环类型的元素不要计算缺口
                            if(db.IsCircleElement(ele.id))
                                continue;
                            // 扩散出来的元素也不需要计算缺口
                            if (db[ele.id].elementType == ElementType.RandomDiffuse && !string.IsNullOrEmpty(db[ele.id].extra))
                            {
                                if (int.TryParse(db[ele.id].extra.Split("|")[0], out int genId))
                                {
                                    if(genId == targetId)
                                        continue;
                                }
                            }

                            if (MatchElementUtil.IsContributingToTarget(targetId, ele.id))
                            {
                                existingCount++;
                            }
                        }
                    }
                }

                // 3. 计算缺口
                int dropQuota = totalNeeded - existingCount;
                if (dropQuota < 0) dropQuota = 0;

                // 4. 记录配额
                if (!quotas.ContainsKey(targetId) && db[targetId].elementType == ElementType.DropBlock)
                {
                    quotas.Add(targetId, dropQuota);
                }
            }
        }
        
        private void AnalyzeTargetColumns()
        {
            LevelData level = _context.CurrentLevel;
            var targetCols = _context.MatchStateContext.TargetValidColumns;
            targetCols.Clear();

            if (level.target == null || level.grid == null) return;

            // 1. 遍历所有目标类型
            foreach (var target in level.target)
            {
                int targetId = target.targetId;

                // 2. 扫描棋盘
                for (int x = 0; x < level.grid.Length; x++) // x 是列
                {
                    var colData = level.grid[x];
                    if (colData == null) continue;

                    for (int y = 0; y < colData.Length; y++)
                    {
                        var cell = colData[y];
                        if (cell == null || cell.elements == null) continue;

                        foreach (var ele in cell.elements)
                        {
                            // 该位置的棋子是否属于当前目标
                            if (MatchElementUtil.IsContributingToTarget(targetId, ele.id))
                            {
                                // 记录这一列为该目标的合法产出地
                                _context.MatchStateContext.RegisterTargetColumn(targetId, x);
                        
                                // 既然这一列已经合法了，不需要再看这一列的其他行了，跳出到下一列
                                goto NextColumn; 
                            }
                        }
                    }
                    NextColumn:;
                }
            }
        }
    }
}