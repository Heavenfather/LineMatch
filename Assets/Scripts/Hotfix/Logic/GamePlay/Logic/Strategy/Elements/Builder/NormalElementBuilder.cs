using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    public class NormalElementBuilder : IElementBuilder
    {
        public ElementType TargetType => ElementType.Normal;
        
        public void Build(GameStateContext context, int entity, in ElementMap config,ElementBuildSource source)
        {
            var world = context.World;
            // 添加普通元素特性
            var pool = world.GetPool<NormalElementComponent>();
            ref var normalElementComponent = ref pool.Add(entity);
            normalElementComponent.IsColorDirty = true;
            normalElementComponent.IsFlashIconDirty = false;
            normalElementComponent.ScaleState = ElementScaleState.None;
            normalElementComponent.FlashIconAniType = NormalFlashIconAniType.None;
            normalElementComponent.ElementColor = MatchElementUtil.GetElementColor(config.Id);
            // 闪烁表现默认值
            normalElementComponent.FlashDuration = 0.8f;
            normalElementComponent.FlashStartScale = 2.0f;
            normalElementComponent.FlashEndScale = 1.5f;
            
            // 普通棋子必定是可连的
            ref var eleComponent = ref world.GetPool<ElementComponent>().Get(entity);
            eleComponent.IsMatchable = true;
        }

        public bool IsElementCanSelectDelete(EcsWorld world, int entity)
        {
            return true;
        }
    }
}