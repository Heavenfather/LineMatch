using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using GameCore.Log;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using HotfixLogic;
using HotfixLogic.Match;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 消除匹配游戏工作流
    /// </summary>
    public partial class MatchGameWorkflow : IGameWorkflow
    {
        private GameState _currentGameState = GameState.None;

        private StateMachine<GameStateContext> _workflowStateMachine;
        private GameStateContext _gameStateContext;
        private MatchMainWindow _matchMainWindow;
        private EventDispatcher _eventDispatcher;
        private bool _isWorking = false;
        private readonly Dictionary<string, object> _shareData = new Dictionary<string, object>(10);

        public void Initialize()
        {
            _workflowStateMachine = new StateMachine<GameStateContext>();
            _gameStateContext = new GameStateContext();
            _gameStateContext.SceneView = new GamePlaySceneView();
            _gameStateContext.Board = new Board();
            _gameStateContext.MatchStateContext = new MatchStateContext();
            
            _workflowStateMachine.Context = _gameStateContext;
            _workflowStateMachine.RegisterState(GameState.Initialize.ToString(), new GameInitializeState());
            _workflowStateMachine.RegisterState(GameState.Start.ToString(), new GameStartState());
            _workflowStateMachine.RegisterState(GameState.End.ToString(), new GameEndState());
            _workflowStateMachine.RegisterState(GameState.Pause.ToString(), new GamePauseState());
            _workflowStateMachine.RegisterState(GameState.Settlement.ToString(), new GameSettlementState());
            _workflowStateMachine.RegisterState(GameState.Restart.ToString(), new GameRestartState());
        }

        public EventDispatcher EventDispatcher => _eventDispatcher;
        

        public T SetShare<T>(string key, T value)
        {
            _shareData.TryAdd(key, value);
            return value;
        }

        public T GetShare<T>(string key)
        {
            if (_shareData.TryGetValue(key, out object value))
            {
                return (T)value;
            }

            return default;
        }

        public async UniTask WorkflowStart()
        {
            if (_isWorking)
            {
                Logger.Warning("GameWorkflow is working, can not start again");
                return;
            }
            
            _isWorking = true;
            _matchMainWindow = GetShare<MatchMainWindow>(GameWorkflowKey.MatchMainWindow);
            _gameStateContext.CurrentMatchType = GetShare<MatchServiceType>(GameWorkflowKey.MatchType);
            _gameStateContext.CurrentLevel = GetShare<LevelData>(GameWorkflowKey.LevelData);
            _gameStateContext.MatchStateContext.SetStep(_gameStateContext.CurrentLevel.stepLimit);
            _gameStateContext.MatchMainWindow =_matchMainWindow;
            _currentGameState = GameState.Initialize;
            _eventDispatcher = MemoryPool.Acquire<EventDispatcher>();
            RegisterEvent();
            await _workflowStateMachine.StartMachine(GameState.Initialize.ToString());
        }

        public async UniTask ChangeGameState(GameState gameState)
        {
            if (_currentGameState == gameState)
            {
                Logger.Warning($"GameWorkflow is already in state {gameState}");
                return;
            }

            _currentGameState = gameState;
            await _workflowStateMachine.ChangeState(gameState.ToString());
            //切换状态，当做工作继续
            _isWorking = true;
        }

        public void WorkflowStop()
        {
            if (!_isWorking)
            {
                Logger.Warning("GameWorkflow is not working, can not stop");
                return;
            }

            _isWorking = false;
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_isWorking)
                return;
            if (!_workflowStateMachine.IsRunning)
                return;

            _workflowStateMachine.MachineUpdate(deltaTime);
        }

        public void Exit()
        {
            MemoryPool.Release(_eventDispatcher);
            _eventDispatcher = null;
            UniTask.Create(async () =>
            {
                await ChangeGameState(GameState.End);
                _workflowStateMachine.MachineExit();
                _gameStateContext.MatchStateContext.Clear();
                _isWorking = false;
                _shareData?.Clear();
                _currentGameState = GameState.None;
            }).Forget();
        }
        
    }
}