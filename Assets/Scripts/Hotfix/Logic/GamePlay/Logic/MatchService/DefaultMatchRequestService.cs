using System.Collections.Generic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public class DefaultMatchRequestService : IMatchRequestService
    {
        public void RequestPlayerLine(EcsWorld world, List<int> selectedEntities, int triggerEntity, int configId)
        {
            CreateMatchRequest(world, new MatchRequestComponent
            {
                Type = MatchRequestType.PlayerLine,
                InvolvedEntities = new List<int>(selectedEntities), // 拷贝列表，防止外部清理
                TriggerEntity = triggerEntity, //就是最后选中的那一个
                TargetEntity = -1,
                ConfigId = configId // 普通的连线中，它就是发起的元素Id
            });
        }

        public void RequestPlayerSquare(EcsWorld world, List<int> selectedEntities, int closedPointEntity, int configId)
        {
            CreateMatchRequest(world, new MatchRequestComponent
            {
                Type = MatchRequestType.PlayerSquare,
                InvolvedEntities = new List<int>(selectedEntities),
                TriggerEntity = selectedEntities.Count > 0 ? selectedEntities[^1] : -1,
                TargetEntity = closedPointEntity, // 闭环点作为 Target
                ConfigId = configId
            });
        }

        public void RequestUseItem(EcsWorld world, int itemId, int targetEntity = -1)
        {
            CreateMatchRequest(world, new MatchRequestComponent
            {
                Type = MatchRequestType.UseItem,
                ItemId = itemId,
                TriggerEntity = -1, // 道具没有 Trigger 实体
                TargetEntity = targetEntity,
                InvolvedEntities = null
            });
        }

        public void RequestPostDropCheck(EcsWorld world, int checkEntity)
        {
            CreateMatchRequest(world, new MatchRequestComponent
            {
                Type = MatchRequestType.PostDropCheck,
                TriggerEntity = checkEntity,
                TargetEntity = -1,
                InvolvedEntities = null //掉落是没有选中触发体的，它作用的是本身
            });
        }

        public void RequestRocket(EcsWorld world, int rocketEntity)
        {
            CreateMatchRequest(world, new MatchRequestComponent
            {
                Type = MatchRequestType.Rocket,
                TriggerEntity = rocketEntity,
                TargetEntity = -1,
                InvolvedEntities = null
            });
        }

        public void RequestBomb(EcsWorld world, Vector2Int bombCoord)
        {
            CreateMatchRequest(world, new MatchRequestComponent
            {
                Type = MatchRequestType.Bomb,
                TriggerEntity = -1,
                TargetEntity = -1,
                InvolvedEntities = null,
                ExtraData = bombCoord
            });
        }

        public void RequestSearchDot(EcsWorld world, int dotEntity)
        {
            CreateMatchRequest(world, new MatchRequestComponent
            {
                Type = MatchRequestType.TowDotsFunctionElement,
                TriggerEntity = dotEntity,
                TargetEntity = -1,
                InvolvedEntities = new List<int>() { dotEntity }
            });
        }

        // --- 内部通用创建逻辑 ---
        private void CreateMatchRequest(EcsWorld world, MatchRequestComponent reqData)
        {
            int entity = world.NewEntity();
            var pool = world.GetPool<MatchRequestComponent>();
            ref var comp = ref pool.Add(entity);

            // 赋值
            comp.Type = reqData.Type;
            comp.TriggerEntity = reqData.TriggerEntity;
            comp.TargetEntity = reqData.TargetEntity;
            comp.InvolvedEntities = reqData.InvolvedEntities;
            comp.ConfigId = reqData.ConfigId;
            comp.ItemId = reqData.ItemId;
            comp.ExtraData = reqData.ExtraData;
        }
    }
}