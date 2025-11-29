using GameConfig;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 功能性棋子的构建
    /// 功能棋子统一链接到这里处理,就不一个个创建构建器了
    /// </summary>
    public class FunctionalElementBuilder : IElementBuilder
    {
        public ElementType TargetType => ElementType.Bomb;
        
        public void Build(GameStateContext context, int entity, in ElementMap config)
        {
            var world = context.World;
            int priority = 0;
            int configId = config.Id;
            if(config.elementType == ElementType.Rocket || config.elementType == ElementType.RocketHorizontal)
            {
                priority = config.elementType == ElementType.RocketHorizontal ? 1 : 2;
                world.GetPool<RocketComponent>().Add(entity);
            }
            else if (config.elementType == ElementType.Bomb)
            {
                priority = 3;
                world.GetPool<BombComponent>().Add(entity);
            }
            else if(config.elementType == ElementType.ColorBall)
            {
                priority = 4;
                world.GetPool<ColorBallComponent>().Add(entity);
            }
            else if(config.elementType == ElementType.StarBomb)
            {
                priority = 4;
                ref var comp = ref world.GetPool<StarBombComponent>().Add(entity);
                comp.StarDotBaseElementId =
                    RandomInitColor(context.CurrentLevel.initColor, context.CurrentLevel.initColorRate);
            }
            else if(config.elementType == ElementType.SearchDot)
            {
                priority = 4;
                ref var comp = ref world.GetPool<SearchDotComponent>().Add(entity);
                comp.SearchDotBaseElementId =
                    RandomInitColor(context.CurrentLevel.initColor, context.CurrentLevel.initColorRate);
            }
            else if(config.elementType == ElementType.TowDotsColoredDot)
            {
                priority = 4;
                world.GetPool<TowDotsColoredBallComponent>().Add(entity);
            }
            else if(config.elementType == ElementType.TowDotsBombDot)
            {
                priority = 4;
                world.GetPool<TowDotsBombDotComponent>().Add(entity);
            }
            else if(config.elementType == ElementType.HorizontalDot)
            {
                priority = 4;
                world.GetPool<HorizontalDotComponent>().Add(entity);
            }
            
            // 功能棋子打上特殊的标签
            ref var specialElementComponent = ref world.GetPool<SpecialElementComponent>().Add(entity);
            specialElementComponent.Priority = priority;
            specialElementComponent.ConfigId = configId;
            specialElementComponent.ElementType = config.elementType;
            specialElementComponent.Entity = entity;
            
            // 处理成是可匹配的
            ref var eleComponent = ref world.GetPool<ElementComponent>().Get(entity);
            eleComponent.IsMatchable = true;
        }

        public bool IsElementCanSelectDelete(EcsWorld world, int entity)
        {
            return true;
        }

        private int RandomInitColor(int[] colorIds, int[] weights)
        {
            //先简单处理 TODO....
            return colorIds[Random.Range(0, colorIds.Length - 1)];
        }
    }
}