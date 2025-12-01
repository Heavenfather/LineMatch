using DG.Tweening;
using GameConfig;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 生成的元素下落动画系统
    /// 负责处理 DropElementSpawnSystem 生成 FallAnimationComponent 元素
    /// </summary>
    public class DropElementAnimationSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _animFilter;
        private GameStateContext _context;
        private IBoard _board;

        private EcsPool<FallAnimationComponent> _fallAnimPool;

        private EcsPool<ElementRenderComponent> _renderPool;

        // private EcsPool<VisualBusyComponent> _busyPool;
        private EcsPool<ElementPositionComponent> _posPool;
        private EcsPool<ElementComponent> _elementPool;

        private const float FALL_DURATION_PER_UNIT = 0.12f; // 每格掉落耗时（基础）
        private const float FALL_MAX_DURATION = 0.45f; // 最大掉落时长（限制最高速度）
        private const float DELAY_PER_ROW = 0.04f; // 行之间的延迟（形成波浪）
        private const float COLUMN_DELAY_FACTOR = 0.05f; // 列之间的延迟（可选，横向波浪）

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _board = _context.Board;

            // 筛选有掉落动画组件的实体
            _animFilter = _world.Filter<FallAnimationComponent>()
                .Include<ElementComponent>()
                .Include<ElementRenderComponent>()
                .End();

            _fallAnimPool = _world.GetPool<FallAnimationComponent>();
            _renderPool = _world.GetPool<ElementRenderComponent>();
            // _busyPool = _world.GetPool<VisualBusyComponent>();
            _posPool = _world.GetPool<ElementPositionComponent>();
            _elementPool = _world.GetPool<ElementComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _animFilter)
            {
                ref var animComp = ref _fallAnimPool.Get(entity);
                ref var renderComp = ref _renderPool.Get(entity);
                ref var elementCom = ref _elementPool.Get(entity);

                if (renderComp.ViewInstance == null)
                {
                    continue;
                }

                Transform viewTrans = renderComp.ViewInstance.transform;

                elementCom.LogicState = ElementLogicalState.Falling;

                // 计算起止点世界坐标
                Vector3 startPos = GetWorldPos(animComp.FromGrid);
                Vector3 endPos = GetWorldPos(animComp.ToGrid);
                viewTrans.position = startPos;

                float distance = Vector3.Distance(startPos, endPos);
                // 距离越长，时间越长，但有上限
                float duration = Mathf.Min(distance * 0.08f + 0.15f, FALL_MAX_DURATION);

                // 延迟计算：
                // Delay = (MaxHeight - ToGrid.y) * DELAY_PER_ROW
                int bottomDist = (_board.Height - 1) - animComp.ToGrid.y;
                float delay = bottomDist * DELAY_PER_ROW;

                // 执行动画 (使用 DOTween)
                viewTrans.DOMove(endPos, duration)
                    // .SetDelay(delay)
                    .SetEase(Ease.OutBounce)
                    .OnComplete(() => OnFallComplete(entity));

                // 移除 Animation 组件
                _fallAnimPool.Del(entity);
            }
        }

        private void OnFallComplete(int entity)
        {
            // 确保实体还在
            if (!_world.IsEntityAliveInternal(entity)) return;

            ref var elementCom = ref _elementPool.Get(entity);
            elementCom.LogicState = ElementLogicalState.Idle;
            // 同步逻辑位置到 View (双重保险，防止浮点误差)
            if (_posPool.Has(entity) && _renderPool.Has(entity))
            {
                ref var pos = ref _posPool.Get(entity);
                ref var render = ref _renderPool.Get(entity);
                var finalWoldPos = GetWorldPos(new Vector2Int(pos.X, pos.Y));
                pos.WorldPosition = finalWoldPos;
                if (render.ViewInstance != null)
                {
                    render.ViewInstance.transform.position = finalWoldPos;
                }
            }
        }

        /// <summary>
        /// 将网格坐标转为世界坐标
        /// </summary>
        private Vector3 GetWorldPos(Vector2Int gridPos)
        {
            return MatchPosUtil.CalculateWorldPosition(gridPos.x, gridPos.y, 1, 1, ElementDirection.None);
        }
    }
}