using System.Threading;
using Cysharp.Threading.Tasks;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 游戏开始状态
    /// 处理游戏开始逻辑 开始游戏循环
    /// </summary>
    public class GameStartState : IStateMachineContext<GameStateContext>
    {
        private bool _isEntered = false;

        //--------- ECS ---------------
        private EcsWorld _world;
        private IEcsSystems _systems;

        public StateMachine<GameStateContext> StateMachine { get; set; }
        public GameStateContext Context { get; set; }

        /// <summary>
        /// 是否每帧执行
        /// </summary>
        public bool PerFrameExecute => true;

        public UniTask OnEnter(CancellationToken token)
        {
            RegisterEcsWorld();
            _systems.Init();
            return UniTask.CompletedTask;
        }

        public UniTask Execute(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return UniTask.CompletedTask;
            _systems?.Run();

            return UniTask.CompletedTask;
        }

        public UniTask OnExit(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        public UniTask<bool> IsCanSwitch(string newStateKey, CancellationToken token)
        {
            return UniTask.FromResult(true);
        }

        private void RegisterEcsWorld()
        {
            _world = new EcsWorld();
            this.Context.World = _world;
            _systems = new EcsSystems(_world, Context);
            this.Context.Systems = _systems;
            _systems
                // !!!!!!!!!! 添加的顺序影响着每个系统的初始化逻辑和更新的先后顺序，谨慎思考添加顺序 !!!!!!!!!!
                // 棋盘生命周期
                .Add(new BoardRootSystem())
                .Add(new BoardInitSystem())
                .Add(new BoardRenderSystem())
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
                // ---------- 不同类型棋子处理 各种棋子对消除执行的反应 ----------------
                .Add(new LockVisualSystem())
                .Add(new RocketSystem())
                .Add(new BombSystem())
                .Add(new TargetElementSystem())
                
                // Normal需要排在最后，因为其它棋子可能需要对它进行处理
                .Add(new NormalElementSystem())
                //------- 各个棋子处理完成格子逻辑 统一交由 ElementDestroySystem 执行回收 -------------
                .Add(new ElementDestroySystem()) // 元素消除
                .Add(new ProjectileSystem()) //生成新的棋子
                .Add(new DropAnalysisSystem()) //处理掉落,分析和生产掉落数据
                .Add(new DropElementSpawnSystem()) //掉落元素生成
                .Add(new DropElementAnimationSystem()) //掉落元素动画
                .Add(new PostDropActionSystem()) // 处理掉落渲染处理

                ;
        }
    }
}