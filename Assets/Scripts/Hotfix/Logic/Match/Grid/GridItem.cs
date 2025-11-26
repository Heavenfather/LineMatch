using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.Define;
using HotfixCore.Extensions;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    public class GridItem : IMemory
    {
        /// <summary>
        /// 格子对象
        /// </summary>
        public GameObject GameObject;

        /// <summary>
        /// 格子当前数据
        /// </summary>
        public GridItemData Data { get; private set; }

        /// <summary>
        /// 连接的上一个格子
        /// </summary>
        public int LinkedLastGridId { get; private set; }

        private List<ElementBase> _pickDelElements = new List<ElementBase>(5);

        /// <summary>
        /// 创建格子
        /// </summary>
        public async UniTask CreateSelf(GridItemData data, Transform parent, Vector3 position, Action callback = null)
        {
            if (GameObject != null)
                return;
            Data = data;
            GameObject =
                await G.ResourceModule.LoadGameObjectAsync($"{MatchConst.ElementAddressBase}/base_grid", parent);
            GameObject.name = $"{Data.Coord.x}-{Data.Coord.y}";
            GameObject.transform.position = position;
            BoxCollider2D collider = GameObject.GetComponent<BoxCollider2D>();
            Vector2 gridSize = GridSystem.GridSize;
            collider.size = gridSize;
            collider.offset = Vector2.zero;
            this.GameObject.transform.Find("Icon").GetComponent<SpriteRenderer>().size =
                new Vector2(gridSize.x, gridSize.y);
            if (data.Elements != null)
            {
                for (int i = 0; i < Data.Elements.Count; i++)
                {
                    InitElement(Data.Elements[i]);
                }
            }
            callback?.Invoke();
        }

        public void ReCreateElement(GridItemData data)
        {
            Data = data;
            if (Data.Elements != null)
            {
                for (int i = 0; i < Data.Elements.Count; i++)
                {
                    InitElement(Data.Elements[i]);
                }
            }
        }

        public ElementBase DoSelect()
        {
            var ele = GetSelectElement(true);
            if (ele is BaseElementItem)
            {
                ele.DoSelect();
                return ele;
            }

            return null;
        }

        public void DoDeselect()
        {
            var ele = GetSelectElement(true);
            if (ele != null)
            {
                ele.DoDeselect();
            }
        }

        public void DoDeRectangleEffect(bool resetScale = true)
        {
            var select = GetSelectElement(true);
            if (select is BaseElementItem ele)
            {
                ele.StopPopTween(resetScale);
            }
        }

        /// <summary>
        /// 获取当前格子位置
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPosition()
        {
            return GameObject.transform.position;
        }

        /// <summary>
        /// 绑定当前格子连接的是哪个格子元素
        /// </summary>
        /// <param name="gridItem"></param>
        public void BindLinkGrid(GridItem gridItem)
        {
            if (gridItem == null)
                return;
            var baseEle = gridItem.GetCanLineElementData();
            LinkedLastGridId = baseEle.UId;
        }

        /// <summary>
        /// 是否邻居格子
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool IsNeighbor(Vector2Int pos)
        {
            Vector2Int selfPos = Data.Coord;
            return (selfPos.x == pos.x - 1 && selfPos.y == pos.y) ||
                   (selfPos.x == pos.x + 1 && selfPos.y == pos.y) ||
                   (selfPos.x == pos.x && selfPos.y == pos.y - 1) ||
                   (selfPos.x == pos.x && selfPos.y == pos.y + 1);
        }

        /// <summary>
        /// 是否可以与目标连接一块
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool CanMatchTo(GridItem other)
        {
            if (other == null || other.Data.Elements == null)
                return false;

            var baseEle = GetCanLineElementData();
            if (baseEle == null)
                return false;
            if (other.Data.Elements == null)
                return false;
            var otherEle = other.GetCanLineElementData();

            //彩色球可以与任意基础棋子相连
            if (baseEle.ElementType == ElementType.ColorBall &&
                otherEle.ElementType == ElementType.Normal)
            {
                return true;
            }

            if (otherEle.ElementType == ElementType.Normal)
            {
                if (otherEle.ConfigId == baseEle.ConfigId)
                    return true;
            }

            //功能棋子可以与功能棋子相连
            if (ElementSystem.Instance.IsSpecialElement(otherEle.ElementType) &&
                ElementSystem.Instance.IsSpecialElement(baseEle.ElementType))
            {
                return true;
            }

            return false;
        }

        public ElementItemData GetCanLineElementData()
        {
            if (Data.Elements != null)
            {
                bool haveLock = false;
                for (int i = 0; i < Data.Elements.Count; i++)
                {
                    if (Data.Elements[i].ElementType == ElementType.Lock)
                    {
                        haveLock = true;
                        break;
                    }

                    if (Data.Elements[i].ElementType == ElementType.TargetBlock)
                    {
                        haveLock = true;
                        break;
                    }
                }

                if (haveLock)
                {
                    return null;
                }
                
                for (int i = 0; i < Data.Elements.Count; i++)
                {
                    if(Data.Elements[i].ElementType == ElementType.Normal ||
                       ElementSystem.Instance.IsSpecialElement(Data.Elements[i].ElementType))
                        return Data.Elements[i];
                }
            }

            return null;
        }
        
        /// <summary>
        /// 获取会选中的元素
        /// </summary>
        /// <returns></returns>
        public ElementBase GetSelectElement(bool realCoord)
        {
            if (this.Data.IsWhite)
                return null;

            ElementBase selectElement = null;
            var realElements = ElementSystem.Instance.GetGridElements(Data.Coord, realCoord);
            bool hasLockEle = false;
            if (realElements != null && realElements.Count > 0)
            {
                for (int i = 0; i < realElements.Count; i++)
                {
                    if (realElements[i].Data.ElementType == ElementType.Lock ||
                        realElements[i].Data.ElementType == ElementType.TargetBlock)
                    {
                        hasLockEle = true;
                    }

                    if (realElements[i].CanMove())
                    {
                        selectElement = realElements[i];
                    }
                }
            }

            if (hasLockEle)
                return null;

            return selectElement;
        }

        private void InitElement(ElementItemData data)
        {
            if (data.HoldGrid > 1)
            {
                var infos = ElementSystem.Instance.FindCoordHoldGridInfo(this.Data.Coord.x, this.Data.Coord.y);
                if (infos is { Count: > 0 })
                {
                    int indexOf = infos.FindIndex(x => x.ElementId == data.ConfigId);
                    if (indexOf >= 0)
                    {
                        var firstPos = infos[indexOf].StartCoord;
                        if (firstPos == Data.Coord)
                        {
                            ElementBase item = ElementSystem.Instance.GenElement(data, this.GameObject.transform);
                            ElementSystem.Instance.OffsetElementPosition(item);

                            ElementSystem.Instance.AddGridElement(this, item);
                        }
                    }
                }
            }
            else
            {
                ElementBase item = ElementSystem.Instance.GenElement(data, this.GameObject.transform);
                item.GameObject.transform.localPosition = Vector3.zero;
                ElementSystem.Instance.AddGridElement(this, item);
            }
        }

        /// <summary>
        /// 添加元素到格子上
        /// </summary>
        public void PushElement(ElementBase element, bool removeTop = true, bool needSort = true,
            bool doDestroy = false)
        {
            if(element == null)
                return;
            if (element.Data.HoldGrid < 1)
            {
                removeTop = false;
                doDestroy = false;
            }
            if (removeTop)
            {
                RemoveBaseElement(doDestroy);
            }

            Data.PushElement(element.Data);
            element.SetParent(this.GameObject.transform);
            element.GameObject.transform.localScale = Vector3.one;
            ElementSystem.Instance.AddGridElement(this, element, needSort);

            if (element.Data.GridPos != Data.Coord)
            {
                Logger.Error($"Error push element to grid:{Data.Coord}");
            }
        }

        /// <summary>
        /// 获取当前格子上的基础元素 没有的话返回空
        /// </summary>
        /// <returns></returns>
        public BaseElementItem GetBaseElementItem()
        {
            var realElements = ElementSystem.Instance.GetGridElements(Data.Coord, true);
            if (realElements == null || realElements.Count == 0)
            {
                return null;
            }

            int index = realElements.FindIndex(x => x.Data.ElementType == ElementType.Normal);
            if (index < 0)
                return null;
            return realElements[index] as BaseElementItem;
        }

        public void RemoveElement(ElementBase element)
        {
            if (Data == null || element == null)
            {
                return;
            }

            Data.RemoveElement(element.Data);
            ElementSystem.Instance.RemoveGridElement(this, element);
        }

        public void RemoveBaseElement(bool doDestroy = false)
        {
            int index = -1;
            var eles = ElementSystem.Instance.GetGridElements(Data.Coord, true);
            if (eles != null)
            {
                for (int i = 0; i < eles.Count; i++)
                {
                    if(eles[i].CanMove())
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (index != -1 && eles != null)
            {
                if (doDestroy)
                {
                    var delEle = eles[index];
                    Data.RemoveElement(delEle.Data);
                    delEle.DoDestroy(null);
                    ElementSystem.Instance.RemoveGridElement(this, delEle);

                    delEle.AfterDestroy(null).Forget();
                }
                else
                {
                    Data.RemoveElement(eles[index].Data);
                    ElementSystem.Instance.RemoveGridElement(this, eles[index]);
                }
            }
        }

        /// <summary>
        /// 移除格子上的元素
        /// </summary>
        public bool DestroyElement(ElementDestroyContext context)
        {
            _pickDelElements?.Clear();
            var eles = ElementSystem.Instance.GetGridElements(Data.Coord, true);
            if (eles is { Count: > 0 })
            {
                PickDestroyElement(context, eles,ref _pickDelElements);
                if (_pickDelElements != null)
                {
                    for (int i = 0; i < _pickDelElements.Count; i++)
                    {
                        if(context.FilterPickDestroyUId != null && context.FilterPickDestroyUId.Contains(_pickDelElements[i].Data.UId))
                            continue;
                        InterDestroyElement(context, _pickDelElements[i]);
                    }
                }
            }

            bool result = true;
            var infos = ElementSystem.Instance.GetGridElements(Data.Coord, false);
            if (infos is { Count: > 0 })
            {
                //校验是否已经为空，是否需要填充元素
                for (int i = infos.Count - 1; i >= 0; i--)
                {
                    if (infos[i].Data.HoldGrid >= 1)
                    {
                        result = false;
                        break;
                    }
                }
            }
            
            return result && !Data.IsWhite;
        }

        private void PickDestroyElement(ElementDestroyContext context, List<ElementBase> elements,ref List<ElementBase> pickList)
        {
            int delInfoIndex = context.WillDelCoords.FindIndex(x => x.Coord == this.Data.Coord);
            if(delInfoIndex < 0)
            {
                pickList?.Clear();
                return;
            }

            DeleteGridInfo delInfo = context.WillDelCoords[delInfoIndex];
            ElementMapDB elementDB = ConfigMemoryPool.Get<ElementMapDB>();
            int lockElementIndex = -1; //Lock元素
            int normalElementIndex = -1;//普通元素
            int specialElementIndex = -1;//特殊棋子
            int targetBlockIndex = -1;
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                if (elements[i].Data.ElementType == ElementType.Lock)
                {
                    lockElementIndex = i;
                }

                if (elements[i].Data.ElementType == ElementType.Normal)
                {
                    normalElementIndex = i;
                }

                if (ElementSystem.Instance.IsSpecialElement(elements[i].Data.ElementType) && !context.IsGenSpecialCoord(elements[i].Data.GridPos,out var _))
                {
                    specialElementIndex = i;
                }

                if (elements[i].Data.EliminateStyle == EliminateStyle.Target)
                {
                    targetBlockIndex = i;
                }
            }

            if (targetBlockIndex != -1)
            {
                //卷帘目标类型元素，有它在上面，底下元素不能消
                AddPickDestroyResult(elements[targetBlockIndex],delInfo, ref pickList);
                return;
            }
            
            HashSet<int> skipBlockId = new HashSet<int>(5);
            int remainAttachCount = delInfo.AttachCount;
            //上层锁住的元素优先
            if (lockElementIndex != -1)
            {
                AddPickDestroyResult(elements[lockElementIndex],delInfo, ref pickList, skipBlockId);
                int count = elementDB.CalculateTotalEliminateCount(elements[lockElementIndex].Data.ConfigId);
                remainAttachCount -= count;
                if (remainAttachCount > 0)
                {
                    skipBlockId.Add(elements[lockElementIndex].Data.UId);
                }
            }

            if (context.UsingItemId > 0 && specialElementIndex != -1)
            {
                //使用道具的情况下必须添加上功能棋子
                AddPickDestroyResult(elements[specialElementIndex],delInfo, ref pickList);
            }

            if (remainAttachCount <= 0)
            {
                return;
            }

            for (int i = elements.Count - 1; i >= 0; i--)
            {
                if(i == lockElementIndex)
                    continue;
                if (pickList.FindIndex(x => x.Data.UId == elements[i].Data.UId) >= 0)
                    continue;
                if(context.SpreadHoldElementDic != null && context.SpreadHoldElementDic.ContainsKey(elements[i].Data.GridPos))
                {
                    if (context.SpreadHoldElementDic[elements[i].Data.GridPos].Contains(elements[i].Data.UId))
                        continue;
                }
                if(context.IsGenSpecialCoord(elements[i].Data.GridPos,out var genId) && genId == elements[i].Data.ConfigId)
                    continue;
                
                if (elements[i].Data.EliminateCount == 0)
                {
                    AddPickDestroyResult(elements[i],delInfo, ref pickList,skipBlockId);
                }
                else
                {
                    if (delInfo.DelStyle.Contains(elements[i].Data.EliminateStyle) || delInfo.DelStyle.Contains(EliminateStyle.Bomb))
                    {
                        AddPickDestroyResult(elements[i], delInfo, ref pickList, skipBlockId);
                        
                        int count = elementDB.CalculateTotalEliminateCount(elements[i].Data.ConfigId);
                        remainAttachCount -= count;
                        if (remainAttachCount > 0)
                        {
                            //有剩余，说明上层元素是已经被销毁了
                            skipBlockId.Add(elements[i].Data.UId);
                        }
                    }
                }
                if (remainAttachCount <= 0)
                {
                    break;
                }
            }

            for (int i = 0; i < elements.Count; i++)
            {
                if (pickList.FindIndex(x => x.Data.UId == elements[i].Data.UId) >= 0)
                    continue;

                if (context.SpreadHoldElementDic != null &&
                    context.SpreadHoldElementDic.ContainsKey(elements[i].Data.GridPos))
                {
                    if (context.SpreadHoldElementDic[elements[i].Data.GridPos].Contains(elements[i].Data.UId))
                        continue;
                }

                if (elements[i].Data.HoldGrid < 1)
                {
                    if (delInfo.DelStyle.Contains(elements[i].Data.EliminateStyle) || delInfo.DelStyle.Contains(EliminateStyle.Bomb))
                    {
                        AddPickDestroyResult(elements[i],delInfo,ref pickList);
                        break;
                    }
                }
                else
                {
                    bool isOnlyMatch = true;
                    for (int j = 0; j < pickList.Count; j++)
                    {
                        if (pickList[j].Data.EliminateStyle != EliminateStyle.Match)
                        {
                            isOnlyMatch = false;
                            break;
                        }
                    }

                    if (isOnlyMatch)
                    {
                        if (normalElementIndex != -1)
                            AddPickDestroyResult(elements[normalElementIndex],delInfo, ref pickList);
                        if(specialElementIndex != -1)
                            AddPickDestroyResult(elements[specialElementIndex],delInfo,ref pickList);
                    }
                }
            }
        }

        private void AddPickDestroyResult(ElementBase wantAddElement,DeleteGridInfo delInfo,ref List<ElementBase> result,HashSet<int> skipOverBlockList = null)
        {
            if (wantAddElement.Data.ElementType == ElementType.Background)
            {
                if (ElementSystem.Instance.HaveOverElementLock(wantAddElement.Data.GridPos,
                        wantAddElement.Data.ConfigId, out var _, true, (ele) =>
                        {
                            if(skipOverBlockList == null)
                                return false;
                            return skipOverBlockList.Contains(ele.Data.UId);
                        }))
                {
                    return;
                }
            }

            if (delInfo.DelStyle.Contains(wantAddElement.Data.EliminateStyle) ||
                delInfo.DelStyle.Contains(EliminateStyle.Bomb))
            {
                result.Add(wantAddElement);
            }
        }

        private void InterDestroyElement(ElementDestroyContext context, ElementBase ele)
        {
            if (this.Data.Elements == null || this.Data.Elements.Count <= 0)
                return;
            
            int configId = ele.Data.ConfigId;
            if(LevelTargetSystem.Instance.CheckTargetComplete(configId))
                return;
            if (ele.DoDestroy(context))
            {
                context.AddCalAddedCount(configId, 1);
                context.AddDestroyedElement(Data.Coord, configId);
                ElementSystem.Instance.RemoveGridElement(this, ele);
                Data.RemoveElement(ele.Data);

                //变化成下一个元素
                ToNextElement(context, Data.Coord, ele.Data.NextBlockId);
                
                ele.AfterDestroy(context).Forget();
            }
        }
        
        private void ToNextElement(ElementDestroyContext context,Vector2Int coord, int nextId)
        {
            if (nextId > 0)
            {
                var data = ElementSystem.Instance.GenElementItemData(nextId, coord.x, coord.y);
                var genElement = ElementSystem.Instance.GenElement(data, this.GameObject.transform);
                ElementSystem.Instance.OffsetElementPosition(genElement);
                this.PushElement(genElement, false);
                CheckGridElementState(context, genElement);
            }
        }
        
        private void CheckGridElementState(ElementDestroyContext context,ElementBase newElement)
        {
            if(newElement == null)
                return;
            
            if (ElementSystem.Instance.IsNeedPendingDelElement(newElement.Data.ElementType))
            {
                if (context.PendingDelCoords != null &&
                    context.PendingDelCoords.Contains(Data.Coord))
                {
                    InterDestroyElement(context, newElement);
                    context.PendingDelCoords.Remove(Data.Coord);
                }
            }
        }
        
        public void Clear()
        {
            if (GameObject != null)
                GameObject.DestroyImmediate(GameObject.gameObject);
            GameObject = null;
            if (_pickDelElements != null)
            {
                _pickDelElements.Clear();
            }
        }
        
    }
}