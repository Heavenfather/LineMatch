using System;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 收集道具飞行组件
    /// 用于标记需要飞向目标的收集道具
    /// </summary>
    public struct CollectItemFlyComponent : IEcsAutoReset<CollectItemFlyComponent>
    {
        /// <summary>
        /// 道具元素ID（决定飞行动画类型）
        /// </summary>
        public int ElementId;

        /// <summary>
        /// 起始位置
        /// </summary>
        public Vector3 StartPosition;

        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector3 TargetPosition;

        /// <summary>
        /// 飞行索引（用于延迟和错开动画）
        /// </summary>
        public int FlyIndex;

        /// <summary>
        /// 飞行完成回调（存储实体ID，在System中处理）
        /// </summary>
        public int CallbackEntityId;

        /// <summary>
        /// 是否已经开始飞行
        /// </summary>
        public bool IsFlying;

        /// <summary>
        /// 是否飞行完成
        /// </summary>
        public bool IsComplete;

        /// <summary>
        /// 是否是游戏结算的道具
        /// </summary>
        public bool IsGameSettlement;

        public void AutoReset(ref CollectItemFlyComponent com)
        {
            com.ElementId = 0;
            com.StartPosition = Vector3.zero;
            com.TargetPosition = Vector3.zero;
            com.FlyIndex = 0;
            com.CallbackEntityId = -1;
            com.IsFlying = false;
            com.IsComplete = false;
            com.IsGameSettlement = false;
        }
    }
}
