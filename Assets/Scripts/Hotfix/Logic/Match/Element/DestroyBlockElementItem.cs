using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class DestroyBlockElementItem : BlockElementItem
    {
        private int _delOtherCnt = 0;
        protected override void OnInitialized()
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[Data.ConfigId];
            int.TryParse(config.extra, out _delOtherCnt);
            base.OnInitialized();
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            if(_delOtherCnt <= 0)
                return false;
            if (!context.RandomDiffuseWaitCoords.Add(Data.GridPos))
            {
                var delCoords = PickDestroyCoords(context);
                DoDestroyOtherBlock(context, delCoords);
                return true;
            }
            return false;
        }

        private List<ElementBase> PickDestroyCoords(ElementDestroyContext context)
        {
            List<ElementBase> pickElements = new List<ElementBase>(_delOtherCnt);
            var allCoords = ElementSystem.Instance.GridElements.Keys;
            int pickedCount = 0;
            
            // 先选择障碍物类型的格子
            List<ElementBase> baseElements = new List<ElementBase>();
            List<ElementBase> specialElements = new List<ElementBase>();
            foreach (var v in allCoords)
            {
                if (pickedCount >= _delOtherCnt) break;
        
                var elements = ElementSystem.Instance.GetGridElements(v, false);
                if(elements == null || elements.Count <= 0)
                    continue;
                bool canPick = true;
                ElementBase element = null;
                for (int i = elements.Count - 1; i >= 0; i--)
                {
                    if (elements[i] is TargetBlockElementItem || elements[i] is CollectElementItem || elements[i] is DestroyBlockElementItem)
                    {
                        canPick = false;
                        break;
                    }
                    //这里先不要把功能棋子和基础棋子放进来
                    if (ElementSystem.Instance.IsSpecialElement(elements[i].Data.ElementType))
                    {
                        canPick = false;
                        
                        if (!ElementSystem.Instance.HaveOverElementLock(elements, -1, out var _))
                        {
                            specialElements.Add(elements[i]);
                        }
                        break;
                    }

                    if (elements[i].Data.ElementType == ElementType.Normal)
                    {
                        canPick = false;
                        if (!ElementSystem.Instance.HaveOverElementLock(elements, -1, out var _))
                        {
                            baseElements.Add(elements[i]);
                        }
                        break;
                    }
                    
                    element = elements[i];
                    break;
                }
                if(canPick == false)
                    continue;
                if (element != null)
                {
                    if (context.DestroyOtherBlockPickedCoords != null && !context.DestroyOtherBlockPickedCoords.Contains(element.Data.GridPos))
                    {
                        pickedCount++;
                        pickElements.Add(element);
                        context.AddDestroyBlockPickCoord(element.Data.GridPos);
                    }
                }
            }
            // 再选择普通类型的格子
            if (pickedCount < _delOtherCnt)
            {
                if (baseElements.Count > 0)
                {
                    for (int i = 0; i < baseElements.Count; i++)
                    {
                        if (pickedCount >= _delOtherCnt) break;
                        if (context.DestroyOtherBlockPickedCoords != null && !context.DestroyOtherBlockPickedCoords.Contains(baseElements[i].Data.GridPos))
                        {
                            pickedCount++;
                            pickElements.Add(baseElements[i]);
                            context.AddDestroyBlockPickCoord(baseElements[i].Data.GridPos);
                        }
                    }
                }
            }

            // 再选择功能棋子格子
            if (pickedCount < _delOtherCnt)
            {
                if (specialElements.Count > 0)
                {
                    for (int i = 0; i < specialElements.Count; i++)
                    {
                        if (pickedCount >= _delOtherCnt) break;
                        if (context.DestroyOtherBlockPickedCoords != null && !context.DestroyOtherBlockPickedCoords.Contains(specialElements[i].Data.GridPos))
                        {
                            pickedCount++;
                            pickElements.Add(specialElements[i]);
                            context.AddDestroyBlockPickCoord(specialElements[i].Data.GridPos);
                        }
                    }
                }
            }
            return pickElements;
        }

        private void DoDestroyOtherBlock(ElementDestroyContext context, List<ElementBase> pickEles)
        {
            if (pickEles == null || pickEles.Count <= 0)
            {
                PlayEffect();
                return;
            }
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            context.AddWaitElementDestroyTask(tcs.Task);

            for (int i = 0; i < pickEles.Count; i++)
            {
                var element = pickEles[i];
                context.AddGridDelCoord(element.Data.GridPos);
                if (ElementSystem.Instance.IsSpecialElement(element.Data.ElementType))
                {
                    ValidateManager.Instance.CollectAutoElement(context, element);
                }
                else
                {
                    context.AddWillDelCoord(element.Data.GridPos, EliminateStyle.Bomb, element.Data.UId, Data.GridPos);
                    context.AddWillDelCoord(element.Data.GridPos, element.Data.EliminateStyle, element.Data.UId);
                    if(element.Data.ElementType == ElementType.Normal)
                        ValidateManager.Instance.AddEffectBaseElementScore(element.Data.ConfigId);
                }
            }
            
            GridSystem gridSystem = context.GridSystem;
            Vector3 startPos = GridSystem.GetGridPositionByCoord(Data.GridPos.x, Data.GridPos.y);
            List<Vector3> endPos = new List<Vector3>(pickEles.Count);
            for (int i = 0; i < pickEles.Count; i++)
            {
                endPos.Add(GridSystem.GetGridPositionByCoord(pickEles[i].Data.GridPos.x, pickEles[i].Data.GridPos.y));
            }

            List<DeleteGridInfo> delInfos = new List<DeleteGridInfo>(context.WillDelCoords);
            gridSystem.DoTrailEmitter(startPos, endPos, (index) =>
            {
                if(index < 0 || index >= pickEles.Count)
                {
                    tcs.TrySetResult();
                    return;
                }
                gridSystem.DoElement(pickEles[index].Data.GridPos, context, delInfos, true).Forget();
            }, () =>
            {
                PlayEffect();
                tcs.TrySetResult();
            });
        }

    }
}