using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 蔓延火元素构建器
    /// </summary>
    public class SpreadFireElementBuilder : IElementBuilder
    {
        public ElementType TargetType => ElementType.SpreadFire;

        public void Build(GameStateContext context, int entity, in ElementMap config, ElementBuildSource source)
        {
            var world = context.World;

            // 1. 添加SpreadFireComponent
            var firePool = world.GetPool<SpreadFireComponent>();
            ref var fire = ref firePool.Add(entity);
            fire.FireConfigId = config.Id;
            
            world.GetPool<ElementCheckStableTag>().Add(entity);
        }

        public bool IsElementCanSelectDelete(EcsWorld world, int entity)
        {
            // 火能被直接选中消除
            return true;
        }
    }
}
