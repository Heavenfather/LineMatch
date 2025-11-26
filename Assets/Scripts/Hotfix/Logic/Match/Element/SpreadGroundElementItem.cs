using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    public class SpreadGroundElementItem : BlockElementItem
    {
        private HashSet<Vector2Int> _expansionCoords = new HashSet<Vector2Int>();
        private int _genNextElement = -1;
        private int _targetId = -1;
        private const float _waitTime = 1.5f;

        //2x2矩阵
        private readonly Vector2Int[] _rect = new Vector2Int[3] { new(0, 1), new(1, 0), new(1, 1) };

        protected override void OnInitialized()
        {
            _genNextElement = Data.NextBlockId;
            BuildTargetId(_genNextElement);
            base.OnInitialized();
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            Data.EliminateCount--;
            ExpansionElement(context);
            PlayEffect();
            return true;
        }

        public override void Clear()
        {
            base.Clear();
            _expansionCoords?.Clear();
        }
        
        private void ExpansionElement(ElementDestroyContext context)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ElementMap config = db[Data.ConfigId];
            _expansionCoords.Clear();
            if (int.TryParse(config.extra, out int layer))
            {
                CalculateExpansion(Data.GridPos, layer, context.GridSystem);
                PushGroundElementToGrid(context);
            }
        }

        private void CalculateExpansion(Vector2Int startCoord, int layers, GridSystem gridSystem)
        {
            AddBaseTiles(startCoord);
            if (layers <= 0)
                return;

            ExpandAdaptively(startCoord, layers, gridSystem);
        }

        private void AddBaseTiles(Vector2Int origin)
        {
            // 2x2元素的四个坐标点
            foreach (var tile in _rect)
            {
                Vector2Int coord = tile + origin;
                _expansionCoords.Add(coord);
            }
        }

        private void ExpandAdaptively(Vector2Int origin, int layers, GridSystem gridSystem)
        {
            // 计算理想正方形边界
            (int width, int height) = gridSystem.GetBoardSize();
            bool isCanInfectRight = IsCanInfectCoord(new Vector2Int(origin.x + 1 + layers, origin.y), gridSystem);
            bool isCanInfectLeft = IsCanInfectCoord(new Vector2Int(origin.x - layers, origin.y), gridSystem);
            bool isCanInfectTop = IsCanInfectCoord(new Vector2Int(origin.x, origin.y - layers), gridSystem);
            bool isCanInfectBottom = IsCanInfectCoord(new Vector2Int(origin.x, origin.y + 1 + layers), gridSystem);
            int idealMinX = 0;
            int idealMaxX = 0;
            int idealMinY = 0;
            int idealMaxY = 0;
            bool isSquare = false;
            if (isCanInfectRight && isCanInfectTop)
            {
                //上、右扩散正方向
                idealMinX = origin.x;
                idealMaxX = origin.x + 1 + layers;
                idealMinY = origin.y - layers;
                idealMaxY = origin.y + 1;
                isSquare = true;
            }
            else if (isCanInfectLeft && isCanInfectBottom)
            {
                //下、左扩散正方向
                idealMinX = origin.x - layers;
                idealMaxX = origin.x;
                idealMinY = origin.y;
                idealMaxY = origin.y + 1 + layers;
                isSquare = true;
            }
            else if (isCanInfectTop && isCanInfectLeft)
            {
                //左、上扩散正方向
                idealMinX = origin.x - layers;
                idealMaxX = origin.x;
                idealMinY = origin.y - layers;
                idealMaxY = origin.y;
                isSquare = true;
            }
            else if (isCanInfectBottom && isCanInfectRight)
            {
                //右、下扩散正方向
                idealMinX = origin.x;
                idealMaxX = origin.x + 1 + layers;
                idealMinY = origin.y;
                idealMaxY = origin.y + 1 + layers;
                isSquare = true;
            }

            if (isSquare)
            {
                // 计算实际可用边界
                int actualMinX = Mathf.Max(idealMinX, 0);
                int actualMaxX = Mathf.Min(idealMaxX, width - 1);
                int actualMinY = Mathf.Max(idealMinY, 0);
                int actualMaxY = Mathf.Min(idealMaxY, height - 1);

                // 添加扩散区域的所有格子
                for (int x = actualMinX; x <= actualMaxX; x++)
                {
                    for (int y = actualMinY; y <= actualMaxY; y++)
                    {
                        // 跳过原始2x2区域（已添加）
                        if (x >= origin.x && x <= origin.x + 1 &&
                            y >= origin.y && y <= origin.y + 1) continue;

                        TryAddPosition(new Vector2Int(x, y), gridSystem);
                    }
                }
            }
            else
            {
                //矩形扩散模式
                ExpandToRectangle(origin, layers, gridSystem);
            }
        }

        // 边界不足时扩展为最大矩形
        private void ExpandToRectangle(Vector2Int origin, int layers, GridSystem gridSystem)
        {
            // 计算最大可能矩形边界
            (int width, int height) = gridSystem.GetBoardSize();
            int minX = Mathf.Max(origin.x - layers, 0);
            int maxX = Mathf.Min(origin.x + 1 + layers, width - 1);
            int minY = Mathf.Max(origin.y - layers, 0);
            int maxY = Mathf.Min(origin.y + 1 + layers, height - 1);

            // 添加边界扩展区域
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    // 跳过原始2x2区域（已添加）
                    if (x >= origin.x && x <= origin.x + 1 &&
                        y >= origin.y && y <= origin.y + 1) continue;
                    TryAddPosition(new Vector2Int(x, y), gridSystem);
                }
            }
        }

        private void TryAddPosition(Vector2Int pos, GridSystem gridSystem)
        {
            if (IsCanInfectCoord(pos, gridSystem))
            {
                _expansionCoords.Add(pos);
            }
        }

        private void PushGroundElementToGrid(ElementDestroyContext context)
        {
            foreach (var coord in _expansionCoords)
            {
                var grid = context.GridSystem.GetGridByCoord(coord);
                if (grid == null)
                    continue;
                if(coord == Data.GridPos)
                    continue;
                var element = GenGroundElement(coord, grid.GameObject);
                element.GameObject.transform.localPosition = Vector3.zero;
                grid.PushElement(element, false);
                context.AddHoldSpreadElement(coord, element.Data.UId);
            }

            G.EventModule.DispatchEvent(GameEventDefine.OnMatchAddTargetNum,
                EventTwoParam<int, int>.Create(_targetId, _expansionCoords.Count + 1)); //+1是加上自身位置的元素，自身生成的元素在GridItem中添加
        }

        private bool IsCanInfectCoord(Vector2Int coord, GridSystem gridSystem)
        {
            if (!gridSystem.IsValidPosition(coord.x, coord.y))
                return false;
            var elements = ElementSystem.Instance.GetGridElements(coord, false);
            if (elements == null)
                return false;
            if (elements.Count <= 0)
                return true;
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            for (int i = 0; i < elements.Count; i++)
            {
                //不允许扩展条件
                // 1.元素占格是4格的
                // 2.元素是背景元素
                // 3.元素是循环收集的元素
                if (elements[i].Data.HoldGrid >= 4 ||
                    elements[i].Data.ElementType == ElementType.Background ||
                    db.IsCircleElement(elements[i].Data.ConfigId))
                    return false;
            }

            return true;
        }

        private ElementBase GenGroundElement(Vector2Int coord, GameObject parent)
        {
            ElementItemData elementData = ElementSystem.Instance.GenElementItemData(_genNextElement, coord.x, coord.y);
            var element = ElementSystem.Instance.GenElement(elementData, parent.transform);
            return element;
        }

        private void BuildTargetId(int genNextId)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[genNextId];
            if (config.nextBlock > 0)
            {
                if (_targetId == genNextId)
                    return;
                BuildTargetId(config.nextBlock);
            }
            else
            {
                _targetId = genNextId;
            }
        }
    }
}