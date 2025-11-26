using System.Threading;
using Cysharp.Threading.Tasks;
using HotfixCore.MemoryPool;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 消除游戏状态管理
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// 是否每帧执行
        /// </summary>
        bool PerFrameExecute { get;}
        
        /// <summary>
        /// 进入状态
        /// </summary>
        /// <returns></returns>
        UniTask OnEnter(CancellationToken token);

        /// <summary>
        /// 状态逻辑更新
        /// </summary>
        /// <returns></returns>
        UniTask Execute(CancellationToken token);
        
        /// <summary>
        /// 退出状态
        /// </summary>
        /// <returns></returns>
        UniTask OnExit(CancellationToken token);
        
        /// <summary>
        /// 是否可以切换状态
        /// </summary>
        /// <returns></returns>
        UniTask<bool> IsCanSwitch(string newStateKey,CancellationToken token);
    }
    
    /// <summary>
    /// 状态机上下文定义接口
    /// </summary>
    public interface IStateContext
    {
        
    }
    
    /// <summary>
    /// 状态机上下文
    /// 统一状态机注入和上下文传递
    /// </summary>
    public interface IStateMachineContext<TContext> : IState where TContext : class, IStateContext
    {       
        /// <summary>
        /// 状态机实例
        /// </summary>
        StateMachine<TContext> StateMachine { get; set; }
        
        /// <summary>
        /// 状态机上下文
        /// </summary>
        TContext Context { get; set; }
    }
}