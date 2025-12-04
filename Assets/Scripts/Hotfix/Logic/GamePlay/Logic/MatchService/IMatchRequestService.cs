using System.Collections.Generic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 消除请求服务 - 统一负责生产消除请求实体
    /// </summary>
    public interface IMatchRequestService
    {
        /// <summary>
        /// 发起玩家连线消除请求
        /// </summary>
        void RequestPlayerLine(EcsWorld world, List<int> selectedEntities, int triggerEntity, int configId);

        /// <summary>
        /// 发起玩家方块/闭环消除请求
        /// </summary>
        void RequestPlayerSquare(EcsWorld world, List<int> selectedEntities, int closedPointEntity, int configId);

        /// <summary>
        /// 发起道具消除请求
        /// </summary>
        void RequestUseItem(EcsWorld world, int itemId, Vector2Int targetGridPos);

        /// <summary>
        /// 发起掉落后的自动检测请求 (如全屏爆、收集物掉落等等)
        /// </summary>
        void RequestPostDropCheck(EcsWorld world, int checkEntity);

        /// <summary>
        /// 单个火箭消除请求
        /// </summary>
        void RequestRocket(EcsWorld world, int rocketEntity);

        /// <summary>
        /// 单个炸弹
        /// </summary>
        void RequestBomb(EcsWorld world, Vector2Int bombCoord);

        /// <summary>
        /// 单个搜寻点被动爆炸
        /// </summary>
        /// <param name="world"></param>
        /// <param name="dotEntity"></param>
        void RequestSearchDot(EcsWorld world, int dotEntity);

        /// <summary>
        /// 单个星爆点被动爆炸
        /// </summary>
        /// <param name="world"></param>
        /// <param name="dotEntity"></param>
        void RequestStarBombDot(EcsWorld world, int dotEntity);
    }
}