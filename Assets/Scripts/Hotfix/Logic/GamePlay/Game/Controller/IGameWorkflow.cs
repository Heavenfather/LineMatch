using Cysharp.Threading.Tasks;
using HotfixCore.Module;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 游戏工作流接口
    /// 定义游戏工作流的基本操作，包括初始化、启动、状态改变和退出等
    /// </summary>
    public interface IGameWorkflow
    {
        EventDispatcher EventDispatcher { get; }
        
        GameState CurrentGameState { get; }
        
        /// <summary>
        /// 设置共享数据
        /// </summary>
        /// <returns></returns>
        /// <param name="key">共享数据的键</param>
        /// <param name="value">共享数据的值</param>
        /// <typeparam name="T">共享数据的类型</typeparam>
        /// <returns>设置的共享数据值</returns>
        T SetShare<T>(string key, T value);
        
        /// <summary>
        /// 获取共享数据
        /// </summary>
        /// <param name="key">共享数据的键</param>
        /// <typeparam name="T">共享数据的类型</typeparam>
        /// <returns>共享数据的值</returns>
        T GetShare<T>(string key);
        
        /// <summary>
        /// 初始化游戏工作流-整个游戏生命周期只运行一次
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 游戏工作流启动-每次消除开始都会调用一次
        /// </summary>
        UniTask WorkflowStart();

        /// <summary>
        /// 改变游戏状态
        /// </summary>
        /// <param name="gameState">目标游戏状态</param>
        UniTask ChangeGameState(GameState gameState);
        
        /// <summary>
        /// 游戏工作流停止
        /// </summary>
        void WorkflowStop();
        
        /// <summary>
        /// 游戏更新
        /// </summary>
        /// <param name="deltaTime">自上次更新以来的时间间隔</param>
        void OnUpdate(float deltaTime);

        /// <summary>
        /// 游戏退出-每次消除退出都会调用一次
        /// </summary>
        void Exit();
    }
}