using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 需要收集目标类型的元素
    /// </summary>
    public struct TargetElementComponent
    {
        /// <summary>
        /// 收集物id
        /// </summary>
        public int TargetConfigId;
        
        /// <summary>
        /// 收集物总数量
        /// </summary>
        public int TargetTotal;
        
        /// <summary>
        /// 剩余数量
        /// </summary>
        public int RemainTargetNum;

        /// <summary>
        /// 本次收集到的所有目标实体id
        /// </summary>
        public HashSet<int> CollectedTargetEntities;
    }
}