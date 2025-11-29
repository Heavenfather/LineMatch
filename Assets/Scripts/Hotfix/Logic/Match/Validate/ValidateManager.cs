using System;
using System.Collections.Generic;
using GameConfig;
using GameCore.Singleton;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;
using Random = UnityEngine.Random;

namespace HotfixLogic.Match
{
    public class ValidateManager : LazySingleton<ValidateManager>
    {
        private Dictionary<string, Func<ElementDestroyContext, ElementBase, ElementBase, float>>
            _composeFunc = null;

        private Dictionary<ValidateType, IValidate> _validates;
        private readonly Dictionary<GridHoldInfo, HashSet<Vector2Int>> _gridBlockSideMap = new();
        private readonly HashSet<Vector2Int> _effBlockCoords = new HashSet<Vector2Int>();
        private ElementDestroyContext _context;
        public ElementDestroyContext Context => _context;
        private SpecialPieceScoreDB _db => ConfigMemoryPool.Get<SpecialPieceScoreDB>();

        //四方向检测
        public readonly Vector2Int[] NeighborDirs = new[]
        {
            new Vector2Int(0, 1), // 上
            new Vector2Int(-1, 0), // 左
            new Vector2Int(1, 0), // 右
            new Vector2Int(0, -1), // 下
        };

        //八方向检测
        public readonly Vector2Int[] EightNeighborDirs = new[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
            new Vector2Int(1, 1), // 右上
            new Vector2Int(1, -1), // 右下
            new Vector2Int(-1, 1), // 左上
            new Vector2Int(-1, -1) // 左下
        };

        protected override void OnInitialized()
        {
            _validates = new Dictionary<ValidateType, IValidate>
            {
                { ValidateType.One, new ValidateOne() },
                { ValidateType.Line, new ValidateLine() },
                { ValidateType.Square, new ValidateSquare() }
            };
            _context = new ElementDestroyContext();

            _composeFunc =
                new Dictionary<string, Func<ElementDestroyContext, ElementBase, ElementBase, float>>()
                {
                    [$"{ElementType.Rocket}-{ElementType.Rocket}"] = this.RocketAndRocket,

                    [$"{ElementType.Rocket}-{ElementType.RocketHorizontal}"] = this.RocketAndRocket,

                    [$"{ElementType.RocketHorizontal}-{ElementType.RocketHorizontal}"] = this.RocketAndRocket,

                    [$"{ElementType.Bomb}-{ElementType.Rocket}"] = this.RocketAndBomb,

                    [$"{ElementType.Bomb}-{ElementType.RocketHorizontal}"] = this.RocketAndBomb,

                    [$"{ElementType.ColorBall}-{ElementType.Rocket}"] = this.RocketAndColorBall,

                    [$"{ElementType.ColorBall}-{ElementType.RocketHorizontal}"] = this.RocketAndColorBall,

                    [$"{ElementType.Bomb}-{ElementType.Bomb}"] = this.BombAndBomb,

                    [$"{ElementType.ColorBall}-{ElementType.Bomb}"] = this.BombAndColorBall,

                    [$"{ElementType.ColorBall}-{ElementType.ColorBall}"] = this.ColorBallAndColorBall,
                    
                    [$"{ElementType.ColorBall}-{ElementType.Normal}"] = this.ColorBallAndNormal,
                };
        }

        public void BuildBlockInfos(GridItemData[,] grids, GridSystem gridSystem)
        {
            _gridBlockSideMap.Clear();
            _context?.Clear();

            for (int x = 0; x < grids.GetLength(0); x++)
            {
                for (int y = 0; y < grids.GetLength(1); y++)
                {
                    var grid = grids[x, y];
                    if (grid != null && grid.Elements != null)
                    {
                        for (int i = 0; i < grid.Elements.Count; i++)
                        {
                            //存储旁消类型的元素
                            var infos = ElementSystem.Instance.FindCoordHoldGridInfo(x, y);
                            if (infos == null)
                                break;
                            AddSideMap(infos);
                        }
                    }
                }
            }

            _context.GridSystem = gridSystem;
        }

        /// <summary>
        /// 校验选中的格子
        /// </summary>
        /// <param name="selectedItems">当前选中的所有格子</param>
        /// <param name="isRectangle">是否闭环</param>
        /// <param name="validatedCallback">校验完成后的回调函数</param>
        /// <param name="useItemId">使用道具id</param>
        public void Judge(in List<GridItem> selectedItems, in bool isRectangle, Action<bool> validatedCallback,
            int useItemId = -1)
        {
            if (selectedItems == null)
            {
                validatedCallback?.Invoke(false);
                return;
            }

            if (useItemId == -1 && selectedItems.Count == 1)
            {
                var gridItem = selectedItems[0];
                var topElement = gridItem.Data.GetTopElement();
                if (topElement == null)
                {
                    validatedCallback?.Invoke(false);
                    return;
                }
                if (!ElementSystem.Instance.IsSpecialElement(topElement.ElementType, true))
                {
                    validatedCallback?.Invoke(false);
                    return;
                }
            }

            _context.Clear();
            _context.UsingItemId = useItemId;
            ValidateType type = ValidateType.One;
            if (selectedItems.Count > 1)
            {
                if (selectedItems.Count <= 3)
                {
                    //三点不可能连成一个矩形
                    type = ValidateType.Line;
                }
                else if (isRectangle)
                {
                    type = ValidateType.Square;
                }
                else
                {
                    type = ValidateType.Line;
                }
            }

            if (_validates.TryGetValue(type, out IValidate validate))
                validate.Validate(_context, selectedItems, validatedCallback);
        }

        public void JudgeAllSpecialElements(ref HashSet<Vector2Int> delCoords)
        {
            _context.Clear();

            var allElements = ElementSystem.Instance.GridElements;
            foreach (var elements in allElements.Values)
            {
                foreach (var element in elements)
                {
                    if (!ElementSystem.Instance.IsSpecialElement(element.Data.ElementType))
                        continue;
                    if (element.Data.ElementType == ElementType.Rocket ||
                        element.Data.ElementType == ElementType.RocketHorizontal)
                        AttachRocket(_context, element.Data.GridPos, element.Data.Direction, element.Data.UId);
                    else if (element.Data.ElementType == ElementType.Bomb)
                        AttachBomb(_context, element.Data.GridPos, element.Data.UId);
                    else if (element.Data.ElementType == ElementType.ColorBall)
                    {
                        AttachColorBall(_context, -1, element.Data.GridPos, element.Data.UId);
                    }

                    delCoords.Add(element.Data.GridPos);
                }
            }
        }

        /// <summary>
        /// 查找受影响的旁消类型障碍物元素
        /// </summary>
        /// <returns></returns>
        public HashSet<Vector2Int> FindEffBlockCoords(HashSet<Vector2Int> effCoords, ElementDestroyContext context)
        {
            _effBlockCoords.Clear();
            //收集受影响障碍物
            if (_gridBlockSideMap.Count > 0)
            {
                ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
                foreach (var kp in _gridBlockSideMap)
                {
                    bool checkLock = ElementSystem.Instance.HaveOverElementLock(kp.Key.StartCoord, kp.Key.ElementId,out var lockId);
                    if (checkLock)
                    {
                        if (db[lockId].eliminateStyle != EliminateStyle.Side)
                        {
                            continue;
                        }
                    }

                    bool shouldTrigger = false;
                    foreach (var coord in kp.Value)
                    {
                        if (effCoords.Contains(coord))
                        {
                            shouldTrigger = true;
                            context.AddGridDelCoord(coord);
                        }
                    }

                    if (shouldTrigger)
                    {
                        _effBlockCoords.Add(kp.Key.StartCoord);
                    }
                }
            }

            return _effBlockCoords;
        }

        /// <summary>
        /// 仅收集水的元素
        /// </summary>
        /// <param name="effCoords"></param>
        /// <returns></returns>
        public HashSet<Vector2Int> FindEffBlockCoordsOnlyWater(HashSet<Vector2Int> effCoords)
        {
            _effBlockCoords.Clear();
            //收集受影响障碍物
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            if (_gridBlockSideMap.Count > 0)
            {
                foreach (var kp in _gridBlockSideMap)
                {
                    bool shouldTrigger = false;
                    foreach (var coord in effCoords)
                    {
                        if (db[kp.Key.ElementId].elementType == ElementType.SpreadWater && kp.Value.Contains(coord))
                        {
                            shouldTrigger = true;
                            break;
                        }
                    }

                    if (shouldTrigger)
                    {
                        _effBlockCoords.Add(kp.Key.StartCoord);
                    }
                }
            }

            return _effBlockCoords;
        }

        public void AddSideElement(ElementBase element)
        {
            if (element == null)
                return;
            if (element.Data.EliminateStyle != EliminateStyle.Side)
                return;
            GridHoldInfo info = new GridHoldInfo()
            {
                ElementId = element.Data.ConfigId,
                StartCoord = element.Data.GridPos,
                AllHoldGridPos = new HashSet<Vector2Int>(1) { element.Data.GridPos },
            };
            info.LinkElementIds = new();
            ElementSystem.Instance.BuildLinkElementId(info.ElementId, info.LinkElementIds);
            AddSideMap(info);
        }

        public void RemoveSideCoords(int elementId, Vector2Int coord, HashSet<Vector2Int> removeCoords)
        {
            foreach (var kp in _gridBlockSideMap)
            {
                GridHoldInfo info = kp.Key;
                HashSet<Vector2Int> coords = kp.Value;
                if (info.ElementId == elementId && info.StartCoord == coord)
                {
                    foreach (var delCoord in removeCoords)
                    {
                        coords.Remove(delCoord);
                    }
                }
            }
        }

        public void RemoveSideElement(ElementItemData elementData, Vector2Int pos)
        {
            // ElementMapDB mapDB = ConfigMemoryPool.Get<ElementMapDB>();
            HashSet<GridHoldInfo> delInfos = new HashSet<GridHoldInfo>();
            foreach (var coord in _gridBlockSideMap)
            {
                if (coord.Key.ElementId == elementData.ConfigId && coord.Key.AllHoldGridPos.Contains(pos))
                {
                    delInfos.Add(coord.Key);
                }
            }

            if (delInfos.Count > 0)
            {
                foreach (var info in delInfos)
                {
                    _gridBlockSideMap.Remove(info);
                    if (info.AllHoldGridPos.Count >= 4)
                    {
                        if (elementData.ElementType != ElementType.SpreadGround &&
                            elementData.NextBlockId > 0)
                        {
                            GridHoldInfo nextInfo = new GridHoldInfo()
                            {
                                ElementId = elementData.NextBlockId,
                                StartCoord = info.StartCoord,
                                Order = info.Order,
                                AllHoldGridPos = info.AllHoldGridPos,
                                LinkElementIds = info.LinkElementIds
                            };
                            AddSideMap(nextInfo);
                        }
                    }
                }
            }
        }

        public float InvokeComposeElement(ElementDestroyContext context, ElementBase firstElement,
            ElementBase secondElement)
        {
            if (_composeFunc.TryGetValue($"{firstElement.Data.ElementType}-{secondElement.Data.ElementType}",
                    out var method))
            {
                context.AddWillDelCoord(firstElement.Data.GridPos, EliminateStyle.Match, firstElement.Data.UId);
                context.AddWillDelCoord(secondElement.Data.GridPos, EliminateStyle.Match, secondElement.Data.UId);
                context.AddEffGridCoord(firstElement.Data.GridPos);
                context.AddEffGridCoord(secondElement.Data.GridPos);
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchTriple,
                    EventOneParam<string>.Create($"{firstElement.Data.ElementType}-{secondElement.Data.ElementType}"));
                return method.Invoke(context, firstElement, secondElement);
            }

            return 0;
        }

        private void AddSideMap(List<GridHoldInfo> allInfos)
        {
            ElementMapDB mapDB = ConfigMemoryPool.Get<ElementMapDB>();
            foreach (var info in allInfos)
            {
                if (mapDB[info.ElementId].eliminateStyle != EliminateStyle.Side)
                    continue;
                AddSideMap(info);
            }
        }

        private void AddSideMap(GridHoldInfo info)
        {
            foreach (var holdGridPos in info.AllHoldGridPos)
            {
                if (!_gridBlockSideMap.ContainsKey(info))
                    _gridBlockSideMap.Add(info, new HashSet<Vector2Int>());
                foreach (var dir in NeighborDirs)
                {
                    _gridBlockSideMap[info].Add(dir + holdGridPos);
                }
            }
        }

        #region 单个功能棋子效果

        public void AttachRocket(ElementDestroyContext context, Vector2Int coord, ElementDirection direction, int uid)
        {
            (int width, int height) = context.GridSystem.GetBoardSize();

            context.AddWillDelCoord(coord, EliminateStyle.Match, uid);
            context.AddEffGridCoord(coord);
            context.AddCurrentEffectId((int)ElementIdConst.Rocket);
            context.AddCurrentEffectId((int)ElementIdConst.RocketHorizontal);

            if (direction == ElementDirection.Right)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2Int checkCoord = new Vector2Int(x, coord.y);
                    AddDelGridElement(checkCoord, context, coord);
                }
            }
            else if (direction == ElementDirection.Up)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int checkCoord = new Vector2Int(coord.x, y);
                    AddDelGridElement(checkCoord, context, coord);
                }
            }

            int score = _db.CalScore(SpecialElementType.Rocket,
                ElementSystem.Instance.GetSpecialElementConfigId(ElementType.Rocket));

            MatchManager.Instance.AddScore(score);
        }

        public void AttachBomb(ElementDestroyContext context, Vector2Int coord, int uid)
        {
            //爆炸周围5x5的格子
            context.AddWillDelCoord(coord, EliminateStyle.Match, uid);
            context.AddEffGridCoord(coord);
            context.AddCurrentEffectId((int)ElementIdConst.Bomb);
            
            var bombCoords = GetBombCoords(coord);
            for (int i = 0; i < bombCoords.Count; i++)
            {
                AddDelGridElement(bombCoords[i], context, coord);
            }

            int score = _db.CalScore(SpecialElementType.Bomb,
                ElementSystem.Instance.GetSpecialElementConfigId(ElementType.Bomb));
            MatchManager.Instance.AddScore(score);
        }
        
        public void AttachBombBlock(ElementDestroyContext context, Vector2Int coord, int uid)
        {
            context.AddWillDelCoord(coord, EliminateStyle.Side, uid);
            context.AddEffGridCoord(coord);
            context.AddCurrentEffectId((int)ElementIdConst.Bomb);
            foreach (var dir in EightNeighborDirs)
            {
                Vector2Int bombCoord = dir + coord;
                AddDelGridElement(bombCoord, context, coord);
            }
        }

        public List<Vector2Int> GetBombCoords(Vector2Int coord)
        {
            List<Vector2Int> bombCoords = new List<Vector2Int>(24);
            var matchType = MatchManager.Instance.CurrentMatchGameType;
            if (matchType == MatchGameType.NormalMatch)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        Vector2Int bombCoord = new Vector2Int(coord.x + dx, coord.y + dy);
                        if (bombCoord != coord) // 避免重复添加中心点
                        {
                            bombCoords.Add(bombCoord);
                        }
                    }
                }
            }
            else
            {
                foreach (var dir in EightNeighborDirs)
                {
                    Vector2Int bombCoord = dir + coord;
                    bombCoords.Add(bombCoord);
                }
            }

            return bombCoords;
        }

        public void AttachColorBall(ElementDestroyContext context, int elementId, Vector2Int colorBallCoord, int uid)
        {
            context.AddWillDelCoord(colorBallCoord, EliminateStyle.Match, uid);
            context.AddEffGridCoord(colorBallCoord);
            context.AddCurrentEffectId((int)ElementIdConst.ColorBall);
            if (elementId > 0)
            {
                var allElements = ElementSystem.Instance.GridElements;
                foreach (var grid in allElements)
                {
                    bool result = ElementSystem.Instance.TryGetBaseElement(grid.Value, out int index);
                    if (result && index >= 0 && grid.Value[index].Data.ConfigId == elementId)
                    {
                        context.AddEffGridCoord(grid.Key);
                        context.AddWillDelCoord(grid.Key, EliminateStyle.Match, grid.Value[index].Data.UId);
                    }
                }

                var effBlockCoords = FindEffBlockCoords(context.EffGridCoords, context);
                if (effBlockCoords.Count > 0)
                {
                    foreach (var coord in effBlockCoords)
                    {
                        var sideElements = ElementSystem.Instance.GetGridElements(coord, true);
                        if (sideElements is { Count: > 0 })
                        {
                            context.AddEffGridCoord(coord);
                            context.AddWillDelCoord(coord, EliminateStyle.Side, sideElements[^1].Data.UId);
                        }
                    }
                }

                context.FilterElementId = elementId;
            }

            int score = _db.CalScore(SpecialElementType.ColorBall,
                ElementSystem.Instance.GetSpecialElementConfigId(ElementType.ColorBall));
            MatchManager.Instance.AddScore(score);
        }

        /// <summary>
        /// 全屏消除
        /// </summary>
        public void BombBoardPanel(ElementDestroyContext context,List<Vector2Int> filterCoords = null)
        {
            context.IsColorBallClearAll = true;
            var allElements = ElementSystem.Instance.GridElements;
            foreach (var coord in allElements.Keys)
            {
                if(filterCoords != null && filterCoords.Contains(coord))
                    continue;

                var elements = ElementSystem.Instance.GetGridElements(coord, false);
                if (elements == null || elements.Count <= 0)
                    continue;
                int lockIndex = -1;
                int targetBlockIndex = -1;
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Data.ElementType == ElementType.Lock)
                    {
                        lockIndex = i;
                        break;
                    }

                    if (elements[i].Data.ElementType == ElementType.TargetBlock)
                    {
                        targetBlockIndex = i;
                        break;
                    }
                }
                
                if (targetBlockIndex >= 0)
                    continue;

                if (lockIndex >= 0)
                {
                    context.AddWillDelCoord(elements[lockIndex].Data.GridPos, elements[lockIndex].Data.EliminateStyle,
                        elements[lockIndex].Data.UId);
                    if (elements[lockIndex].Data.EliminateStyle != EliminateStyle.Bomb)
                        context.AddWillDelCoord(elements[lockIndex].Data.GridPos, EliminateStyle.Bomb,
                            elements[lockIndex].Data.UId);
                    context.AddEffGridCoord(elements[lockIndex].Data.GridPos);
                }
                else
                {
                    for (int i = elements.Count - 1; i >= 0; i--)
                    {
                        var ele = elements[i];
                        context.AddCurrentEffectId(ele.Data.ConfigId);
                        if (!context.EffGridCoords.Contains(ele.Data.GridPos))
                        {
                            if (ele.Data.ElementType == ElementType.Normal)
                            {
                                //添加计算基础元素得分
                                AddEffectBaseElementScore(ele.Data.ConfigId);
                            }
                            else if (ElementSystem.Instance.IsSpecialElement(ele.Data.ElementType))
                            {
                                // AddSpecialElementScore(ele.Data.ElementType);
                                CollectAutoElement(context, ele);
                            }
                        }

                        context.AddWillDelCoord(ele.Data.GridPos, ele.Data.EliminateStyle, ele.Data.UId);
                        if (ele.Data.EliminateStyle != EliminateStyle.Bomb)
                            context.AddWillDelCoord(ele.Data.GridPos, EliminateStyle.Bomb, ele.Data.UId);
                        context.AddEffGridCoord(ele.Data.GridPos);
                        if (ele.Data.HoldGrid >= 1)
                            break;
                    }
                }
            }
        }

        public void AddEffectBaseElementScore(int elementId)
        {
            BlockDiffScoreDB db = ConfigMemoryPool.Get<BlockDiffScoreDB>();
            int score = db.CalScoreNotRect(elementId, 1);
            MatchManager.Instance.AddScore(score);
        }

        #endregion

        #region 组合功能棋子效果

        /// <summary>
        /// 火箭+火箭 十字状消除
        /// </summary>
        private float RocketAndRocket(ElementDestroyContext context, ElementBase firstElement,
            ElementBase secondElement)
        {
            if(firstElement.Data.GridPos == secondElement.Data.GridPos)
                return 0;
            context.IsRocketAndRocket = true;
            Vector2Int destroyCoord = firstElement.Data.GridPos;

            //强制修改数据 因为不管朝向如何，火箭+火箭就是要十字消除
            if (firstElement.Data.Direction == secondElement.Data.Direction)
            {
                if (firstElement.Data.Direction == ElementDirection.Up)
                {
                    secondElement.Data.Direction = ElementDirection.Right;
                    secondElement.GameObject.transform.localEulerAngles = new Vector3(0, 0, -90);
                }
                else if (firstElement.Data.Direction == ElementDirection.Right)
                {
                    secondElement.Data.Direction = ElementDirection.Up;
                    secondElement.GameObject.transform.localEulerAngles = new Vector3(0, 0, 90);
                }
            }

            context.AddRocketAndRocket(firstElement.Data);
            context.AddRocketAndRocket(secondElement.Data);
            (int width, int height) = context.GridSystem.GetBoardSize();
            for (int x = 0; x < width; x++)
            {
                Vector2Int coord = new Vector2Int(x, destroyCoord.y);
                AddDelGridElement(coord, context, destroyCoord);
            }

            for (int y = 0; y < height; y++)
            {
                Vector2Int coord = new Vector2Int(destroyCoord.x, y);
                AddDelGridElement(coord, context, destroyCoord);
            }

            int rocketId = ElementSystem.Instance.GetSpecialElementConfigId(ElementType.Rocket);
            int score = _db.CalScore(SpecialElementType.RandB, rocketId, rocketId);
            MatchManager.Instance.AddScore(score);
            
            TaskManager.Instance.AddTaskCalculate(TaskTag.UseRocketAndRocket);
            return 0;
        }

        /// <summary>
        /// 火箭+炸药 横竖十字状各消除3行
        /// </summary>
        private float RocketAndBomb(ElementDestroyContext context, ElementBase firstElement, ElementBase secondElement)
        {
            context.IsRocketAndBomb = true;
            (int width, int height) = context.GridSystem.GetBoardSize();
            int[] xCoord = new int[3]
                { secondElement.Data.GridPos.x - 1, secondElement.Data.GridPos.x, secondElement.Data.GridPos.x + 1 };
            int[] yCoord = new int[3]
                { secondElement.Data.GridPos.y - 1, secondElement.Data.GridPos.y, secondElement.Data.GridPos.y + 1 };
            for (int x = 0; x < xCoord.Length; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int delCoord = new Vector2Int(xCoord[x], y);
                    AddDelGridElement(delCoord, context, secondElement.Data.GridPos);
                }
            }

            for (int y = 0; y < yCoord.Length; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2Int delCoord = new Vector2Int(x, yCoord[y]);
                    AddDelGridElement(delCoord, context, secondElement.Data.GridPos);
                }
            }

            int rocketId = ElementSystem.Instance.GetSpecialElementConfigId(ElementType.Rocket);
            int bombId = ElementSystem.Instance.GetSpecialElementConfigId(ElementType.Bomb);
            int score = _db.CalScore(SpecialElementType.RandB, rocketId, bombId);
            MatchManager.Instance.AddScore(score);
            
            TaskManager.Instance.AddTaskCalculate(TaskTag.UseBombAndRocket);
            return 0;
        }

        /// <summary>
        /// 火箭+彩色球
        /// </summary>
        private float RocketAndColorBall(ElementDestroyContext context, ElementBase firstElement,
            ElementBase secondElement)
        {
            int elementId = ElementSystem.Instance.RandomPickBoardBaseElementId();
            int rocketId = ElementSystem.Instance.GetSpecialElementConfigId(ElementType.Rocket);
            int rocketHorizontalId = ElementSystem.Instance.GetSpecialElementConfigId(ElementType.RocketHorizontal);
            var colorBallGridItem = context.GridSystem.GetGridByCoord(firstElement.Data.GridPos);
            context.IsColorBallLineRocket = true;
            context.LinkSpecialBallCoord = colorBallGridItem.Data.Coord;
            List<Vector2Int> newGenRocketCoord = new List<Vector2Int>();

            if (elementId > 0)
            {
                var allElements = ElementSystem.Instance.GridElements;

                foreach (var kp in allElements)
                {
                    var coord = kp.Key;
                    var elements = kp.Value;
                    if (elements == null || elements.Count <= 0)
                        continue;
                    ElementBase effElement = null;
                    bool result = ElementSystem.Instance.TryGetBaseElement(elements, out int _);
                    if (result)
                    {
                        for (int i = 0; i < elements.Count; i++)
                        {
                            if (elements[i].Data.ConfigId == elementId)
                            {
                                effElement = elements[i];
                                break;
                            }
                        }
                    }

                    if (effElement == null)
                        continue;
                    newGenRocketCoord.Add(coord);
                }
                context.AddCalAddedCount(elementId, newGenRocketCoord.Count);
            }

            newGenRocketCoord.Add(colorBallGridItem.Data.Coord);

            foreach (var v in newGenRocketCoord)
            {
                Vector2Int genCoord = v;
                var id = Random.Range(0, 2) == 0 ? rocketId : rocketHorizontalId;
                var dir = id == rocketHorizontalId ? ElementDirection.Right : ElementDirection.Up;

                var itemData = ElementSystem.Instance.GenElementItemData(id, genCoord.x, genCoord.y);
                context.AddBallLineSpecialItem(itemData);

                //计算火箭爆炸影响范围
                AttachRocket(context, genCoord, dir, itemData.UId);
            }

            int colorBallId = ElementSystem.Instance.GetSpecialElementConfigId(ElementType.ColorBall);
            int score = _db.CalScore(SpecialElementType.CandRB, rocketId, colorBallId);
            MatchManager.Instance.AddScore(score);
            
            TaskManager.Instance.AddTaskCalculate(TaskTag.UseColorBallAndRocket);
            return 0;
        }


        /// <summary>
        /// 炸药+炸药 9*9格子消失
        /// </summary>
        private float BombAndBomb(ElementDestroyContext context, ElementBase firstElement, ElementBase secondElement)
        {
            //以第一个炸弹为原点
            Vector2Int sourceCoord = firstElement.Data.GridPos;
            var delCoords = GetBombAndBombDelCoords(sourceCoord);
            for (int i = 0; i < delCoords.Count; i++)
            {
                AddDelGridElement(delCoords[i], context, sourceCoord);
            }

            context.AddDoubleBombCoord(firstElement.Data.GridPos);
            context.AddDoubleBombCoord(secondElement.Data.GridPos);

            int bombId = ElementSystem.Instance.GetSpecialElementConfigId(ElementType.Bomb);
            int score = _db.CalScore(SpecialElementType.RandB, bombId, bombId);
            MatchManager.Instance.AddScore(score);
            
            TaskManager.Instance.AddTaskCalculate(TaskTag.UseBombAndBomb);
            return 0;
        }

        public List<Vector2Int> GetBombAndBombDelCoords(Vector2Int sourceCoord)
        {
            int[] xCoord = new int[9];
            xCoord[0] = sourceCoord.x;
            for (int i = 1; i < 5; i++)
            {
                xCoord[i] = sourceCoord.x + i;
                xCoord[^i] = sourceCoord.x - i;
            }

            int[] yCoord = new int[9];
            yCoord[0] = sourceCoord.y;
            for (int i = 1; i < 5; i++)
            {
                yCoord[i] = sourceCoord.y + i;
                yCoord[^i] = sourceCoord.y - i;
            }
            List<Vector2Int> delCoords = new List<Vector2Int>();
            for (int x = 0; x < xCoord.Length; x++)
            {
                for (int y = 0; y < yCoord.Length; y++)
                {
                    Vector2Int delCoord = new Vector2Int(xCoord[x], yCoord[y]);
                    delCoords.Add(delCoord);
                }
            }
            return delCoords;
        }

        /// <summary>
        /// 炸药+彩色球
        /// </summary>
        private float BombAndColorBall(ElementDestroyContext context, ElementBase firstElement,
            ElementBase secondElement)
        {
            int bombId = ElementSystem.Instance.GetSpecialElementConfigId(ElementType.Bomb);
            int elementId = ElementSystem.Instance.RandomPickBoardBaseElementId();
            var colorBallGridItem = context.GridSystem.GetGridByCoord(firstElement.Data.GridPos);
            context.IsColorBallLineBomb = true;
            context.LinkSpecialBallCoord = colorBallGridItem.Data.Coord;
            List<Vector2Int> newGenRocketCoord = new List<Vector2Int>();
            
            if (elementId > 0)
            {
                var allElements = ElementSystem.Instance.GridElements;
                foreach (var kp in allElements)
                {
                    var coord = kp.Key;
                    var elements = kp.Value;
                    if (elements == null || elements.Count <= 0)
                        continue;

                    ElementBase effElement = null;
                    bool result = ElementSystem.Instance.TryGetBaseElement(elements, out int _);
                    if (result)
                    {
                        for (int i = 0; i < elements.Count; i++)
                        {
                            if (elements[i].Data.ConfigId == elementId)
                            {
                                effElement = elements[i];
                                break;
                            }
                        }
                    }

                    if (effElement == null)
                        continue;
                    newGenRocketCoord.Add(coord);
                }
                context.AddCalAddedCount(elementId, newGenRocketCoord.Count);

                //彩色球自身也会形成
                newGenRocketCoord.Add(colorBallGridItem.Data.Coord);
            }
            else
            {
                newGenRocketCoord.Add(colorBallGridItem.Data.Coord);
            }
                
            //先移动自己的位置
            foreach (var v in newGenRocketCoord)
            {
                Vector2Int genCoord = v;
                var itemData = ElementSystem.Instance.GenElementItemData(bombId, genCoord.x, genCoord.y);
                context.AddBallLineSpecialItem(itemData);

                //计算爆炸影响范围
                AttachBomb(context, genCoord, itemData.UId);
            }
            int colorBallId = ElementSystem.Instance.GetSpecialElementConfigId(ElementType.ColorBall);
            int score = _db.CalScore(SpecialElementType.CandRB, bombId, colorBallId);
            MatchManager.Instance.AddScore(score);
            
            TaskManager.Instance.AddTaskCalculate(TaskTag.UseColorBallAndBomb);
            return 0;
        }

        /// <summary>
        /// 彩球+彩球 整个面板消除1次
        /// </summary>
        private float ColorBallAndColorBall(ElementDestroyContext context, ElementBase firstElement,
            ElementBase secondElement)
        {
            context.AddDoubleColorBallCoord(firstElement.Data.GridPos);
            context.AddDoubleColorBallCoord(secondElement.Data.GridPos);

            BombBoardPanel(context);

            int colorBallId = ElementSystem.Instance.GetSpecialElementConfigId(ElementType.ColorBall);
            int score = _db.CalScore(SpecialElementType.RandC, colorBallId, colorBallId);
            MatchManager.Instance.AddScore(score);
            
            TaskManager.Instance.AddTaskCalculate(TaskTag.UseDoubleColorBall);

            AudioUtil.PlaySound("audio/match/match_double_ball");
            return 0;
        }

        private float ColorBallAndNormal(ElementDestroyContext context, ElementBase coloredElement, ElementBase baseElement)
        {
            AttachColorBall(context, baseElement.Data.ConfigId, coloredElement.Data.GridPos, coloredElement.Data.UId);
            context.LinkSpecialBallCoord = coloredElement.Data.GridPos;
            coloredElement.Data.ColorBallDestroyId = baseElement.Data.ConfigId;
            context.IsColorBallLineNormal = true;
            return 0;
        }

        public void CollectAutoElement(ElementDestroyContext context, ElementBase element)
        {
            if (element == null)
                return;
            if (!ElementSystem.Instance.IsSpecialElement(element.Data.ElementType) && element.Data.ElementType != ElementType.BombBlock)
                return;
            if (context.WillDelCoords.FindIndex(x => x.ElementUIds.Contains(element.Data.UId)) >= 0)
                return;

            if (element.Data.ElementType == ElementType.Rocket ||
                element.Data.ElementType == ElementType.RocketHorizontal)
            {
                AttachRocket(context, element.Data.GridPos, element.Data.Direction, element.Data.UId);
            }
            else if (element.Data.ElementType == ElementType.Bomb)
            {
                AttachBomb(context, element.Data.GridPos, element.Data.UId);
            }
            else if (element.Data.ElementType == ElementType.ColorBall)
            {
                context.AddWillDelCoord(element.Data.GridPos, element.Data.EliminateStyle, element.Data.UId);
                context.AddEffGridCoord(element.Data.GridPos);
            }
            else if (element.Data.ElementType == ElementType.BombBlock)
            {
                AttachBombBlock(context, element.Data.GridPos, element.Data.UId);
            }
        }

        public void AddDelGridElement(Vector2Int coord, ElementDestroyContext context, Vector2Int bombCoord,int attachCount = 0)
        {
            var elements = ElementSystem.Instance.GetGridElements(coord, false);
            if (elements != null && elements.Count > 0)
            {
                if (context.GridSystem.IsValidPosition(coord.x, coord.y))
                {
                    context.AddGridDelCoord(coord);
                }

                int lockIndex = -1;
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Data.ElementType == ElementType.Lock)
                    {
                        lockIndex = i;
                        break;
                    }

                    if (elements[i].Data.ElementType == ElementType.TargetBlock)
                        return;
                }

                if (lockIndex >= 0)
                {
                    context.AddWillDelCoord(elements[lockIndex].Data.GridPos, elements[lockIndex].Data.EliminateStyle,
                        elements[lockIndex].Data.UId, bombCoord,attachCount);
                    if (elements[lockIndex].Data.EliminateStyle != EliminateStyle.Bomb)
                        context.AddWillDelCoord(elements[lockIndex].Data.GridPos, EliminateStyle.Bomb,
                            elements[lockIndex].Data.UId, bombCoord,attachCount);
                    context.AddEffGridCoord(elements[lockIndex].Data.GridPos);
                    // context.AddOrRemoveSpecialEleCoord(elements[lockIndex].Data.GridPos, true);
                }
                else
                {
                    for (int i = elements.Count - 1; i >= 0; i--)
                    {
                        var ele = elements[i];
                        context.AddCurrentEffectId(ele.Data.ConfigId);
                        if (ElementSystem.Instance.IsSpecialElement(ele.Data.ElementType) || ele.Data.ElementType == ElementType.BombBlock)
                        {
                            //功能棋子通过CollectAutoElement来添加
                            CollectAutoElement(context, ele);
                            continue;
                        }

                        if (ele.Data.ElementType == ElementType.Normal &&
                            !context.EffGridCoords.Contains(ele.Data.GridPos))
                        {
                            //添加计算基础元素得分
                            AddEffectBaseElementScore(ele.Data.ConfigId);
                        }

                        context.AddWillDelCoord(ele.Data.GridPos, ele.Data.EliminateStyle, ele.Data.UId, bombCoord,attachCount);
                        if (ele.Data.EliminateStyle != EliminateStyle.Bomb)
                            context.AddWillDelCoord(ele.Data.GridPos, EliminateStyle.Bomb, ele.Data.UId, bombCoord,attachCount);
                        context.AddEffGridCoord(ele.Data.GridPos);
                        if (ele.Data.HoldGrid >= 1)
                            break;
                    }
                }
            }
        }

        #endregion
    }
}