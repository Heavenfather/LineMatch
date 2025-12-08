using System.Collections.Generic;
using GameConfig;
using HotfixCore.Extensions;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 蔓延火系统
    /// 处理火的蔓延和消除逻辑
    /// </summary>
    public class SpreadFireSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private GameStateContext _context;
        private IBoard _board;
        private IMatchService _matchService;
        private IElementFactoryService _elementFactory;

        private EcsFilter _fireFilter;
        private EcsFilter _eliminatedFireFilter;
        private EcsFilter _boardStable;
        private EcsFilter _roundFilter;
        private EcsFilter _fireRequestFilter;

        private EcsPool<SpreadFireComponent> _firePool;
        private EcsPool<ElementComponent> _elementPool;
        private EcsPool<ElementPositionComponent> _positionPool;
        private EcsPool<GridCellComponent> _gridCellPool;
        private EcsPool<PendingActionsComponent> _pendingActionsPool;

        private List<int> _validFireEntities = new List<int>();
        private List<Vector2Int> _validSpreadTargets = new List<Vector2Int>();

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _board = _context.Board;
            _elementFactory = MatchBoot.Container.Resolve<IElementFactoryService>();
            _matchService = _context.ServiceFactory.GetService(_context.CurrentMatchType);

            _roundFilter = _world.Filter<RoundEndTag>().End();
            _fireRequestFilter = _world.Filter<SpreadFireRequestComponent>().End();
            _boardStable = _world.Filter<BoardStableCheckSystemTag>().End();
            _fireFilter = _world.Filter<SpreadFireComponent>()
                .Include<ElementComponent>()
                .Include<ElementPositionComponent>()
                .End();

            _eliminatedFireFilter = _world.Filter<SpreadFireComponent>()
                .Include<EliminatedTag>()
                .End();

            _firePool = _world.GetPool<SpreadFireComponent>();
            _elementPool = _world.GetPool<ElementComponent>();
            _positionPool = _world.GetPool<ElementPositionComponent>();
            _gridCellPool = _world.GetPool<GridCellComponent>();
            _pendingActionsPool = _world.GetPool<PendingActionsComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            // 1. 实时监听是否有火被消除
            // 只要这一帧有任何火被打上 EliminatedTag，就标记本回合“有火被消”
            if (_eliminatedFireFilter.GetEntitiesCount() > 0)
            {
                foreach (var fireEntity in _eliminatedFireFilter)
                {
                    _elementFactory.AddDestroyElementTag2Entity(_world, fireEntity);
                }
            }

            // 2. 等待棋盘稳定信号
            if (_boardStable.GetEntitiesCount() > 0 && _roundFilter.GetEntitiesCount() > 0 &&
                _fireFilter.GetEntitiesCount() > 0)
            {
                foreach (var entity in _fireRequestFilter)
                {
                    TrySpreadOneFire();
                    _world.DelEntity(entity);
                }
            }
        }

        private void TrySpreadOneFire()
        {
            _validFireEntities.Clear();

            // 收集所有场上存活的火
            foreach (var entity in _fireFilter)
            {
                ref var ele = ref _elementPool.Get(entity);
                // 确保只有 Idle 状态的火才能蔓延（避免正在掉落或消除中的火参与）
                if (ele.LogicState == ElementLogicalState.Idle)
                {
                    _validFireEntities.Add(entity);
                }
            }

            if (_validFireEntities.Count == 0) return;
            _validFireEntities.Shuffle();

            foreach (var fireEntity in _validFireEntities)
            {
                // 检查这个火周围是否有空位可烧
                if (CheckAndSpreadFrom(fireEntity))
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 检查指定火源周围是否有合法目标，如果有，随机选一个蔓延
        /// </summary>
        private bool CheckAndSpreadFrom(int fireEntity)
        {
            ref var posComp = ref _positionPool.Get(fireEntity);
            ref var fireComp = ref _firePool.Get(fireEntity);

            _validSpreadTargets.Clear();
            var dirs = MatchPosUtil.EightNeighborDirs;
            foreach (var dir in dirs)
            {
                var neighbor = new Vector2Int(posComp.X + dir.x, posComp.Y + dir.y);
                if (IsValidSpreadTarget(neighbor.x, neighbor.y))
                {
                    _validSpreadTargets.Add(neighbor);
                }
            }

            if (_validSpreadTargets.Count == 0) return false;

            // 随机选一个目标
            int randIndex = Random.Range(0, _validSpreadTargets.Count);
            Vector2Int targetPos = _validSpreadTargets[randIndex];

            // 执行蔓延
            DoSpread(fireComp.FireConfigId, targetPos);
            return true;
        }

        /// <summary>
        /// 判断目标格子是否可以被火蔓延
        /// </summary>
        private bool IsValidSpreadTarget(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _board.Width || y >= _board.Height)
                return false;
            if (!_board.TryGetGridEntity(x, y, out int gridEntity)) return false;

            ref var grid = ref _gridCellPool.Get(gridEntity);
            // 空格子不能烧（通常火需要附着物，如果你的设计是空地也能烧，去掉这个判断）
            if (grid.IsBlank || grid.StackedEntityIds == null || grid.StackedEntityIds.Count == 0) return false;

            // 检查最上层的元素
            int topEntity = grid.StackedEntityIds[^1];
            if (!_elementPool.Has(topEntity)) return false;

            ref var ele = ref _elementPool.Get(topEntity);

            // 已经被标记消除的不能烧
            if (ele.LogicState != ElementLogicalState.Idle) return false;

            // 已经是火了，不能烧
            if (_firePool.Has(topEntity)) return false;

            if (ele.Type == ElementType.Normal || _matchService.IsSpecialElement(ele.ConfigId))
            {
                return true;
            }

            return false;
        }

        private void DoSpread(int fireConfigId, Vector2Int targetPos)
        {
            // 生成蔓延指令
            int actionEntity = _world.NewEntity();
            ref var pending = ref _pendingActionsPool.Add(actionEntity);
            pending.Actions = new List<AtomicAction>
            {
                new AtomicAction
                {
                    Type = MatchActionType.Spawn2Other, // 替换为火
                    ExtraData = new GenItemData
                    {
                        ConfigId = fireConfigId,
                        GenCoord = targetPos,
                        ElementSize = new Vector2Int(1, 1)
                    }
                }
            };
        }
    }
}