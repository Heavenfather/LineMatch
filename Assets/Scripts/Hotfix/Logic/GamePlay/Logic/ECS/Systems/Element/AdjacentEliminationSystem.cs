using System.Collections.Generic;
using GameConfig;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 旁消系统（通用）
    /// 处理所有具有AdjacentEliminationComponent的元素的旁消逻辑
    /// 
    /// 工作流程：
    /// 1. 监听有EliminatedTag的实体（被消除的棋子）
    /// 2. 收集所有被消除的格子坐标
    /// 3. 检查这些格子周围是否有AdjacentEliminationComponent
    /// 4. 如果有，给这些元素添加EliminatedTag（触发旁消）
    /// </summary>
    public class AdjacentEliminationSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private IElementFactoryService _factoryService;

        private EcsFilter _eliminatedFilter;
        private EcsFilter _adjacentFilter;
        private EcsFilter _fireRequestFilter;
        private EcsFilter _fireElementFilter;

        private EcsPool<EliminatedTag> _eliminatedPool;
        private EcsPool<ElementPositionComponent> _positionPool;
        private EcsPool<AdjacentEliminationComponent> _adjacentPool;
        private EcsPool<ElementComponent> _elementPool;
        private EcsPool<NormalElementComponent> _normalPool;
        private EcsPool<SpreadFireComponent> _firePool;

        private bool _haveFireEliminate = false;
        private HashSet<Vector2Int> _eliminatedCoords = new HashSet<Vector2Int>();

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _factoryService = MatchBoot.Container.Resolve<IElementFactoryService>();

            _fireElementFilter = _world.Filter<SpreadFireComponent>().Include<ElementComponent>().End();
            _fireRequestFilter = _world.Filter<SpreadFireRequestComponent>().End();
            // 筛选被消除的实体
            _eliminatedFilter = _world.Filter<EliminatedTag>()
                .Include<ElementPositionComponent>()
                .End();

            // 筛选有AdjacentEliminationComponent的实体
            _adjacentFilter = _world.Filter<AdjacentEliminationComponent>()
                .Include<ElementComponent>()
                .Include<ElementPositionComponent>()
                .Exclude<EliminatedTag>() // 排除已经有Tag的
                .End();

            _normalPool = _world.GetPool<NormalElementComponent>();
            _eliminatedPool = _world.GetPool<EliminatedTag>();
            _positionPool = _world.GetPool<ElementPositionComponent>();
            _adjacentPool = _world.GetPool<AdjacentEliminationComponent>();
            _elementPool = _world.GetPool<ElementComponent>();
            _firePool = _world.GetPool<SpreadFireComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            // 1. 收集本轮被消除的格子坐标
            CollectEliminatedCoords();

            if (_eliminatedCoords.Count == 0)
            {
                // 没有消除，重置旁消标记
                ResetAdjacentFlags();
                return;
            }

            // 2. 检查旁消元素并触发
            TriggerAdjacentElimination();

            // 3. 重置旁消标记
            ResetAdjacentFlags();

            // 4. 清理数据
            _eliminatedCoords.Clear();
        }

        /// <summary>
        /// 收集本轮被消除的格子坐标
        /// </summary>
        private void CollectEliminatedCoords()
        {
            _eliminatedCoords.Clear();

            foreach (var entity in _eliminatedFilter)
            {
                if (!_positionPool.Has(entity))
                    continue;
                ref var eliminateCom = ref _eliminatedPool.Get(entity);
                if(eliminateCom.Reason != EliminateReason.Damage)
                    continue;
                // 只处理普通棋子被消除时才触发其它旁消
                if (!_normalPool.Has(entity))
                    continue;

                ref var pos = ref _positionPool.Get(entity);
                _eliminatedCoords.Add(new Vector2Int(pos.X, pos.Y));
            }
        }

        /// <summary>
        /// 触发旁消
        /// </summary>
        private void TriggerAdjacentElimination()
        {
            _haveFireEliminate = false;
            foreach (var entity in _adjacentFilter)
            {
                ref var adjacent = ref _adjacentPool.Get(entity);
                ref var element = ref _elementPool.Get(entity);
        
                // 跳过已经被触发的
                if (adjacent.IsTriggeredThisRound)
                    continue;
        
                // 跳过已经在Dying状态的
                if (element.LogicState == ElementLogicalState.Dying)
                    continue;
                
                if(element.EliminateStyle != EliminateStyle.Side)
                    continue;
                
                ref var pos = ref _positionPool.Get(entity);
                Vector2Int coord = new Vector2Int(pos.X, pos.Y);
        
                // 检查周围是否有消除
                if (HasEliminationNearby(coord, adjacent.OnlyFourDirections))
                {
                    // 触发旁消
                    TriggerElimination(entity, ref adjacent);
                    if (_firePool.Has(entity))
                    {
                        _haveFireEliminate = true;
                    }
                }
            }
            
            if(_haveFireEliminate == false)
                CheckHaveFireEliminate();
        }

        private void CheckHaveFireEliminate()
        {
            if(_fireElementFilter.GetEntitiesCount() <= 0)
                return;
            if (_fireRequestFilter.GetEntitiesCount() > 0)
                return;
            
            // 本回合没有火元素被消除，则本回合火会蔓延
            int entity = _world.NewEntity();
            var requestPool = _world.GetPool<SpreadFireRequestComponent>();
            requestPool.Add(entity);
        }

        
        /// <summary>
        /// 检查周围是否有消除
        /// </summary>
        private bool HasEliminationNearby(Vector2Int coord, bool onlyFourDirections)
        {
            if (onlyFourDirections)
            {
                // 只检查上下左右四个方向
                foreach (var dir in MatchPosUtil.NeighborDirs)
                {
                    Vector2Int neighborCoord = coord + dir;
                    if (_eliminatedCoords.Contains(neighborCoord))
                        return true;
                }
            }
            else
            {
                // 检查八个方向
                foreach (var dir in MatchPosUtil.EightNeighborDirs)
                {
                    Vector2Int neighborCoord = coord + dir;
                    if (_eliminatedCoords.Contains(neighborCoord))
                        return true;
                }
            }
        
            return false;
        }

        /// <summary>
        /// 触发消除
        /// </summary>
        private void TriggerElimination(int entity, ref AdjacentEliminationComponent adjacent)
        {
            // 标记为已触发
            adjacent.IsTriggeredThisRound = true;

            // 添加EliminatedTag
            _factoryService.AddEliminateTag2Entity(_world, entity, 1, EliminateReason.Side);
        }

        /// <summary>
        /// 重置旁消标记
        /// </summary>
        private void ResetAdjacentFlags()
        {
            foreach (var entity in _adjacentFilter)
            {
                ref var adjacent = ref _adjacentPool.Get(entity);
                adjacent.IsTriggeredThisRound = false;
            }
        }
    }
}