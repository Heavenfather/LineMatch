using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 重新开始状态
    /// 职责：清理 ECS 世界、回收资源、重置数据，然后跳转到初始化
    /// </summary>
    public class GameRestartState : IStateMachineContext<GameStateContext>
    {
        public StateMachine<GameStateContext> StateMachine { get; set; }
        public GameStateContext Context { get; set; }
        public bool PerFrameExecute { get; } = false;

        public async UniTask OnEnter(CancellationToken token)
        {
            // 1. 清理所有棋子实体和View
            RecycleAllVisuals();

            // 2. 重置所有格子状态
            ResetAllGrids();

            // 3. 重置游戏数据
            ResetGameData();
            
            // 4.清理ECS世界
            ClearEcsWorld();
        }

        public async UniTask Execute(CancellationToken token)
        {
            // 跳转回初始化状态 (重新走一遍 BoardInit -> Spawn)
            Context.IsGameReStart = true;
            await MatchBoot.GameWorkflow.ChangeGameState(GameState.Initialize);
        }

        public async UniTask OnExit(CancellationToken token)
        {
            
        }

        public UniTask<bool> IsCanSwitch(string newStateKey, CancellationToken token)
        {
            return UniTask.FromResult(true);
        }

        /// <summary>
        /// 回收所有棋子实体
        /// </summary>
        private void RecycleAllVisuals()
        {
            var world = Context.World;
            var filter = world.Filter<ElementRenderComponent>().End();
            var renderPool = world.GetPool<ElementRenderComponent>();

            // 销毁所有棋子
            foreach (var entity in filter)
            {
                // 销毁View
                if (renderPool.Has(entity))
                {
                    ref var render = ref renderPool.Get(entity);
                    if (render.ViewInstance != null)
                    {
                        ElementObjectPool.Instance.Recycle(render.ViewInstance.gameObject);
                        render.ViewInstance = null;
                    }
                }

                // 删除实体
                world.DelEntity(entity);
            }
        }

        /// <summary>
        /// 重置所有格子状态
        /// </summary>
        private void ResetAllGrids()
        {
            IBoard board = Context.Board;
            board.Clear();
        }

        /// <summary>
        /// 重置游戏数据
        /// </summary>
        private void ResetGameData()
        {
            // 重置MatchManager的数据
            if (MatchManager.Instance != null)
            {
                MatchManager.Instance.ClearData();
            }

            // 重置Context中的状态数据
            Context.MatchStateContext.Clear();
            Context.MatchStateContext.SetStep(Context.CurrentLevel.stepLimit);
        }

        private void ClearEcsWorld()
        {
            Context.World?.Destroy();
            Context.Systems?.Destroy();
            
            Context.World = null;
            Context.Systems = null;
        }
    }
}
