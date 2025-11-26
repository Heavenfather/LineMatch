namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// ECS世界配置
    /// </summary>
    public struct EcsConfig
    {
        /// <summary>
        /// 实体数量
        /// </summary>
        public int Entities;

        /// <summary>
        /// 已回收实体数量
        /// </summary>
        public int RecycledEntities;

        /// <summary>
        /// 组件池数量
        /// </summary>
        public int Pools;

        /// <summary>
        /// 过滤器数量
        /// </summary>
        public int Filters;

        /// <summary>
        /// 组件池密集度
        /// </summary>
        public int PoolDenseSize;

        /// <summary>
        /// 组件池回收大小
        /// </summary>
        public int PoolRecycledSize;

        /// <summary>
        /// 实体组件大小
        /// </summary>
        public int EntityComponentsSize;
        
        public const int EntitiesDefault = 512;
        public const int RecycledEntitiesDefault = 512;
        public const int PoolsDefault = 512;
        public const int FiltersDefault = 512;
        public const int PoolDenseSizeDefault = 512;
        public const int PoolRecycledSizeDefault = 512;
        public const int EntityComponentsSizeDefault = 64;
    }
}