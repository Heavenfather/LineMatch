using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Log;
using HotfixCore.MemoryPool;

namespace Hotfix.Logic.GamePlay
{
    public class StateMachine<TContext> where TContext : class, IStateContext
    {
        private IState _currentState;
        private CancellationTokenSource _cts;
        private readonly Dictionary<string, IState> _states = new Dictionary<string, IState>();
        private bool _isRunning;
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 状态机上下文
        /// </summary>
        public TContext Context { get; set; }

        /// <summary>
        /// 注册状态
        /// </summary>
        /// <param name="state">状态实例</param>
        /// <param name="stateKey">状态键名</param>
        public void RegisterState(string stateKey, IState state)
        {
            if (string.IsNullOrEmpty(stateKey))
            {
                Logger.Error("StateMachine: 状态键名不能为空");
                return;
            }

            if (_states.ContainsKey(stateKey))
            {
                Logger.Warning($"StateMachine: 状态 '{stateKey}' 已注册，将被覆盖");
            }

            if (state is IStateMachineContext<TContext> contextAwareState)
            {
                contextAwareState.StateMachine = this;
                contextAwareState.Context = this.Context;
            }

            _states[stateKey] = state;
        }

        /// <summary>
        /// 启动状态机
        /// </summary>
        /// <param name="stateKey"></param>
        public async UniTask StartMachine(string stateKey)
        {
            if (_isRunning)
            {
                Logger.Warning("StateMachine: 状态机已运行，无法重复启动");
                return;
            }

            _isRunning = true;
            await ChangeState(stateKey);
        }

        /// <summary>
        /// 是否当前状态
        /// </summary>
        /// <param name="stateKey">状态键名</param>
        /// <returns></returns>
        public bool IsState(string stateKey)
        {
            if (!_states.ContainsKey(stateKey))
                return false;
            if (_currentState == null)
                return false;
            return _currentState.GetType() == _states[stateKey].GetType();
        }

        /// <summary>
        /// 取消当前状态
        /// </summary>
        /// <param name="stateKey"></param>
        public void CancelState(string stateKey)
        {
            if (_currentState == null)
                return;

            if (_currentState.GetType() != _states[stateKey].GetType())
                return;

            _cts?.Cancel();
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        public async UniTask ChangeState(string stateKey)
        {
            if (string.IsNullOrEmpty(stateKey))
            {
                Logger.Error("StateMachine: 状态键名不能为空");
                return;
            }

            if (!_states.TryGetValue(stateKey, out var newState))
            {
                Logger.Error($"StateMachine: 未找到状态 '{stateKey}'，请先注册");
                return;
            }

            if (_currentState != null)
            {
                bool isCanSwitch = await _currentState.IsCanSwitch(stateKey, _cts.Token);
                if (!isCanSwitch)
                {
                    Logger.Warning($"StateMachine: 状态 '{stateKey}' 切换失败，不允许 {_currentState.GetType()} -> {newState.GetType()}");
                    return;
                }
            }

            if (_currentState != null)
            {
                _cts?.Cancel();
                await _currentState.OnExit(CancellationToken.None);
            }

            _currentState = newState;
            _cts = new CancellationTokenSource();
            try
            {
                await _currentState.OnEnter(_cts.Token);
                await _currentState.Execute(_cts.Token);
            }
            catch (Exception e)
            {
                Logger.Warning($"StateMachine ChangeState Error: {e}");
            }
        }

        /// <summary>
        /// 退出状态机
        /// </summary>
        public void MachineExit()
        {
            _cts?.Cancel();
            _currentState?.OnExit(CancellationToken.None).Forget();
            _currentState = null;
            _isRunning = false;
        }

        /// <summary>
        /// 状态机更新
        /// 每帧调用，用于处理需要每帧执行的状态
        /// </summary>
        /// <param name="deltaTime"></param>
        public void MachineUpdate(float deltaTime)
        {
            if (_currentState is { PerFrameExecute: true })
            {
                _currentState.Execute(_cts.Token).Forget();
            }
        }

        /// <summary>
        /// 获取当前状态键名（用于调试）
        /// </summary>
        public string GetCurrentStateKey()
        {
            foreach (var kvp in _states)
            {
                if (kvp.Value == _currentState)
                    return kvp.Key;
            }

            return null;
        }
    }
}