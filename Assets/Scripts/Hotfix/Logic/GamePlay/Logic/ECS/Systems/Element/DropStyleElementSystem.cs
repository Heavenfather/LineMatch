using System.Collections.Generic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 掉落式障碍物系统
    /// 处理掉落到最后一行自动收集的障碍物（如熊、蝴蝶等）
    /// </summary>
    public class DropStyleElementSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private GameStateContext _context;
        private IBoard _board;
        private ICollectItemFlyService _flyService;
        private IElementFactoryService _elementService;
        private Camera _mainCamera;

        private EcsFilter _dropStyleFilter;
        private EcsFilter _boardStableFilter;
        private EcsPool<DropStyleElementComponent> _dropStylePool;
        private EcsPool<ElementComponent> _elementPool;
        private EcsPool<ElementPositionComponent> _positionPool;
        private EcsPool<ElementRenderComponent> _renderPool;
        private EcsPool<GridCellComponent> _gridCellPool;
        private EcsPool<EliminatedTag> _eliminatedPool;
        private EcsPool<DestroyElementTagComponent> _destroyPool;

        // 已处理的实体（避免重复处理）
        private HashSet<int> _processedEntities = new HashSet<int>();

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _board = _context.Board;
            _elementService = MatchBoot.Container.Resolve<IElementFactoryService>();
            _flyService = MatchBoot.Container.Resolve<ICollectItemFlyService>();
            _mainCamera = _context.SceneView.GetSceneRootComponent<Camera>("MainCamera", "");

            _dropStyleFilter = _world.Filter<DropStyleElementComponent>()
                .Include<ElementComponent>()
                .Include<ElementPositionComponent>()
                .End();
            _boardStableFilter = _world.Filter<BoardStableCheckSystemTag>().End();

            _dropStylePool = _world.GetPool<DropStyleElementComponent>();
            _elementPool = _world.GetPool<ElementComponent>();
            _positionPool = _world.GetPool<ElementPositionComponent>();
            _renderPool = _world.GetPool<ElementRenderComponent>();
            _gridCellPool = _world.GetPool<GridCellComponent>();
            _eliminatedPool = _world.GetPool<EliminatedTag>();
            _destroyPool = _world.GetPool<DestroyElementTagComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            // 1. 处理被爆炸伤害的DropStyle元素
            ProcessBombDamage();

            // 2. 检测棋盘是否稳定
            if (!IsBoardStable())
                return;

            // 3. 检测并收集到达最后一行的DropStyle元素
            ProcessBottomRowCollection();
        }

        /// <summary>
        /// 处理被爆炸伤害的DropStyle元素
        /// </summary>
        private void ProcessBombDamage()
        {
            foreach (var entity in _dropStyleFilter)
            {
                // 检查是否有消除标签
                if (!_eliminatedPool.Has(entity))
                    continue;

                ref var dropStyle = ref _dropStylePool.Get(entity);

                // 如果可以被爆炸消除
                if (dropStyle.IsCanBombDel)
                {
                    // 移除消除标签（不让ElementDestroySystem处理）
                    // _eliminatedPool.Del(entity);

                    // 收集并销毁
                    CollectAndDestroy(entity);
                }
            }
        }

        /// <summary>
        /// 处理到达最后一行的收集
        /// </summary>
        private void ProcessBottomRowCollection()
        {
            _processedEntities.Clear();

            foreach (var entity in _dropStyleFilter)
            {
                // 跳过已处理的
                if (_processedEntities.Contains(entity))
                    continue;

                // 跳过已经有销毁标签的
                if (_destroyPool.Has(entity))
                    continue;

                // 检查元素是否处于空闲状态（掉落动画已完成）
                if (!IsElementIdle(entity))
                    continue;

                // 检查是否在最后一行
                if (IsAtBottomRow(entity))
                {
                    CollectAndDestroy(entity);
                    _processedEntities.Add(entity);
                }
            }
        }

        /// <summary>
        /// 检查元素是否处于空闲状态（动画已完成）
        /// </summary>
        private bool IsElementIdle(int entity)
        {
            if (!_elementPool.Has(entity))
                return false;

            ref var element = ref _elementPool.Get(entity);
            return element.LogicState == ElementLogicalState.Idle;
        }

        /// <summary>
        /// /// /// /// /// 检查元素是否在其所在列的最后一行
        /// 最后一行定义：该列中Y值最大的非空白格子
        /// </summary>
        private bool IsAtBottomRow(int entity)
        {
            if (!_positionPool.Has(entity))
                return false;

            ref var pos = ref _positionPool.Get(entity);
            int column = pos.X;
            int currentRow = pos.Y;

            // 从当前行向下查找（Y值增大方向），看是否还有非空白格子
            for (int y = currentRow + 1; y < _board.Height; y++)
            {
                if (!_board.TryGetGridEntity(column, y, out int gridEntity))
                    continue;

                ref var gridCell = ref _gridCellPool.Get(gridEntity);

                // 如果下方有非空白格子，说明不是最后一行
                if (!gridCell.IsBlank)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 收集并销毁元素
        /// </summary>
        private void CollectAndDestroy(int entity)
        {
            if (!_positionPool.Has(entity) || !_elementPool.Has(entity))
                return;

            ref var pos = ref _positionPool.Get(entity);
            ref var element = ref _elementPool.Get(entity);

            // 获取元素的世界位置
            Vector3 startPos = pos.WorldPosition;

            // 获取目标位置
            Vector3 targetScreenPos = _context.MatchMainWindow.GetTargetObjectScreenPos(element.ConfigId);
            Vector3 targetWorldPos = _mainCamera.ScreenToWorldPoint(targetScreenPos);
            targetWorldPos.z = 0;

            // 请求收集道具飞行
            _flyService.RequestCollectItemFly(
                _world,
                element.ConfigId,
                startPos,
                targetWorldPos,
                null
            );

            // 添加销毁标签
            if (!_destroyPool.Has(entity))
            {
                _elementService.AddDestroyElementTag2Entity(_world, entity);
                // _destroyPool.Add(entity);
            }
        }

        /// <summary>
        /// 检查棋盘是否稳定
        /// </summary>
        private bool IsBoardStable()
        {
            return _boardStableFilter.GetEntitiesCount() > 0;
        }
    }
}