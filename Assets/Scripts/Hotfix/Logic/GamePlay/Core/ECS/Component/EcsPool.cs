using System;
using System.Runtime.CompilerServices;

namespace Hotfix.Logic.GamePlay
{
    public sealed class EcsPool<T> : IEcsPool where T : struct
    {
        /// <summary>
        /// 自动重置处理器委托，用于组件重置时的回调
        /// </summary>
        /// <param name="component">要重置的组件引用</param>
        delegate void AutoResetHandler(ref T component);

        /// <summary>
        /// 组件池类，用于管理实体组件的存储和操作
        /// </summary>
        readonly Type _type;
        
        /// <summary>
        /// 组件池所属的世界
        /// </summary>
        readonly EcsWorld _world;
        /// <summary>
        /// 组件类型ID
        /// </summary>
        readonly short _id;
        /// <summary>
        /// 自动重置处理器，用于组件重置时的回调
        /// </summary>
        readonly AutoResetHandler _autoResetHandler;
        /// <summary>
        /// 实体密集数组，存储符合过滤条件的实体ID
        /// </summary>
        T[] _denseItems;
        /// <summary>
        /// 实体稀疏数组，用于快速查找实体ID对应的索引
        /// </summary>
        int[] _sparseItems;
        /// <summary>
        /// 实体密集数组当前数量
        /// </summary>
        int _denseItemsCount;
        /// <summary>
        /// 实体回收数组，用于存储已删除的实体ID
        /// </summary>
        int[] _recycledItems;
        /// <summary>
        /// 实体回收数组当前数量
        /// </summary>
        int _recycledItemsCount;
        /// <summary>
        /// 伪实例，用于创建自动重置处理器
        /// </summary>
        T _fakeInstance;

        public EcsPool(EcsWorld world, short id, int denseCapacity, int sparseCapacity, int recycledCapacity)
        {
            _type = typeof(T);
            _world = world;
            _id = id;
            _denseItems = new T[denseCapacity + 1];
            _sparseItems = new int[sparseCapacity];
            _denseItemsCount = 1;
            _recycledItems = new int[recycledCapacity];
            _recycledItemsCount = 0;

            var isAutoReset = typeof(IEcsAutoReset<T>).IsAssignableFrom(_type);
            if (isAutoReset)
            {
                var autoResetMethod = typeof(T).GetMethod(nameof(IEcsAutoReset<T>.AutoReset));
                if (autoResetMethod == null)
                {
                    throw new Exception(
                        $"IEcsAutoReset<{typeof(T).Name}> explicit implementation not supported, use implicit instead.");
                }

                _autoResetHandler = (AutoResetHandler)Delegate.CreateDelegate(
                    typeof(AutoResetHandler),
                    _fakeInstance,
                    autoResetMethod);
            }
        }
        
        /// <summary>
        /// 获取组件池所属的世界
        /// </summary>
        /// <returns>所属的EcsWorld实例</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EcsWorld GetWorld()
        {
            return _world;
        }

        /// <summary>
        /// 获取组件类型的ID
        /// </summary>
        /// <returns>组件类型ID</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetId()
        {
            return _id;
        }

        /// <summary>
        /// 获取组件类型
        /// </summary>
        /// <returns>组件类型</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type GetComponentType()
        {
            return _type;
        }

        /// <summary>
        /// 调整稀疏数组的容量
        /// </summary>
        /// <param name="capacity">新的容量大小</param>
        void IEcsPool.Resize(int capacity)
        {
            Array.Resize(ref _sparseItems, capacity);
        }

        /// <summary>
        /// 获取实体的原始组件数据
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <returns>组件数据对象</returns>
        object IEcsPool.GetRaw(int entity)
        {
            return Get(entity);
        }

        /// <summary>
        /// 设置实体的原始组件数据
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <param name="dataRaw">组件数据对象</param>
        void IEcsPool.SetRaw(int entity, object dataRaw)
        {
#if DEBUG
            if (dataRaw == null || dataRaw.GetType() != _type)
            {
                throw new Exception($"Invalid component data, valid \"{typeof(T).Name}\" instance required.");
            }

            if (_sparseItems[entity] <= 0)
            {
                throw new Exception($"Component \"{typeof(T).Name}\" not attached to entity.");
            }
#endif
            _denseItems[_sparseItems[entity]] = (T)dataRaw;
        }

        /// <summary>
        /// 向实体添加原始组件数据
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <param name="dataRaw">组件数据对象</param>
        void IEcsPool.AddRaw(int entity, object dataRaw)
        {
#if DEBUG
            if (dataRaw == null || dataRaw.GetType() != _type)
            {
                throw new Exception($"Invalid component data, valid \"{typeof(T).Name}\" instance required.");
            }
#endif
            ref var data = ref Add(entity);
            data = (T)dataRaw;
        }

        /// <summary>
        /// 获取密集数组的原始引用
        /// </summary>
        /// <returns>密集数组</returns>
        public T[] GetRawDenseItems()
        {
            return _denseItems;
        }

        /// <summary>
        /// 获取密集数组当前数量的引用
        /// </summary>
        /// <returns>密集数组数量的引用</returns>
        public ref int GetRawDenseItemsCount()
        {
            return ref _denseItemsCount;
        }

        /// <summary>
        /// 获取稀疏数组的原始引用
        /// </summary>
        /// <returns>稀疏数组</returns>
        public int[] GetRawSparseItems()
        {
            return _sparseItems;
        }

        /// <summary>
        /// 获取回收数组的原始引用
        /// </summary>
        /// <returns>回收数组</returns>
        public int[] GetRawRecycledItems()
        {
            return _recycledItems;
        }

        /// <summary>
        /// 获取回收数组当前数量的引用
        /// </summary>
        /// <returns>回收数组数量的引用</returns>
        public ref int GetRawRecycledItemsCount()
        {
            return ref _recycledItemsCount;
        }

        /// <summary>
        /// 向实体添加组件
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <returns>新添加组件的引用</returns>
        public ref T Add(int entity)
        {
#if DEBUG
            if (!_world.IsEntityAliveInternal(entity))
            {
                throw new Exception("Cant touch destroyed entity.");
            }

            if (_sparseItems[entity] > 0)
            {
                throw new Exception($"Component \"{typeof(T).Name}\" already attached to entity.");
            }
#endif
            int idx;
            if (_recycledItemsCount > 0)
            {
                idx = _recycledItems[--_recycledItemsCount];
            }
            else
            {
                idx = _denseItemsCount;
                if (_denseItemsCount == _denseItems.Length)
                {
                    Array.Resize(ref _denseItems, _denseItemsCount << 1);
                }

                _denseItemsCount++;
                _autoResetHandler?.Invoke(ref _denseItems[idx]);
            }

            _sparseItems[entity] = idx;
            _world.OnEntityChangeInternal(entity, _id, true);
            _world.AddComponentToRawEntityInternal(entity, _id);
            return ref _denseItems[idx];
        }

        /// <summary>
        /// 获取实体组件的引用
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <returns>组件引用</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(int entity)
        {
#if DEBUG
            if (!_world.IsEntityAliveInternal(entity))
            {
                throw new Exception("Cant touch destroyed entity.");
            }

            if (_sparseItems[entity] == 0)
            {
                throw new Exception($"Cant get \"{typeof(T).Name}\" component - not attached.");
            }
#endif
            return ref _denseItems[_sparseItems[entity]];
        }

        /// <summary>
        /// 检查实体是否拥有该组件
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <returns>如果实体拥有组件返回true，否则false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(int entity)
        {
#if DEBUG
            if (!_world.IsEntityAliveInternal(entity))
            {
                throw new Exception("Cant touch destroyed entity.");
            }
#endif
            return _sparseItems[entity] > 0;
        }

        /// <summary>
        /// 从实体中删除组件
        /// </summary>
        /// <param name="entity">实体ID</param>
        public void Del(int entity)
        {
#if DEBUG
            if (!_world.IsEntityAliveInternal(entity))
            {
                throw new Exception("Cant touch destroyed entity.");
            }
#endif
            ref var sparseData = ref _sparseItems[entity];
            if (sparseData > 0)
            {
                _world.OnEntityChangeInternal(entity, _id, false);
                if (_recycledItemsCount == _recycledItems.Length)
                {
                    Array.Resize(ref _recycledItems, _recycledItemsCount << 1);
                }

                _recycledItems[_recycledItemsCount++] = sparseData;
                if (_autoResetHandler != null)
                {
                    _autoResetHandler.Invoke(ref _denseItems[sparseData]);
                }
                else
                {
                    _denseItems[sparseData] = default;
                }

                sparseData = 0;
                var componentsCount = _world.RemoveComponentFromRawEntityInternal(entity, _id);
                if (componentsCount == 0)
                {
                    _world.DelEntity(entity);
                }
            }
        }

        /// <summary>
        /// 将组件从源实体复制到目标实体
        /// </summary>
        /// <param name="srcEntity">源实体ID</param>
        /// <param name="dstEntity">目标实体ID</param>
        public void Copy(int srcEntity, int dstEntity)
        {
#if DEBUG
            if (!_world.IsEntityAliveInternal(srcEntity))
            {
                throw new Exception("Cant touch destroyed src-entity.");
            }

            if (!_world.IsEntityAliveInternal(dstEntity))
            {
                throw new Exception("Cant touch destroyed dest-entity.");
            }
#endif
            if (Has(srcEntity))
            {
                ref var srcData = ref Get(srcEntity);
                if (!Has(dstEntity))
                {
                    Add(dstEntity);
                }

                ref var dstData = ref Get(dstEntity);
                dstData = srcData;
            }
        }
    }
}