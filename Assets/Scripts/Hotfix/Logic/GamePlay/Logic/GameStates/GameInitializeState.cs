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
        // -------- ECS ------------
        private EcsWorld _world;
        private IEcsSystems _systems;

        public StateMachine<GameStateContext> StateMachine { get; set; }

        public GameStateContext Context { get; set; }

        public bool PerFrameExecute { get; } = false;

        public async UniTask OnEnter(CancellationToken token)
        {
            if (Context.IsGameReStart == false)
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

                // 初始化收集道具服务
                InitializeCollectItemService();
            }

            RegisterEcsWorld();
        }

        public async UniTask Execute(CancellationToken token)
        {
            this.Context.Systems.Init();
            //场景加载完成后进入开始阶段
            await MatchBoot.GameWorkflow.ChangeGameState(GameState.Start);
        }

        public async UniTask OnExit(CancellationToken token)
        {
        }

        public UniTask<bool> IsCanSwitch(string newStateKey, CancellationToken token)
        {
            return UniTask.FromResult(true);
        }

        private void InitializeCollectItemService()
        {
            // 获取或创建CollectItemRoot
            Transform poolRoot = Context.SceneView.GetSceneRootTransform("MatchCanvas", "CollectItemRoot");
            if (poolRoot == null)
            {
                Transform matchCanvas = Context.SceneView.GetSceneRootTransform("MatchCanvas", "");
                GameObject collectRoot = new GameObject("CollectItemRoot");
                collectRoot.transform.SetParent(matchCanvas, false);
                poolRoot = collectRoot.transform;
            }

            // 创建对象池服务
            var poolService = new CollectItemPoolService(poolRoot);
            
            // 创建并注册收集道具飞行服务
            var collectFlyService = new CollectItemFlyService(poolService);
            MatchBoot.Container.RegisterSingleton<ICollectItemFlyService>(collectFlyService);
        }

        private void RegisterEcsWorld()
        {
            _world = new EcsWorld();
            Context.World = _world;
            _systems = new EcsSystems(_world, Context);
            Context.Systems = _systems;
            
            _systems
                // !!!!!!!!!! 添加的顺序影响着每个系统的初始化逻辑和更新的先后顺序，谨慎思考添加顺序 !!!!!!!!!!
                // 棋盘生命周期
                .Add(new BoardRootSystem())
                .Add(new BoardInitSystem())
                .Add(new BoardRenderSystem())
                // 开局道具应用
                .Add(new BeginItemApplySystem())
                // 棋子构建，棋子的渲染和创建
                .Add(new ElementSpawnSystem())
                .Add(new ElementViewInitSystem())
                // 入场动画
                .Add(new PieceSpawnAnimationSystem())
                // ------------------------------------------------
                .Add(new MatchInputSystem()) // 连线输入
                .Add(new MatchLineVisualSystem())

                // ----------- 消除匹配逻辑 这里一定要弄清楚消除的顺序 -------------------------
                .Add(new MatchAnalysisSystem()) //消除分析
                .Add(new ActionExecutionSystem()) //消除执行
                .Add(new AdjacentEliminationSystem()) //通用旁消特性处理
                .Add(new MultiLayerElementSystem()) //通用多层元素处理
                // ---------- 不同类型棋子处理 各种棋子对消除执行的反应 ----------------
                .Add(new SpreadWaterSystem())
                .Add(new SpreadWaterVisualSystem())
                .Add(new LockVisualSystem())
                .Add(new RocketSystem())
                .Add(new BombSystem())
                .Add(new TargetElementSystem())
                .Add(new BombElementSystem())
                .Add(new SearchDotsSystem())
                .Add(new StarBombSystem())
                .Add(new VariableColorSystem())
                .Add(new HorizontalDotSystem())
                .Add(new TowDotsBombDotSystem())
                .Add(new TowDotsColoredBallSystem())
                .Add(new DropStyleElementSystem())
                .Add(new SpreadFireSystem())
                .Add(new BlockElementSystem())
                
                // Normal需要排在最后，因为其它棋子可能需要对它进行处理
                .Add(new NormalElementSystem())
                .Add(new CollectItemFlySystem())// 收集道具飞行系统
                //------- 各个棋子处理完成格子逻辑 统一交由 ElementDestroySystem 执行回收 -------------
                .Add(new ElementDestroySystem()) // 元素消除
                

                .Add(new DropAnalysisSystem()) //处理掉落,分析和生产掉落数据
                .Add(new DropElementSpawnSystem()) //掉落元素生成
                .Add(new DropElementAnimationSystem()) //掉落元素动画
                .Add(new PostDropActionSystem()) // 处理掉落渲染处理
                
                .Add(new ShuffleSystem()) // 洗牌检测
                .Add(new ShuffleAnimationSystem())
                .Add(new GameResultCheckSystem())
                .Add(new GameSettlementSystem())
                
#if UNITY_EDITOR
                .Add (new Hotfix.Logic.GamePlay.Debugger.EcsWorldDebugSystem ())
                .Add (new Hotfix.Logic.GamePlay.Debugger.EcsSystemsDebugSystem ())
#endif
                ;
        }
    }
}