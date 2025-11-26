using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 游戏初始化状态
    /// 处理游戏初始化逻辑,比如加载游戏资源，准备关卡数据等
    /// </summary>
    public class GameInitializeState : IStateMachineContext<GameStateContext>
    {
        public StateMachine<GameStateContext> StateMachine { get; set; }
        
        public GameStateContext Context { get; set; }
        
        public bool PerFrameExecute { get; } = false;

        public async UniTask OnEnter(CancellationToken token)
        {
            //加载游戏场景
            await Context.SceneView.LoadScene();
            //预加载游戏所用到的资源
            var resourceProvider = MatchBoot.Container.Resolve<IElementResourceProvider>();
            //对象池根节点
            Transform poolRoot = Context.SceneView.GetSceneRootTransform("MatchCanvas", "EleRoot");
            // 分析需要哪些资源
            var requiredIds = resourceProvider.AnalyzeLevelRequiredElements(
                Context.CurrentLevel,
                Context.ServiceFactory.GetService(Context.CurrentMatchType));
            // 执行预加载
            await resourceProvider.PreloadResources(requiredIds, poolRoot);
        }

        public async UniTask Execute(CancellationToken token)
        {
            //场景加载完成后进入开始阶段
            await StateMachine.ChangeState(GameState.Start.ToString());
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