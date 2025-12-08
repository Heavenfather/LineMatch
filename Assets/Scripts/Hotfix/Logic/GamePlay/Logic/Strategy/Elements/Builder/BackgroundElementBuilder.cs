using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 背景类型元素构建，它是存在于棋子的最下方
    /// Normal棋子消除时，它也会跟着受影响
    /// </summary>
    public class BackgroundElementBuilder : IElementBuilder
    {
        public ElementType TargetType => ElementType.Background;

        public void Build(GameStateContext context, int entity, in ElementMap config, ElementBuildSource source)
        {
            // 使用通用的多层元素辅助类
            // canBeDirectlySelected: false - 背景元素不能被直接选中消除
            MultiLayerElementBuilderHelper.AddMultiLayerComponent(context, entity, config);
            
            // 保留BackgroundComponent用于标识（可选，用于特殊逻辑判断）
            var backgroundPool = context.World.GetPool<BackgroundComponent>();
            backgroundPool.Add(entity);
        }

        public bool IsElementCanSelectDelete(EcsWorld world, int entity)
        {
            // 背景元素不能被直接选中消除，但可以接收伤害
            return false;
        }
    }
}