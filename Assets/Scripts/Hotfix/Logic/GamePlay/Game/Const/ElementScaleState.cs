namespace Hotfix.Logic.GamePlay
{
    public enum ElementScaleState
    {
        None,
        PunchOnce,  // 选中时弹一下（放大并保持）
        Breathing,  // 闭环时呼吸循环
        Shake,       // 道具使用时抖动
        Change,
    }
}