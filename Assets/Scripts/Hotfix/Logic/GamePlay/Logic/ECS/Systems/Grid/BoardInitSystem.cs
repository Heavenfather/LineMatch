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
                    _context.Board.RegisterGridEntity(x, y, gridEntity,gridComp.IsBlank);
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
                                int entity = _elementFactory.CreateElementEntity(_context, _matchService, info.ElementId,
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
                        int entity = _elementFactory.CreateElementEntity(_context, _matchService, configId, x, y);
                        FillElementEntityToGrids(entity, x, y, 1, 1);
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

    }
}