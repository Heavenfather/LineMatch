using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 游戏重新开始状态
    /// 处理游戏重新开始逻辑 重新开始游戏,重置游戏数据等
    /// </summary>
    public class GameRestartState : IStateMachineContext<GameStateContext>
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