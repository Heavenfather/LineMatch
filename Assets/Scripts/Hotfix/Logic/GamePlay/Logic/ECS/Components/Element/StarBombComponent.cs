using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public struct StarBombComponent : IEcsAutoReset<StarBombComponent>
    {
        /// <summary>
        /// 随机变身的基础棋子id
        /// </summary>
        public int StarDotBaseElementId;

        /// <summary>
        /// 颜色是否需要更新
        /// </summary>
        public bool IsColorDirty;

        /// <summary>
        /// 星爆点消除的目标实体列表（3x3范围+一行一列）
        /// </summary>
        public List<int> TargetEntities;
        
        public void AutoReset(ref StarBombComponent com)
        {
            com.TargetEntities?.Clear();
        }
    }
}