using System.Collections.Generic;
using GameConfig;
using Hotfix.Define;
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
        private ElementMapDB _elementMap;
        private EcsPool<ElementRenderComponent> _renderPool;
        private EcsPool<GridCellComponent> _gridPool;
        private EcsPool<ElementPositionComponent> _posPool;
        private EcsPool<ElementComponent> _elementPool;
        private EcsPool<DestroyElementTagComponent> _destroyPool;
        private Dictionary<int,int> _destroyedElementMap;

        public void Init(IEcsSystems systems)
        {
            _context = systems.GetShared<GameStateContext>();
            _board = _context.Board;
            _world = systems.GetWorld();
            _elementMap = ConfigMemoryPool.Get<ElementMapDB>();
            _destroyFilter = _world.Filter<DestroyElementTagComponent>().Include<ElementComponent>().Include<EliminatedTag>().End();
            _destroyPool = _world.GetPool<DestroyElementTagComponent>();
            _elementPool = _world.GetPool<ElementComponent>();
            _posPool = _world.GetPool<ElementPositionComponent>();
            _gridPool = _world.GetPool<GridCellComponent>();
            _renderPool = _world.GetPool<ElementRenderComponent>();

            _destroyedElementMap = new Dictionary<int, int>(20);
        }

        public void Run(IEcsSystems systems)
        {
            bool hasAnyDestroy = false;
            if (_destroyedElementMap.Count > 0)
                _destroyedElementMap.Clear();
            foreach (var entity in _destroyFilter)
            {
                if(!_world.IsEntityAliveInternal(entity))
                    continue;
                
                hasAnyDestroy = true;
                
                // 收集被消除的格子坐标（用于水蔓延）
                if (_posPool.Has(entity))
                {
                    ref var pos = ref _posPool.Get(entity);
                    _context.MatchStateContext.RoundEliminateCoords.Add(new Vector2Int(pos.X, pos.Y));
                }
                
                // 1. 播放通用死亡特效 (差异化的表现由 ActionExecutionSystem 预先处理)
                // 这里只播最基础的 如有飘字什么的
                PlayCommonDestroyEffect(entity);
                
                // 2.从格子中移除
                RemoveFromGrid(entity);
                
                // 4. 回收 View (GameObject)
                RecycleView(entity);
                
                // 5. 计算销毁的元素数量/加分
                ref var eleCom = ref _elementPool.Get(entity);
                CalculateDelElement(eleCom.ConfigId);
                AddBlockScore(eleCom.ConfigId);
                CalculateCoin();
                
                // 6. 彻底销毁 ECS 实体
                _world.DelEntity(entity);
            }
            
            if (hasAnyDestroy)
            {
                // 6. 通知掉落系统 
                _board.IsBoardDirty = true;

                LevelTargetSystem.Instance.CalculateTarget(_destroyedElementMap);
                
                // 7. 触发水蔓延检测
                TriggerWaterSpread(_context.MatchStateContext.RoundEliminateCoords);
            }
        }

        /// <summary>
        /// 触发水蔓延检测
        /// </summary>
        private void TriggerWaterSpread(HashSet<Vector2Int> eliminatedCoords)
        {
            if (eliminatedCoords == null || eliminatedCoords.Count == 0)
                return;

            // 检查是否有水元素
            var waterPool = _world.GetPool<SpreadWaterComponent>();
            bool hasWater = false;

            // 简单检查：遍历所有水实体
            var waterFilter = _world.Filter<SpreadWaterComponent>().End();
            foreach (var waterEntity in waterFilter)
            {
                hasWater = true;
                break;
            }

            if (!hasWater)
                return;

            // 获取水的配置ID
            int waterConfigId = 0;
            foreach (var waterEntity in waterFilter)
            {
                ref var water = ref waterPool.Get(waterEntity);
                waterConfigId = water.WaterConfigId;
                break;
            }

            if (waterConfigId == 0)
                return;

            // 创建水蔓延请求
            int spreadEntity = _world.NewEntity();
            var spreadPool = _world.GetPool<SpreadWaterSpreadComponent>();
            ref var spread = ref spreadPool.Add(spreadEntity);
            spread.EliminatedCoords = new List<Vector2Int>(eliminatedCoords);
            spread.WaterConfigId = waterConfigId;
            spread.IsProcessed = false;
            spread.NeedFlowAnimation = true;
        }
        
        private void RemoveFromGrid(int entity)
        {
            // 1. 获取棋子位置
            if (!_posPool.Has(entity)) return;
            ref var pos = ref _posPool.Get(entity);
            ref var eleCom = ref _elementPool.Get(entity);
            for (int x = 0; x < eleCom.Width; x++)
            {
                int eleX = x + pos.X;
                for (int y = 0; y < eleCom.Height; y++)
                {
                    int eleY = y + pos.Y;
                    var gridEntity = _board[eleX, eleY];
                    ref var grid = ref _gridPool.Get(gridEntity);
                    if (grid.StackedEntityIds != null && grid.StackedEntityIds.Contains(entity))
                    {
                        grid.StackedEntityIds.Remove(entity);
                    }
                }
            }
        }

        private void CalculateDelElement(int configId, int count = 1)
        {
            if (_destroyedElementMap.ContainsKey(configId))
            {
                _destroyedElementMap[configId] += count;
            }
            else
            {
                _destroyedElementMap.Add(configId, count);
            }
        }

        private bool AddBlockScore(int elementId)
        {
            ref readonly ElementMap config = ref _elementMap[elementId];
            if (config.elementType == ElementType.Normal)
                return false;
            int baseScore = config.score;
            MatchManager.Instance.AddScore(baseScore);
            return true;
        }

        private void CalculateCoin()
        {
            if (!_context.MatchStateContext.IsGameSettlement)
                return;
            if (_destroyedElementMap.ContainsKey((int)ElementIdConst.Coin))
                _destroyedElementMap[(int)ElementIdConst.Coin] += 1;
            else
                _destroyedElementMap.Add((int)ElementIdConst.Coin, 1);
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
                ref var posCom = ref _posPool.Get(entity);
                GameObject grid = _board.GetGridInstance(posCom.X, posCom.Y);
                MatchEffectManager.Instance.PlayObjectEffect(element.ConfigId, null, grid?.transform);
            }
            // 简单的消失动画，复杂的在消除发生时已经由 ActionExecutionSystem 处理了
        }
    }
}