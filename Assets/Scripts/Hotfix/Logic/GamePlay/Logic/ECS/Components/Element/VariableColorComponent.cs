namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 通用可变色（白色）组件
    /// </summary>
    public struct VariableColorComponent
    {
        /// <summary>
        /// 当前颜色Id 0表示无颜色
        /// </summary>
        public int CurrentColorId;

        /// <summary>
        /// 当前颜色Id对应的格子坐标 x
        /// </summary>
        public int X;

        /// <summary>
        /// 当前颜色Id对应的格子坐标 y
        /// </summary>
        public int Y;

        /// <summary>
        /// 颜色是否需要更新
        /// </summary>
        public bool IsColorDirty;

        /// <summary>
        /// 当棋子拥有颜色后，是否会被冻结连接
        /// </summary>
        public bool IsCanFreezeConnect;
    }
}