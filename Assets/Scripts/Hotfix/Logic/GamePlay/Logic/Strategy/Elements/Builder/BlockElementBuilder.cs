using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    public class BlockElementBuilder : IElementBuilder
    {
        public ElementType TargetType => ElementType.Block;

        public void Build(GameStateContext context, int entity, in ElementMap config)
        {
            var world = context.World;
            world.GetPool<BlockElementComponent>().Add(entity);
        }

        public bool IsElementCanSelectDelete(EcsWorld world, int entity)
        {
            return true;
        }
    }
}