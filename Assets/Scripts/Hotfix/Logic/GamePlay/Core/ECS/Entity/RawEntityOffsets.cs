namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 实体数据在内部数组存储中的偏移量定义
    /// 用于更高效的直接使用数组下标获取实体组件
    /// </summary>
    public static class RawEntityOffsets
    {
        /// <summary>
        /// 实体组件数量偏移量
        /// </summary>
        public const int ComponentsCount = 0;

        /// <summary>
        /// 实体 代数在实体数据数组中的偏移量
        /// </summary>
        public const int Gen = 1;

        /// <summary>
        /// 实体组件列表在实体数据中的起始量偏移值
        /// </summary>
        public const int Components = 2;
    }
}