using System;
using System.Runtime.CompilerServices;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// ECS过滤器类，用于筛选符合特定组件组合的实体
    /// </summary>
    public sealed class EcsFilter
    {
        /// <summary>
        /// 所属的ECS世界
        /// </summary>
        readonly EcsWorld _world;

        /// <summary>
        /// 组件掩码，定义过滤条件
        /// </summary>
        readonly EcsWorld.EcsMask _mask;

        /// <summary>
        /// 实体密集数组，存储符合过滤条件的实体ID
        /// </summary>
        int[] _denseEntities;

        /// <summary>
        /// 实体数量，记录当前符合过滤条件的实体数量
        /// </summary>
        int _entitiesCount;

        /// <summary>
        /// 实体稀疏索引数组，用于快速查找实体在密集数组中的位置
        /// </summary>
        internal int[] SparseEntities;

        /// <summary>
        /// 锁计数器，用于同步操作
        /// </summary>
        int _lockCount;

        /// <summary>
        /// 延迟操作数组，用于批量处理添加/移除实体操作
        /// </summary>
        DelayedOp[] _delayedOps;

        /// <summary>
        /// 延迟操作计数器，记录当前延迟操作的数量
        /// </summary>
        int _delayedOpsCount;

        /// <summary>
        /// 构造函数，初始化过滤器
        /// </summary>
        /// <param name="world">所属的世界</param>
        /// <param name="mask">组件掩码，定义过滤条件</param>
        /// <param name="denseCapacity">密集数组初始容量</param>
        /// <param name="sparseCapacity">稀疏数组初始容量</param>
        internal EcsFilter(EcsWorld world, EcsWorld.EcsMask mask, int denseCapacity, int sparseCapacity)
        {
            _world = world;
            _mask = mask;
            _denseEntities = new int[denseCapacity];
            SparseEntities = new int[sparseCapacity];
            _entitiesCount = 0;
            _delayedOps = new DelayedOp[512];
            _delayedOpsCount = 0;
            _lockCount = 0;
        }

        public int this[int idx]
        {
            get
            {
                if (idx < 0 || idx >= _entitiesCount)
                    return -1;
                return _denseEntities[idx];
            }
        }

        /// <summary>
        /// 获取过滤器所属的世界
        /// </summary>
        /// <returns>所属的ECS世界</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EcsWorld GetWorld()
        {
            return _world;
        }

        /// <summary>
        /// 获取过滤器中实体的数量
        /// </summary>
        /// <returns>实体数量</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEntitiesCount()
        {
            return _entitiesCount;
        }

        /// <summary>
        /// 获取原始实体数组的引用
        /// </summary>
        /// <returns>实体密集数组</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int[] GetRawEntities()
        {
            return _denseEntities;
        }

        /// <summary>
        /// 获取稀疏索引数组的引用
        /// </summary>
        /// <returns>稀疏索引数组</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int[] GetSparseIndex()
        {
            return SparseEntities;
        }

        /// <summary>
        /// 获取枚举器，用于遍历过滤器中的实体
        /// </summary>
        /// <returns>实体枚举器</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            _lockCount++;
            return new Enumerator(this);
        }

        /// <summary>
        /// 调整稀疏索引数组的大小
        /// </summary>
        /// <param name="capacity">新的容量</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ResizeSparseIndex(int capacity)
        {
            Array.Resize(ref SparseEntities, capacity);
        }

        /// <summary>
        /// 获取过滤器的组件掩码
        /// </summary>
        /// <returns>组件掩码</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal EcsWorld.EcsMask GetMask()
        {
            return _mask;
        }

        /// <summary>
        /// 向过滤器添加实体
        /// </summary>
        /// <param name="entity">要添加的实体ID</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddEntity(int entity)
        {
            if (AddDelayedOp(true, entity))
            {
                return;
            }

            if (_entitiesCount == _denseEntities.Length)
            {
                Array.Resize(ref _denseEntities, _entitiesCount << 1);
            }

            _denseEntities[_entitiesCount++] = entity;
            SparseEntities[entity] = _entitiesCount;
        }

        /// <summary>
        /// 从过滤器中移除实体
        /// </summary>
        /// <param name="entity">要移除的实体ID</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveEntity(int entity)
        {
            if (AddDelayedOp(false, entity))
            {
                return;
            }

            var idx = SparseEntities[entity] - 1;
            SparseEntities[entity] = 0;
            _entitiesCount--;
            if (idx < _entitiesCount)
            {
                _denseEntities[idx] = _denseEntities[_entitiesCount];
                SparseEntities[_denseEntities[idx]] = idx + 1;
            }
        }

        /// <summary>
        /// 添加延迟操作，用于在过滤器锁定时延迟实体添加/移除操作
        /// </summary>
        /// <param name="added">true表示添加操作，false表示移除操作</param>
        /// <param name="entity">实体ID</param>
        /// <returns>如果操作被延迟返回true，否则false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool AddDelayedOp(bool added, int entity)
        {
            if (_lockCount <= 0)
            {
                return false;
            }

            if (_delayedOpsCount == _delayedOps.Length)
            {
                Array.Resize(ref _delayedOps, _delayedOpsCount << 1);
            }

            ref var op = ref _delayedOps[_delayedOpsCount++];
            op.Added = added;
            op.Entity = entity;
            return true;
        }

        /// <summary>
        /// 解锁过滤器，处理所有延迟的操作
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Unlock()
        {
#if DEBUG
            if (_lockCount <= 0)
            {
                throw new Exception($"Invalid lock-unlock balance for \"{GetType().Name}\".");
            }
#endif
            _lockCount--;
            if (_lockCount == 0 && _delayedOpsCount > 0)
            {
                for (int i = 0, iMax = _delayedOpsCount; i < iMax; i++)
                {
                    ref var op = ref _delayedOps[i];
                    if (op.Added)
                    {
                        AddEntity(op.Entity);
                    }
                    else
                    {
                        RemoveEntity(op.Entity);
                    }
                }

                _delayedOpsCount = 0;
            }
        }

        /// <summary>
        /// 过滤器实体枚举器，用于安全地遍历过滤器中的实体
        /// </summary>
        public struct Enumerator : IDisposable
        {
            readonly EcsFilter _filter;
            readonly int[] _entities;
            readonly int _count;
            int _idx;

            /// <summary>
            /// 构造函数，初始化枚举器
            /// </summary>
            /// <param name="filter">所属的过滤器</param>
            public Enumerator(EcsFilter filter)
            {
                _filter = filter;
                _entities = filter._denseEntities;
                _count = filter._entitiesCount;
                _idx = -1;
            }

            /// <summary>
            /// 获取当前枚举的实体ID
            /// </summary>
            public int Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _entities[_idx];
            }

            /// <summary>
            /// 移动到下一个实体
            /// </summary>
            /// <returns>如果还有实体返回true，否则false</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return ++_idx < _count;
            }

            /// <summary>
            /// 释放枚举器，解锁过滤器
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _filter.Unlock();
            }
        }

        /// <summary>
        /// 延迟操作结构体，用于存储延迟的实体添加/移除操作
        /// </summary>
        struct DelayedOp
        {
            /// <summary>
            /// 操作类型：true表示添加，false表示移除
            /// </summary>
            public bool Added;

            /// <summary>
            /// 实体ID
            /// </summary>
            public int Entity;
        }
    }
}