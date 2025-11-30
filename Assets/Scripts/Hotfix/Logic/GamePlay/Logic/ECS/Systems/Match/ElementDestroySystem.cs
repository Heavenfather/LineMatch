using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 元素销毁系统，负责消费 DestroyElementTagComponent，销毁元素。
    /// </summary>
    public class ElementDestroySystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _destroyFilter;
        private IBoard _board;
        private GameStateContext _context;
        private EcsPool<ElementRenderComponent> _renderPool;
        private EcsPool<GridCellComponent> _gridPool;
        private EcsPool<ElementPositionComponent> _posPool;
        private EcsPool<ElementComponent> _elementPool;
        private EcsPool<DestroyElementTagComponent> _destroyPool;

        public void Init(IEcsSystems systems)
        {
            _context = systems.GetShared<GameStateContext>();
            _board = _context.Board;
            _world = systems.GetWorld();
            _destroyFilter = _world.Filter<DestroyElementTagComponent>().Include<ElementComponent>().End();
            _destroyPool = _world.GetPool<DestroyElementTagComponent>();
            _elementPool = _world.GetPool<ElementComponent>();
            _posPool = _world.GetPool<ElementPositionComponent>();
            _gridPool = _world.GetPool<GridCellComponent>();
            _renderPool = _world.GetPool<ElementRenderComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            bool hasAnyDestroy = false;
            foreach (var entity in _destroyFilter)
            {
                hasAnyDestroy = true;
                // 1. 播放通用死亡特效 (差异化的表现由 ActionExecutionSystem 预先处理)
                // 这里只播最基础的 如有飘字什么的
                PlayCommonDestroyEffect(entity);
                
                // 2.从格子中移除
                RemoveFromGrid(entity);
                // 3.保底卷帘类型的障碍物收集处理? 这一部分已经在 ActionExecutionSystem 中处理
                // CheckCollection(entity);
                
                // 4. 回收 View (GameObject)
                RecycleView(entity);
                
                // 5. 彻底销毁 ECS 实体
                _world.DelEntity(entity);
            }
            
            if (hasAnyDestroy)
            {
                // 6. 通知掉落系统 
                _board.IsBoardDirty = true;
            }
        }

        private void RemoveFromGrid(int entity)
        {
            // 1. 获取棋子位置
            if (!_posPool.Has(entity)) return;
            ref var pos = ref _posPool.Get(entity);
            // 遍历整个棋盘，找出含有该实体的格子；因为有些棋子是横跨的
            _board.ForeachBoard(gridEntity =>
            {
                // 2. 从格子的堆叠列表中移除自己
                ref var grid = ref _gridPool.Get(gridEntity);
                if (grid.IsBlank)
                    return;
                if (grid.StackedEntityIds != null && grid.StackedEntityIds.Contains(entity))
                {
                    grid.StackedEntityIds.Remove(entity);
                }
            });
        }

        private void RecycleView(int entity)
        {
            if (_renderPool.Has(entity))
            {
                ref var render = ref _renderPool.Get(entity);
                if (render.ViewInstance != null)
                {
                    ElementObjectPool.Instance.Recycle(render.ViewInstance.gameObject);
                    render.ViewInstance = null;
                }
            }
        }

        private void PlayCommonDestroyEffect(int entity)
        {
            if (_elementPool.Has(entity))
            {
                ref var element = ref _elementPool.Get(entity);
                GameObject grid = _board.GetGridInstance(element.OriginGridPosition.x, element.OriginGridPosition.y);
                MatchEffectManager.Instance.PlayObjectEffect(element.ConfigId, null, grid?.transform);
            }
            // 简单的消失动画，复杂的在消除发生时已经由 ActionExecutionSystem 处理了
        }
    }
}