using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using HotfixCore.Module;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 游戏结算系统
    /// </summary>
    public class GameSettlementSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private GameStateContext _context;
        private TrailEmitter _trailEmitter;
        private IBoard _board;
        private IMatchService _matchService;
        private Camera _mainCamera;
        private ICollectItemFlyService _flyService;
        
        // 用于判断棋盘是否忙碌
        private EcsFilter _boardSystemCheckFilter;
        private EcsFilter _flyFilter;
        
        private EcsFilter _settlementFilter;
        private EcsPool<GameSettlementComponent> _settlementPool;
        private EcsPool<ElementComponent> _elementPool;
        private EcsPool<ElementPositionComponent> _positionPool;
        private EcsPool<NormalElementComponent> _normalElementPool;
        private EcsPool<GridCellComponent> _gridCellPool;
        private EcsPool<PendingActionsComponent> _pendingActionsPool;
        private bool _isExecuted = false;
        private bool _isFireComplete = false;
        private bool _isFinalResultCheck = false;

        // 当前结算的目标列表（用于回调）
        private List<int> _currentTargets = new List<int>();
        private List<Vector2Int> _damagePos = new List<Vector2Int>();

        public void Init(IEcsSystems systems)
        {
            _isFireComplete = false;
            _isExecuted = false;
            _isFinalResultCheck = false;
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _board = _context.Board;
            _matchService = _context.ServiceFactory.GetService(_context.CurrentMatchType);
            _trailEmitter = _context.SceneView.GetSceneRootComponent<TrailEmitter>("MatchCanvas", "GridBoard");
            _mainCamera = _context.SceneView.GetSceneRootComponent<Camera>("MainCamera", "");
            _flyService = MatchBoot.Container.Resolve<ICollectItemFlyService>();

            _flyFilter = _world.Filter<CollectItemFlyComponent>().End();
            _settlementFilter = _world.Filter<GameSettlementComponent>().End();
            _settlementPool = _world.GetPool<GameSettlementComponent>();
            _elementPool = _world.GetPool<ElementComponent>();
            _positionPool = _world.GetPool<ElementPositionComponent>();
            _normalElementPool = _world.GetPool<NormalElementComponent>();
            _gridCellPool = _world.GetPool<GridCellComponent>();
            _pendingActionsPool = _world.GetPool<PendingActionsComponent>();
            
            // 初始化过滤器
            _boardSystemCheckFilter = _world.Filter<BoardStableCheckSystemTag>().End();
        }

        public void Run(IEcsSystems systems)
        {
            if (_isFireComplete && !_isFinalResultCheck)
            {
                if (IsGameIdle())
                {
                    _isFinalResultCheck = true;
                    OnFinalGameResult().Forget();
                }

                return;
            }
            
            if (_isExecuted)
                return;
            foreach (var entity in _settlementFilter)
            {
                ref var settlement = ref _settlementPool.Get(entity);

                // 处理结算流程
                ProcessSettlement(ref settlement);
                _isExecuted = true;

                _settlementPool.Del(entity);
            }
        }

        /// <summary>
        /// 处理结算流程
        /// </summary>
        private void ProcessSettlement(ref GameSettlementComponent settlement)
        {
            // 1. 查找目标棋子
            _currentTargets.Clear();
            _damagePos.Clear();
            _currentTargets = FindTargets(settlement.RemainStep);

            if (_currentTargets.Count == 0) return;
            Vector3 startPos = _mainCamera.ScreenToWorldPoint(settlement.StepTextWorldPos);
            List<Vector3> targetPos = new List<Vector3>(_currentTargets.Count);
            for (int i = 0; i < _currentTargets.Count; i++)
            {
                ref var position = ref _positionPool.Get(_currentTargets[i]);
                targetPos.Add(position.WorldPosition);
                _damagePos.Add(new Vector2Int(position.X, position.Y));
            }

            _trailEmitter.Emitter(startPos, targetPos, OnStepComplete, OnComplete, TrailEmitterType.StepTrail);
        }

        private void OnComplete()
        {
            List<AtomicAction> actions = new List<AtomicAction>();
            actions.Add(new AtomicAction() { Type = MatchActionType.Delay, Value = 300 });

            var allBomb = GetAllBomb();
            if (allBomb.Count > 0)
            {
                _damagePos.AddRange(allBomb);
            }
            for (int i = 0; i < _damagePos.Count; i++)
            {
                actions.Add(new AtomicAction()
                {
                    Type = MatchActionType.Damage,
                    Value = 1,
                    GridPos = _damagePos[i],
                });
                RequestCoinFly(_damagePos[i]);
            }

            int pendingEntity = _world.NewEntity();
            ref var pending = ref _pendingActionsPool.Add(pendingEntity);
            pending.Actions = actions;
            
            _isFireComplete = true;
        }

        private async UniTask OnFinalGameResult()
        {
            MatchManager.Instance.TickScoreChange();
            await UniTask.Delay(TimeSpan.FromSeconds(1.2f));
            int remainStep = _context.MatchStateContext.RemainStep;
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchStepMoveEnd,EventOneParam<int>.Create(remainStep));
        }
        
        private void RequestCoinFly(Vector2Int gridPos)
        {
            _flyService.ResetFlyIndex();
            var startPos = MatchPosUtil.CalculateWorldPosition(gridPos.x, gridPos.y, 1, 1, ElementDirection.None);
            var coinScreenPos = _context.MatchMainWindow.GetCoinScreenPosition();
            var targetPos = _mainCamera.ScreenToWorldPoint(coinScreenPos);
            _flyService.RequestCollectItemFly(_world, (int)CollectItemEnum.Coin, startPos, targetPos,
                OnCoinFlyComplete, 0, true);
        }

        private void OnCoinFlyComplete()
        {
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchBeginCollectResultCoin);
        }
        
        /// <summary>
        /// 检查游戏是否处于空闲状态
        /// 只有当所有系统都"沉默"了，才是真正的回合结束
        /// </summary>
        private bool IsGameIdle()
        {
            return _boardSystemCheckFilter.GetEntitiesCount() > 0 &&
                   _flyFilter.GetEntitiesCount() <= 0;
        }
        
        private void OnStepComplete(int index)
        {
            ref var position = ref _positionPool.Get(_currentTargets[index]);
            List<AtomicAction> actions = new List<AtomicAction>()
            {
                new AtomicAction()
                {
                    Type = MatchActionType.Spawn2Other,
                    ExtraData = new GenItemData()
                    {
                        ConfigId = _matchService.RandomFunctionElement(),
                        GenCoord = new Vector2Int(position.X, position.Y), ElementSize = Vector2Int.one
                    }
                }
            };
            int pendingEntity = _world.NewEntity();
            ref var pending = ref _pendingActionsPool.Add(pendingEntity);
            pending.Actions = actions;
        }

        private List<Vector2Int> GetAllBomb()
        {
            List<Vector2Int> coords = new List<Vector2Int>();
            var specialCom = _world.GetPool<SpecialElementComponent>();
            for (int x = 0; x < _board.Width; x++)
            {
                for (int y = 0; y < _board.Height; y++)
                {
                    var gridEntity = _board[x, y];
                    ref var gridCell = ref _gridCellPool.Get(gridEntity);

                    if (gridCell.IsBlank || gridCell.StackedEntityIds == null)
                        continue;

                    foreach (var entityId in gridCell.StackedEntityIds)
                    {
                        if (specialCom.Has(entityId))
                        {
                            coords.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }
            return coords;
        }

        /// <summary>
        /// 查找目标棋子
        /// </summary>
        private List<int> FindTargets(int maxCount)
        {
            var candidates = new List<int>();

            // 遍历棋盘收集候选目标
            for (int x = 0; x < _board.Width; x++)
            {
                for (int y = 0; y < _board.Height; y++)
                {
                    var gridEntity = _board[x, y];
                    ref var gridCell = ref _gridCellPool.Get(gridEntity);

                    if (gridCell.IsBlank || gridCell.StackedEntityIds == null)
                        continue;

                    foreach (var entityId in gridCell.StackedEntityIds)
                    {
                        if (IsValidTarget(entityId))
                        {
                            candidates.Add(entityId);
                        }
                    }
                }
            }

            // 随机打乱
            candidates.Shuffle();

            // 取前 N 个
            int count = Mathf.Min(maxCount, candidates.Count);
            return candidates.GetRange(0, count);
        }

        private bool IsValidTarget(int entityId)
        {
            if (!_normalElementPool.Has(entityId))
                return false;

            if (!_positionPool.Has(entityId))
                return false;

            if (!_elementPool.Has(entityId))
                return false;

            return true;
        }
    }
}