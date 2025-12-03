using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Module;
using HotfixLogic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 游戏结算状态
    /// </summary>
    public class GameSettlementState : IStateMachineContext<GameStateContext>
    {
        public StateMachine<GameStateContext> StateMachine { get; set; }

        public GameStateContext Context { get; set; }

        public bool PerFrameExecute => false;

        public async UniTask OnEnter(CancellationToken token)
        {
            
        }

        public async UniTask Execute(CancellationToken token)
        {
            // await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
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