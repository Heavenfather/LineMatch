using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using GameCore.Log;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Module;
using HotfixLogic;
using HotfixLogic.Match;

namespace Hotfix.Logic.GamePlay
{
    public partial class MatchGameWorkflow
    {
        private void RegisterEvent()
        {
            EventDispatcher.AddEventListener<EventTwoParam<int, bool>>(GameEventDefine.OnMatchAddStep, OnMatchAddStep,
                this);
            EventDispatcher.AddEventListener(GameEventDefine.OnGameSuccess, OnGameSuccess, this);
            EventDispatcher.AddEventListener<EventOneParam<bool>>(GameEventDefine.OnGameFailure, OnGameFailure, this);
            EventDispatcher.AddEventListener<EventThreeParam<List<int>, List<int>, bool>>(GameEventDefine.OnMatchUpdateSpecialElements,OnMatchUpdateSpecialElements,this);
        }

        private void OnGameFailure(EventOneParam<bool> obj)
        {
            bool isNoneMatchFail = obj.Arg;
            _matchMainWindow.PlaySpine(MatchConst.SPINE_FAIL, isLoop: true);
            PopLoseWindow(isNoneMatchFail).Forget();
        }

        private void OnGameSuccess()
        {
            Logger.Info($"OnGameSuccess level : {_gameStateContext.CurrentLevel.id}");
            _matchMainWindow.PlaySpine(MatchConst.SPINE_SUCCESS, isLoop: true);
            //剩余步数结算分数
            var difficulty = MatchManager.Instance.GetMatchDifficulty(_gameStateContext.CurrentLevel.difficulty);
            LevelDiffScoreDB db = ConfigMemoryPool.Get<LevelDiffScoreDB>();
            int stepScore = db.CalScore(difficulty, _gameStateContext.MatchStateContext.RemainStep);
            Logger.Debug("剩余步数计分：" + stepScore);
            MatchManager.Instance.AddScore(stepScore);
            MatchManager.Instance.TickScoreChange();


            var baseCoin = MatchManager.Instance.GetBaseCoin(_gameStateContext.CurrentLevel.difficulty);
            if (MatchManager.Instance.GetLevelStar(MatchManager.Instance.CurLevelID) != 0)
            {
                baseCoin = 0;
            }
            //结算切换金币目标
            LevelTargetSystem.Instance.AddTarget((int)ElementIdConst.Coin, baseCoin);

            _matchMainWindow.SwitchTargetToCoin(baseCoin);
            DelayShowWinFinish().Forget();
        }

        private async UniTask DelayShowWinFinish()
        {
            await UniTask.Delay(1000);
            Action callback = () =>
            {
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    if (_gameStateContext.MatchStateContext.RemainStep > 0)
                    {
                        _matchMainWindow.TweenRemainStep(_gameStateContext.MatchStateContext.RemainStep);
                    }

                    var entity = _gameStateContext.World.NewEntity();
                    EcsPool<GameSettlementComponent> pool = _gameStateContext.World.GetPool<GameSettlementComponent>();
                    ref var component = ref pool.Add(entity);
                    component.RemainStep = _gameStateContext.MatchStateContext.RemainStep;
                    component.StepTextWorldPos = _matchMainWindow.GetStepWorldPosition();
                    // component.CoinIconWorldPos = _matchMainWindow.GetCoinScreenPosition();

                    _gameStateContext.MatchStateContext.IsGameSettlement = true;
                }).SetAutoKill(true);
            };
            G.UIModule.ShowUIAsync<MatchWinFinish>("", callback);
        }

        private void OnMatchAddStep(EventTwoParam<int, bool> obj)
        {
            _gameStateContext.MatchStateContext.IsResultTriggered = false;
            _gameStateContext.MatchStateContext.AddStep(obj.Arg1);
        }

        private async UniTask PopLoseWindow(bool isNoneMatchFail)
        {
            G.UIModule.ScreenLock("MatchFailure", true);
            // 失败界面
            var win = await G.UIModule.ShowUIAsyncAwait<MatchResultLose>("", isNoneMatchFail,
                LevelTargetSystem.Instance.TargetElements);
            win?.SetLevel(_gameStateContext.CurrentLevel);
            G.UIModule.ScreenLock("MatchFailure", false);
        }
        
        private void OnMatchUpdateSpecialElements(EventThreeParam<List<int>, List<int>, bool> obj)
        {
            if(obj.Arg3 == false)
                return;
            var itemIds = obj.Arg1;
            int entity = _gameStateContext.World.NewEntity();
            var pool = _gameStateContext.World.GetPool<GameContinueRequestComponent>();
            ref var request = ref pool.Add(entity);
            request.ContinueElements = itemIds;
        }
    }
}