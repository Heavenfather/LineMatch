using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.Logic.Match;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class MatchLineBlockElementItem : BlockElementItem
    {
        private int _matchElementId;
        
        protected override void OnInitialized()
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[Data.ConfigId];
            int.TryParse(config.extra, out _matchElementId);
            base.OnInitialized();
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            if (_matchElementId <= 0)
                return false;
            if (context.GenSquareElementId <= 0)
                return false;
            if(_matchElementId != context.GenSquareElementId)
                return false;
            
            //立即重置数值，否则会影响多个
            context.GenSquareElementId = -1;
            
            DoLineDestroy(context).Forget();
            return true;
        }

        private async UniTask DoLineDestroy(ElementDestroyContext context)
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            context.AddWaitElementDestroyTask(tcs.Task);
            List<Vector2Int> delList = null;
            if (Data.Direction == ElementDirection.Right)
            {
                //横向消除
                delList = MatchTweenUtil.GetRocketDelPos(Data.GridPos, ElementDirection.Right);
                
            }
            else if (Data.Direction == ElementDirection.Up)
            {
                //纵向消除
                delList = MatchTweenUtil.GetRocketDelPos(Data.GridPos, ElementDirection.Up);
            }

            if (delList != null && delList.Count > 0)
            {
                HashSet<Vector2Int> finalDelCoords = new HashSet<Vector2Int>(delList.Count);
                for (int i = 0; i < delList.Count; i++)
                {
                    var elements = ElementSystem.Instance.GetGridElements(delList[i], false);
                    bool canAttach = ElementSystem.Instance.IsCanAttachElements(elements);
                    if (canAttach)
                    {
                        context.AddGridDelCoord(delList[i]);
                        var element = elements[^1];
                        finalDelCoords.Add(element.Data.GridPos);

                        if (ElementSystem.Instance.IsSpecialElement(element.Data.ElementType))
                        {
                            ValidateManager.Instance.CollectAutoElement(context, element);
                        }
                        else
                        {
                            context.AddWillDelCoord(element.Data.GridPos, element.Data.EliminateStyle,
                                element.Data.UId);
                            if (element.Data.EliminateStyle != EliminateStyle.Bomb)
                                context.AddWillDelCoord(element.Data.GridPos, EliminateStyle.Bomb, element.Data.UId,
                                    Data.GridPos);
                            if (element.Data.ElementType == ElementType.Normal)
                            {
                                ValidateManager.Instance.AddEffectBaseElementScore(element.Data.ConfigId);
                            }
                        }
                    }
                }

                if (finalDelCoords.Count > 0)
                {
                    List<UniTask> tasks = new List<UniTask>(finalDelCoords.Count);
                    List<DeleteGridInfo> delGridInfos = new List<DeleteGridInfo>(context.WillDelCoords);
                    context.AddFilterPickUId(Data.UId);
                    foreach (var coord in finalDelCoords)
                    {
                        tasks.Add(context.GridSystem.DoElement(coord, context, delGridInfos, true));
                    }
                    PlayEffect();
                    await UniTask.WhenAll(tasks);
                    tcs.TrySetResult();
                }
                else
                {
                    tcs.TrySetResult();
                }
            }
            else
            {
                tcs.TrySetResult();
            }
        }

        public void ResetIdleState(bool haveLock)
        {
            var icon = this.GameObject.transform.Find("Icon");
            if (icon != null)
            {
                ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
                ElementMap config = db[Data.ConfigId];
                SetIdleSpine(icon, config.idleSpine, true, !haveLock);
            }
        }
    }
    
    public static class MatchLineBlockElementItemUtil
    {
        public static void UpdateIdleAnimation()
        {
            var allElements = ElementSystem.Instance.GridElements;
            foreach (var elements in allElements.Values)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i] is MatchLineBlockElementItem element)
                    {
                        bool haveOver = false;
                        var coordEles = ElementSystem.Instance.GetGridElements(elements[i].Data.GridPos, false);
                        if (coordEles.Count > 1)
                        {
                            for (int j = coordEles.Count - 1; j >= 0; j--)
                            {
                                if (coordEles[j].Data.ConfigId == element.Data.ConfigId)
                                    continue;
                                if (coordEles[j].Data.ElementType == ElementType.TargetBlock ||
                                    coordEles[j].Data.ElementType == ElementType.Lock)
                                {
                                    haveOver = true;
                                    break;
                                }
                            }
                        }

                        element.ResetIdleState(!haveOver);
                    }
                }
            }
        }
    }
}