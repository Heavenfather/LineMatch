using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Hotfix.Logic.GamePlay
{
    public sealed class EcsWorld
    {
        short[] _entities;
        int _entitiesItemSize;
        int _entitiesCount;
        int[] _recycledEntities;
        int _recycledEntitiesCount;
        IEcsPool[] _pools;
        short _poolsCount;
        readonly int _poolDenseSize;
        readonly int _poolRecycledSize;
        readonly Dictionary<Type, IEcsPool> _poolHashes;
        readonly Dictionary<int, EcsFilter> _hashedFilters;
        readonly List<EcsFilter> _allFilters;
        List<EcsFilter>[] _filtersByIncludedComponents;
        List<EcsFilter>[] _filtersByExcludedComponents;
        EcsMask[] _masks;
        int _masksCount;
        bool _destroyed;

        public EcsWorld(in EcsConfig cfg = default)
        {
            var capacity = cfg.Entities > 0 ? cfg.Entities : EcsConfig.EntitiesDefault;
            // 计算每个实体在数组中的大小：组件列表起始位置 + 配置的组件数量
            _entitiesItemSize = RawEntityOffsets.Components + (cfg.EntityComponentsSize > 0
                ? cfg.EntityComponentsSize
                : EcsConfig.EntityComponentsSizeDefault);
            _entities = new short[capacity * _entitiesItemSize];
            capacity = cfg.RecycledEntities > 0 ? cfg.RecycledEntities : EcsConfig.RecycledEntitiesDefault;
            _recycledEntities = new int[capacity];
            _entitiesCount = 0;
            _recycledEntitiesCount = 0;
            // pools.
            capacity = cfg.Pools > 0 ? cfg.Pools : EcsConfig.PoolsDefault;
            _pools = new IEcsPool[capacity];
            _poolHashes = new Dictionary<Type, IEcsPool>(capacity);
            _filtersByIncludedComponents = new List<EcsFilter>[capacity];
            _filtersByExcludedComponents = new List<EcsFilter>[capacity];
            _poolDenseSize = cfg.PoolDenseSize > 0 ? cfg.PoolDenseSize : EcsConfig.PoolDenseSizeDefault;
            _poolRecycledSize = cfg.PoolRecycledSize > 0 ? cfg.PoolRecycledSize : EcsConfig.PoolRecycledSizeDefault;
            _poolsCount = 0;
            // filters.
            capacity = cfg.Filters > 0 ? cfg.Filters : EcsConfig.FiltersDefault;
            _hashedFilters = new Dictionary<int, EcsFilter>(capacity);
            _allFilters = new List<EcsFilter>(capacity);
            // masks.
            _masks = new EcsMask[64];
            _masksCount = 0;
            _destroyed = false;
        }

        /// <summary>
        /// 获取或创建指定类型的组件池
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>对应类型的组件池</returns>
        public EcsPool<T> GetPool<T>() where T : struct
        {
            var poolType = typeof(T);
            if (_poolHashes.TryGetValue(poolType, out var rawPool))
            {
                return (EcsPool<T>)rawPool;
            }

            if (_poolsCount == short.MaxValue)
            {
                throw new Exception("No more room for new component into this world.");
            }

            var pool = new EcsPool<T>(this, _poolsCount, _poolDenseSize, GetWorldSize(), _poolRecycledSize);
            _poolHashes[poolType] = pool;
            if (_poolsCount == _pools.Length)
            {
                var newSize = _poolsCount << 1;
                Array.Resize(ref _pools, newSize);
                Array.Resize(ref _filtersByIncludedComponents, newSize);
                Array.Resize(ref _filtersByExcludedComponents, newSize);
            }

            _pools[_poolsCount++] = pool;
            return pool;
        }

        /// <summary>
        /// 根据类型ID获取组件池
        /// </summary>
        /// <param name="typeId">组件类型ID</param>
        /// <returns>对应的组件池，如果不存在则返回null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEcsPool GetPoolById(int typeId)
        {
            return typeId >= 0 && typeId < _poolsCount ? _pools[typeId] : null;
        }

        /// <summary>
        /// 根据类型获取组件池
        /// </summary>
        /// <param name="type">组件类型</param>
        /// <returns>对应的组件池，如果不存在则返回null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEcsPool GetPoolByType(Type type)
        {
            return _poolHashes.TryGetValue(type, out var pool) ? pool : null;
        }

        /// <summary>
        /// 获取指定实体的所有组件对象
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <param name="list">组件对象数组引用，如果为null或容量不足会自动创建</param>
        /// <returns>组件的数量</returns>
        public int GetComponents(int entity, ref object[] list)
        {
            var entityOffset = GetRawEntityOffset(entity);
            var itemsCount = _entities[entityOffset + RawEntityOffsets.ComponentsCount];
            if (itemsCount == 0)
            {
                return 0;
            }

            if (list == null || list.Length < itemsCount)
            {
                list = new object[_pools.Length];
            }

            var dataOffset = entityOffset + RawEntityOffsets.Components;
            for (var i = 0; i < itemsCount; i++)
            {
                list[i] = _pools[_entities[dataOffset + i]].GetRaw(entity);
            }

            return itemsCount;
        }

        /// <summary>
        /// 获取指定实体的所有组件类型
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <param name="list">组件类型数组引用，如果为null或容量不足会自动创建</param>
        /// <returns>组件类型的数量</returns>
        public int GetComponentTypes(int entity, ref Type[] list)
        {
            var entityOffset = GetRawEntityOffset(entity);
            var itemsCount = _entities[entityOffset + RawEntityOffsets.ComponentsCount];
            if (itemsCount == 0)
            {
                return 0;
            }

            if (list == null || list.Length < itemsCount)
            {
                list = new Type[_pools.Length];
            }

            var dataOffset = entityOffset + RawEntityOffsets.Components;
            for (var i = 0; i < itemsCount; i++)
            {
                list[i] = _pools[_entities[dataOffset + i]].GetComponentType();
            }

            return itemsCount;
        }

        /// <summary>
        /// 将源实体的所有组件复制到目标实体
        /// </summary>
        /// <param name="srcEntity">源实体ID</param>
        /// <param name="dstEntity">目标实体ID</param>
        public void CopyEntity(int srcEntity, int dstEntity)
        {
            var entityOffset = GetRawEntityOffset(srcEntity);
            var itemsCount = _entities[entityOffset + RawEntityOffsets.ComponentsCount];
            if (itemsCount > 0)
            {
                var dataOffset = entityOffset + RawEntityOffsets.Components;
                for (var i = 0; i < itemsCount; i++)
                {
                    _pools[_entities[dataOffset + i]].Copy(srcEntity, dstEntity);
                }
            }
        }

        /// <summary>
        /// 内部方法：检查实体是否存活
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <returns>实体是否存活</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsEntityAliveInternal(int entity)
        {
            return entity >= 0 && entity < _entitiesCount &&
                   _entities[GetRawEntityOffset(entity) + RawEntityOffsets.Gen] > 0;
        }

        /// <summary>
        /// 内部方法：向原始实体数据添加组件引用
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <param name="poolId">组件池ID</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddComponentToRawEntityInternal(int entity, short poolId)
        {
            var offset = GetRawEntityOffset(entity);
            var dataCount = _entities[offset + RawEntityOffsets.ComponentsCount];
            if (dataCount + RawEntityOffsets.Components == _entitiesItemSize)
            {
                // resize entities.
                ExtendEntitiesCache();
                offset = GetRawEntityOffset(entity);
            }

            _entities[offset + RawEntityOffsets.ComponentsCount]++;
            _entities[offset + RawEntityOffsets.Components + dataCount] = poolId;
        }

        /// <summary>
        /// 内部方法：从原始实体数据移除组件引用
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <param name="poolId">组件池ID</param>
        /// <returns>移除后实体的组件数量</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal short RemoveComponentFromRawEntityInternal(int entity, short poolId)
        {
            var offset = GetRawEntityOffset(entity);
            var dataCount = _entities[offset + RawEntityOffsets.ComponentsCount];
            dataCount--;
            _entities[offset + RawEntityOffsets.ComponentsCount] = dataCount;
            var dataOffset = offset + RawEntityOffsets.Components;
            for (var i = 0; i <= dataCount; i++)
            {
                if (_entities[dataOffset + i] == poolId)
                {
                    if (i < dataCount)
                    {
                        // fill gap with last item.
                        _entities[dataOffset + i] = _entities[dataOffset + dataCount];
                    }

                    return dataCount;
                }
            }

            return 0;
        }

        /// <summary>
        /// 销毁世界，删除所有实体和组件池
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Destroy()
        {
            _destroyed = true;
            for (var i = _entitiesCount - 1; i >= 0; i--)
            {
                // 使用 RawEntityOffsets.ComponentsCount 检查实体是否有组件，有则删除
                if (_entities[GetRawEntityOffset(i) + RawEntityOffsets.ComponentsCount] > 0)
                {
                    DelEntity(i);
                }
            }

            _pools = Array.Empty<IEcsPool>();
            _poolHashes.Clear();
            _hashedFilters.Clear();
            _allFilters.Clear();
            _filtersByIncludedComponents = Array.Empty<List<EcsFilter>>();
            _filtersByExcludedComponents = Array.Empty<List<EcsFilter>>();
        }

        /// <summary>
        /// 获取实体在数组中的偏移量
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRawEntityOffset(int entity)
        {
            return entity * _entitiesItemSize;
        }

        /// <summary>
        /// 世界爆炸没有
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAlive()
        {
            return !_destroyed;
        }

        /// <summary>
        /// 创建新实体或从回收池获取实体
        /// </summary>
        /// <returns></returns>
        public int NewEntity()
        {
            int entity;
            if (_recycledEntitiesCount > 0)
            {
                entity = _recycledEntities[--_recycledEntitiesCount];
                // 使用 RawEntityOffsets.Gen 将回收实体的代数取反，标记为重新激活
                _entities[GetRawEntityOffset(entity) + RawEntityOffsets.Gen] *= -1;
            }
            else
            {
                // new entity.
                if (_entitiesCount * _entitiesItemSize == _entities.Length)
                {
                    // resize entities and component pools.
                    var newSize = _entitiesCount << 1;
                    Array.Resize(ref _entities, newSize * _entitiesItemSize);
                    for (int i = 0, iMax = _poolsCount; i < iMax; i++)
                    {
                        _pools[i].Resize(newSize);
                    }

                    for (int i = 0, iMax = _allFilters.Count; i < iMax; i++)
                    {
                        _allFilters[i].ResizeSparseIndex(newSize);
                    }
                }

                entity = _entitiesCount++;
                // 使用 RawEntityOffsets.Gen 设置新实体的代数为1
                _entities[GetRawEntityOffset(entity) + RawEntityOffsets.Gen] = 1;
            }

            return entity;
        }

        /// <summary>
        /// 获取指定实体的组件数量
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <returns>实体拥有的组件数量</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetComponentsCount(int entity)
        {
            return _entities[GetRawEntityOffset(entity) + RawEntityOffsets.ComponentsCount];
        }

        /// <summary>
        /// 删除指定实体，将其标记为已销毁
        /// </summary>
        /// <param name="entity">要删除的实体ID</param>
        /// <exception cref="Exception"></exception>
        public void DelEntity(int entity)
        {
            if (entity < 0 || entity >= _entitiesCount)
            {
                throw new Exception("Cant touch destroyed entity.");
            }

            var entityOffset = GetRawEntityOffset(entity);
            var componentsCount = _entities[entityOffset + RawEntityOffsets.ComponentsCount];
            ref var entityGen = ref _entities[entityOffset + RawEntityOffsets.Gen];
            if (entityGen < 0)
            {
                return;
            }

            if (componentsCount > 0)
            {
                for (var i = entityOffset + RawEntityOffsets.Components + componentsCount - 1;
                     i >= entityOffset + RawEntityOffsets.Components;
                     i--)
                {
                    _pools[_entities[i]].Del(entity);
                }
            }
            else
            {
                entityGen = (short)(entityGen == short.MaxValue ? -1 : -(entityGen + 1));
                if (_recycledEntitiesCount == _recycledEntities.Length)
                {
                    Array.Resize(ref _recycledEntities, _recycledEntitiesCount << 1);
                }

                _recycledEntities[_recycledEntitiesCount++] = entity;
            }
        }

        /// <summary>
        /// 获取所有活跃实体的ID
        /// </summary>
        /// <param name="entities">实体ID数组引用，如果为null或容量不足会自动创建</param>
        /// <returns>活跃实体的数量</returns>
        public int GetAllEntities(ref int[] entities)
        {
            var count = _entitiesCount - _recycledEntitiesCount;
            if (entities == null || entities.Length < count)
            {
                entities = new int[count];
            }

            var id = 0;
            var offset = 0;
            for (int i = 0, iMax = _entitiesCount; i < iMax; i++, offset += _entitiesItemSize)
            {
                if (_entities[offset + RawEntityOffsets.Gen] > 0 &&
                    _entities[offset + RawEntityOffsets.ComponentsCount] >= 0)
                {
                    entities[id++] = i;
                }
            }

            return count;
        }

        /// <summary>
        /// 获取所有组件池
        /// </summary>
        /// <param name="pools">组件池数组引用，如果为null或容量不足会自动创建</param>
        /// <returns>组件池的数量</returns>
        public int GetAllPools(ref IEcsPool[] pools)
        {
            var count = _poolsCount;
            if (pools == null || pools.Length < count)
            {
                pools = new IEcsPool[count];
            }

            Array.Copy(_pools, 0, pools, 0, _poolsCount);
            return _poolsCount;
        }

        /// <summary>
        /// 获取指定实体的生成版本号
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <returns>实体的生成版本号</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetEntityGen(int entity)
        {
            return _entities[GetRawEntityOffset(entity) + RawEntityOffsets.Gen];
        }

        /// <summary>
        /// 获取原始实体项的大小（以短整数为单位）
        /// </summary>
        /// <returns>每个实体项的大小</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRawEntityItemSize()
        {
            return _entitiesItemSize;
        }

        /// <summary>
        /// 获取已使用的实体总数（包括已回收的实体）
        /// </summary>
        /// <returns>已使用的实体数量</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetUsedEntitiesCount()
        {
            return _entitiesCount;
        }

        /// <summary>
        /// 创建包含指定组件的筛选器掩码
        /// </summary>
        /// <typeparam name="T">包含的组件类型</typeparam>
        /// <returns>筛选器掩码</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EcsMask Filter<T>() where T : struct
        {
            var mask = _masksCount > 0 ? _masks[--_masksCount] : new EcsMask(this);
            return mask.Include<T>();
        }

        /// <summary>
        /// 获取世界的总容量（可容纳的实体数量）
        /// </summary>
        /// <returns>世界的总容量</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWorldSize()
        {
            return _entities.Length / _entitiesItemSize;
        }

        /// <summary>
        /// 获取组件池的数量
        /// </summary>
        /// <returns>组件池总数</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPoolsCount()
        {
            return _poolsCount;
        }

        /// <summary>
        /// 获取活跃实体数量（不包括已回收的实体）
        /// </summary>
        /// <returns>活跃实体数量</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEntitiesCount()
        {
            return _entitiesCount - _recycledEntitiesCount;
        }

        /// <summary>
        /// 获取原始实体数组
        /// </summary>
        /// <returns>原始实体数据数组</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short[] GetRawEntities()
        {
            return _entities;
        }

        /// <summary>
        /// 内部方法：获取或创建筛选器
        /// </summary>
        /// <param name="mask">筛选器掩码</param>
        /// <param name="capacity">初始容量</param>
        /// <returns>筛选器实例和是否为新创建的标志</returns>
        (EcsFilter, bool) GetFilterInternal(EcsMask mask, int capacity = 512)
        {
            var hash = mask.Hash;
            var exists = _hashedFilters.TryGetValue(hash, out var filter);
            if (exists)
            {
                return (filter, false);
            }

            filter = new EcsFilter(this, mask, capacity, GetWorldSize());
            _hashedFilters[hash] = filter;
            _allFilters.Add(filter);
            // add to component dictionaries for fast compatibility scan.
            for (int i = 0, iMax = mask.includeCount; i < iMax; i++)
            {
                var list = _filtersByIncludedComponents[mask.include[i]];
                if (list == null)
                {
                    list = new List<EcsFilter>(8);
                    _filtersByIncludedComponents[mask.include[i]] = list;
                }

                list.Add(filter);
            }

            for (int i = 0, iMax = mask.excludeCount; i < iMax; i++)
            {
                var list = _filtersByExcludedComponents[mask.exclude[i]];
                if (list == null)
                {
                    list = new List<EcsFilter>(8);
                    _filtersByExcludedComponents[mask.exclude[i]] = list;
                }

                list.Add(filter);
            }

            for (int i = 0, iMax = _entitiesCount; i < iMax; i++)
            {
                if (_entities[GetRawEntityOffset(i) + RawEntityOffsets.ComponentsCount] > 0 &&
                    IsMaskCompatible(mask, i))
                {
                    filter.AddEntity(i);
                }
            }

            return (filter, true);
        }

        /// <summary>
        /// 内部方法：处理实体组件变更
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <param name="componentType">组件类型ID</param>
        /// <param name="added">是否为添加操作</param>
        public void OnEntityChangeInternal(int entity, short componentType, bool added)
        {
            var includeList = _filtersByIncludedComponents[componentType];
            var excludeList = _filtersByExcludedComponents[componentType];
            if (added)
            {
                // add component.
                if (includeList != null)
                {
                    foreach (var filter in includeList)
                    {
                        if (IsMaskCompatible(filter.GetMask(), entity))
                        {
                            if (filter.SparseEntities[entity] > 0)
                            {
                                throw new Exception("Entity already in filter.");
                            }

                            filter.AddEntity(entity);
                        }
                    }
                }

                if (excludeList != null)
                {
                    foreach (var filter in excludeList)
                    {
                        if (IsMaskCompatibleWithout(filter.GetMask(), entity, componentType))
                        {
                            if (filter.SparseEntities[entity] == 0)
                            {
                                throw new Exception("Entity not in filter.");
                            }

                            filter.RemoveEntity(entity);
                        }
                    }
                }
            }
            else
            {
                // remove component.
                if (includeList != null)
                {
                    foreach (var filter in includeList)
                    {
                        if (IsMaskCompatible(filter.GetMask(), entity))
                        {
                            if (filter.SparseEntities[entity] == 0)
                            {
                                throw new Exception("Entity not in filter.");
                            }

                            filter.RemoveEntity(entity);
                        }
                    }
                }

                if (excludeList != null)
                {
                    foreach (var filter in excludeList)
                    {
                        if (IsMaskCompatibleWithout(filter.GetMask(), entity, componentType))
                        {
                            if (filter.SparseEntities[entity] > 0)
                            {
                                throw new Exception("Entity already in filter.");
                            }

                            filter.AddEntity(entity);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 扩展实体缓存容量
        /// </summary>
        void ExtendEntitiesCache()
        {
            var newItemSize = RawEntityOffsets.Components + ((_entitiesItemSize - RawEntityOffsets.Components) << 1);
            var newEntities = new short[GetWorldSize() * newItemSize];
            var oldOffset = 0;
            var newOffset = 0;
            for (int i = 0, iMax = _entitiesCount; i < iMax; i++)
            {
                // amount of entity data (components + header).
                var entityDataLen = _entities[oldOffset + RawEntityOffsets.ComponentsCount] +
                                    RawEntityOffsets.Components;
                for (var j = 0; j < entityDataLen; j++)
                {
                    newEntities[newOffset + j] = _entities[oldOffset + j];
                }

                oldOffset += _entitiesItemSize;
                newOffset += newItemSize;
            }

            _entitiesItemSize = newItemSize;
            _entities = newEntities;
        }

        /// <summary>
        /// 检查实体是否符合筛选器掩码
        /// </summary>
        /// <param name="filterMask">筛选器掩码</param>
        /// <param name="entity">实体ID</param>
        /// <returns>实体是否符合掩码要求</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsMaskCompatible(EcsMask filterMask, int entity)
        {
            for (int i = 0, iMax = filterMask.includeCount; i < iMax; i++)
            {
                if (!_pools[filterMask.include[i]].Has(entity))
                {
                    return false;
                }
            }

            for (int i = 0, iMax = filterMask.excludeCount; i < iMax; i++)
            {
                if (_pools[filterMask.exclude[i]].Has(entity))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查实体是否符合排除指定组件的筛选器掩码
        /// </summary>
        /// <param name="filterMask">筛选器掩码</param>
        /// <param name="entity">实体ID</param>
        /// <param name="componentId">要排除的组件ID</param>
        /// <returns>实体是否符合排除指定组件后的掩码要求</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsMaskCompatibleWithout(EcsMask filterMask, int entity, int componentId)
        {
            for (int i = 0, iMax = filterMask.includeCount; i < iMax; i++)
            {
                var typeId = filterMask.include[i];
                if (typeId == componentId || !_pools[typeId].Has(entity))
                {
                    return false;
                }
            }

            for (int i = 0, iMax = filterMask.excludeCount; i < iMax; i++)
            {
                var typeId = filterMask.exclude[i];
                if (typeId != componentId && _pools[typeId].Has(entity))
                {
                    return false;
                }
            }

            return true;
        }


        public class EcsMask
        {
            readonly EcsWorld _world;
            internal int[] include;
            internal int[] exclude;
            internal int includeCount;
            internal int excludeCount;
            internal int Hash;

            internal EcsMask(EcsWorld world)
            {
                _world = world;
                include = new int[8];
                exclude = new int[2];
                Reset();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void Reset()
            {
                includeCount = 0;
                excludeCount = 0;
                Hash = 0;
            }

            /// <summary>
            /// 包含指定组件类型在筛选器中
            /// </summary>
            /// <typeparam name="T">要包含的组件类型</typeparam>
            /// <returns>当前掩码实例用于链式调用</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EcsMask Include<T>() where T : struct
            {
                var poolId = _world.GetPool<T>().GetId();
                if (includeCount == include.Length)
                {
                    Array.Resize(ref include, includeCount << 1);
                }

                include[includeCount++] = poolId;
                return this;
            }

            /// <summary>
            /// 排除指定组件类型在筛选器中
            /// </summary>
            /// <typeparam name="T">要排除的组件类型</typeparam>
            /// <returns>当前掩码实例用于链式调用</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EcsMask Exclude<T>() where T : struct
            {
                var poolId = _world.GetPool<T>().GetId();
                if (excludeCount == exclude.Length)
                {
                    Array.Resize(ref exclude, excludeCount << 1);
                }

                exclude[excludeCount++] = poolId;
                return this;
            }

            /// <summary>
            /// 结束掩码定义并创建或获取对应的筛选器
            /// </summary>
            /// <param name="capacity">筛选器初始容量</param>
            /// <returns>创建的筛选器实例</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EcsFilter End(int capacity = 512)
            {
                Array.Sort(include, 0, includeCount);
                Array.Sort(exclude, 0, excludeCount);
                // calculate hash.
                unchecked
                {
                    Hash = includeCount + excludeCount;
                    for (int i = 0, iMax = includeCount; i < iMax; i++)
                    {
                        Hash = Hash * 314159 + include[i];
                    }

                    for (int i = 0, iMax = excludeCount; i < iMax; i++)
                    {
                        Hash = Hash * 314159 - exclude[i];
                    }
                }

                var (filter, isNew) = _world.GetFilterInternal(this, capacity);
                if (!isNew)
                {
                    Recycle();
                }

                return filter;
            }

            /// <summary>
            /// 回收掩码实例到世界池中以便重用
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void Recycle()
            {
                Reset();
                if (_world._masksCount == _world._masks.Length)
                {
                    Array.Resize(ref _world._masks, _world._masksCount << 1);
                }

                _world._masks[_world._masksCount++] = this;
            }
        }
    }
}