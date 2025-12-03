using System.Collections.Generic;
using HotfixLogic.Match;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 掉落元素生成系统
    /// 负责消费掉落分析系统生产的数据,生成元素实体
    /// </summary>
    public class DropElementSpawnSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsPool<DropSpawnRequestComponent> _reqPool;
        private DropStrategyService _dropStrategy;
        private IElementFactoryService _factory;
        private GameStateContext _context;
        private IBoard _board;
        private IMatchService _matchService;
        private EcsFilter _filter;
        private EcsPool<FallAnimationComponent> _fallAnimPool;
        private EcsPool<GridCellComponent> _gridPool;

        private List<DropAnalysisComponent> _dropAnalysis;
        private EcsPool<DropAnalysisComponent> _dropAnalysisPool;
        private EcsFilter _dropAnalysisFilter;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _board = _context.Board;
            _matchService = _context.ServiceFactory.GetService(_context.CurrentMatchType);
            _reqPool = _world.GetPool<DropSpawnRequestComponent>();
            _factory = _context.ServiceFactory.GetElementFactoryService();
            _fallAnimPool = _world.GetPool<FallAnimationComponent>();
            _gridPool = _world.GetPool<GridCellComponent>();

            _dropStrategy = MatchBoot.Container.Resolve<DropStrategyService>();

            _filter = _world.Filter<DropSpawnRequestComponent>().End();

            _dropAnalysisPool = _world.GetPool<DropAnalysisComponent>();
            _dropAnalysis = new List<DropAnalysisComponent>();
            _dropAnalysisFilter = _world.Filter<DropAnalysisComponent>().End();
        }

        public void Run(IEcsSystems systems)
        {
            // 先在同一帧生成掉落分析产生的请求
            foreach (var entity in _dropAnalysisFilter)
            {
                ref var dropAnalysis = ref _dropAnalysisPool.Get(entity);
                _dropAnalysis.Add(dropAnalysis);
                _world.DelEntity(entity);
            }

            foreach (var entity in _filter)
            {
                ref var req = ref _reqPool.Get(entity);

                // 遍历每一个需要生成的行
                int offset = -1;
                foreach (var row in req.TargetRows)
                {
                    // 1. 调用策略计算
                    int configId = _dropStrategy.CalculateDropId(req.Column, row, _context, _dropAnalysis);
                    if (configId <= 0)
                    {
                        Logger.Error($" {req.Column} 没有随机到掉落元素？");
                        continue;
                    }

                    _context.MatchStateContext.AddDropCount(configId);
                    UpdateGlobalQuota(configId, _context.MatchStateContext);

                    // 2. 创建实体逻辑位置直接设为  视觉位置需要设在棋盘上方，并播放掉落动画
                    int newEntity = _factory.CreateElementEntity(
                        _context,
                        _matchService,
                        configId,
                        req.Column,
                        row, 1, 1 //暂时只支持1x1的元素掉落，如果后面有需要再来支持
                    );

                    // 3. 注册到 Grid
                    RegisterToGrid(req.Column, row, newEntity);

                    // 4. 添加掉落动画组件
                    AddSpawnFallAnimation(newEntity, req.Column, row, offset);
                    --offset;
                }

                // 销毁请求
                _world.DelEntity(entity);
            }

            if (_dropAnalysis.Count > 0)
                _dropAnalysis.Clear();
        }

        private void UpdateGlobalQuota(int configId, MatchStateContext state)
        {
            // 1. 如果直接是 TargetID
            if (state.GlobalDropQuotas.ContainsKey(configId))
            {
                state.ConsumeDropQuota(configId);
            }

            // 2. 如果是变体
            foreach (var key in state.GlobalDropQuotas.Keys)
            {
                if (key == configId) continue; // 已经扣过了

                if (MatchElementUtil.IsContributingToTarget(key, configId))
                {
                    state.ConsumeDropQuota(key);
                }
            }
        }

        private void AddSpawnFallAnimation(int entity, int col, int targetRow, int offset)
        {
            // 给新生成的棋子加动画组件
            // View 层解析到 Y < 0 时，会转换成屏幕上方的世界坐标
            ref var anim = ref _fallAnimPool.Add(entity);
            anim.FromGrid = new Vector2Int(col, offset);
            anim.ToGrid = new Vector2Int(col, targetRow);
        }

        private void RegisterToGrid(int x, int y, int entityId)
        {
            int gridEntity = _board[x, y];
            ref var grid = ref _gridPool.Get(gridEntity);
            if (grid.StackedEntityIds == null) grid.StackedEntityIds = new List<int>();
            grid.StackedEntityIds.Add(entityId);
        }
    }
}