using System;
using System.Collections.Generic;
using System.Linq;
using GameConfig;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    [Serializable]
    public class LevelData
    {
        /// <summary>
        /// 关卡id
        /// </summary>
        public int id;

        /// <summary>
        /// 参考关卡id
        /// </summary>
        public int referenceId;

        /// <summary>
        /// 格子列数
        /// </summary>
        public int gridCol;

        /// <summary>
        /// 格子行数
        /// </summary>
        public int gridRow;

        /// <summary>
        /// 初始化时基础棋子颜色(id)
        /// </summary>
        public int[] initColor;

        /// <summary>
        /// 初始化时对应的基础棋子生成概率
        /// </summary>
        public int[] initColorRate;

        /// <summary>
        /// 掉落基础棋子列表
        /// </summary>
        public int[] dropColor;

        /// <summary>
        /// 掉落基础棋子的概率
        /// </summary>
        public int[] dropColorRate;

        /// <summary>
        /// 关卡难度值
        /// </summary>
        public int difficulty;

        /// <summary>
        /// 收集的目标id
        /// </summary>
        public TargetElement[] target;

        /// <summary>
        /// 要求的三星分数
        /// </summary>
        public int fullScore;

        /// <summary>
        /// 关卡限制步数
        /// </summary>
        public int stepLimit;

        /// <summary>
        /// 格子列表
        /// x,y 对应元素列表
        /// </summary>
        public LevelElement[][] grid;

        /// <summary>
        /// 元素掉落标记信息
        /// </summary>
        public List<DropFlag> dropFlags;

        /// <summary>
        /// 配置元素占据的格子信息
        /// </summary>
        private List<GridHoldInfo> _blockElementHoldGridInfo = null;

        private Dictionary<Vector2Int, List<GridHoldInfo>> _coordToGridMap = null;

        private Dictionary<Vector2Int, List<GridHoldInfo>> _coordFindCache = null;
        
        public void Clear()
        {
            _blockElementHoldGridInfo?.Clear();
            _blockElementHoldGridInfo = null;
            _coordToGridMap?.Clear();
            _coordToGridMap = null;
            _coordFindCache?.Clear();
            _coordFindCache = null; 
        }

        /// <summary>
        /// 构造元素占据的格子列表数据
        /// 格子列表的第一个表示元素的起始点
        /// </summary>
        /// <returns></returns>
        public List<GridHoldInfo> BuildElementHoldGridMap(bool rebuildMap = false)
        {
#if UNITY_EDITOR
            int gridEleCount = 0;
            for (int x = 0; x < this.grid.Length; x++)
            {
                for (int y = 0; y < this.grid[x].Length; y++)
                {
                    var ele = grid[x][y];
                    if (ele.elements.Count > 0)
                        gridEleCount += ele.elements.Count;
                }
            }

            if (_blockElementHoldGridInfo != null && gridEleCount != _blockElementHoldGridInfo.Count)
            {
                rebuildMap = true;
            }

#endif

            if (_blockElementHoldGridInfo == null || rebuildMap)
            {
                _blockElementHoldGridInfo ??= new List<GridHoldInfo>();
                _coordToGridMap ??= new Dictionary<Vector2Int, List<GridHoldInfo>>();
                _coordFindCache?.Clear();
                _blockElementHoldGridInfo.Clear();
                _coordToGridMap.Clear();
                if (grid != null)
                {
                    for (int x = 0; x < this.grid.Length; x++)
                    {
                        for (int y = 0; y < this.grid[x].Length; y++)
                        {
                            var ele = grid[x][y];
                            if (ele.elements.Count > 0)
                            {
                                int order = 0;
                                for (int i = 0; i < ele.elements.Count; i++)
                                {
                                    GridHoldInfo info = new GridHoldInfo();
                                    info.ElementId = ele.elements[i].id;
                                    info.Order = order;
                                    order++;
                                    info.AllHoldGridPos = new HashSet<Vector2Int>();
                                    info.LinkElementIds = new List<int>();
                                    info.TargetElementId = ele.elements[i].targetElementId;
                                    info.TargetElementNum = ele.elements[i].targetElementCount;
                                    info.ElementHeight = ele.elements[i].elementHeight;
                                    info.ElementWidth = ele.elements[i].elementWid;
                                    BuildLinkElement(info.ElementId, info.LinkElementIds);
                                    for (int j = 0; j < ele.elements[i].holdCoordsX.Length; j++)
                                    {
                                        var pos = new Vector2Int(ele.elements[i].holdCoordsX[j],
                                            ele.elements[i].holdCoordsY[j]);
                                        if (j == 0)
                                        {
                                            info.StartCoord = pos;
                                        }

                                        info.AllHoldGridPos.Add(pos);
                                        if (!_coordToGridMap.ContainsKey(pos))
                                        {
                                            _coordToGridMap.Add(pos, new List<GridHoldInfo>());
                                        }

                                        _coordToGridMap[pos].Add(info);
                                    }

                                    _blockElementHoldGridInfo.Add(info);
                                }
                            }
                        }
                    }
                }
            }

            return _blockElementHoldGridInfo;
        }

        public void BuildLinkElement(int elementId, List<int> elementIds)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[elementId];
            if (!elementIds.Contains(elementId))
                elementIds.Add(elementId);

            if (config.nextBlock > 0 && !elementIds.Contains(config.nextBlock))
            {
                if (config.nextBlock == elementId)
                    return;
                BuildLinkElement(config.nextBlock, elementIds);
            }
        }

        /// <summary>
        /// 找出指定坐标所有占据的元素信息
        /// </summary>
        /// <returns></returns>
        public List<GridHoldInfo> FindCoordHoldGridInfo(int x, int y, bool rebuildMap = false, bool realCoord = true)
        {
            BuildElementHoldGridMap(rebuildMap);
            var coord = new Vector2Int(x, y);
            if (_coordFindCache != null && _coordFindCache.TryGetValue(coord, out var cachedResult))
            {
                return cachedResult;
            }

            if (realCoord)
            {
                if (_coordToGridMap.TryGetValue(coord, out var result))
                {
                    _coordFindCache ??= new Dictionary<Vector2Int, List<GridHoldInfo>>();
                    _coordFindCache.TryAdd(coord, result);
                    return result;
                }

                return null;
            }

            List<GridHoldInfo> results = new List<GridHoldInfo>();
            foreach (var kvp in _coordToGridMap)
            {
                foreach (var gridInfo in kvp.Value)
                {
                    if (gridInfo.AllHoldGridPos.Contains(coord))
                    {
                        results.Add(gridInfo);
                    }
                }
            }
            _coordFindCache ??= new Dictionary<Vector2Int, List<GridHoldInfo>>();
            _coordFindCache.TryAdd(coord, results);
            return results;
        }

        /// <summary>
        /// 移除初始配置的元素信息
        /// </summary>
        public void RemoveHoldGridElement(Vector2Int coord, ElementItemData elementData)
        {
            if (!_coordToGridMap.ContainsKey(coord))
                return;
            // ElementMapDB mapDB = ConfigMemoryPool.Get<ElementMapDB>();
            List<Vector2Int> needRemove = null;
            foreach (var map in _coordToGridMap)
            {
                int index = -1;
                for (int i = 0; i < map.Value.Count; i++)
                {
                    if (map.Value[i].StartCoord == coord && map.Value[i].LinkElementIds.Contains(elementData.ConfigId))
                    {
                        if (elementData.HoldGrid >= 4)
                        {
                            if (elementData.ElementType != ElementType.SpreadGround && elementData.NextBlockId > 0)
                            {
                                var tempInfo = _coordToGridMap[map.Key][i];
                                tempInfo.ElementId = elementData.NextBlockId;
                                _coordToGridMap[map.Key][i] = tempInfo;
                                continue;
                            }
                        }

                        index = i;
                        break;
                    }
                }

                if (index < 0)
                {
                    continue;
                }

                // int index = map.Value.FindIndex(x => x.ElementId == elementId);
                map.Value.RemoveAt(index);

                if (map.Value.Count <= 0)
                {
                    needRemove ??= new List<Vector2Int>();
                    needRemove.Add(map.Key);
                }
            }

            if (needRemove is { Count: > 0 })
            {
                for (int i = 0; i < needRemove.Count; i++)
                {
                    _coordToGridMap.Remove(needRemove[i]);
                    if (_coordFindCache != null && _coordFindCache.ContainsKey(needRemove[i]))
                    {
                        _coordFindCache.Remove(needRemove[i]);
                    }
                }
            }
        }

        public void RemoveHoldGridElementCoord(Vector2Int removeCoord, ElementItemData elementData)
        {
            if (!_coordToGridMap.ContainsKey(removeCoord))
                return;
            _coordToGridMap.Remove(removeCoord);
            if(_coordFindCache != null && _coordFindCache.ContainsKey(removeCoord))
                _coordFindCache.Remove(removeCoord);
            foreach (var map in _coordToGridMap)
            {
                int index = map.Value.FindIndex(x => x.ElementId == elementData.ConfigId);
                if (index >= 0 && map.Value[index].AllHoldGridPos.Contains(removeCoord))
                {
                    map.Value[index].AllHoldGridPos.Remove(removeCoord);
                }
            }

        }

        public void DeepClone(LevelData other)
        {
            if (other == null)
                return;

            other.gridCol = this.gridCol;
            other.gridRow = this.gridRow;
            other.difficulty = this.difficulty;
            other.fullScore = this.fullScore;
            other.stepLimit = this.stepLimit;
            other.referenceId = this.referenceId;

            other.initColor = (int[])this.initColor?.Clone();
            other.initColorRate = (int[])this.initColorRate?.Clone();
            other.dropColor = (int[])this.dropColor?.Clone();
            other.dropColorRate = (int[])this.dropColorRate?.Clone();
            if (this.target != null)
            {
                other.target = new TargetElement[this.target.Length];
                for (int i = 0; i < this.target.Length; i++)
                {
                    if (this.target[i] == null)
                        continue;
                    other.target[i] = new TargetElement()
                    {
                        targetId = this.target[i].targetId, targetNum = this.target[i].targetNum
                    };
                }

                other.target = other.target.Where(x => x != null).ToArray();
            }
            else
            {
                other.target = null;
            }

            if (this.grid != null)
            {
                other.grid = new LevelElement[this.grid.Length][];
                for (int i = 0; i < this.grid.Length; i++)
                {
                    if (this.grid[i] != null)
                    {
                        other.grid[i] = new LevelElement[this.grid[i].Length];
                        for (int j = 0; j < this.grid[i].Length; j++)
                        {
                            LevelElement srcElement = this.grid[i][j];
                            if (srcElement != null)
                            {
                                List<GridElement> eles = new List<GridElement>();
                                for (int k = 0; k < srcElement.elements.Count; k++)
                                {
                                    var ele = new GridElement()
                                    {
                                        id = srcElement.elements[k].id,
                                        targetElementId = srcElement.elements[k].targetElementId,
                                        targetElementCount = srcElement.elements[k].targetElementCount,
                                        elementWid = srcElement.elements[k].elementWid,
                                        elementHeight = srcElement.elements[k].elementHeight,
                                        holdCoordsX = (int[])srcElement.elements[k].holdCoordsX?.Clone(),
                                        holdCoordsY = (int[])srcElement.elements[k].holdCoordsY?.Clone(),
                                    };
                                    eles.Add(ele);
                                }

                                LevelElement newElement = new LevelElement
                                {
                                    isWhite = srcElement.isWhite,
                                    elements = eles,
                                };
                                other.grid[i][j] = newElement;
                            }
                            else
                            {
                                other.grid[i][j] = null;
                            }
                        }
                    }
                    else
                    {
                        other.grid[i] = null;
                    }
                }
            }
            else
            {
                other.grid = null;
            }

            if (this.dropFlags != null)
            {
                other.dropFlags = new List<DropFlag>(this.dropFlags.Count);
                for (int i = 0; i < this.dropFlags.Count; i++)
                {
                    other.dropFlags.Add(new DropFlag()
                    {
                        dropX = this.dropFlags[i].dropX,
                        dropElements = new List<DropFlagElement>(this.dropFlags[i].dropElements),
                    });
                }
            }
        }

        public bool Validate()
        {
            if (id < 0)
            {
                Logger.Error($"请输入正确的id:{id}");
                return false;
            }

            if (gridCol < 1 || gridRow < 1)
            {
                Logger.Error($"请输入正确的格子宽高");
                return false;
            }

            if (stepLimit < 1)
            {
                Logger.Error("请输入正确的行动步数");
                return false;
            }

            if (fullScore <= 0)
            {
                Logger.Error("请输入正确的三星通关分数");
                return false;
            }

            if (grid == null)
            {
                Logger.Error("格子数据异常");
                return false;
            }

            if (target == null)
            {
                Logger.Error("关卡目标异常");
                return false;
            }
            else
            {
                List<int> targetIds = new List<int>(target.Length);
                for (int i = 0; i < target.Length; i++)
                {
                    if (!targetIds.Contains(target[i].targetId))
                        targetIds.Add(target[i].targetId);
                    else
                    {
                        Logger.Error($"关卡目标存在重复 [{target[i].targetId}] 请修改!");
                        return false;
                    }
                }
            }

            if (initColor == null || initColor.Length == 0)
            {
                Logger.Error("基础棋子未配置");
                return false;
            }
            else
            {
                List<int> targetIds = new List<int>(target.Length);
                for (int i = 0; i < initColor.Length; i++)
                {
                    if (!targetIds.Contains(initColor[i]))
                        targetIds.Add(initColor[i]);
                    else
                    {
                        Logger.Error($"基础棋子重复 [{initColor[i]}] 请修改!");
                        return false;
                    }
                }

                for (int i = 0; i < initColorRate.Length; i++)
                {
                    if (initColorRate[i] <= 0)
                        Logger.Error($"基础棋子的概率为0?为什么不直接删掉呢?");
                }
            }

            if (dropColor == null || dropColor.Length == 0)
            {
                Logger.Error("掉落棋子未配置");
                return false;
            }
            else
            {
                List<int> targetIds = new List<int>(target.Length);
                for (int i = 0; i < dropColor.Length; i++)
                {
                    if (!targetIds.Contains(dropColor[i]))
                        targetIds.Add(dropColor[i]);
                    else
                    {
                        Logger.Error($"掉落棋子重复 [{dropColor[i]}] 请修改!");
                        return false;
                    }
                }

                for (int i = 0; i < dropColorRate.Length; i++)
                {
                    if (dropColorRate[i] <= 0)
                        Logger.Error($"掉落棋子的概率为0?为什么不直接删掉呢?");
                }
            }

            return true;
        }
    }

    [Serializable]
    public class LevelElement
    {
        /// <summary>
        /// 是否是空白格子
        /// </summary>
        public bool isWhite;

        /// <summary>
        /// 关卡格子上对应的堆叠元素
        /// </summary>
        public List<GridElement> elements;
    }

    /// <summary>
    /// 格子上的元素信息
    /// </summary>
    [Serializable]
    public class GridElement
    {
        /// <summary>
        /// 元素配置id
        /// </summary>
        public int id;

        /// <summary>
        /// 元素目标id
        /// </summary>
        public int targetElementId;

        /// <summary>
        /// 元素目标数量
        /// </summary>
        public int targetElementCount;

        /// <summary>
        /// 元素占据格子宽度
        /// </summary>
        public int elementWid;

        /// <summary>
        /// 元素占据格子高度
        /// </summary>
        public int elementHeight;

        /// <summary>
        /// 元素占据的格子-X轴
        /// </summary>
        public int[] holdCoordsX;

        /// <summary>
        /// 元素占据的格子-Y轴
        /// </summary>
        public int[] holdCoordsY;
    }

    [Serializable]
    public class TargetElement
    {
        /// <summary>
        /// 收集的元素目标id
        /// </summary>
        public int targetId;

        /// <summary>
        /// 收集目标数量
        /// </summary>
        public int targetNum;
    }

    /// <summary>
    /// 掉落配置标记
    /// </summary>
    [Serializable]
    public class DropFlag
    {
        /// <summary>
        /// 掉落格子X轴
        /// </summary>
        public int dropX;

        /// <summary>
        /// 掉落元素配置信息
        /// </summary>
        public List<DropFlagElement> dropElements;
    }

    /// <summary>
    /// 元素掉落元素信息
    /// </summary>
    [Serializable]
    public class DropFlagElement
    {
        /// <summary>
        /// 元素Id
        /// </summary>
        public int elementId;

        /// <summary>
        /// 掉落该元素的概率,万分比
        /// </summary>
        public int dropRate;

        /// <summary>
        /// 掉落限制最大数量
        /// </summary>
        public int dropLimitMax;
    }
}