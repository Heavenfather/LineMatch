using System;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 组件池接口，定义组件池的基本操作
    /// </summary>
    public interface IEcsPool
    {
        /// <summary>
        /// 调整组件池的容量
        /// </summary>
        /// <param name="capacity">新的容量大小</param>
        void Resize(int capacity);

        /// <summary>
        /// 检查实体是否拥有该组件
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <returns>如果实体拥有组件返回true，否则false</returns>
        bool Has(int entity);

        /// <summary>
        /// 从实体中删除组件
        /// </summary>
        /// <param name="entity">实体ID</param>
        void Del(int entity);

        /// <summary>
        /// 向实体添加原始组件数据
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <param name="dataRaw">组件数据对象</param>
        void AddRaw(int entity, object dataRaw);

        /// <summary>
        /// 获取实体的原始组件数据
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <returns>组件数据对象</returns>
        object GetRaw(int entity);

        /// <summary>
        /// 设置实体的原始组件数据
        /// </summary>
        /// <param name="entity">实体ID</param>
        /// <param name="dataRaw">组件数据对象</param>
        void SetRaw(int entity, object dataRaw);

        /// <summary>
        /// 获取组件类型的ID
        /// </summary>
        /// <returns>组件类型ID</returns>
        int GetId();

        /// <summary>
        /// 获取组件类型
        /// </summary>
        /// <returns>组件类型</returns>
        Type GetComponentType();

        /// <summary>
        /// 将组件从源实体复制到目标实体
        /// </summary>
        /// <param name="srcEntity">源实体ID</param>
        /// <param name="dstEntity">目标实体ID</param>
        void Copy(int srcEntity, int dstEntity);
    }

    /// <summary>
    /// 组件自动重置接口，用于组件被回收时自动重置状态
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    public interface IEcsAutoReset<T> where T : struct
    {
        /// <summary>
        /// 自动重置组件状态
        /// </summary>
        /// <param name="c">要重置的组件引用</param>
        void AutoReset(ref T c);
    }
}