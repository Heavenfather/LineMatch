using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 待执行指令包组件
    /// </summary>
    public struct PendingActionsComponent
    {
        public List<AtomicAction> Actions; // 原子指令列表
    }
}