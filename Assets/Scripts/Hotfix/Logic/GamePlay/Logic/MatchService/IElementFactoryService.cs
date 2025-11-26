namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 元素工厂服务
    /// </summary>
    public interface IElementFactoryService
    {
        /// <summary>
        /// 设置元素入场策略
        /// </summary>
        /// <param name="strategy"></param>
        void SetSpawnStrategy(IBoardSpawnStrategy strategy);
        
        /// <summary>
        /// 创建元素实体
        /// </summary>
        /// <returns></returns>
        int CreateElementEntity(GameStateContext context,IMatchService matchService, int configId, int x, int y, int width = 1, int height = 1);
    }
}