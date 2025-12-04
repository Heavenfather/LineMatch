using System;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 收集道具飞行服务接口
    /// 提供统一的道具飞行请求接口
    /// </summary>
    public interface ICollectItemFlyService
    {
        /// <summary>
        /// 请求道具飞向目标
        /// </summary>
        /// <param name="world">ECS世界</param>
        /// <param name="elementId">道具元素ID</param>
        /// <param name="startPosition">起始位置</param>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="callback">飞行完成回调</param>
        /// <param name="flyIndex">飞行索引（-1表示自动分配）</param>3
        /// <param name="isGameSettlement">是否是游戏结算</param>
        void RequestCollectItemFly(EcsWorld world, int elementId, Vector3 startPosition, Vector3 targetPosition,
            Action callback = null, int flyIndex = -1,bool isGameSettlement = false);

        /// <summary>
        /// 重置飞行索引
        /// </summary>
        void ResetFlyIndex();

        /// <summary>
        /// 获取下一个飞行索引
        /// </summary>
        int GetNextFlyIndex();
    }
}
