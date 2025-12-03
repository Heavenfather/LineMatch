using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 待执行指令包组件
    /// </summary>
    public struct PendingActionsComponent
    {
        // 原子指令列表
        public List<AtomicAction> Actions;

        // 当前执行到的指令索引
        public int ExecutionIndex;

        // 当前正在进行的倒计时 (秒)
        public float CurrentWaitTimer;

        // 标记是否已经执行过预处理（合并优化）
        public bool IsOptimized;
    }
}