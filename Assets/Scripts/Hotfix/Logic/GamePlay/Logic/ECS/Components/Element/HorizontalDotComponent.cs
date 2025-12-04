using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 直线消除点组件
    /// 消除效果：消除所在行的所有棋子
    /// </summary>
    public struct HorizontalDotComponent : IEcsAutoReset<HorizontalDotComponent>
    {
        /// <summary>
        /// 直线消除点的目标实体列表（一整行）
        /// </summary>
        public List<int> TargetEntities;
        
        public void AutoReset(ref HorizontalDotComponent com)
        {
            com.TargetEntities?.Clear();
        }
    }
}
