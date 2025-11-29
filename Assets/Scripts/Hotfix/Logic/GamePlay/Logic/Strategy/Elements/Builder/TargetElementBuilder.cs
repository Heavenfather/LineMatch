using System.Collections.Generic;
using GameConfig;
using HotfixLogic.Match;

namespace Hotfix.Logic.GamePlay
{
    public class TargetElementBuilder : IElementBuilder
    {
        public ElementType TargetType => ElementType.TargetBlock;

        public void Build(GameStateContext context, int entity, in ElementMap config)
        {
            var world = context.World;
            var posPool = world.GetPool<ElementPositionComponent>();
            ref ElementPositionComponent posComponent = ref posPool.Get(entity);
            List<GridHoldInfo> holdInfos = context.CurrentLevel.FindCoordHoldGridInfo(posComponent.X, posComponent.Y);
            if (holdInfos != null)
            {
                for (int i = 0; i < holdInfos.Count; i++)
                {
                    if (holdInfos[i].ElementId == config.Id)
                    {
                        var pool = world.GetPool<TargetElementComponent>();
                        ref var component = ref pool.Add(entity);
                        component.TargetConfigId = holdInfos[i].TargetElementId;
                        component.TargetTotal = holdInfos[i].TargetElementNum;
                        component.RemainTargetNum = holdInfos[i].TargetElementNum;
                        break;
                    }
                }
            }
        }

        public bool IsElementCanSelectDelete(EcsWorld world, int entity)
        {
            //需要收集目标的元素 它被爆到时，它不做任何反应，它只做收集反应
            return false;
        }
    }
}