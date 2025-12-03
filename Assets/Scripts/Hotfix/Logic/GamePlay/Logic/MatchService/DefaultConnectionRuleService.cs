using System.Collections.Generic;
using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    public class DefaultConnectionRuleService : IConnectionRuleService
    {
        public bool IsSelectable(EcsWorld world, int entity)
        {
            // 1. 必须有 ElementComponent
            var elePool = world.GetPool<ElementComponent>();
            if (!elePool.Has(entity)) return false;

            ref var ele = ref elePool.Get(entity);

            // 2. 必须可交互 (不是正在消除中、不是不可选的障碍物)
            if (!ele.IsMatchable && !ele.IsMovable) return false;
            var busyPool = world.GetPool<VisualBusyComponent>();
            if (busyPool.Has(entity)) return false;
            
            // 3. 棋子在逻辑上必须还可以交互
            var statePool = world.GetPool<ElementComponent>();
            if (statePool.Has(entity))
            {
                ref var state = ref statePool.Get(entity);
                if (state.LogicState != ElementLogicalState.Idle)
                    return false;
            }

            return true;
        }

        public bool CanConnect(EcsWorld world, int fromEntity, int toEntity, int matchConfigId,
            IMatchService matchService,List<int> currentSelectedEntities)
        {
            var elePool = world.GetPool<ElementComponent>();
            ref var fromEle = ref elePool.Get(fromEntity);
            ref var toEle = ref elePool.Get(toEntity);

            // 1.普通棋子不管哪种消除方式都是一样的，就是检查颜色是否相同
            if (fromEle.Type == ElementType.Normal && toEle.Type == ElementType.Normal)
                return fromEle.ConfigId == toEle.ConfigId;

            // 2.特殊的检测，就放到不同的消除服务中处理 彩球逻辑/炸弹/火箭等特殊连线逻辑
            return matchService.CanConnect(world, in fromEle, in toEle, matchConfigId, fromEntity, toEntity,currentSelectedEntities);
        }
    }
}