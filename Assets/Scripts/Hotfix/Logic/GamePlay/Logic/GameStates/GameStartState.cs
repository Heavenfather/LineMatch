using System.Threading;
using Cysharp.Threading.Tasks;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 游戏开始状态
    /// 处理游戏开始逻辑 开始游戏循环
    /// </summary>
    public class GameStartState : IStateMachineContext<GameStateContext>
    {
        public StateMachine<GameStateContext> StateMachine { get; set; }
        public GameStateContext Context { get; set; }

        /// <summary>
        /// 是否每帧执行
        /// </summary>
        public bool PerFrameExecute => true;

        public UniTask OnEnter(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        public UniTask Execute(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return UniTask.CompletedTask;
            Context.Systems?.Run();

            return UniTask.CompletedTask;
        }

        public UniTask OnExit(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        public UniTask<bool> IsCanSwitch(string newStateKey, CancellationToken token)
        {
            return UniTask.FromResult(true);
        }
    }
}