
using HotfixLogic;
using HotfixLogic.Match;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 游戏状态上下文
    /// </summary>
    public class GameStateContext : IStateContext
    {
        /// <summary>
        /// ECS世界
        /// </summary>
        public EcsWorld World { get; set; }
        
        /// <summary>
        /// ECS系统
        /// </summary>
        public IEcsSystems Systems { get; set; }
        
        /// <summary>
        /// 当前消除类型
        /// </summary>
        public MatchServiceType CurrentMatchType { get; set; }
        
        /// <summary>
        /// 当前场景视图
        /// </summary>
        public ISceneView SceneView { get; set; }
        
        public MatchMainWindow MatchMainWindow { get; set; }
        
        /// <summary>
        /// 棋盘
        /// </summary>
        public IBoard Board { get; set; }
        
        /// <summary>
        /// 消除数据记录上下文信息
        /// </summary>
        public MatchStateContext MatchStateContext { get; set; }
        
        /// <summary>
        /// 匹配服务工厂
        /// </summary>
        public IMatchServiceFactory ServiceFactory => MatchBoot.Container.Resolve<IMatchServiceFactory>();
        
        /// <summary>
        /// 当前关卡数据
        /// </summary>
        public LevelData CurrentLevel { get; set; }
    }
}