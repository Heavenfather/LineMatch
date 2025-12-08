using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// Block障碍物元素构建器
    /// </summary>
    public class BlockElementBuilder : IElementBuilder
    {
        public ElementType TargetType => ElementType.Block;

        public void Build(GameStateContext context, int entity, in ElementMap config, ElementBuildSource source)
        {
            var world = context.World;
            world.GetPool<BlockElementComponent>().Add(entity);

            // 检查是否有多层形态
            var transitionRule = MatchBoot.Container.Resolve<IElementTransitionRuleService>();
            var matchService = context.ServiceFactory.GetService(context.CurrentMatchType);
            
            if (transitionRule.TryTransitionToNextElement(config.Id, matchService, out var nextConfigId))
            {
                // 有多层形态，添加MultiLayerComponent
                // canBeDirectlySelected: true - Block可以被直接选中消除
                MultiLayerElementBuilderHelper.AddMultiLayerComponent(context, entity, config);
            }
        }

        public bool IsElementCanSelectDelete(EcsWorld world, int entity)
        {
            return true;
        }
    }
}