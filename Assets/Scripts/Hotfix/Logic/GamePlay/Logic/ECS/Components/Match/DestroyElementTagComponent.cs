namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 元素销毁标签，被标记的元素都会在ElementDestroySystem中被回收
    /// </summary>
    public struct DestroyElementTagComponent
    {
        /// <summary>
        /// 销毁X的坐标 记录被打上销毁的标签的坐标
        /// </summary>
        public int X;
        
        /// <summary>
        /// 销毁Y的坐标
        /// </summary>
        public int Y;

        /// <summary>
        /// 被销毁的配置Id
        /// </summary>
        public int ConfigId;

        /// <summary>
        /// 被销毁的元素实体Id
        /// </summary>
        public int EntityId;
    }
}