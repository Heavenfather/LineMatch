using System.Collections.Generic;
using GameConfig;
using HotfixCore.MemoryPool;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class GridItemData : IMemory
    {
        /// <summary>
        /// 格子唯一id
        /// </summary>
        /// <returns></returns>
        public int UId;
        
        /// <summary>
        /// 网格坐标
        /// </summary>
        public Vector2Int Coord;

        /// <summary>
        /// 是否空格子
        /// </summary>
        public bool IsWhite;

        /// <summary>
        /// 当前格子的元素堆叠
        /// </summary>
        public List<ElementItemData> Elements { get; private set; }

        /// <summary>
        /// 最上层的元素
        /// </summary>
        /// <returns></returns>
        public ElementItemData GetTopElement()
        {
            if (Elements == null || Elements.Count <= 0)
                return null;
            return Elements[^1];
        }

        /// <summary>
        /// 添加元素到格子上
        /// </summary>
        /// <param name="element"></param>
        public void PushElement(ElementItemData element)
        {
            if (Elements == null)
                Elements = new List<ElementItemData>();
            element.UpdatePos(Coord);
            Elements.Add(element);
            Elements.Sort((a, b) =>
            {
                if (a.ElementType == ElementType.Background)
                    return -1;
                if (b.ElementType == ElementType.Background)
                    return 1;
                if (a.SortOrder < b.SortOrder) return -1;
                if (a.SortOrder > b.SortOrder) return 1;
                        
                if(a.Priority > b.Priority) return -1;
                if(a.Priority < b.Priority) return 1;
                        
                return 0;
            });
        }

        /// <summary>
        /// 移除格子上的元素
        /// </summary>
        /// <param name="element"></param>
        public void RemoveElement(ElementItemData element)
        {
            if (Elements == null || Elements.Count <= 0)
                return;
            int index = -1;
            for (int i = 0; i < Elements.Count; i++)
            {
                if (Elements[i].UId == element.UId)
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
                Elements.RemoveAt(index);
        }
        
        /// <summary>
        /// 格子元素是否被锁住，不允许掉落此处
        /// </summary>
        /// <returns></returns>
        public bool IsGridLock()
        {
            if (IsWhite)
                return true;
            var l = ElementSystem.Instance.GetGridElements(Coord, false);
            if (l is { Count: > 0 })
            {
                for (int i = l.Count - 1; i >= 0; i--)
                {
                    if ((l[i].Data.HoldGrid >= 1 || l[i].Data.ElementType == ElementType.Lock) && 
                        l[i].Data.ElementType != ElementType.Normal && 
                        l[i].Data.ElementType != ElementType.Collect && 
                        l[i].Data.ElementType != ElementType.JumpCollect && 
                        l[i].Data.ElementType != ElementType.DropBlock && 
                        l[i].Data.ElementType != ElementType.BombBlock && 
                        l[i].Data.ElementType != ElementType.Coin && 
                        l[i].Data.ElementType != ElementType.DestroyBlock && 
                        l[i].Data.ElementType != ElementType.SpreadParterre &&
                        !ElementSystem.Instance.IsSpecialElement(l[i].Data.ElementType))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否需要补充基础棋子到该格子上
        /// </summary>
        /// <returns></returns>
        public bool IsNeedFillBaseElement()
        {
            if (IsWhite)
                return false;
            if (IsGridLock())
                return false;
            var l = ElementSystem.Instance.GetGridElements(Coord, false);
            if (l is { Count: > 0 })
            {
                for (int i = l.Count - 1; i >= 0; i--)
                {
                    if (l[i].Data.HoldGrid >= 1)
                        return false;
                }
            }

            return true;
        }

        public void Clear()
        {
            UId = -1;
            Coord = Vector2Int.zero;
            Elements?.Clear();
        }
    }
}