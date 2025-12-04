using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// TowDots玩法的彩球组件
    /// </summary>
    public struct TowDotsColoredBallComponent : IEcsAutoReset<TowDotsColoredBallComponent>
    {
        public HashSet<int> CollectedEntities;

        public void AutoReset(ref TowDotsColoredBallComponent com)
        {
            com.CollectedEntities?.Clear();
        }
    }
}