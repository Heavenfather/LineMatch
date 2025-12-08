using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using HotfixCore.Extensions;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 蔓延水视觉效果系统
    /// 处理水的流动动画、连接线、阴影等视觉效果
    /// 对应旧代码的DrawSpreadWaterUtil
    /// </summary>
    public class SpreadWaterVisualSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private GameStateContext _context;
        private IBoard _board;

        private EcsFilter _waterFilter;
        private EcsFilter _spreadFilter;
        private EcsPool<SpreadWaterComponent> _waterPool;
        private EcsPool<ElementRenderComponent> _renderPool;
        private EcsPool<ElementPositionComponent> _positionPool;
        private EcsPool<GridCellComponent> _gridCellPool;
        private EcsPool<SpreadWaterSpreadComponent> _spreadPool;

        // 水的字典：坐标 -> 实体ID
        private Dictionary<Vector2Int, int> _waterDict = new Dictionary<Vector2Int, int>();
        // 水的连接关系：水实体 -> 连接的水实体列表
        private Dictionary<int, List<int>> _waterLinkDict = new Dictionary<int, List<int>>();
        bool _isInit;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _board = _context.Board;

            _waterFilter = _world.Filter<SpreadWaterComponent>().End();
            _spreadFilter = _world.Filter<SpreadWaterSpreadComponent>().End();
            _waterPool = _world.GetPool<SpreadWaterComponent>();
            _renderPool = _world.GetPool<ElementRenderComponent>();
            _positionPool = _world.GetPool<ElementPositionComponent>();
            _gridCellPool = _world.GetPool<GridCellComponent>();
            _spreadPool = _world.GetPool<SpreadWaterSpreadComponent>();

            // 初始化所有水的视觉效果
            // InitializeAllWater();
        }

        public void Run(IEcsSystems systems)
        {
            bool hasNewWater = false;

            // 处理水蔓延的流动动画
            foreach (var entity in _waterFilter)
            {
                ref var water = ref _waterPool.Get(entity);
                ref var render = ref _renderPool.Get(entity);
                if (water.IsVisualInitialized == false && render.ViewInstance != null)
                {
                    ref var pos = ref _positionPool.Get(entity);
                    
                    var waterLine = render.ViewInstance.transform.Find("WaterLine").GetComponent<GridWaterLine>();
                    waterLine.gameObject.SetActive(false);
                    waterLine.ResetLine();

                    var waterShadow = render.ViewInstance.transform.Find("WaterShadow").GetComponent<GridWaterShadow>();
                    waterShadow.SetGridPos(new Vector2Int(pos.X, pos.Y));
                    waterShadow.HideAllShadow();
                    water.IsVisualInitialized = true;
                    
                    render.ViewInstance.Icon.SetVisible(!water.IsInitHideIcon);

                    hasNewWater = true;
                }
            }

            if (hasNewWater && !_isInit) InitializeAllWater();

            foreach (var entity in _spreadFilter)
            {
                if (_spreadPool.Has(entity))
                {
                    ref var spread = ref _spreadPool.Get(entity);
                    // 如果需要播放流动动画且还未处理
                    if (spread.NeedFlowAnimation && spread.IsProcessed && spread.SpreadTargets != null &&
                        spread.SpreadTargets.Count > 0)
                    {
                        // 播放流动动画
                        PlayFlowAnimation(entity, spread.SpreadTargets).Forget();
                        spread.NeedFlowAnimation = false;
                    }
                
                }
            }
        }

        /// <summary>
        /// 初始化所有水的视觉效果
        /// 对应旧代码的InitWater()
        /// </summary>
        private void InitializeAllWater()
        {
            _isInit = true;

            _waterDict.Clear();
            _waterLinkDict.Clear();

            // 1. 收集所有水实体
            foreach (var waterEntity in _waterFilter)
            {
                if (!_positionPool.Has(waterEntity))
                    continue;

                ref var pos = ref _positionPool.Get(waterEntity);
                Vector2Int gridPos = new Vector2Int(pos.X, pos.Y);
                _waterDict[gridPos] = waterEntity;
            }

            // 2. 建立水之间的连接关系
            if (_waterDict.Count > 0)
            {
                foreach (var kvp in _waterDict)
                {
                    int waterEntity = kvp.Value;
                    WaterLinkNeighbour(waterEntity, false);
                }
            }

            // 3. 更新所有水的阴影
            UpdateAllWaterShadow();
        }

        /// <summary>
        /// 播放水流动画
        /// 对应旧代码的DoFlowWater()
        /// </summary>
        private async UniTask PlayFlowAnimation(int spreadEntity, HashSet<Vector2Int> newWaterCoords)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

            var newWaterList = new List<Vector2Int>(newWaterCoords);
            var newWaterDict = new Dictionary<Vector2Int, int>();
            var flowBatchList = new List<List<int>>(); // 每批次要播放流动动画的水实体

            int newWaterCount = 0;
            while (true)
            {
                List<int> waterFlowBatch = new List<int>();
                newWaterCount = newWaterList.Count;

                // 找出可以连接到已有水的新水
                for (int i = newWaterList.Count - 1; i >= 0; i--)
                {
                    var neighborPos = GetNeighborPositions(newWaterList[i]);
                    bool found = false;

                    foreach (var pos in neighborPos)
                    {
                        // 如果邻居位置有水（旧水或新水）
                        if (_waterDict.ContainsKey(pos) || newWaterDict.ContainsKey(pos))
                        {
                            // 获取当前位置的水实体
                            int waterEntity = GetWaterEntityAtPosition(newWaterList[i]);
                            if (waterEntity >= 0)
                            {
                                waterFlowBatch.Add(waterEntity);
                                newWaterDict[newWaterList[i]] = waterEntity;
                                newWaterList.RemoveAt(i);
                                found = true;
                                break;
                            }
                        }

                        if (found) break;
                    }
                }

                // 如果没有找到新的可连接的水
                if (newWaterCount == newWaterList.Count)
                {
                    // 将剩余的水也加入（孤立的水）
                    foreach (var pos in newWaterList)
                    {
                        int waterEntity = GetWaterEntityAtPosition(pos);
                        if (waterEntity >= 0)
                        {
                            waterFlowBatch.Add(waterEntity);
                            newWaterDict[pos] = waterEntity;
                        }
                    }

                    if (waterFlowBatch.Count > 0)
                    {
                        flowBatchList.Add(waterFlowBatch);
                    }
                    break;
                }

                if (waterFlowBatch.Count > 0)
                {
                    flowBatchList.Add(waterFlowBatch);
                }

                if (newWaterList.Count == 0) break;
            }

            // 逐批次播放流动动画
            foreach (var waterBatch in flowBatchList)
            {
                foreach (var waterEntity in waterBatch)
                {
                    if (!_positionPool.Has(waterEntity))
                        continue;

                    var pos = _positionPool.Get(waterEntity);
                    Vector2Int waterGridPos = new Vector2Int(pos.X, pos.Y);
                    var neighborPos = GetNeighborPositions(waterGridPos);

                    // 找到邻居中的水，播放流动动画
                    for (int i = 0; i < neighborPos.Count; i++)
                    {
                        var neighborCoord = neighborPos[i];
                        if (_waterDict.ContainsKey(neighborCoord))
                        {
                            int neighborWaterEntity = _waterDict[neighborCoord];
                            ElementDirection dir = GetFlowDirection(i);



                            // 播放流动动画
                            FlowWater(neighborWaterEntity, dir, false);

                            // 更新字典和连接关系
                            _waterDict[waterGridPos] = waterEntity;
                            BindWaterLink(neighborWaterEntity, waterEntity);
                            break;
                        }
                    }

                    // 确保水在字典中
                    if (!_waterDict.ContainsKey(waterGridPos))
                    {
                        _waterDict[waterGridPos] = waterEntity;
                    }
                }

                // 等待流动动画播放
                await UniTask.Delay(TimeSpan.FromSeconds(0.25f));

                // 建立新水的连接关系
                foreach (var waterEntity in waterBatch)
                {
                    WaterLinkNeighbour(waterEntity, false);
                }
            }

            // 显示所有新水的图标
            foreach (var waterCoord in newWaterCoords)
            {
                int waterEntity = GetWaterEntityAtPosition(waterCoord);
                if (waterEntity >= 0)
                {
                    SetWaterIconVisible(waterEntity, true);
                }
            }

            // 更新所有水的阴影
            UpdateAllWaterShadow();

            _world.DelEntity(spreadEntity);
        }

        /// <summary>
        /// 连接相邻的水
        /// 对应旧代码的WaterLinkNeighbour()
        /// </summary>
        private void WaterLinkNeighbour(int waterEntity, bool isFlow)
        {
            if (!_positionPool.Has(waterEntity))
                return;

            ref var pos = ref _positionPool.Get(waterEntity);
            Vector2Int waterPos = new Vector2Int(pos.X, pos.Y);
            var neighborPos = GetNeighborPositions(waterPos);

            // 上
            if (_waterDict.ContainsKey(neighborPos[0]) && !WaterIsLinked(waterEntity, _waterDict[neighborPos[0]]))
            {
                FlowWater(waterEntity, ElementDirection.Up, isFlow);
                BindWaterLink(waterEntity, _waterDict[neighborPos[0]]);
            }

            // 左
            if (_waterDict.ContainsKey(neighborPos[1]) && !WaterIsLinked(waterEntity, _waterDict[neighborPos[1]]))
            {
                FlowWater(waterEntity, ElementDirection.Left, isFlow);
                BindWaterLink(waterEntity, _waterDict[neighborPos[1]]);
            }

            // 右
            if (_waterDict.ContainsKey(neighborPos[2]) && !WaterIsLinked(waterEntity, _waterDict[neighborPos[2]]))
            {
                FlowWater(waterEntity, ElementDirection.Right, isFlow);
                BindWaterLink(waterEntity, _waterDict[neighborPos[2]]);
            }

            // 下
            if (_waterDict.ContainsKey(neighborPos[3]) && !WaterIsLinked(waterEntity, _waterDict[neighborPos[3]]))
            {
                FlowWater(waterEntity, ElementDirection.Down, isFlow);
                BindWaterLink(waterEntity, _waterDict[neighborPos[3]]);
            }
        }

        /// <summary>
        /// 播放水流动画
        /// 对应旧代码的FlowWater()
        /// </summary>
        private void FlowWater(int waterEntity, ElementDirection dir, bool immediately)
        {
            Debug.Log("FlowWater waterEntity = " + waterEntity + " dir = " + dir);

            if (!_renderPool.Has(waterEntity))
                return;

            ref var render = ref _renderPool.Get(waterEntity);
            if (render.ViewInstance == null)
                return;

            ref var pos = ref _positionPool.Get(waterEntity);

            // 调用SpreadWaterElementItem的FlowWater方法
            var shadowNode = render.ViewInstance.transform.Find("WaterShadow");
            var waterLineNode = render.ViewInstance.transform.Find("WaterLine");
            
            var flowPos = pos.WorldPosition;
            switch (dir)
            {
                case ElementDirection.Left:
                    flowPos.x -= 0.8f;
                    break;
                case ElementDirection.Right:
                    flowPos.x += 0.8f;
                    break;
                case ElementDirection.Up:
                    flowPos.y -= 0.8f;
                    break;
                case ElementDirection.Down:
                    flowPos.y += 0.8f;
                    break;
            }

            waterLineNode.SetVisible(true);
            var waterLine = waterLineNode.GetComponent<GridWaterLine>();
            waterLine.SetMovePosition(dir, flowPos, immediately);
            var waterShadow = shadowNode.GetComponent<GridWaterShadow>();
            waterShadow.SetWaterFlow(dir);
        }

        /// <summary>
        /// 设置水图标可见性
        /// </summary>
        private void SetWaterIconVisible(int waterEntity, bool visible)
        {
            if (!_renderPool.Has(waterEntity))
                return;

            ref var render = ref _renderPool.Get(waterEntity);
            if (render.ViewInstance == null)
                return;
            render.ViewInstance.Icon.SetVisible(visible);
        }

        /// <summary>
        /// 更新所有水的阴影
        /// 对应旧代码的UpdateWaterShadow()
        /// </summary>
        private void UpdateAllWaterShadow()
        {
            foreach (var kvp in _waterDict)
            {
                Vector2Int waterPos = kvp.Key;
                int waterEntity = kvp.Value;

                var neighborPos = GetNeighborPositions(waterPos);
                var neighborWaterPosList = new List<Vector2Int>();

                foreach (var pos in neighborPos)
                {
                    if (_waterDict.ContainsKey(pos))
                    {
                        neighborWaterPosList.Add(pos);
                    }
                }

                // 更新水的阴影
                UpdateWaterShadow(waterEntity, neighborWaterPosList);
            }
        }

        /// <summary>
        /// 更新单个水的阴影
        /// </summary>
        private void UpdateWaterShadow(int waterEntity, List<Vector2Int> neighborWaterPos)
        {
            if (!_renderPool.Has(waterEntity))
                return;

            ref var render = ref _renderPool.Get(waterEntity);
            if (render.ViewInstance == null)
                return;
            
            ref var pos = ref _positionPool.Get(waterEntity);
            var shadowNode = render.ViewInstance.transform.Find("WaterShadow");
            var waterItem = shadowNode.GetComponent<GridWaterShadow>();
            if (waterItem != null)
            {
                waterItem.SetGridPos(new Vector2Int(pos.X, pos.Y));
                waterItem.UpdateShadow(neighborWaterPos);
            }
        }

        /// <summary>
        /// 绑定水的连接关系
        /// </summary>
        private void BindWaterLink(int water1, int water2)
        {
            if (!_waterLinkDict.ContainsKey(water1))
            {
                _waterLinkDict[water1] = new List<int>();
            }

            if (!_waterLinkDict[water1].Contains(water2))
            {
                _waterLinkDict[water1].Add(water2);
            }
        }

        /// <summary>
        /// 检查两个水是否已经连接
        /// </summary>
        private bool WaterIsLinked(int water1, int water2)
        {
            if (!_waterLinkDict.ContainsKey(water2))
                return false;

            return _waterLinkDict[water2].Contains(water1);
        }

        /// <summary>
        /// 获取指定位置的水实体
        /// </summary>
        private int GetWaterEntityAtPosition(Vector2Int coord)
        {
            if (!_board.TryGetGridEntity(coord.x, coord.y, out int gridEntity))
                return -1;

            ref var gridCell = ref _gridCellPool.Get(gridEntity);
            if (gridCell.IsBlank || gridCell.StackedEntityIds == null)
                return -1;

            foreach (var entityId in gridCell.StackedEntityIds)
            {
                if (_waterPool.Has(entityId))
                    return entityId;
            }

            return -1;
        }

        /// <summary>
        /// 获取四个方向的邻居位置（上、左、右、下）
        /// </summary>
        private List<Vector2Int> GetNeighborPositions(Vector2Int gridPos)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            var dirs = MatchPosUtil.NeighborDirs;
            foreach (var dir in dirs)
            {
                neighbors.Add(gridPos + dir);
            }
            return neighbors;
        }

        /// <summary>
        /// 根据索引获取方向
        /// </summary>
        private ElementDirection GetFlowDirection(int waterNeighbourIdx)
        {
            return waterNeighbourIdx switch
            {
                0 => ElementDirection.Down,
                1 => ElementDirection.Right,
                2 => ElementDirection.Left,
                3 => ElementDirection.Up,
                _ => ElementDirection.None
            };
        }


    }
}
