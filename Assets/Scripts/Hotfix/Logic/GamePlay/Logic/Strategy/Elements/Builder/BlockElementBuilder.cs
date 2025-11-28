using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    public class BlockElementBuilder : IElementBuilder
    {
        public ElementType TargetType => ElementType.Block;

        public void Build(GameStateContext context, int entity, in ElementMap config)
        {
        }

        public bool IsElementCanSelected(EcsWorld world, int entity)
        {
            return true;
        }
    }
}