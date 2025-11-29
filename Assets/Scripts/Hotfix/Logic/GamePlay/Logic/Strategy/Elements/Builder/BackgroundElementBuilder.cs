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

        public void Build(GameStateContext context, int entity, in ElementMap config)
        {
            var com = context.World.GetPool<BackgroundComponent>();
            com.Add(entity);

        }

        public bool IsElementCanSelectDelete(EcsWorld world, int entity)
        {
            return false;
        }
    }
}