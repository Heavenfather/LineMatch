using System.Collections.Generic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 水蔓延请求组件
    /// 用于触发水的蔓延逻辑
    /// </summary>
    public struct SpreadWaterSpreadComponent : IEcsAutoReset<SpreadWaterSpreadComponent>
    {
        /// <summary>
        /// 本次消除的格子坐标列表
        /// </summary>
        public List<Vector2Int> EliminatedCoords;

        /// <summary>
        /// 水的配置ID
        /// </summary>
        public int WaterConfigId;

        /// <summary>
        /// 蔓延的目标格子坐标列表
        /// </summary>
        public HashSet<Vector2Int> SpreadTargets;

        /// <summary>
        /// 是否已经处理完成
        /// </summary>
        public bool IsProcessed;

        /// <summary>
        /// 是否需要播放流动动画
        /// </summary>
        public bool NeedFlowAnimation;

        public void AutoReset(ref SpreadWaterSpreadComponent com)
        {
            com.EliminatedCoords?.Clear();
            com.WaterConfigId = 0;
            com.SpreadTargets?.Clear();
            com.IsProcessed = false;
            com.NeedFlowAnimation = true;
        }
    }
}
