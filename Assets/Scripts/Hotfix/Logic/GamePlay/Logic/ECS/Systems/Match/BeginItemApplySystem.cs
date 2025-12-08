using System.Collections.Generic;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 开局道具应用系统
    /// 将开局道具和连胜奖励道具应用到棋盘上
    /// </summary>
    public class BeginItemApplySystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private GameStateContext _context;
        private IMatchService _matchService;
        private IElementFactoryService _elementFactory;
        private IBoard _board;

        private EcsFilter _resultItemsFilter;
        private EcsPool<GridCellComponent> _gridCellPool;
        private EcsPool<ElementComponent> _elementPool;
        private EcsPool<ElementPositionComponent> _positionPool;
        private EcsPool<PendingActionsComponent> _pendingActionsPool;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _matchService = _context.ServiceFactory.GetService(_context.CurrentMatchType);
            _elementFactory = MatchBoot.Container.Resolve<IElementFactoryService>();
            _board = _context.Board;

            _resultItemsFilter = _world.Filter<GameContinueRequestComponent>().End();
            _gridCellPool = _world.GetPool<GridCellComponent>();
            _elementPool = _world.GetPool<ElementComponent>();
            _positionPool = _world.GetPool<ElementPositionComponent>();
            _pendingActionsPool = _world.GetPool<PendingActionsComponent>();

            ApplyBonusItems();
        }


        public void Run(IEcsSystems systems)
        {
            // 后面完善这个功能时，连胜道具生成的时机不一致，到时需要 新建一个实体 然后监听那个实体是否仍然存在再来执行连胜道具的展示
            // 现在就只在初始化的时候执行一遍

            // 复活道具
            if (_resultItemsFilter.GetEntitiesCount() > 0)
            {
                foreach (var entity in _resultItemsFilter)
                {
                    ref var resultCom = ref _world.GetPool<GameContinueRequestComponent>().Get(entity);
                    ApplyContinueItems(resultCom.ContinueElements);
                    _world.DelEntity(entity);
                }

                return;
            }
        }

        private void ApplyContinueItems(List<int> items)
        {
            var replacePlan = _matchService.GetGameContinueBestApplyPositions(_world, items);

            foreach (var kvp in replacePlan)
            {
                Vector2Int coord = kvp.Key;
                int newConfigId = kvp.Value;

                CreateSpawn2OtherAction(coord, newConfigId);
            }
        }

        private void CreateSpawn2OtherAction(Vector2Int coord, int configId)
        {
            int actionEntity = _world.NewEntity();
            ref var pending = ref _pendingActionsPool.Add(actionEntity);
            pending.Actions = new List<AtomicAction>()
            {
                new AtomicAction()
                {
                    Type = MatchActionType.Spawn2Other,
                    ExtraData = new GenItemData()
                    {
                        ConfigId = configId,
                        GenCoord = coord,
                        ElementSize = Vector2Int.one
                    }
                }
            };
        }

        private void ApplyBonusItems()
        {
            var beginUseElements = MatchManager.Instance.GetBeginUseElements();
            var winStreakElements = MatchManager.Instance.GetWinStreakElements();
            // 1. 获取所有需要生成的道具
            List<int> bonusItems = new List<int>(beginUseElements);
            bonusItems.AddRange(winStreakElements);
            if (bonusItems.Count == 0) return;

            // 2. 获取可替换目标格子
            Dictionary<int, int> replacePlan = _matchService.GetBonusSpawnPositions(_world, bonusItems);

            // 3. 执行替换
            foreach (var kvp in replacePlan)
            {
                int oldEntity = kvp.Key;
                int newConfigId = kvp.Value;

                ReplaceElement(oldEntity, newConfigId);
            }

            MatchManager.Instance.ClearBeginUseElements();
        }

        /// <summary>
        /// 替换棋子为功能棋子
        /// </summary>
        private void ReplaceElement(int oldEntityId, int newFunctionElementId)
        {
            // 1. 获取旧棋子的位置信息
            if (!_positionPool.Has(oldEntityId))
                return;

            ref var oldPos = ref _positionPool.Get(oldEntityId);
            int x = oldPos.X;
            int y = oldPos.Y;

            // 2. 获取旧棋子的尺寸
            ref var oldElement = ref _elementPool.Get(oldEntityId);
            int width = oldElement.Width;
            int height = oldElement.Height;

            // 3. 从格子中移除旧棋子
            RemoveElementFromGrids(oldEntityId, x, y, width, height);

            // 4. 删除旧棋子实体
            _world.DelEntity(oldEntityId);

            // 5. 创建新的功能棋子
            int newEntityId = _elementFactory.CreateElementEntity(
                _context,
                _matchService,
                newFunctionElementId,
                ElementBuildSource.Dynamic,
                x, y,
                width, height
            );

            // 6. 将新棋子添加到格子中
            AddElementToGrids(newEntityId, x, y, width, height);
        }

        /// <summary>
        /// 从格子中移除实体
        /// </summary>
        private void RemoveElementFromGrids(int entityId, int startX, int startY, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int cx = startX + i;
                    int cy = startY + j;

                    if (!_board.TryGetGridEntity(cx, cy, out int gridEntity))
                        continue;

                    ref var gridCell = ref _gridCellPool.Get(gridEntity);
                    if (gridCell.StackedEntityIds != null)
                    {
                        gridCell.StackedEntityIds.Remove(entityId);
                    }
                }
            }
        }

        /// <summary>
        /// 将实体添加到格子中
        /// </summary>
        private void AddElementToGrids(int entityId, int startX, int startY, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int cx = startX + i;
                    int cy = startY + j;

                    if (!_board.TryGetGridEntity(cx, cy, out int gridEntity))
                        continue;

                    ref var gridCell = ref _gridCellPool.Get(gridEntity);
                    if (gridCell.StackedEntityIds == null)
                    {
                        gridCell.StackedEntityIds = new List<int>();
                    }

                    gridCell.StackedEntityIds.Add(entityId);
                }
            }
        }
    }
}