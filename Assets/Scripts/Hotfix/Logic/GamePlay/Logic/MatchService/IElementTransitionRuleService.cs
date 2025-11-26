namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 元素转换规则服务接口
    /// </summary>
    public interface IElementTransitionRuleService
    {
        /// <summary>
        /// 尝试转换下一个元素
        /// </summary>
        /// <param name="currentElement"></param>
        /// <param name="matchService"></param>
        /// <param name="nextElement"></param>
        /// <returns></returns>
        bool TryTransitionToNextElement(int currentElement, IMatchService matchService, out int nextElement);
    }
}