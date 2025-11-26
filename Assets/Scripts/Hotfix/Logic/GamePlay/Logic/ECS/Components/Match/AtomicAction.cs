using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 指令操作数据
    /// </summary>
    public struct AtomicAction
    {
        public MatchActionType Type;
        public int TargetEntity;      // 作用对象
        public Vector2Int GridPos;    // 作用位置
        public int Value;             // 参数 (如扣除层数值、分数、变身ID，不同的指令类型有不同的含义)
        public object ExtraData;      // 额外数据 （具体含义根据指令类型而定）
    }
}