using System.Collections.Generic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 引导输入过滤系统
    /// 负责在引导过程中过滤和限制玩家的输入操作
    /// </summary>
    public class GuideInputFilterSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;
        private GameStateContext _context;
        
        private EcsFilter _guideFilter;
        private EcsPool<GuideStateComponent> _guideStatePool;
        private EcsPool<GuidePathComponent> _guidePathPool;
        private EcsPool<MatchInputComponent> _inputPool;
        
        private int _guideEntity = -1;
        
        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            
            _guideFilter = _world.Filter<GuideStateComponent>().End();
            _guideStatePool = _world.GetPool<GuideStateComponent>();
            _guidePathPool = _world.GetPool<GuidePathComponent>();
            _inputPool = _world.GetPool<MatchInputComponent>();
        }
        
        public void Run(IEcsSystems systems)
        {
            // 查找引导实体
            foreach (var entity in _guideFilter)
            {
                _guideEntity = entity;
                break;
            }
            
            if (_guideEntity < 0) return;
            
            ref var guideState = ref _guideStatePool.Get(_guideEntity);
            
            // 如果没有在引导中，直接返回
            if (!guideState.IsGuiding) return;
            
            // 只处理强引导的输入限制
            if (guideState.GuideType != GuideType.Force) return;
            
            // 检查是否有引导路径组件
            if (!_guidePathPool.Has(_guideEntity)) return;
            
            ref var guidePath = ref _guidePathPool.Get(_guideEntity);
            
            // 如果不限制路径，直接返回
            if (!guidePath.IsRestrictPath) return;
            
            // 过滤输入：只允许引导路径上的格子被选中
            FilterInputByGuidePath(ref guidePath);
        }
        
        /// <summary>
        /// 根据引导路径过滤输入
        /// </summary>
        private void FilterInputByGuidePath(ref GuidePathComponent guidePath)
        {
            // 这里可以在 MatchInputSystem 中添加一个检查方法
            // 在连线时检查当前格子是否在引导路径中
            // 如果不在，则不允许连接
            
            // 实现方式：
            // 1. 在 MatchInputComponent 中添加一个 AllowedCoords 字段
            // 2. 在这里设置 AllowedCoords = guidePath.PathCoords
            // 3. 在 MatchInputSystem 的 TryConnectNext 中检查 AllowedCoords
        }
        
        /// <summary>
        /// 检查坐标是否在引导路径中
        /// </summary>
        public bool IsCoordInGuidePath(Vector2Int coord)
        {
            if (_guideEntity < 0) return true;
            
            ref var guideState = ref _guideStatePool.Get(_guideEntity);
            if (!guideState.IsGuiding) return true;
            if (guideState.GuideType != GuideType.Force) return true;
            
            if (!_guidePathPool.Has(_guideEntity)) return true;
            
            ref var guidePath = ref _guidePathPool.Get(_guideEntity);
            if (!guidePath.IsRestrictPath) return true;
            
            if (guidePath.PathCoords == null || guidePath.PathCoords.Count == 0) return true;
            
            return guidePath.PathCoords.Contains(coord);
        }
    }
}
