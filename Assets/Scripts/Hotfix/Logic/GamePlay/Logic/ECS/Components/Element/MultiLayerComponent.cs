namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 多层元素组件
    /// 特性：
    /// 1. 支持多层消除，每次伤害消除一层
    /// 2. 消除一层后自动转换到下一层形态
    /// 3. 所有层都消除后完全销毁
    /// 4. 支持一次性消除多层
    /// 
    /// 使用方式：
    /// - 在Builder中添加此组件并初始化RemainingLayers
    /// - MultiLayerElementSystem会自动处理伤害和形态转换
    /// </summary>
    public struct MultiLayerComponent
    {
        /// <summary>
        /// 当前剩余层数
        /// 用于判断是否完全消除
        /// </summary>
        public int RemainingLayers;

        /// <summary>
        /// 是否已经被处理了
        /// </summary>
        public bool IsEliminate;

        /// <summary>
        /// 是否将要转换形态
        /// </summary>
        public bool IsWillTransform;
    }
}
