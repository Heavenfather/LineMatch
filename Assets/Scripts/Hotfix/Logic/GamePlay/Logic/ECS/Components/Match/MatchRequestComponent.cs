using System.Collections.Generic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 消除请求组件
    /// </summary>
    public struct MatchRequestComponent
    {
        public MatchRequestType Type;
        public int TriggerEntity; // 触发者
        public int TargetEntity; // 目标者 (如交换对象/道具释放点)
        public Vector2Int TargetGridPos; // 目标格子
        public List<int> InvolvedEntities; // 所有涉及的棋子 (连线路径)
        public int ConfigId; // 涉及的颜色/配置ID
        public int ItemId; // 如果是 UseItem，记录道具ID
        public object ExtraData;
    }
}