using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameConfig;
using UnityEngine;

namespace HotfixLogic.Match
{
    /// <summary>
    /// 删除格子信息
    /// </summary>
    public struct DeleteGridInfo : IEquatable<DeleteGridInfo>
    {
        /// <summary>
        /// 格子坐标
        /// </summary>
        public Vector2Int Coord;

        /// <summary>
        /// 该格子上会被删除的元素样式
        /// </summary>
        public HashSet<EliminateStyle> DelStyle;

        /// <summary>
        /// 当次影响到该格子的爆炸元素坐标
        /// </summary>
        public HashSet<Vector2Int> BombEffectCoord;

        /// <summary>
        /// 消除的元素唯一id
        /// </summary>
        public HashSet<int> ElementUIds;

        /// <summary>
        /// 消除元素的配置id
        /// </summary>
        public HashSet<int> ElementConfigIds;

        /// <summary>
        /// 爆炸撞击到该格子的次数
        /// </summary>
        public int AttachCount;

        public DeleteGridInfo(Vector2Int coord, EliminateStyle delStyle, Vector2Int bombEffectCoord,
            int elementUId, int attachCount)
        {
            Coord = coord;
            DelStyle = new HashSet<EliminateStyle>() { delStyle };
            BombEffectCoord = new HashSet<Vector2Int>() { bombEffectCoord };
            AttachCount = attachCount;
            ElementConfigIds = new HashSet<int>();
            ElementUIds = new HashSet<int>();
            AddElementUId(elementUId);
        }
        
        public void ReduceAttachCount(int reduceCount)
        {
            AttachCount = Mathf.Max(0, AttachCount - reduceCount);
        }

        public void AddElementUId(int uid)
        {
            ElementUIds.Add(uid);
            var configId = ElementSystem.Instance.GetConfigIdByUId(Coord, uid);
            if (configId != -1)
            {
                ElementConfigIds.Add(configId);
                if (ElementSystem.Instance.IsBombBlockElement(configId))
                {
                    DelStyle.Add(EliminateStyle.Bomb);
                }
            }
        }

        public bool Equals(DeleteGridInfo other)
        {
            return Coord.Equals(other.Coord) && Equals(DelStyle, other.DelStyle) &&
                   Equals(BombEffectCoord, other.BombEffectCoord) && Equals(ElementUIds, other.ElementUIds) &&
                   AttachCount == other.AttachCount;
        }

        public override bool Equals(object obj)
        {
            return obj is DeleteGridInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Coord, DelStyle, BombEffectCoord, ElementUIds, AttachCount);
        }
    }

    /// <summary>
    /// 生成功能棋子的信息
    /// </summary>
    public struct GenSpecialElementInfo
    {
        /// <summary>
        /// 形成的功能棋子id
        /// </summary>
        public int GenConfigId;
        
        /// <summary>
        /// 生成位置
        /// </summary>
        public Vector2Int Coord;
        
        /// <summary>
        /// 功能棋子类型
        /// </summary>
        public ElementType ElementType;
    }

    public class ElementDestroyContext
    {
        /// <summary>
        /// 当前消除时候有消除到火元素
        /// </summary>
        public bool HasDestroyFireElement = false;

        /// <summary>
        /// 是否彩色球清空所有同色
        /// </summary>
        public bool IsColorBallClearAll = false;
        
        public bool IsRocketAndBomb = false;
        
        /// <summary>
        /// 正在使用的道具id
        /// </summary>
        public int UsingItemId = -1;

        /// <summary>
        /// 消除后过滤会掉落的元素
        /// </summary>
        public int FilterElementId = -1;

        /// <summary>
        /// 收集掉落的元素id
        /// </summary>
        public int CollectDropId = -1;

        /// <summary>
        /// 形成方格的元素Id
        /// </summary>
        public int GenSquareElementId;

        /// <summary>
        /// 格子管理系统
        /// </summary>
        public GridSystem GridSystem;
        
        /// <summary>
        /// 消除会受影响的格子
        /// </summary>
        public HashSet<Vector2Int> EffGridCoords;

        /// <summary>
        /// 可能会额外删除的格子坐标
        /// </summary>
        public HashSet<Vector2Int> ExtraDelGridCoords;

        /// <summary>
        /// 需要等待即将会被消除的格子
        /// </summary>
        public HashSet<Vector2Int> PendingDelCoords = new HashSet<Vector2Int>();

        /// <summary>
        /// 所有删除的格子坐标
        /// </summary>
        public HashSet<Vector2Int> AllDelGridCoord;

        /// <summary>
        /// 当前消除的元素id
        /// </summary>
        public HashSet<int> CurrentEffectId;

        /// <summary>
        /// 过滤删除的棋子
        /// </summary>
        public HashSet<int> FilterPickDestroyUId;
        
        /// <summary>
        /// 炸全屏的元素坐标
        /// </summary>
        public List<Vector2Int> BombPanelElementCoord;
        
        /// <summary>
        /// 会触发消除的元素格子
        /// </summary>
        public List<DeleteGridInfo> WillDelCoords;

        /// <summary>
        /// 销毁成功的元素集合
        /// </summary>
        public Dictionary<Vector2Int, int> DestroyedElements;

        /// <summary>
        /// 传染的元素列表
        /// </summary>
        public Dictionary<int, HashSet<Vector2Int>> InfectElements;

        /// <summary>
        /// 统计的删除目标 key=elementId value=count
        /// </summary>
        public Dictionary<int, int> CalAddedDelTargets;

        /// <summary>
        /// 生成的功能棋子列表
        /// </summary>
        public List<GenSpecialElementInfo> GenSpecialInfos;

        /// <summary>
        /// 是否有新生成的功能棋子
        /// </summary>
        public bool IsCombineSpecialElement = false;
        
        /// <summary>
        /// 彩球和特殊棋子连接
        /// </summary>
        public List<ElementItemData> BallLineSpecialItems;

        /// <summary>
        /// 和特殊棋子连接的彩球
        /// </summary>
        public Vector2Int LinkSpecialBallCoord;

        public bool IsColorBallLineNormal = false;

        public bool IsColorBallLineRocket = false;

        public bool IsColorBallLineBomb = false;

        public bool IsRocketAndRocket = false;

        public bool IsBurningCollectItemCanRelease = false;

        public bool IsCalculateCoinState = false;

        public bool IsAutoReleaseBomb = false;
        
        /// <summary>
        /// 火箭连火箭
        /// </summary>
        public List<ElementItemData> RocketAndRocket;
        
        /// <summary>
        /// 炸弹连炸弹
        /// </summary>
        public List<Vector2Int> DoubleBombCoords;

        /// <summary>
        /// 彩球连彩球
        /// </summary>
        public List<Vector2Int> DoubleColorBallCoords;

        /// <summary>
        /// 新增水的坐标
        /// </summary>
        public List<Vector2Int> NewWaterCoords;

        /// <summary>
        /// 等待元素销毁的异步列表
        /// </summary>
        public List<UniTask> WaitElementDestroyTasks;

        /// <summary>
        /// 会被牵连销毁的彩带元素
        /// </summary>
        public HashSet<Vector2Int> ColoredRibbonWaitDelCoords;

        /// <summary>
        /// 扩散元素，在延迟删除时不会被删除的列表
        /// </summary>
        public Dictionary<Vector2Int, HashSet<int>> SpreadHoldElementDic;

        /// <summary>
        /// 功能棋子删除后过滤坐标
        /// </summary>
        public HashSet<Vector2Int> SpecialElementDelFilterCoords;

        public HashSet<int> ColorBallFilterIds;

        /// <summary>
        /// 随机扩散元素等待掉落后再扩散
        /// </summary>
        public HashSet<Vector2Int> RandomDiffuseWaitCoords = new HashSet<Vector2Int>();

        public HashSet<Vector2Int> DestroyOtherBlockPickedCoords = new HashSet<Vector2Int>();
        
        public void AddWillDelCoord(Vector2Int coord, EliminateStyle style, int uid, Vector2Int bombCoord = default,int attachCount = 0)
        {
            if (WillDelCoords == null)
                WillDelCoords = new();
            int index = -1;
            for (int i = 0; i < WillDelCoords.Count; i++)
            {
                if (WillDelCoords[i].Coord == coord)
                {
                    index = i;
                    break;
                }
            }
            if (index < 0)
                WillDelCoords.Add(new DeleteGridInfo(coord,style,bombCoord,uid,attachCount == 0 ? 1 : attachCount));
            else
            {
                if (style == EliminateStyle.Bomb && WillDelCoords[index].DelStyle.Contains(style) &&
                    !WillDelCoords[index].BombEffectCoord.Contains(bombCoord))
                {
                    var temp = WillDelCoords[index];
                    if (attachCount != 0)
                        temp.AttachCount = attachCount;
                    else
                        temp.AttachCount += 1;
                    WillDelCoords[index] = temp;
                }
                else if (attachCount != 0)
                {
                    var temp = WillDelCoords[index];
                    temp.AttachCount = attachCount;
                    WillDelCoords[index] = temp;
                }

                if (style == EliminateStyle.Bomb)
                    WillDelCoords[index].BombEffectCoord.Add(bombCoord);
                WillDelCoords[index].DelStyle.Add(style);
                WillDelCoords[index].AddElementUId(uid);
            }
        }

        public void AddCurrentEffectId(int id)
        {
            if (CurrentEffectId == null)
                CurrentEffectId = new HashSet<int>();
            CurrentEffectId.Add(id);
        }

        public void AddBombPanelElement(Vector2Int coord)
        {
            if (BombPanelElementCoord == null)
                BombPanelElementCoord = new List<Vector2Int>();
            if (!BombPanelElementCoord.Contains(coord))
                BombPanelElementCoord.Add(coord);
        }
        
        public void AddInfectElement(int id, HashSet<Vector2Int> infectCoords)
        {
            if (InfectElements == null)
            {
                InfectElements = new Dictionary<int, HashSet<Vector2Int>>();
            }

            if (!InfectElements.TryAdd(id, infectCoords))
            {
                foreach (var coord in infectCoords)
                {
                    InfectElements[id].Add(coord);
                }
            }
        }

        public void AddEffGridCoord(Vector2Int coord)
        {
            if (EffGridCoords == null)
                EffGridCoords = new HashSet<Vector2Int>();
            EffGridCoords.Add(coord);
        }

        public void AddDestroyedElement(Vector2Int coord, int elementId)
        {
            if (DestroyedElements == null)
            {
                DestroyedElements = new Dictionary<Vector2Int, int>();
            }

            DestroyedElements.TryAdd(coord, elementId);
        }

        public void AddSpecialElement(ElementType type, Vector2Int pos)
        {
            if (!ElementSystem.Instance.IsSpecialElement(type))
                return;
            int id = ElementSystem.Instance.GetSpecialElementConfigId(type);
            if (id == -1)
                return;

            if (GenSpecialInfos == null)
                GenSpecialInfos = new List<GenSpecialElementInfo>();
            GenSpecialElementInfo info = new GenSpecialElementInfo();
            info.Coord = pos;
            info.ElementType = type;
            info.GenConfigId = ElementSystem.Instance.GetSpecialElementConfigId(type);

            GenSpecialInfos.Add(info);
            IsCombineSpecialElement = GenSpecialInfos.Count >= 1;
        }

        public bool TryGetSpecialElement(Vector2Int coord, out ElementItemData itemData)
        {
            itemData = null;

            if (GenSpecialInfos == null || GenSpecialInfos.Count <= 0)
                return false;
            
            for (int i = 0; i < GenSpecialInfos.Count; i++)
            {
                if (GenSpecialInfos[i].Coord == coord)
                {
                    int configId = ElementSystem.Instance.GetSpecialElementConfigId(GenSpecialInfos[i].ElementType);
                    itemData = ElementSystem.Instance.GenElementItemData(configId, coord.x, coord.y);
                    return true;
                }
            }

            return false;
        }

        public bool IsGenSpecialCoord(Vector2Int coord,out int configId)
        {
            configId = -1;
            if (GenSpecialInfos == null)
                return false;
            int index = GenSpecialInfos.FindIndex(info => info.Coord == coord);
            if (index >= 0)
            {
                configId = GenSpecialInfos[index].GenConfigId;
            }
            return index >= 0;
        }
        
        public void AddDoubleBombCoord(Vector2Int coord)
        {
            if (DoubleBombCoords == null)
                DoubleBombCoords = new List<Vector2Int>();
            DoubleBombCoords.Add(coord);
        }

        public void AddRocketAndRocket(ElementItemData data)
        {
            if (RocketAndRocket == null)
                RocketAndRocket = new List<ElementItemData>();
            RocketAndRocket.Add(data);
        }

        public void AddDoubleColorBallCoord(Vector2Int coord)
        {
            if (DoubleColorBallCoords == null)
                DoubleColorBallCoords = new List<Vector2Int>();
            DoubleColorBallCoords.Add(coord);
        }

        public void AddWaterCoord(Vector2Int coord)
        {
            if (NewWaterCoords == null)
                NewWaterCoords = new List<Vector2Int>();
            NewWaterCoords.Add(coord);
        }

        public void AddBallLineSpecialItem(ElementItemData data)
        {
            if (BallLineSpecialItems == null)
                BallLineSpecialItems = new List<ElementItemData>();
            BallLineSpecialItems.Add(data);
        }

        public void AddCalAddedCount(int elementId, int count)
        {
            if (CalAddedDelTargets == null)
                CalAddedDelTargets = new Dictionary<int, int>();
            CalAddedDelTargets.TryAdd(elementId, 0);
            CalAddedDelTargets[elementId] += count;
        }

        public void AddPendingDelCoords(Vector2Int coord)
        {
            if (PendingDelCoords == null)
                PendingDelCoords = new HashSet<Vector2Int>();
            PendingDelCoords.Add(coord);
        }

        public void AddExtraDelCoords(Vector2Int coord)
        {
            if (ExtraDelGridCoords == null)
                ExtraDelGridCoords = new HashSet<Vector2Int>();
            ExtraDelGridCoords.Add(coord);
        }

        public void AddGridDelCoord(Vector2Int coord)
        {
            if (AllDelGridCoord == null)
                AllDelGridCoord = new HashSet<Vector2Int>();
            AllDelGridCoord.Add(coord);
        }

        public void AddWaitElementDestroyTask(UniTask task)
        {
            if (WaitElementDestroyTasks == null)
                WaitElementDestroyTasks = new();
            WaitElementDestroyTasks.Add(task);
        }

        public void AddColoredRibbonCoord(Vector2Int coord)
        {
            if(ColoredRibbonWaitDelCoords == null)
                ColoredRibbonWaitDelCoords = new();
            ColoredRibbonWaitDelCoords.Add(coord);
        }

        public void AddHoldSpreadElement(Vector2Int coord, int uid)
        {
            if (SpreadHoldElementDic == null)
                SpreadHoldElementDic = new Dictionary<Vector2Int, HashSet<int>>();
            SpreadHoldElementDic.TryAdd(coord, new HashSet<int>());
            SpreadHoldElementDic[coord].Add(uid);
        }

        public void AddSpecialElementDelFilterCoord(Vector2Int coord)
        {
            if (SpecialElementDelFilterCoords == null)
                SpecialElementDelFilterCoords = new HashSet<Vector2Int>();
            SpecialElementDelFilterCoords.Add(coord);
        }

        public void AddColorBallFilterId(int id)
        {
            if (ColorBallFilterIds == null)
                ColorBallFilterIds = new HashSet<int>();
            ColorBallFilterIds.Add(id);
        }

        public void AddDestroyBlockPickCoord(Vector2Int coord)
        {
            if(DestroyOtherBlockPickedCoords == null)
                DestroyOtherBlockPickedCoords = new HashSet<Vector2Int>();
            DestroyOtherBlockPickedCoords.Add(coord);
        }

        public void AddFilterPickUId(int UId)
        {
            if(FilterPickDestroyUId == null)
                FilterPickDestroyUId = new HashSet<int>();
            FilterPickDestroyUId.Add(UId);
        }
        
        public void Clear()
        {
            UsingItemId = -1;
            FilterElementId = -1;
            CollectDropId = -1;
            GenSquareElementId = -1;
            IsColorBallClearAll = false;
            HasDestroyFireElement = false;
            IsCombineSpecialElement = false;
            IsBurningCollectItemCanRelease = false;
            IsAutoReleaseBomb = false;
            WillDelCoords?.Clear();
            EffGridCoords?.Clear();
            InfectElements?.Clear();
            DestroyedElements?.Clear();
            GenSpecialInfos?.Clear();
            CurrentEffectId?.Clear();
            // CalAddedDelTargets?.Clear();
            // PendingDelCoords?.Clear();
            ExtraDelGridCoords?.Clear();
            AllDelGridCoord?.Clear();
            // BombPanelElements?.Clear();
            BallLineSpecialItems?.Clear();
            DoubleBombCoords?.Clear();
            RocketAndRocket?.Clear();
            DoubleColorBallCoords?.Clear();
            NewWaterCoords?.Clear();
            WaitElementDestroyTasks?.Clear();
            ColoredRibbonWaitDelCoords?.Clear();
            BombPanelElementCoord?.Clear();
            SpreadHoldElementDic?.Clear();
            RandomDiffuseWaitCoords?.Clear();
            ColorBallFilterIds?.Clear();
            DestroyOtherBlockPickedCoords?.Clear();
            LinkSpecialBallCoord = new Vector2Int(-1, -1);
            SpecialElementDelFilterCoords?.Clear();
            IsCalculateCoinState = false;
            FilterPickDestroyUId?.Clear();
        }
    }
}