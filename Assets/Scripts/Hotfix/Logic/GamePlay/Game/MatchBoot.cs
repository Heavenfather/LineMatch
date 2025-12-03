using Cysharp.Threading.Tasks;
using HotfixCore.Utils;
using HotfixLogic;
using HotfixLogic.Match;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 游戏启动
    /// </summary>
    public static class MatchBoot
    {
        private static GameDIContainer _matchServiceContainer = null;
        public static GameDIContainer Container => _matchServiceContainer;

        private static IGameWorkflow _gameWorkflow = null;
        public static IGameWorkflow GameWorkflow => _gameWorkflow;

        static MatchBoot()
        {
            BootInitialize();
        }

        /// <summary>
        /// 启动消除游戏
        /// </summary>
        public static async UniTask BootStart(LevelData levelData, MatchServiceType matchType, MatchMainWindow window)
        {
            Logger.Debug("Match boot start");
            // 启动游戏工作流
            _gameWorkflow.SetShare(GameWorkflowKey.LevelData,levelData);
            _gameWorkflow.SetShare(GameWorkflowKey.MatchType, matchType);
            _gameWorkflow.SetShare(GameWorkflowKey.MatchMainWindow, window);
            await _gameWorkflow.WorkflowStart();

            // 开始游戏更新循环
            UnityUtil.AddUpdateListener(BootUpdate);
        }

        /// <summary>
        /// 退出消除游戏
        /// </summary>
        public static void BootExit()
        {
            Logger.Debug("Match boot exit");
            UnityUtil.RemoveUpdateListener(BootUpdate);
            _gameWorkflow.Exit();
        }

        private static void BootInitialize()
        {
            // 初始化依赖注入容器
            _matchServiceContainer = new GameDIContainer();
            //消除匹配模式
            _matchServiceContainer.RegisterSingleton<IMatchServiceFactory, MatchServiceFactory>();
            _matchServiceContainer.RegisterSingleton<IConnectionRuleService, DefaultConnectionRuleService>();
            // 元素工厂服务
            _matchServiceContainer.RegisterSingleton<IElementFactoryService, DefaultElementFactoryService>();
            _matchServiceContainer.RegisterSingleton<IElementResourceProvider, ElementResourceProvider>();
            _matchServiceContainer.RegisterSingleton<IElementTransitionRuleService, DefaultElementTransitionRule>();
            _matchServiceContainer.RegisterSingleton<IMatchRequestService, DefaultMatchRequestService>();
            _matchServiceContainer.RegisterSingleton(new DropStrategyService());

            _gameWorkflow = new MatchGameWorkflow();
            _gameWorkflow.Initialize();
        }

        private static void BootUpdate()
        {
            _gameWorkflow.OnUpdate(Time.deltaTime);
        }
    }
}