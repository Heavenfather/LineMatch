using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 游戏暂停状态
    /// 处理游戏暂停逻辑 暂停游戏运行 但不退出游戏
    /// </summary>
    public class GamePauseState : IState
    {
        /// <summary>
        /// 是否每帧执行
        /// </summary>
        public bool PerFrameExecute => false;

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