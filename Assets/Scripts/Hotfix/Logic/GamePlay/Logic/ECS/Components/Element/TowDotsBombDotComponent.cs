using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// TowDots玩法的爆炸点组件（白色点）
    /// 可以与任意Normal棋子相连，连接后变成对应颜色
    /// 消除效果：以最后连接点为中心的3x3范围爆炸
    /// </summary>
    public struct TowDotsBombDotComponent : IEcsAutoReset<TowDotsBombDotComponent>
    {
        /// <summary>
        /// 已连到的棋子实体
        /// </summary>
        public List<int> ConnectedEntities;

        /// <summary>
        /// 爆炸点的目标实体列表（3x3范围）
        /// </summary>
        public List<int> TargetEntities;
        
        public void AutoReset(ref TowDotsBombDotComponent com)
        {
            com.ConnectedEntities?.Clear();
            com.TargetEntities?.Clear();
        }
    }
}