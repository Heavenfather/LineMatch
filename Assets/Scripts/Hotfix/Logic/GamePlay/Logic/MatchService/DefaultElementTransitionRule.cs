using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    public class DefaultElementTransitionRule : IElementTransitionRuleService
    {
        private ElementMapDB _elementMap;
        
        /// <summary>
        /// 尝试转换下一个元素
        /// </summary>
        /// <param name="currentElement"></param>
        /// <param name="matchService"></param>
        /// <param name="nextElement"></param>
        /// <returns></returns>
        public bool TryTransitionToNextElement(int currentElement, IMatchService matchService, out int nextElement)
        {
            _elementMap = ConfigMemoryPool.Get<ElementMapDB>();
            nextElement = 0;
            
            ref readonly ElementMap currentConfig = ref _elementMap[currentElement];
            if (currentConfig.nextBlock <= 0)
                return false;
            
            nextElement = currentConfig.nextBlock;
            //其它特殊类型的转换 TODO....
            return true;
        }
    }
}