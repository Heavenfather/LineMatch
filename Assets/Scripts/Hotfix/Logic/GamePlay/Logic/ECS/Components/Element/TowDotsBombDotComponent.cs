using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// TowDots玩法的爆炸点组件
    /// </summary>
    public struct TowDotsBombDotComponent
    {
        /// <summary>
        /// 已连到的棋子实体
        /// </summary>
        public List<int> ConnectedEntities;
    }
}