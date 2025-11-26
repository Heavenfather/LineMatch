using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 游戏结束状态
    /// 处理游戏结束逻辑 退出整个消除游戏
    /// </summary>
    public class GameEndState : IStateMachineContext<GameStateContext>
    {
        /// <summary>
        /// 是否每帧执行
        /// </summary>
        public bool PerFrameExecute => false;
        
        public StateMachine<GameStateContext> StateMachine { get; set; }
        
        public GameStateContext Context { get; set; }

        public async UniTask OnEnter(CancellationToken token)
        {
            
        }

        public async UniTask Execute(CancellationToken token)
        {
            Context.World?.Destroy();
            Context.Systems?.Destroy();
            
            Context.World = null;
            Context.Systems = null;
            Context.Board.Clear();
            Context.CurrentLevel = null;
            
            // 安全卸载
            await ElementObjectPool.Instance.ClearAllPool(false);
            Context.SceneView.UnloadScene();
        }

        public async UniTask OnExit(CancellationToken token)
        {
        }

        public UniTask<bool> IsCanSwitch(string newStateKey, CancellationToken token)
        {
            return UniTask.FromResult(true);
        }

    }
}