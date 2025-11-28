using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 元素构建器接口：负责为特定类型的元素添加额外的组件
    /// 消除元素组件膨胀的问题
    /// </summary>
    public interface IElementBuilder
    {
        /// <summary>
        /// 该构建器处理的元素类型
        /// </summary>
        ElementType TargetType { get; }

        /// <summary>
        /// 执行构建
        /// </summary>
        void Build(GameStateContext context, int entity, in ElementMap config);

        /// <summary>
        /// 当元素被爆到时，它能否被选中做销毁操作
        /// </summary>
        /// <param name="world"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool IsElementCanSelected(EcsWorld world, int entity);
    }
}