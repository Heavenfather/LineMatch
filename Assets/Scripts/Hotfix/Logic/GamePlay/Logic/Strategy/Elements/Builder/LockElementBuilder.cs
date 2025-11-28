using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    public class LockElementBuilder : IElementBuilder
    {
        public ElementType TargetType => ElementType.Lock;

        public void Build(GameStateContext context, int entity, in ElementMap config)
        {
            var world = context.World;
            var pool = world.GetPool<LockComponent>();
            pool.Add(entity);
            
            // 有锁住元素的标记，这个实体就不能移动 也不能被匹配
            ref var eleComp = ref world.GetPool<ElementComponent>().Get(entity);
            eleComp.IsMovable = false;
            
            // 根据不同的nameFlag，添加更多不同的组件  在数据层面就把这个组件组织好
            if (config.nameFlag == "vine")
            {
                world.GetPool<VineComponent>().Add(entity);
            }
            else if (config.nameFlag == "snow")
            {
                world.GetPool<IceComponent>().Add(entity);
            }
        }

        public bool IsElementCanSelected(EcsWorld world, int entity)
        {
            return true;
        }
    }
}