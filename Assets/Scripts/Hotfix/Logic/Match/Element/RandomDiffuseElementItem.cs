using System.Collections.Generic;
using DG.Tweening;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;
using Random = UnityEngine.Random;

namespace HotfixLogic.Match
{
    public class RandomDiffuseElementItem : BlockElementItem
    {
        private const float MOVE_DURATION = 0.3f;
        private const float DELAY_TIME = 0.05f;

        protected override void OnInitialized()
        {
            SetVisibleExtraEffect(true);
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            if (string.IsNullOrEmpty(Data.Extra))
            {
                return base.OnDestroy(context);
            }

            if (IsLockState())
                return false;
            
            if (!context.RandomDiffuseWaitCoords.Add(Data.GridPos))
            {
                PlayEffect();
                SetVisibleExtraEffect(false);
                ExpansionElement(context);
                return true;
            }

            return false;
        }

        private void SetVisibleExtraEffect(bool visible)
        {
            var extraEffect = this.GameObject.transform.Find("Match_eff_mao_daiji_02");
            if (extraEffect != null)
            {
                extraEffect.SetVisible(visible);
            }
        }

        private bool IsLockState()
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            string[] splitExt = Data.Extra.Split('|');
            int.TryParse(splitExt[0], out int nextElementId);
            if (db[Data.ConfigId].nameFlag == "caomeilan")
            {
                List<int> nextIds = new List<int>() { nextElementId };
                db.RefElementNextList(nextElementId, ref nextIds);
                int realTargetElement = -1;
                Dictionary<int, int> targetElements = LevelTargetSystem.Instance.TargetElements;
                for (int i = 0; i < nextIds.Count; i++)
                {
                    if (targetElements.ContainsKey(nextIds[i]))
                    {
                        realTargetElement = nextIds[i];
                    }
                }

                if (realTargetElement == -1)
                    return false;
                bool isFinish = LevelTargetSystem.Instance.IsTargetFinish(realTargetElement);
                if (isFinish)
                    return true;
                int boardHaveCount = ElementSystem.Instance.GetBoardElementCount(nextIds);
                int remainCount = targetElements[realTargetElement];
                if (boardHaveCount >= remainCount)
                {
                    return true;
                }
            }

            return false;
        }
        
        private void ExpansionElement(ElementDestroyContext context)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            string[] splitExt = Data.Extra.Split('|');
            int.TryParse(splitExt[0], out int nextElementId);
            int.TryParse(splitExt[1], out int count);
            if(count <= 0)
            {
                return;
            }

            G.UIModule.ScreenLock(MatchConst.MatchRandomDiffuseMoveLock, true);
            List<Vector2Int> elements = new(count);
            Vector2Int randomCoord;
            int successCount = 0;
            for (int i = 0; i < count; i++)
            {
                if(!db.IsCircleElement(this.Data.ConfigId) && !LevelTargetSystem.Instance.IsRandomDiffuseCanPop(nextElementId))
                    continue;
                bool success = false;
                if (db[nextElementId].elementType == ElementType.Collect ||
                    db[nextElementId].elementType == ElementType.JumpCollect)
                {
                    randomCoord = PickCollectRandomCoord(context.GridSystem, elements, out success, nextElementId, count);
                    Logger.Debug($"随机到 ：{randomCoord}");
                }
                else
                {
                    randomCoord = ElementSystem.Instance.RandomPickBaseElement(elements, out success, nextElementId,FilterRandomCondition);
                }

                if (!success)
                {
                    if (!db.IsCircleElement(this.Data.ConfigId))
                    {
                        //这里因为随机摆放位置失败，但是统计的元素数量是固定的，如果没随机成功，这里直接扣除掉数量
                        int reduce = count - successCount;
                        Dictionary<int, int> dic = new Dictionary<int, int>() { [nextElementId] = reduce };
                        G.EventModule.DispatchEvent(GameEventDefine.OnMatchStepMove,
                            EventOneParam<Dictionary<int, int>>.Create(dic));
                        Logger.Warning($"随机元素失败:{this.Data.GridPos},将直接减掉 {nextElementId} 数量 {reduce}");
                    }

                    break;
                }

                successCount++;
                LevelTargetSystem.Instance.AddRandomDiffuseCount(nextElementId, 1);
                var gridItem = context.GridSystem.GetGridByCoord(randomCoord);
                var itemData = ElementSystem.Instance.GenElementItemData(nextElementId, randomCoord.x, randomCoord.y);
                var elementItem = ElementSystem.Instance.GenElement(itemData, gridItem.GameObject.transform);
                context.AddHoldSpreadElement(gridItem.Data.Coord, elementItem.Data.UId);
                elementItem.GameObject.transform.position = GameObject.transform.position;
                gridItem.PushElement(elementItem, doDestroy: true);
                elementItem.GameObject.transform.DOLocalMove(Vector3.zero, MOVE_DURATION).SetAutoKill(true).OnComplete(
                        () =>
                        {
                            G.UIModule.ScreenLock(MatchConst.MatchRandomDiffuseMoveLock, false);
                        })
                    .SetDelay(i * DELAY_TIME);
            }

            if (successCount == 0)
            {
                G.UIModule.ScreenLock(MatchConst.MatchRandomDiffuseMoveLock, false);
            }
        }

        private bool FilterRandomCondition(ElementBase ele)
        {
            if (ele.Data.GridPos == this.Data.GridPos)
                return false;
            return true;
        }

        private Vector2Int PickCollectRandomCoord(GridSystem gridSystem, List<Vector2Int> filterList, out bool success,
            int filterId, int pickCount)
        {
            //弹出的收集类型元素 优先掉落到顶部
            (int width, int height) = gridSystem.GetBoardSize();
            List<Vector2Int> randomCoords = new List<Vector2Int>(width);
            for (int y = 0; y < height; y++)
            {
                int lastX = gridSystem.FindLastNotEmptyX(y);
                for (int x = 0; x < lastX + 1; x++)
                {
                    if (!gridSystem.IsValidPosition(x, y))
                        continue;
                    if (filterList.FindIndex(t => t == new Vector2Int(x, y)) >= 0)
                        continue;
                    var gridItem = gridSystem.GetGridByCoord(x, y);
                    var baseElement = gridItem.GetBaseElementItem();
                    if (baseElement != null)
                    {
                        if (baseElement.Data.ConfigId == filterId)
                            continue;
                        randomCoords.Add(new Vector2Int(x, y));
                    }
                }

                if (randomCoords.Count >= pickCount - filterList.Count)
                    break;
            }

            success = randomCoords.Count > 0;

            return randomCoords.Count > 0 ? randomCoords[Random.Range(0, randomCoords.Count)] : Vector2Int.zero;
        }
    }
}