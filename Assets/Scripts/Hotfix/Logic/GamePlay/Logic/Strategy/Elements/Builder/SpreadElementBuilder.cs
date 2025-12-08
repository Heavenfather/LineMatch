using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    public class SpreadElementBuilder : IElementBuilder
    {
        public ElementType TargetType => ElementType.SpreadWater;

        public void Build(GameStateContext context, int entity, in ElementMap config,ElementBuildSource source)
        {
            var world = context.World;
            if(config.elementType == ElementType.SpreadWater)
            {
                ref var waterCom = ref world.GetPool<SpreadWaterComponent>().Add(entity);
                waterCom.WaterConfigId = config.Id;
                waterCom.IsVisualInitialized = false;
                waterCom.IsInitHideIcon = source != ElementBuildSource.Config;
            }
        }

        public bool IsElementCanSelectDelete(EcsWorld world, int entity)
        {
            return false;
        }
    }
}