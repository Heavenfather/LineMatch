using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Module;
using HotfixLogic;
using HotfixLogic.Match;

namespace Hotfix.Logic.GamePlay
{
    public class GameResultCheckSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private GameStateContext _context;
        private MatchStateContext _matchContext;

        // 用于判断棋盘是否忙碌
        private EcsFilter _boardSystemCheckFilter;
        private EcsFilter _roundFilter;
        private EcsPool<RoundEndTag> _roundPool;
        
        // 上一次检查的结果，用于避免重复触发
        private ECheckMatchResult _lastResult = ECheckMatchResult.Actioning;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _matchContext = _context.MatchStateContext;

            _roundFilter = _world.Filter<RoundEndTag>().End();
            _roundPool = _world.GetPool<RoundEndTag>();

            // 初始化过滤器
            _boardSystemCheckFilter = _world.Filter<BoardStableCheckSystemTag>().End();
        }

        public void Run(IEcsSystems systems)
        {
            // 0. 如果已经结算过了，直接跳过
            if (_context.MatchStateContext.IsResultTriggered) 
                return;
            
            // 1. 必须确保游戏处于空闲状态
            if (!IsGameIdle()) 
                return;

            // 2. 检查游戏结果
            var result = CheckResult();
            OnRoundEnd();
            
            // 3. 如果结果发生变化，触发相应事件
            if (result != _lastResult)
            {
                _lastResult = result;
                HandleResultChange(result);
            }
        }

        private void OnRoundEnd()
        {
            if(_roundFilter.GetEntitiesCount() <= 0)
                return;
            foreach (var entity in _roundFilter)
            {
                _roundPool.Del(entity);
            }
            MatchManager.Instance.TickScoreChange();
        }
        
        /// <summary>
        /// 检查游戏是否处于空闲状态
        /// 只有当所有系统都"沉默"了，才是真正的回合结束
        /// </summary>
        private bool IsGameIdle()
        {
            return _boardSystemCheckFilter.GetEntitiesCount() > 0;
        }
        
        /// <summary>
        /// 检查游戏结果
        /// </summary>
        private ECheckMatchResult CheckResult()
        {
            // 1. 检查目标是否完成
            bool isTargetComplete = CheckTargetsComplete();
            
            // 2. 检查是否有金币目标
            bool hasCoinTarget = HasCoinTarget();
            
            // 3. 根据不同情况判断结果
            // 有金币目标且其他目标完成
            if (hasCoinTarget && isTargetComplete)
            {
                // 必须用完所有步数才算成功
                if (_matchContext.RemainStep > 0)
                {
                    return ECheckMatchResult.Actioning;
                }
                else
                {
                    return ECheckMatchResult.Success;
                }
            }
            
            // 情况 2: 步数用完但目标未完成
            if (_matchContext.RemainStep <= 0 && !isTargetComplete)
            {
                return ECheckMatchResult.Failure;
            }
            
            // 情况 3: 目标完成
            if (isTargetComplete)
            {
                return ECheckMatchResult.Success;
            }
            
            // 情况 4: 游戏继续
            return ECheckMatchResult.Actioning;
        }
        
        /// <summary>
        /// 检查所有目标是否完成
        /// </summary>
        private bool CheckTargetsComplete()
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            
            foreach (var kp in LevelTargetSystem.Instance.TargetElements)
            {
                // 跳过金币类型的目标
                if (db.IsCoinTypeElement(kp.Key))
                    continue;
                
                // 如果有任何目标未完成，返回 false
                if (kp.Value > 0)
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 检查是否有金币目标
        /// </summary>
        private bool HasCoinTarget()
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            
            foreach (var kp in LevelTargetSystem.Instance.TargetElements)
            {
                if (db.IsCoinTypeElement(kp.Key))
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 处理结果变化
        /// 只负责触发事件，不处理具体逻辑
        /// </summary>
        private void HandleResultChange(ECheckMatchResult result)
        {
            switch (result)
            {
                case ECheckMatchResult.Success:
                    OnGameSuccess();
                    break;
                    
                case ECheckMatchResult.Failure:
                    OnGameFailure();
                    break;
                    
                case ECheckMatchResult.Actioning:
                    // 游戏继续，不需要特殊处理
                    break;
            }
        }
        
        /// <summary>
        /// 游戏成功
        /// </summary>
        private void OnGameSuccess()
        {
            _context.MatchStateContext.IsResultTriggered = true;
            FreezeAllElement();
            // 触发成功事件
            G.EventModule.DispatchEvent(GameEventDefine.OnGameSuccess);
        }
        
        /// <summary>
        /// 游戏失败
        /// </summary>
        private void OnGameFailure()
        {
            _context.MatchStateContext.IsResultTriggered = true;
            // 触发失败事件
            G.EventModule.DispatchEvent(GameEventDefine.OnGameFailure, EventOneParam<bool>.Create(false));
        }
        
        private void FreezeAllElement()
        {
            int[] allEntity = null;
            _world.GetAllEntities(ref allEntity);
            EcsPool<ElementComponent> elementPool = _world.GetPool<ElementComponent>();
            for (int i = 0; i < allEntity.Length; i++)
            {
                int entity = allEntity[i];
                if (elementPool.Has(entity))
                {
                    ref var elementCom = ref elementPool.Get(entity);
                    elementCom.LogicState = ElementLogicalState.Freeze;
                }
            }
        }
    }
}