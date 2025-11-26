namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 消除服务工厂接口
    /// </summary>
    public interface IMatchServiceFactory
    {
        /// <summary>
        /// 获取消除服务实例
        /// </summary>
        /// <param name="serviceType">消除服务类型</param>
        /// <returns></returns>
        IMatchService GetService(MatchServiceType serviceType);
        
        /// <summary>
        /// 获取元素工厂服务实例
        /// </summary>
        /// <returns></returns>
        IElementFactoryService GetElementFactoryService();

        /// <summary>
        /// 获取匹配规则实例
        /// </summary>
        /// <param name="ruleType"></param>
        /// <returns></returns>
        IMatchRule GetMatchRule(MatchRequestType ruleType);
    }
}