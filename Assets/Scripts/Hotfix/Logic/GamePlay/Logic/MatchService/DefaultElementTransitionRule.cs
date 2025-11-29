using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    public class DefaultElementTransitionRule : IElementTransitionRuleService
    {
        /// <summary>
        /// 尝试转换下一个元素
        /// </summary>
        /// <param name="currentElement"></param>
        /// <param name="matchService"></param>
        /// <param name="nextElement"></param>
        /// <returns></returns>
        public bool TryTransitionToNextElement(int currentElement, IMatchService matchService, out int nextElement)
        {
             var mapDB = ConfigMemoryPool.Get<ElementMapDB>();
            nextElement = 0;
            
            ref readonly ElementMap currentConfig = ref mapDB[currentElement];
            if (currentConfig.nextBlock <= 0)
                return false;
            
            nextElement = currentConfig.nextBlock;
            //其它特殊类型的转换 TODO....
            return true;
        }
    }
}