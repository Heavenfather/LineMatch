using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public struct SearchDotComponent : IEcsAutoReset<SearchDotComponent>
    {
        /// <summary>
        /// 随机变身的基础棋子id
        /// </summary>
        public int SearchDotBaseElementId;

        public bool IsColorDirty;

        public List<int> SearchDotsEntities;
        
        public void AutoReset(ref SearchDotComponent com)
        {
            com.SearchDotsEntities?.Clear();
        }
    }
}