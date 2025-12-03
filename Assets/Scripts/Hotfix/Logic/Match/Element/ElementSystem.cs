using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using GameCore.Singleton;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using JetBrains.Annotations;
using UnityEngine;
using Logger = GameCore.Log.Logger;
using Random = UnityEngine.Random;

namespace HotfixLogic.Match
{
    public class ElementSystem : LazySingleton<ElementSystem>
    {
        // 别名法数据结构
        private int[] _alias;
        private float[] _prob;

        private LevelData _levelData;
        public LevelData CurLevelData => _levelData;
        private Dictionary<Vector2Int, List<ElementBase>> _gridElements = null; //棋盘上格子元素
        private List<LevelDropElementInfo> _dropElements = null; //掉落障碍物的元素信息
        private List<GridElementInfo> _configElementInfos = null; //配置的棋盘元素信息
        private Dictionary<int, Color> _elementColorMap = null;
        public Dictionary<int, Color> ElementColorMap => _elementColorMap;
        private List<int> _levelUsingElements = null;
        private ElementMapDB _elementMapDB;
        private Dictionary<ElementType, int> _specialElements = new Dictionary<ElementType, int>(4)
        {
            [ElementType.Rocket] = (int)ElementIdConst.Rocket,
            [ElementType.Bomb] = (int)ElementIdConst.Bomb,
            [ElementType.ColorBall] = (int)ElementIdConst.ColorBall,
            [ElementType.RocketHorizontal] = (int)ElementIdConst.RocketHorizontal,
        };
        
        public int LightBlockInitId { get; set; }

        private int _perFrameBatchElement = 10;
        
        /// <summary>
        /// 当前棋盘上所有的格子信息
        /// </summary>
        public Dictionary<Vector2Int, List<ElementBase>> GridElements => _gridElements;

        public Transform PoolRoot { private set; get; }

        private static bool _isDynamicChangedColor;
        public static string MatchBgColor = "";
        
        protected override void OnInitialized()
        {
            G.EventModule.AddEventListener<EventOneParam<BoardColorStruck>>(GameEventDefine.OnOkChangeBoardColor,OnOkChangeBoardColor,this);
            
            DeviceLevel level = CommonUtil.GetDeviceLevel();
            Logger.Debug($"当前设备等级为：{level}");
            if(level == DeviceLevel.High)
                _perFrameBatchElement = 15;
            else if(level == DeviceLevel.Middle)
                _perFrameBatchElement = 7;
            else
                _perFrameBatchElement = 5;
        }

        private void OnOkChangeBoardColor(EventOneParam<BoardColorStruck> obj)
        {
            _isDynamicChangedColor = true;
            MatchBgColor = $"#{ColorUtility.ToHtmlStringRGB(obj.Arg.BgColor).ToLower()}";
            BoardColorStruck data = obj.Arg;
            _elementColorMap[1] = data.Green;
            _elementColorMap[2] = data.Blue;
            _elementColorMap[3] = data.Yellow;
            _elementColorMap[4] = data.Red;
            _elementColorMap[5] = data.Purple;
            _elementColorMap[6] = data.Orange;
            _elementColorMap[7] = data.Cycan;
            foreach (var elements in _gridElements.Values)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i] is BaseElementItem elementItem)
                    {
                        elementItem.SetElementColor();
                    }
                }
            }
        }

        public async UniTask Initialize(Transform poolRoot, LevelData levelData)
        {
            _elementMapDB = ConfigMemoryPool.Get<ElementMapDB>();
            _levelData = levelData;
            PoolRoot = poolRoot;
            if (!_isDynamicChangedColor)
                BuildElementColorMap();

            LightBlockInitId = levelData.initColor[Random.Range(0, levelData.initColor.Length)];
            
            _levelUsingElements ??= new List<int>();
            _levelUsingElements.Clear();
            _dropElements ??= new List<LevelDropElementInfo>();
            _dropElements.Clear();
            _configElementInfos ??= new List<GridElementInfo>();
            _configElementInfos.Clear();

            for (int i = 0; i < _levelData.target.Length; i++)
            {
                int id = _levelData.target[i].targetId;
                if (_elementMapDB[id].elementType == ElementType.DropBlock ||
                    _elementMapDB[id].elementType == ElementType.Collect ||
                    _elementMapDB[id].elementType == ElementType.JumpCollect)
                {
                    if (_dropElements.FindIndex(x => x.ElementId == id) < 0)
                    {
                        _dropElements.Add(new LevelDropElementInfo()
                        {
                            DropCount = _levelData.target[i].targetNum,
                            ElementId = id,
                            ElementType = _elementMapDB[id].elementType,
                            DropPosition = new List<Vector2Int>()
                        });
                    }
                }
            }
            
            for (int i = 0; i < _levelData.initColor.Length; i++)
            {
                BuildLevelUsingElement(_levelData.initColor[i], ref _levelUsingElements);
            }

            for (int i = 0; i < _levelData.dropColor.Length; i++)
            {
                BuildLevelUsingElement(_levelData.dropColor[i], ref _levelUsingElements);
            }

            for (int i = 0; i < _levelData.target.Length; i++)
            {
                BuildLevelUsingElement(_levelData.target[i].targetId, ref _levelUsingElements);
            }

            var gridEle = _levelData.BuildElementHoldGridMap();
            for (int i = 0; i < gridEle.Count; i++)
            {
                BuildLevelUsingElement(gridEle[i].ElementId, ref _levelUsingElements);
                int index = -1;
                if (_elementMapDB[gridEle[i].ElementId].elementType == ElementType.VerticalExpand ||
                    _elementMapDB[gridEle[i].ElementId].elementType == ElementType.TargetBlock ||
                    _elementMapDB[gridEle[i].ElementId].elementType == ElementType.FixPosExpand ||
                    _elementMapDB[gridEle[i].ElementId].elementType == ElementType.FixedGridTargetBlock ||
                    _elementMapDB[gridEle[i].ElementId].elementType == ElementType.ColoredRibbonBlock)
                {
                    index = _configElementInfos.FindIndex(x => x.Coord == gridEle[i].StartCoord);
                }
                else
                {
                    index = _configElementInfos.FindIndex(x => x.ElementId == gridEle[i].ElementId);
                }

                List<int> nextEleIds = new List<int>();
                _elementMapDB.RefElementNextList(gridEle[i].ElementId, ref nextEleIds);
                if (index < 0)
                {
                    _configElementInfos.Add(new GridElementInfo()
                    {
                        ElementId = gridEle[i].ElementId,
                        Count = 1,
                        Coord = gridEle[i].StartCoord,
                        ElementHeight = gridEle[i].ElementHeight,
                        ElementWidth = gridEle[i].ElementWidth,
                        PreLinkId = 0,
                        IsConfigElement = true,
                        NextElementIds = nextEleIds
                    });
                    //随机扩撒出来的元素有可能是掉落收集物，这时候掉落收集物不需要再新生成掉落
                    for (int j = 0; j < nextEleIds.Count; j++)
                    {
                        if (_elementMapDB[nextEleIds[j]].elementType == ElementType.RandomDiffuse)
                        {
                            //解析掉落元素
                            if (!string.IsNullOrEmpty(_elementMapDB[nextEleIds[j]].extra))
                            {
                                string[] diffuseArgs = _elementMapDB[nextEleIds[j]].extra.Split("|");
                                if (int.TryParse(diffuseArgs[0], out int diffuseEleId))
                                {
                                    int dropDiffuseIndex = _dropElements.FindIndex(x => x.ElementId == diffuseEleId);
                                    if (dropDiffuseIndex >= 0)
                                    {
                                        _dropElements.RemoveAt(dropDiffuseIndex);
                                    }
                                    if(!_levelUsingElements.Contains(diffuseEleId))
                                        _levelUsingElements.Add(diffuseEleId);
                                }
                            }
                        }
                    }
                }
                else
                {
                    var info = _configElementInfos[index];
                    info.Count++;
                    _configElementInfos[index] = info;
                }

                int dropInfoIndex = _dropElements.FindIndex(x => x.ElementId == gridEle[i].ElementId);
                if (dropInfoIndex < 0)
                {
                    dropInfoIndex = _dropElements.FindIndex(x => nextEleIds.Contains(x.ElementId));
                }

                if (dropInfoIndex >= 0)
                {
                    _dropElements[dropInfoIndex].DropPosition.Add(gridEle[i].StartCoord);
                }
            }
            
            FixConfigElementCount();

            for (int i = 0; i < _configElementInfos.Count; i++)
            {
                if (_elementMapDB[_configElementInfos[i].ElementId].elementType == ElementType.DropBlock ||
                    _elementMapDB[_configElementInfos[i].ElementId].elementType == ElementType.Collect ||
                    _elementMapDB[_configElementInfos[i].ElementId].elementType == ElementType.JumpCollect)
                {
                    BuildDropInfo(_configElementInfos[i]);
                }
            }
            
            //配置固定掉落
            if (_levelData.dropFlags != null)
            {
                for (int i = 0; i < _levelData.dropFlags.Count; i++)
                {
                    var dropFlag = _levelData.dropFlags[i];
                    if (dropFlag.dropElements != null && dropFlag.dropElements.Count > 0)
                    {
                        for (int j = 0; j < dropFlag.dropElements.Count; j++)
                        {
                            DropFlagElement dropElement = dropFlag.dropElements[j];
                            LevelDropElementInfo dropInfo = new LevelDropElementInfo()
                            {
                                ElementId = dropElement.elementId,
                                ElementType = _elementMapDB[dropElement.elementId].elementType,
                                DropCount = dropElement.dropLimitMax,
                                DropPosition = new List<Vector2Int>() { new Vector2Int(dropFlag.dropX, 0) },
                            };
                            _dropElements.Add(dropInfo);
                        }
                    }
                }
            }

            if (_dropElements.Count > 0)
            {
                for (int i = 0; i < _dropElements.Count; i++)
                {
                    _dropElements[i].DropPosition.Sort((a, b) =>
                    {
                        if (a.x == b.x)
                        {
                            if (a.y > b.y)
                                return 1;
                            else if (a.y < b.y)
                                return -1;
                            return 0;
                        }

                        return 0;
                    });
                }
            }

            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            foreach (var specialId in _specialElements.Values)
            {
                if (!_levelUsingElements.Contains(specialId))
                    _levelUsingElements.Add(specialId);
            }

            MatchEffectManager.Instance.ClearCacheKey();
            var tasks = new List<UniTask>();
            for (int i = 0; i < _levelUsingElements.Count; i++)
            {
                ElementType elementType =  db[_levelUsingElements[i]].elementType;
                int elementId =  db[_levelUsingElements[i]].Id;
                int warmCount = MatchConst.BlockElementWarmCount;
                int poolMax = MatchConst.BlockElementMaxCount;
                if (elementType == ElementType.Normal)
                {
                    warmCount = MatchConst.NormalElementWarmCount;
                    poolMax = MatchConst.NormalElementMaxCount;
                }

                if (IsSpecialElement(elementType))
                {
                    warmCount = MatchConst.SpecialElementWarmCount;
                    poolMax = MatchConst.SpecialElementMaxCount;
                }

                tasks.Add(ElementObjectPool.Instance.CreatePool($"Element-{elementId}",
                    $"{MatchConst.ElementAddressBase}/{ db[_levelUsingElements[i]].address}",
                    null, warmCount, poolMax));
                tasks.Add(MatchEffectManager.Instance.PrewarmElementEffects(elementId,warmCount,poolMax));
            }
            tasks.Add(MatchEffectManager.Instance.PrewarmEffects());

            await UniTask.WhenAll(tasks);
        }

        public void AddGridElement(GridItem grid, ElementBase element, bool needSort = true)
        {
            _gridElements ??= new Dictionary<Vector2Int, List<ElementBase>>();
            if (!_gridElements.ContainsKey(grid.Data.Coord))
                _gridElements.Add(grid.Data.Coord, new List<ElementBase>());

            _gridElements[grid.Data.Coord].Add(element);
            ValidateManager.Instance.AddSideElement(element);
            if (needSort)
            {
                if (_gridElements[grid.Data.Coord].Count > 1)
                {
                    _gridElements[grid.Data.Coord].Sort((a, b) =>
                    {
                        if (a.Data.ElementType == ElementType.Background)
                            return -1;
                        if (b.Data.ElementType == ElementType.Background)
                            return 1;
                        if (a.Data.SortOrder < b.Data.SortOrder) return -1;
                        if (a.Data.SortOrder > b.Data.SortOrder) return 1;

                        if (a.Data.Priority > b.Data.Priority) return -1;
                        if (a.Data.Priority < b.Data.Priority) return 1;

                        return 0;
                    });
                }
            }
        }

        public void RemoveGridElement(GridItem grid, ElementBase element)
        {
            if (_gridElements.TryGetValue(grid.Data.Coord, out var elements))
            {
                int index = -1;
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Data.UId == element.Data.UId)
                    {
                        index = i;
                        break;
                    }
                }
                if (index != -1)
                {
                    if (elements[index].Data.EliminateStyle == EliminateStyle.Side)
                    {
                        ValidateManager.Instance.RemoveSideElement(elements[index].Data, grid.Data.Coord);
                    }

                    _levelData.RemoveHoldGridElement(grid.Data.Coord, element.Data);

                    elements.RemoveAt(index);
                }
            }
        }

        public int GetConfigIdByUId(Vector2Int coord,int uid)
        {
            if (_gridElements.TryGetValue(coord, out var elements))
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Data.UId == uid)
                    {
                        return elements[i].Data.ConfigId;
                    }
                }
            }
            return -1;
        }
        
        public void RemoveHoldGridElementCoord(Vector2Int coord, ElementItemData elementData)
        {
            if (_levelData != null)
            {
                _levelData.RemoveHoldGridElementCoord(coord, elementData);
            }
        }

        /// <summary>
        /// 是否需要转换元素目标
        /// </summary>
        /// <returns></returns>
        public bool IsNeedParseTarget(int elementId, out int targetId)
        {
            ref readonly ElementMap config = ref _elementMapDB[elementId];
            if ((config.elementType == ElementType.ColorBlock ||
                 config.elementType == ElementType.ColorBlockPlus ||
                 config.elementType == ElementType.VerticalExpand ||
                 config.elementType == ElementType.FixPosExpand ||
                 config.elementType == ElementType.ColoredLightBlock ||
                 config.elementType == ElementType.ColoredRibbonBlock)
                && !string.IsNullOrEmpty(config.extra))
            {
                int.TryParse(config.extra, out targetId);
                return true;
            }

            targetId = elementId;
            return false;
        }

        /// <summary>
        /// 是否需要等待消除的元素
        /// </summary>
        /// <returns></returns>
        public bool IsNeedPendingDelElement(ElementType elementType)
        {
            return elementType == ElementType.RandomDiffuse ||
                   elementType == ElementType.SpreadGround ||
                   elementType == ElementType.SpreadParterre ||
                   elementType == ElementType.BombBlock ||
                   elementType == ElementType.DestroyBlock;
        }

        public bool IsBombBlockElement(int elementId)
        {
            ref readonly ElementMap config = ref _elementMapDB[elementId];
            return config.elementType == ElementType.BombBlock;
        }

        /// <summary>
        /// 随机获取棋盘上基础元素
        /// </summary>
        /// <returns></returns>
        public int RandomPickBoardBaseElementId(HashSet<int> filterIds = null)
        {
            Dictionary<int, int> baseElementCountMap = new();
            foreach (var elements in _gridElements.Values)
            {
                var baseElement = elements.Find(x => x.Data.ElementType == ElementType.Normal);
                if (baseElement != null)
                {
                    bool haveLock = HaveOverElementLock(baseElement.Data.GridPos, baseElement.Data.ConfigId,out var _);
                    if (!haveLock)
                    {
                        if (filterIds != null && filterIds.Contains(baseElement.Data.ConfigId))
                        {
                            continue;
                        }

                        baseElementCountMap.TryAdd(baseElement.Data.ConfigId, 0);
                        baseElementCountMap[baseElement.Data.ConfigId]++;
                    }
                }
            }

            int maxId = 0;
            int maxCount = -1;
            foreach (var kv in baseElementCountMap)
            {
                if (kv.Value > maxCount)
                {
                    maxId = kv.Key;
                    maxCount = kv.Value;
                }
            }

            if (maxCount <= 0)
                maxId = -1;

            Logger.Debug($"当前棋盘上的最大元素id: {maxId}，数量: {maxCount}");
            return maxId;
        }

        /// <summary>
        /// 根据指定坐标获取该坐标上的所有元素
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="realCoord"></param>
        /// <returns></returns>
        public List<ElementBase> GetGridElements(Vector2Int coord, bool realCoord)
        {
            if (_gridElements == null)
                return null;
            
            bool ContainsElement(List<ElementBase> eles, int uId)
            {
                foreach (var element in eles)
                {
                    if (element.Data.UId == uId)
                        return true;
                }
                return false;
            }
            
            if (_gridElements.TryGetValue(coord, out var elements))
            {
                if (realCoord)
                    return elements;

                var configElements = GetConfigGridElements(coord, false);
                if(configElements == null || configElements.Count <= 0)
                    return elements;

                List<ElementBase> mergedList = new List<ElementBase>(elements.Count + configElements.Count);
                for (int i = 0; i < elements.Count; i++)
                {
                    mergedList.Add(elements[i]);
                }
                foreach (var configElement in configElements)
                {
                    if (!ContainsElement(elements, configElement.Data.UId))
                    {
                        mergedList.Add(configElement);
                    }
                }

                return mergedList;
            }
            
            return realCoord ? null : GetConfigGridElements(coord, false);
        }

        private List<ElementBase> GetConfigGridElements(Vector2Int coord, bool realCoord = true)
        {
            var info = FindCoordHoldGridInfo(coord.x, coord.y, realCoord);
            if (info != null && info.Count > 0)
            {
                List<ElementBase> elements = new List<ElementBase>();
                for (int i = 0; i < info.Count; i++)
                {
                    if (_gridElements.TryGetValue(info[i].StartCoord, out var l))
                    {
                        for (int j = 0; j < l.Count; j++)
                        {
                            if (l[j].Data.ConfigId == info[i].ElementId && info[i].AllHoldGridPos.Contains(coord))
                            {
                                elements.Add(l[j]);
                                break;
                            }
                        }
                    }
                }

                return elements;
            }

            return null;
        }

        /// <summary>
        /// 获取功能棋子配置id
        /// </summary>
        /// <returns></returns>
        public int GetSpecialElementConfigId(ElementType type)
        {
            return _specialElements.GetValueOrDefault(type, -1);
        }

        /// <summary>
        /// 是否有其它元素阻挡着指定元素
        /// </summary>
        /// <returns></returns>
        public bool HaveOverElementLock(Vector2Int coord, int checkEleId,out int lockEleId,bool isCheckBlock = true,Func<ElementBase,bool> skipBlockCheck = null)
        {
            lockEleId = -1;
            var elements = GetGridElements(coord, false);
            if (elements == null || elements.Count == 0)
                return false;
            if (elements.Count == 1 && elements[0].Data.ConfigId == checkEleId)
                return false;

            return HaveOverElementLock(elements, checkEleId,out lockEleId,isCheckBlock,skipBlockCheck);
        }

        public bool HaveOverElementLock(List<ElementBase> elements, int checkEleId,out int lockEleId,bool isCheckBlock = true,Func<ElementBase,bool> skipBlockCheck = null)
        {
            bool bResult = false;
            lockEleId = -1;
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                if (elements[i].Data.ConfigId == checkEleId)
                    continue;

                if (skipBlockCheck != null && skipBlockCheck(elements[i]))
                    continue;
                if (isCheckBlock)
                {
                    if (elements[i].Data.ElementType != ElementType.Normal &&
                        !IsSpecialElement(elements[i].Data.ElementType) &&
                        elements[i].Data.HoldGrid > 1)
                    {
                        lockEleId = elements[i].Data.ConfigId;
                        bResult = true;
                        break;
                    }
                }

                if (elements[i].Data.ElementType == ElementType.TargetBlock ||
                    elements[i].Data.ElementType == ElementType.Lock)
                {
                    lockEleId = elements[i].Data.ConfigId;
                    bResult = true;
                    break;
                }
            }

            return bResult;
        }

        /// <summary>
        /// 根据格子坐标找出障碍物的信息
        /// </summary>
        public List<GridHoldInfo> FindCoordHoldGridInfo(int x, int y, bool realCoord = true)
        {
            if (_levelData == null)
                return null;

            return _levelData.FindCoordHoldGridInfo(x, y, realCoord: realCoord);
        }

        public void Restart()
        {
            foreach (var elements in _gridElements.Values)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    ElementObjectPool.Instance.Recycle(elements[i].GameObject);
                }
            }

            _gridElements?.Clear();
            _configElementInfos?.Clear();
        }

        public async UniTask ClearGridElements()
        {
            if (_gridElements != null)
            {
                foreach (var elements in _gridElements.Values)
                {
                    for (int i = 0; i < elements.Count; i++)
                    {
                        elements[i].AfterDestroy(null, true).Forget();
                    }
                }
            }
            _gridElements?.Clear();
            _configElementInfos?.Clear();

        }

        /// <summary>
        /// 获取元素对象
        /// </summary>
        /// <param name="data"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public ElementBase GenElement(ElementItemData data, Transform parent = null)
        {
            if (parent == null)
                parent = PoolRoot;

            var element = AcqByElementData(data);
            element.Initialize(parent, data);
            return element;
        }

        /// <summary>
        /// 列表是否有可以操作的基础棋子
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="index"></param>
        /// <param name="includeSpecial"></param>
        /// <returns></returns>
        public bool TryGetBaseElement(List<ElementBase> elements, out int index, bool includeSpecial = false)
        {
            index = -1;
            bool haveLock = false;
            if (elements != null)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Data.ElementType == ElementType.Lock)
                        haveLock = true;
                    if (elements[i].Data.ElementType == ElementType.TargetBlock)
                        haveLock = true;
                    if (elements[i].Data.ElementType == ElementType.Normal ||
                        (includeSpecial && IsSpecialElement(elements[i].Data.ElementType)))
                        index = i;
                }
            }

            return !haveLock && index >= 0;
        }

        public bool TryGetWillDestroyElementItem(List<ElementBase> elements, out int index)
        {
            index = 0;
            if(elements == null || elements.Count == 0)
                return false;
            
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                if (elements[i].Data.EliminateStyle == EliminateStyle.Drop ||
                    elements[i].Data.EliminateStyle == EliminateStyle.Target)
                {
                    return false;
                }
            }

            for (int i = elements.Count - 1; i >= 0; i--)
            {
                if (elements[i].Data.HoldGrid >= 1)
                {
                    index = i;
                    return true;
                }
            }
            
            return true;
        }

        /// <summary>
        /// 传入列表是否可以消除
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public bool IsCanAttachElements(List<ElementBase> elements)
        {
            if (elements == null || elements.Count <= 0)
                return false;
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Data.EliminateStyle == EliminateStyle.Target)
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// 生成元素数据
        /// </summary>
        /// <returns></returns>
        public ElementItemData GenElementItemData(int elementId, int x, int y)
        {
            ElementMapDB configDB = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref configDB[elementId];
            var ele = MemoryPool.Acquire<ElementItemData>();
            ele.UId = MatchManager.Instance.GenerateElementId();
            ele.ConfigId = config.Id;
            ele.ElementType = config.elementType;
            ele.EliminateCount = config.eliminateCount;
            ele.EliminateStyle = config.eliminateStyle;
            ele.HoldGrid = config.holdGrid;
            ele.Direction = config.direction;
            ele.NextBlockId = config.nextBlock;
            ele.Priority = 0;
            ele.Extra = config.extra;
            ele.SortOrder = config.sortOrder;
            ele.IsMovable = config.isMovable;
            if (elementId == (int)ElementIdConst.ColorBall)
                ele.Priority = 5;
            else if (elementId == (int)ElementIdConst.Bomb)
                ele.Priority = 4;
            else if (elementId == (int)ElementIdConst.Rocket)
                ele.Priority = 3;
            else if (elementId == (int)ElementIdConst.RocketHorizontal)
                ele.Priority = 2;
            if (config.effEleIds != null)
            {
                ele.EffElementIds ??= new List<int>();
                ele.EffElementIds.Clear();
                for (int i = 0; i < config.effEleIds.Count; i++)
                {
                    ele.EffElementIds.Add(config.effEleIds[i]);
                }

                if (ele.EffElementIds.Count != config.eliminateCount)
                {
                    int currentCount = ele.EffElementIds.Count;
                    for (int i = 0; i < config.eliminateCount - currentCount; i++)
                    {
                        if (config.effEleIds.Count <= 1)
                        {
                            ele.EffElementIds.Add(config.effEleIds[0]);
                        }
                        else
                        {
                            if (i < config.effEleIds.Count)
                                ele.EffElementIds.Add(config.effEleIds[i]);
                        }
                    }
                }
            }

            ele.UpdatePos(x, y);
            return ele;
        }

        /// <summary>
        /// 批量生成对象
        /// </summary>
        /// <param name="elementsData">元素对象数据</param>
        /// <param name="parent">对象附加的父节点</param>
        /// <returns></returns>
        public async UniTask<ElementBase[]> BatchGenElements(IList<ElementItemData> elementsData,
            Transform parent = null)
        {
            // int loadedCount = 0;
            var results = new ElementBase[elementsData.Count];
            for (int i = 0; i < elementsData.Count; i++)
            {
                results[i] = GenElement(elementsData[i], parent);
                // loadedCount++;
                // if (loadedCount % _perFrameBatchElement == 0)
                // {
                //     await UniTask.Yield();
                // }
            }

            return results;
        }

        /// <summary>
        /// 根据传入的概率选中其中的某个元素
        /// </summary>
        /// <param name="baseElements">元素id列表</param>
        /// <param name="elementsRate">对应的id挑选概率</param>
        /// <param name="pickCount">掉落个数</param>
        /// <param name="dropX">掉落的X轴位置</param>
        /// <param name="isInit">是否是初始化生成的</param>
        /// <param name="collectId">掉落障碍物id</param>
        /// <param name="checkTargets"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public int[] PickElementDynamicAdjustment(int[] baseElements, int[] elementsRate, int pickCount, int dropX,
            bool isInit = false, int collectId = -1,Dictionary<int,int> checkTargets = null)
        {
            int[] results = new int[pickCount]; 
            int emptyIndex = -1;
            AdjustDropElements(in baseElements, in elementsRate, out int[] finalElements, out int[] finalElementsRate);
            if (!isInit)
            {
                PickDropElement(dropX, pickCount, ref results, collectId);
                bool notEmpty = true;
                for (int i = 0; i < pickCount; i++)
                {
                    if (results[i] <= 0)
                    {
                        emptyIndex = i;
                        notEmpty = false;
                        break;
                    }
                }

                //已经补满掉落障碍物，不需要再执行下面补充棋子了
                if (notEmpty)
                {
                    return results;
                }
            }

            // 基础校验
            if (finalElements == null || finalElements.Length == 0)
            {
                throw new ArgumentException("元素列表不能为空");
            }

            // 转换万分比概率
            float[] probabilities = ProcessProbabilities(finalElements.Length, finalElementsRate);

            // 执行别名法预处理
            AliasMethodPreprocess(probabilities);

            List<int> notTargetsElement = new List<int>(finalElements.Length);
            List<int> notTargetsElementRate = new List<int>(finalElementsRate.Length);
            if (checkTargets != null)
            {
                for (int i = 0; i < finalElements.Length; i++)
                {
                    if (!checkTargets.ContainsKey(finalElements[i]))
                    {
                        notTargetsElement.Add(finalElements[i]);
                        notTargetsElementRate.Add(finalElementsRate[i]);
                    }
                }
            }

            emptyIndex = Mathf.Max(0, emptyIndex);
            for (int i = emptyIndex; i < pickCount; i++)
            {
                int idx = UnityEngine.Random.Range(0, _prob.Length);
                bool coinFlip = UnityEngine.Random.value < _prob[idx];
             
                int pickedElement = coinFlip ? finalElements[idx] : finalElements[_alias[idx]];
                //动态检查收集目标，避免单次掉落数量过多时，导致补充的元素也被多填入掉落中
                if (_elementMapDB[pickedElement].elementType != ElementType.Normal && checkTargets != null && checkTargets.TryGetValue(pickedElement, out var remainCount))
                {
                    int boardCount = GetBoardElementCount(pickedElement);
                    if (remainCount > 0 && boardCount < remainCount)
                    {
                        checkTargets[pickedElement] -= 1;
                        Logger.Debug($"补充掉落元素：{pickedElement}，当前剩余数量：{remainCount}");
                        results[i] = pickedElement;
                    }
                    else
                    {     
                        //从notTargetsElement里面按照notTargetsElementRate的概率重新选
                        if (notTargetsElement.Count > 0)
                        {
                            // 构建非目标元素的概率表
                            float[] notTargetProbabilities = ProcessProbabilities(notTargetsElement.Count, notTargetsElementRate.ToArray());
                            
                            // 执行别名法预处理
                            int[] notTargetAlias;
                            float[] notTargetProb;
                            AliasMethodPreprocessForList(notTargetProbabilities, out notTargetProb, out notTargetAlias);
                            
                            // 使用别名法选择一个非目标元素
                            int notTargetIdx = UnityEngine.Random.Range(0, notTargetProb.Length);
                            bool notTargetCoinFlip = UnityEngine.Random.value < notTargetProb[notTargetIdx];
                            int notTargetPickedElement = notTargetCoinFlip ? notTargetsElement[notTargetIdx] : notTargetsElement[notTargetAlias[notTargetIdx]];
                            
                            results[i] = notTargetPickedElement;
                            Logger.Debug($"补充掉落元素：{notTargetPickedElement}");
                        }
                        else
                        {
                            // 如果没有非目标元素，则使用原来的元素
                            results[i] = pickedElement;
                        }
                    }
                }
                else
                {
                    results[i] = pickedElement;
                }
            }

            return results;
        }

        private void PickDropElement(int dropX, int pickCount, ref int[] result, int collectId = -1)
        {
            if (_dropElements == null || _dropElements.Count == 0)
                return;

            int dropElementIndex = -1;
            LevelDropElementInfo info = default;
            if (collectId > 0)
            {
                //优先看有没有掉落收集的元素
                dropElementIndex = _dropElements.FindIndex(x => x.ElementId == collectId);
                if (dropElementIndex >= 0)
                    info = _dropElements[dropElementIndex];
            }
            else
            {
                for (int i = 0; i < _dropElements.Count; i++)
                {
                    for (int j = 0; j < _dropElements[i].DropPosition.Count; j++)
                    {
                        if ((_dropElements[i].ElementType != ElementType.Collect || _dropElements[i].ElementType != ElementType.JumpCollect) &&
                            _dropElements[i].DropPosition[j].x == dropX)
                        {
                            info = _dropElements[i];
                            dropElementIndex = i;
                            break;
                        }
                    }

                    if (dropElementIndex >= 0)
                        break;
                }
            }

            if (dropElementIndex < 0)
                return;

            int pickElementId = info.ElementId;
            //收集目标有可能是由其它棋子演变而成，但是掉落需要掉最原始的那个元素，这里回溯找出原始元素
            while (_configElementInfos.FindIndex(x => x.ElementId == pickElementId) >= 0)
            {
                int preLinkId = _configElementInfos.Find(x => x.ElementId == pickElementId).PreLinkId;
                if (preLinkId <= 0)
                    break;
                pickElementId = preLinkId;
            }

            if (info.DropCount < 5 || collectId != -1)
            {
                //需求暂时是小于5个时 单列只会掉一个
                result[0] = pickElementId;
                info.DropCount -= 1;
            }
            else
            {
                int pick = info.DropCount > pickCount ? pickCount : info.DropCount;
                for (int i = 0; i < pick; i++)
                {
                    result[i] = pickElementId;
                }

                info.DropCount = Mathf.Max(0, info.DropCount - pickCount);
            }

            _dropElements[dropElementIndex] = info;
            if (info.DropCount <= 0)
            {
                _dropElements.RemoveAt(dropElementIndex);
            }
        }

        private void AdjustDropElements(in int[] baseElements, in int[] elementsRate, out int[] finalElements,
            out int[] finalElementsRate)
        {
            List<int> notBaseElementIndex = new List<int>(baseElements.Length);
            List<int> finalElementsList = new List<int>(baseElements.Length);
            List<int> finalElementsRateList = new List<int>(baseElements.Length);
            for (int i = 0; i < baseElements.Length; i++)
            {
                if (_elementMapDB[baseElements[i]].elementType != ElementType.Normal &&
                    !IsSpecialElement(_elementMapDB[baseElements[i]].elementType))
                {
                    notBaseElementIndex.Add(i);
                }
                else
                {
                    finalElementsList.Add(baseElements[i]);
                    finalElementsRateList.Add(elementsRate[i]);
                }
            }
            if (notBaseElementIndex.Count <= 0)
            {
                finalElements = baseElements;
                finalElementsRate = elementsRate;
                return;
            }

            //不是基础棋子的掉落，看看有没有收集目标
            var targets = LevelTargetSystem.Instance.TargetElements;
            for (int i = 0; i < notBaseElementIndex.Count; i++)
            {
                int id = baseElements[notBaseElementIndex[i]];
                if (targets.TryGetValue(id, out var remainCount))
                {
                    if (remainCount > 0)
                    {
                        //仍有需要的收集目标 检查棋盘上存在的数量与剩余数量
                        int count = GetBoardElementCount(id);
                        if (count < remainCount)
                        {
                            finalElementsList.Add(id);
                            finalElementsRateList.Add(elementsRate[notBaseElementIndex[i]]);
                        }
                    }
                }
            }
            finalElements = finalElementsList.ToArray();
            finalElementsRate = finalElementsRateList.ToArray();
        }

        private float[] ProcessProbabilities(int elementCount, int[] rate)
        {
            float[] prob = new float[elementCount];
            long totalRate = 0;

            // 处理空概率情况
            if (rate == null || rate.Length == 0)
            {
                float avg = 1f / elementCount;
                Array.Fill(prob, avg);
                return prob;
            }

            // 计算总权重
            for (int i = 0; i < Mathf.Min(rate.Length, elementCount); i++)
            {
                totalRate += Mathf.Clamp(rate[i], 0, 10000);
            }

            // 归一化处理
            float total = totalRate / 10000f;
            float remaining = 1f - total;

            for (int i = 0; i < elementCount; i++)
            {
                if (i < rate.Length)
                {
                    float p = Mathf.Clamp(rate[i], 0, 10000) / 10000f;
                    prob[i] = p + remaining / elementCount;
                }
                else
                {
                    prob[i] = remaining / elementCount;
                }
            }

            // 二次归一化确保总和为1
            float sum = 0;
            foreach (var p in prob) sum += p;
            for (int i = 0; i < prob.Length; i++) prob[i] /= sum;

            return prob;
        }

        /// <summary>
        /// 随机从列表中获取有普通棋子的格子坐标
        /// </summary>
        /// <returns></returns>
        public Vector2Int RandomPickBaseElement(List<Vector2Int> filterList, out bool success, int filterId = 0,
            Func<ElementBase, bool> condition = null)
        {
            var validCoords = GetValidCoordsVecs(filterList, filterId, condition);

            if (validCoords.Count == 0)
            {
                success = false;
                return Vector2Int.zero;
            }

            success = true;
            int randomIndex = Random.Range(0, validCoords.Count);
            return validCoords[randomIndex];
        }

        public List<Vector2Int> GetValidCoordsVecs(List<Vector2Int> filterList, int filterId = 0,
            Func<ElementBase, bool> condition = null)
        {
            var validCoords = new List<Vector2Int>(_gridElements.Count);

            Vector2Int up = new Vector2Int(0, -1);
            foreach (var kvp in _gridElements)
            {
                Vector2Int coord = kvp.Key;
                var upElements = GetGridElements(coord + up, true);
                if (upElements is { Count: > 0 })
                {
                    if (upElements.FindIndex(x => x is CollectElementItem) >= 0)
                        continue;
                }

                var elements = kvp.Value;

                if (TryGetBaseElement(elements, out int index))
                {
                    var element = elements[index];
                    if (element.Data.ElementType != ElementType.Normal)
                        continue;
                    if (filterId != 0 && element.Data.ConfigId == filterId)
                        continue;
                    if (condition != null && !condition(element))
                        continue;
                    if (filterList != null && filterList.FindIndex(x => x == coord) < 0)
                    {
                        validCoords.Add(coord);
                    }
                }
            }

            return validCoords;
        }

        /// <summary>
        /// 获取当前棋盘上所有的指定元素
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<ElementBase> GetAllTargetElements(ElementType type)
        {
            List<ElementBase> results = new List<ElementBase>();
            foreach (var kvp in _gridElements)
            {
                var elements = kvp.Value;
                if (elements == null || elements.Count <= 0)
                    continue;
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Data.ElementType == type)
                    {
                        results.Add(elements[i]);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 获取当前棋盘上所有的指定ID元素
        /// </summary>
        /// <returns></returns>
        public List<ElementBase> GetAllElementsById(int id,HashSet<Vector2Int> filterCoords = null)
        {
            List<ElementBase> results = new List<ElementBase>();
            foreach (var kvp in _gridElements)
            {
                var elements = kvp.Value;
                if (elements == null || elements.Count <= 0)
                    continue;

                for (int i = 0; i < elements.Count; i++)
                {
                    bool haveLock = HaveOverElementLock(elements[i].Data.GridPos, elements[i].Data.ConfigId,out var _);
                    if(haveLock)
                        continue;
                    if (elements[i].Data.ConfigId == id)
                    {
                        if(filterCoords != null && filterCoords.Contains(elements[i].Data.GridPos))
                            continue;
                        results.Add(elements[i]);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 找出彩带气球障碍物配置的起始和结束节点
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="checkLine"></param>
        /// <returns></returns>
        public (int, int) FindColoredRibbonStartEndPos(ElementDirection direction,int checkLine)
        {
            int start = 99;
            int end = -1;
            if (direction != ElementDirection.None)
            {
                foreach (var elementInfo in _configElementInfos)
                {
                    if (_elementMapDB[elementInfo.ElementId].elementType != ElementType.ColoredRibbonBlock)
                        continue;
                    if(_elementMapDB[elementInfo.ElementId].direction != direction)
                        continue;
                    //纵向的需要检测x轴是否一致来确定同一列
                    if (direction == ElementDirection.Up && elementInfo.Coord.x == checkLine)
                    {
                        if (start > elementInfo.Coord.y)
                            start = elementInfo.Coord.y;
                        if (end < elementInfo.Coord.y)
                            end = elementInfo.Coord.y;
                    }

                    //横向的需要检测y轴是否一致来确定同一行
                    if (direction == ElementDirection.Right && elementInfo.Coord.y == checkLine)
                    {
                        if (start > elementInfo.Coord.x)
                            start = elementInfo.Coord.x;
                        if (end < elementInfo.Coord.x)
                            end = elementInfo.Coord.x;
                    }

                }
            }
            
            return (start, end);
        }
        
        /// <summary>
        /// 指定坐标是否含有基础元素
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public bool HasBaseElement(Vector2Int coord)
        {
            var elements = GetGridElements(coord, true);
            if (elements == null || elements.Count == 0)
                return false;
            return elements.Find(x => x.Data.ElementType == ElementType.Normal) != null;
        }

        /// <summary>
        /// 棋盘上是否存在蔓延的火元素
        /// </summary>
        /// <returns></returns>
        public bool HasFireElement(out List<ElementBase> fireElements)
        {
            bool bResult = false;
            fireElements = null;
            foreach (var element in _gridElements.Values)
            {
                int index = element.FindIndex(x => x.Data.ElementType == ElementType.SpreadFire);
                if (index >= 0)
                {
                    bResult = true;
                    fireElements ??= new List<ElementBase>();
                    fireElements.Add(element[index]);
                }
            }

            return bResult;
        }

        /// <summary>
        /// 是否是占据多格的元素id
        /// </summary>
        /// <returns></returns>
        public bool IsHoldMulGridElement(int elementId, out ElementDirection direction, out int horizontalCount,
            Vector2Int coord)
        {
            ref readonly ElementMap config = ref _elementMapDB[elementId];
            if (config.elementType == ElementType.TargetBlock)
            {
                direction = ElementDirection.Right;
                horizontalCount = 0;
                int index = _configElementInfos.FindIndex(x => x.Coord == coord);
                if (index >= 0)
                {
                    horizontalCount = _configElementInfos[index].ElementWidth;
                }

                return true;
            }

            if (config.elementType == ElementType.VerticalExpand)
            {
                direction = config.direction;
                horizontalCount = 0;
                int index = _configElementInfos.FindIndex(x => x.Coord == coord);
                if (index >= 0)
                {
                    horizontalCount = direction == ElementDirection.Up || direction == ElementDirection.Down
                        ? 0
                        : _configElementInfos[index].ElementWidth;
                }

                return true;
            }

            if (config.elementType == ElementType.FixPosExpand)
            {
                direction = config.direction;
                horizontalCount = 0;
                int index = _configElementInfos.FindIndex(x => x.Coord == coord);
                if (index >= 0)
                {
                    horizontalCount = direction == ElementDirection.Up || direction == ElementDirection.Down
                        ? 0
                        : _configElementInfos[index].ElementWidth;
                }

                return true;
            }

            direction = config.direction;
            if ((int)config.holdGrid == 4)
            {
                direction = ElementDirection.Right;
                horizontalCount = 2;
            }
            else if ((int)config.holdGrid == 9)
            {
                direction = ElementDirection.Right;
                horizontalCount = 3;
            }
            else
                horizontalCount = (int)config.holdGrid;

            return config.holdGrid > 1;
        }
        
        public void OffsetElementPosition(ElementBase item)
        {
            item.GameObject.transform.localPosition = Vector3.zero;
            if (item.Data.HoldGrid > 1)
            {
                var elementSize = GridSystem.GridSize;
                float size = elementSize.x / 2.0f;
                if (item.Data.HoldGrid >= 9)
                {
                    size = elementSize.x;
                }
                if (item.Data.Direction == ElementDirection.None)
                {
                    item.GameObject.transform.localPosition = new Vector3(size, -size);
                }
                else if (item.Data.Direction == ElementDirection.Right)
                {
                    item.GameObject.transform.localPosition = new Vector3(size, 0);
                }
                else if (item.Data.Direction == ElementDirection.Down)
                {
                    item.GameObject.transform.localPosition = new Vector3(0, -size);
                }
            }
        }

        /// <summary>
        /// 是否特殊元素
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="withoutColorBall">是否排除彩球</param>
        /// <returns></returns>
        public bool IsSpecialElement(ElementType elementType, bool withoutColorBall = false)
        {
            if (_specialElements.ContainsKey(elementType))
            {
                if (withoutColorBall && elementType == ElementType.ColorBall)
                    return false;
                return true;
            }

            return false;
        }

        public bool IsSpecialElement(int elementId)
        {
            foreach (var idValue in _specialElements.Values)
            {
                if (elementId == idValue)
                    return true;
            }

            return false;
        }

        public bool IsBombElement(int elementId)
        {
            bool bResult = IsSpecialElement(_elementMapDB[elementId].elementType, true);
            if (bResult)
                return true;
            if(_elementMapDB[elementId].elementType == ElementType.BombBlock)
                return true;
            return false;
        }

        public int GetBoardElementCount(int elementId)
        {
            if (elementId <= 0)
                return 0;
            if (_gridElements == null)
                return 0;
            int boardHaveCount = 0;
            foreach (var elements in _gridElements.Values)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elementId == elements[i].Data.ConfigId)
                    {
                        boardHaveCount++;
                    }
                }
            }
            return boardHaveCount;
        }

        public int GetBoardElementCount(List<int> elementIds)
        {
            if (_gridElements == null)
                return 0;
            int boardHaveCount = 0;
            foreach (var elements in _gridElements.Values)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elementIds.Contains(elements[i].Data.ConfigId))
                    {
                        boardHaveCount++;
                    }
                }
            }
            return boardHaveCount;
        }
        
        public int RandomLightBlockColorId()
        {
            List<int> dropIds = new List<int>(_levelData.dropColor);
            int index = dropIds.FindIndex(x => x == LightBlockInitId);
            if (index >= 0)
            {
                dropIds.RemoveAt(index);
            }
            if(dropIds.Count <= 0)
                return LightBlockInitId;

            int colorId = dropIds[Random.Range(0, dropIds.Count)];
            return colorId;
        }

        /// <summary>
        /// 是否是卷帘目标的元素
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        public bool IsTargetBlockTarget(int elementId)
        {
            foreach (var gridElement in _gridElements)
            {
                for (int i = 0; i < gridElement.Value.Count; i++)
                {
                    var element = gridElement.Value[i];
                    if(element.Data.ElementType != ElementType.TargetBlock && 
                       element.Data.ElementType != ElementType.FixedGridTargetBlock)
                        continue;
                    if (element is TargetBlockElementItem targetBlockElement)
                    {
                        int targetId = targetBlockElement.GetTargetId();
                        if (targetId == elementId)
                            return true;
                    }
                }
            }

            return false;
        }

        public Vector3 GetTargetBlockPosition(int targetEleId)
        {
            var list = GetTargetBlockPositions(targetEleId, 1);
            return list.Count > 0 ? list[0] : Vector3.zero;
        }
        
        /// <summary>
        /// 获取收集目标棋子障碍物位置
        /// </summary>
        /// <param name="targetEleId"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        public List<Vector3> GetTargetBlockPositions(int targetEleId, int totalCount)
        {
            List<Vector3> result = new List<Vector3>(5);
            bool isFull = false;
            foreach (var gridElement in _gridElements)
            {
                for (int i = 0; i < gridElement.Value.Count; i++)
                {
                    var element = gridElement.Value[i];
                    if (element is TargetBlockElementItem targetBlockElement)
                    {
                        int targetId = targetBlockElement.GetTargetId();
                        if (targetId != targetEleId)
                            break;
                        int remainCount = targetBlockElement.GetRemainCount();
                        if (remainCount >= totalCount)
                        {
                            isFull = true;
                            result.Add(targetBlockElement.GetTextNumPosition());
                            break;
                        }
                        else
                        {
                            result.Add(targetBlockElement.GetTextNumPosition());
                            totalCount -= remainCount;
                        }
                    }
                }

                if (isFull)
                    break;
            }

            return result;
        }

        public List<TargetBlockElementItem> GetTargetBlockElements(int targetEleId) {
            List<TargetBlockElementItem> result = new List<TargetBlockElementItem>();
            foreach (var gridElement in _gridElements)
            {
                for (int i = 0; i < gridElement.Value.Count; i++)
                {
                    var element = gridElement.Value[i];
                    if (element is TargetBlockElementItem targetBlockElement)
                    {
                        int targetId = targetBlockElement.GetTargetId();
                        if (targetId == targetEleId)
                        {
                            result.Add(targetBlockElement);
                        }
                    }
                }
            }
            return result;
        }

        public Color GetElementColor(int elementId)
        {
            if (_elementColorMap == null)
            {
                BuildElementColorMap();
            }
            Color color = Color.white;
            if(_elementColorMap.ContainsKey(elementId))
                color = _elementColorMap[elementId];
            return color;
        }

        private void BuildElementColorMap()
        {
            _elementColorMap ??= new Dictionary<int, Color>();
            LevelMapImageDB db = ConfigMemoryPool.Get<LevelMapImageDB>();

            var lineColorMap = db.GetLineColors(MatchManager.Instance.CurLevelID, MatchManager.Instance.MaxLevel);

            
            if (ColorUtility.TryParseHtmlString(lineColorMap[1], out var color1))
                _elementColorMap[1] = color1;
            if (ColorUtility.TryParseHtmlString(lineColorMap[2], out var color2))
                _elementColorMap[2] = color2;
            if (ColorUtility.TryParseHtmlString(lineColorMap[3], out var color3))
                _elementColorMap[3] = color3;
            if (ColorUtility.TryParseHtmlString(lineColorMap[4], out var color4))
                _elementColorMap[4] = color4;
            if (ColorUtility.TryParseHtmlString(lineColorMap[5], out var color5))
                _elementColorMap[5] = color5;
            if (ColorUtility.TryParseHtmlString(lineColorMap[6], out var color6))
                _elementColorMap[6] = color6;
            if (ColorUtility.TryParseHtmlString(lineColorMap[7], out var color7))
                _elementColorMap[7] = color7;
        }

        public void BuildLinkElementId(int elementId, List<int> elementIds)
        {
            _levelData.BuildLinkElement(elementId, elementIds);
        }

        private void AliasMethodPreprocess(float[] probabilities)
        {
            int n = probabilities.Length;
            _prob = new float[n];
            _alias = new int[n];

            Queue<int> small = new Queue<int>();
            Queue<int> large = new Queue<int>();

            // 缩放概率
            float[] scaled = new float[n];
            for (int i = 0; i < n; ++i)
            {
                scaled[i] = probabilities[i] * n;
                if (scaled[i] < 1.0f)
                {
                    small.Enqueue(i);
                }
                else
                {
                    large.Enqueue(i);
                }
            }

            // 构建别名表
            while (small.Count > 0 && large.Count > 0)
            {
                int l = small.Dequeue();
                int g = large.Dequeue();

                _prob[l] = scaled[l];
                _alias[l] = g;

                scaled[g] = (scaled[g] + scaled[l]) - 1f;
                if (scaled[g] < 1f)
                {
                    small.Enqueue(g);
                }
                else
                {
                    large.Enqueue(g);
                }
            }

            // 处理剩余概率
            while (large.Count > 0)
            {
                int g = large.Dequeue();
                _prob[g] = 1f;
            }

            while (small.Count > 0)
            {
                int l = small.Dequeue();
                _prob[l] = 1f;
            }
        }
        
        private void AliasMethodPreprocessForList(float[] probabilities, out float[] prob, out int[] alias)
        {
            int n = probabilities.Length;
            prob = new float[n];
            alias = new int[n];

            Queue<int> small = new Queue<int>();
            Queue<int> large = new Queue<int>();

            // 缩放概率
            float[] scaled = new float[n];
            for (int i = 0; i < n; ++i)
            {
                scaled[i] = probabilities[i] * n;
                if (scaled[i] < 1.0f)
                {
                    small.Enqueue(i);
                }
                else
                {
                    large.Enqueue(i);
                }
            }

            // 构建别名表
            while (small.Count > 0 && large.Count > 0)
            {
                int l = small.Dequeue();
                int g = large.Dequeue();

                prob[l] = scaled[l];
                alias[l] = g;

                scaled[g] = (scaled[g] + scaled[l]) - 1f;
                if (scaled[g] < 1f)
                {
                    small.Enqueue(g);
                }
                else
                {
                    large.Enqueue(g);
                }
            }

            // 处理剩余概率
            while (large.Count > 0)
            {
                int g = large.Dequeue();
                prob[g] = 1f;
            }

            while (small.Count > 0)
            {
                int l = small.Dequeue();
                prob[l] = 1f;
            }
        }
        
        private void BuildLevelUsingElement(int elementId, ref List<int> usingIds)
        {
            ref readonly ElementMap config = ref _elementMapDB[elementId];
            if (!usingIds.Contains(elementId))
                usingIds.Add(elementId);

            if (config.nextBlock > 0 && !usingIds.Contains(config.nextBlock))
            {
                if (config.nextBlock == elementId)
                    return;
                BuildLevelUsingElement(config.nextBlock, ref usingIds);
            }
        }

        private void FixConfigElementCount()
        {
            void FillNextBlockId(GridElementInfo info)
            {
                ElementMap config = _elementMapDB[info.ElementId];
                if (config.nextBlock > 0)
                {
                    int nextBlockInfoIndex = _configElementInfos.FindIndex(x => x.ElementId == config.nextBlock);
                    if (nextBlockInfoIndex < 0)
                    {
                        List<int> nextEleIds = new List<int>();
                        _elementMapDB.RefElementNextList(config.nextBlock, ref nextEleIds);
                        var nextInfo = new GridElementInfo()
                        {
                            ElementId = config.nextBlock,
                            Count = info.Count,
                            Coord = info.Coord,
                            ElementWidth = info.ElementWidth,
                            ElementHeight = info.ElementHeight,
                            PreLinkId = info.ElementId,
                            IsConfigElement = false,
                            NextElementIds = nextEleIds
                        };
                        _configElementInfos.Add(nextInfo);

                        if (_elementMapDB[nextInfo.ElementId].nextBlock > 0)
                            FillNextBlockId(nextInfo);
                    }
                }
            }

            //目标掉落物可能是由其它元素演化而成的，在这里修复掉落物数量
            for (int i = _configElementInfos.Count - 1; i >= 0; i--)
            {
                FillNextBlockId(_configElementInfos[i]);
            }
        }

        private void BuildDropInfo(GridElementInfo holdInfo)
        {
            int index = _dropElements.FindIndex(x => x.ElementId == holdInfo.ElementId && holdInfo.IsConfigElement);
            if (index < 0)
            {
                index = _dropElements.FindIndex(x =>
                    holdInfo.NextElementIds.Contains(x.ElementId) && holdInfo.IsConfigElement);
            }

            if (index >= 0)
            {
                LevelDropElementInfo info = _dropElements[index];
                info.DropCount -= holdInfo.Count;
                if (!info.DropPosition.Contains(holdInfo.Coord))
                    info.DropPosition.Add(holdInfo.Coord);

                _dropElements[index] = info;
                if (info.DropCount <= 0)
                {
                    _dropElements.RemoveAt(index);
                }
            }
        }

        private ElementBase AcqByElementData(ElementItemData data)
        {
            ElementBase element = null;
            switch (data.ElementType)
            {
                case ElementType.Normal:
                    element = MemoryPool.Acquire<BaseElementItem>();
                    break;
                case ElementType.Rocket:
                case ElementType.RocketHorizontal:
                    element = MemoryPool.Acquire<RockElementItem>();
                    break;
                case ElementType.Bomb:
                    element = MemoryPool.Acquire<BombElementItem>();
                    break;
                case ElementType.ColorBall:
                    element = MemoryPool.Acquire<ColorBallElementItem>();
                    break;
                case ElementType.Block:
                    element = MemoryPool.Acquire<BlockElementItem>();
                    break;
                case ElementType.Collect:
                    element = MemoryPool.Acquire<CollectElementItem>();
                    break;
                case ElementType.Lock:
                    element = MemoryPool.Acquire<LockElementItem>();
                    break;
                case ElementType.Background:
                    element = MemoryPool.Acquire<BackgroundElementItem>();
                    break;
                case ElementType.SpreadWater:
                    element = new SpreadWaterElementItem(); //水的不走内存池了,因为不会归还
                    break;
                case ElementType.SpreadFire:
                    element = MemoryPool.Acquire<SpreadFireElementItem>();
                    break;
                case ElementType.DropBlock:
                    element = MemoryPool.Acquire<DropBlockElementItem>();
                    break;
                case ElementType.ColorBlock:
                    element = MemoryPool.Acquire<ColorBlockElementItem>();
                    break;
                case ElementType.SpreadGround:
                    element = MemoryPool.Acquire<SpreadGroundElementItem>();
                    break;
                case ElementType.RandomDiffuse:
                    element = MemoryPool.Acquire<RandomDiffuseElementItem>();
                    break;
                case ElementType.TargetBlock:
                    element = MemoryPool.Acquire<TargetBlockElementItem>();
                    break;
                case ElementType.VerticalExpand:
                    element = MemoryPool.Acquire<VerticalExpandElementItem>();
                    break;
                case ElementType.SpreadParterre:
                    element = MemoryPool.Acquire<SpreadParterreElementItem>();
                    break;
                case ElementType.ColorBlockPlus:
                    element = MemoryPool.Acquire<ColorBlockPlusElementItem>();
                    break;
                case ElementType.FixPosExpand:
                    element = MemoryPool.Acquire<FixPosExpandElementItem>();
                    break;
                case ElementType.ColoredRibbonBlock:
                    element = MemoryPool.Acquire<ColoredRibbonBlockElementItem>();
                    break;
                case ElementType.ColoredLightBlock:
                    element = MemoryPool.Acquire<ColoredLightBlockElementItem>();
                    break;
                case ElementType.FixedGridTargetBlock:
                    element = MemoryPool.Acquire<FixedGridTargetBlockElementItem>();
                    break;
                case ElementType.JumpCollect:
                    element = MemoryPool.Acquire<JumpCollectElementItem>();
                    break;
                case ElementType.AdjustCollect:
                    element = MemoryPool.Acquire<AdjustCollectElementItem>();
                    break;
                case ElementType.BurningCollect:
                    element = MemoryPool.Acquire<BurningCollectElementItem>();
                    break;
                case ElementType.Coin:
                    element = MemoryPool.Acquire<CoinElementItem>();
                    break;
                case ElementType.BombBlock:
                    element = MemoryPool.Acquire<BombBlockElementItem>();
                    break;
                case ElementType.DestroyBlock:
                    element = MemoryPool.Acquire<DestroyBlockElementItem>();
                    break;
                case ElementType.MatchLineBlock:
                    element = MemoryPool.Acquire<MatchLineBlockElementItem>();
                    break;
                default:
                    element = MemoryPool.Acquire<BlockElementItem>();
                    Logger.Error($"无法识别的元素类型 [{data.ElementType}]");
                    break;
            }

            return element;
        }
    }
}