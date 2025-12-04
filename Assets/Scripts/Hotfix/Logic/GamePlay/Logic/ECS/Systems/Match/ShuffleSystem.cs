using System.Collections.Generic;
using GameConfig;
using GameCore.Localization;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 洗牌系统
    /// 处理骰子道具的洗牌逻辑
    /// </summary>
    public class ShuffleSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;
        private GameStateContext _context;
        private IBoard _board;
        private IMatchService _matchService;

        // 过滤器
        private EcsFilter _shuffleFilter; // 洗牌请求
        private EcsPool<SpecialElementComponent> _specialPool;

        // 用于判断棋盘是否忙碌
        private EcsFilter _boardSystemCheckFilter;

        // 组件池
        private EcsPool<ShuffleRequestComponent> _reqPool;
        private EcsPool<ElementComponent> _elePool;
        private EcsPool<GridCellComponent> _gridPool;
        private EcsPool<ElementPositionComponent> _posPool;
        private EcsPool<NormalElementComponent> _normalPool;
        private EcsPool<ShuffleAnimationComponent> _animPool;
        private bool _checkOnce;

        // 临时缓存
        public struct ShuffleNode
        {
            public int EntityId;
            public Vector2Int CurrentPos;
            public int ConfigId;
            public ElementType Type;
        }

        private List<ShuffleNode> _nodes = new List<ShuffleNode>();
        private List<Vector2Int> _availablePositions = new List<Vector2Int>();

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _matchService = _context.ServiceFactory.GetService(_context.CurrentMatchType);
            _board = _context.Board;

            _specialPool = _world.GetPool<SpecialElementComponent>();
            _reqPool = _world.GetPool<ShuffleRequestComponent>();
            _elePool = _world.GetPool<ElementComponent>();
            _gridPool = _world.GetPool<GridCellComponent>();
            _normalPool = _world.GetPool<NormalElementComponent>();
            _posPool = _world.GetPool<ElementPositionComponent>();
            _animPool = _world.GetPool<ShuffleAnimationComponent>();

            // 1. 监听洗牌请求
            _shuffleFilter = _world.Filter<ShuffleRequestComponent>().End();

            // 2. 监听棋盘稳定通知
            _boardSystemCheckFilter = _world.Filter<BoardStableCheckSystemTag>().End();
        }

        public void Run(IEcsSystems systems)
        {
            if (_shuffleFilter.GetEntitiesCount() > 0)
            {
                // 洗牌请求
                foreach (var entity in _shuffleFilter)
                {
                    if (!ExecuteShuffle())
                    {
                        // 极端情况：如果连洗牌都洗不了（比如只有一个棋子，或者全是孤岛）
                        Logger.Info("Shuffle Failed! Trigger Game Over.");
                        TriggerGameFail();
                    }

                    _world.DelEntity(entity);
                }

                return;
            }

            if (!IsGameIdle())
            {
                _checkOnce = false;
                return;
            }

            if (_checkOnce)
                return;

            _checkOnce = true;
            // 执行死局检测
            CheckDeadlock();
        }

        /// <summary>
        /// 执行死局检测
        /// </summary>
        private void CheckDeadlock()
        {
            // 重新收集当前棋盘上的棋子信息
            CollectNodes();

            if (_nodes.Count < 2) return;

            // 基于收集到的数据进行算法分析
            if (!HasPotentialMatch(_nodes))
            {
                CreateShuffleRequest();
            }
        }

        /// <summary>
        /// 检查游戏是否处于空闲状态
        /// 只有当所有系统都"沉默"了，才是真正的回合结束
        /// </summary>
        private bool IsGameIdle()
        {
            return _boardSystemCheckFilter.GetEntitiesCount() > 0;
        }

        /// <summary>
        /// 发起洗牌请求
        /// </summary>
        private void CreateShuffleRequest()
        {
            int entity = _world.NewEntity();
            ref var req = ref _reqPool.Add(entity);
        }

        private bool ExecuteShuffle()
        {
            // 1. 收集数据
            CollectNodes();
            if (_nodes.Count < 2) return false; // 无法洗牌

            // 2. 提取位置列表并打乱
            _availablePositions.Clear();
            foreach (var node in _nodes) _availablePositions.Add(node.CurrentPos);

            // 尝试多次洗牌以寻找自然解
            int maxAttempts = 10;
            bool foundSolution = false;

            List<ShuffleNode> simNodes = new List<ShuffleNode>(_nodes);

            for (int i = 0; i < maxAttempts; i++)
            {
                // 打乱位置列表
                ShufflePositionsList(_availablePositions);

                // 将打乱后的位置临时赋给节点进行检查
                for (int j = 0; j < simNodes.Count; j++)
                {
                    var node = simNodes[j];
                    node.CurrentPos = _availablePositions[j]; // 赋予新位置
                    simNodes[j] = node;
                }

                if (HasPotentialMatch(simNodes))
                {
                    foundSolution = true;
                    break;
                }
            }

            // 3. 强制策略
            if (!foundSolution)
            {
                Logger.Info("[BoardShuffleSystem] Random shuffle failed. Applying ForcePair...");
                if (!ApplyForcePair(simNodes))
                {
                    // 如果连强制配对都做不到（说明没有相邻的格子），那就是真·死局
                    return false;
                }
                else
                {
                    CommonUtil.ShowCommonTips(LocalizationPool.Get("Match/ForceChanged"));
                }
            }
            else
            {
                CommonUtil.ShowCommonTips(LocalizationPool.Get("Match/NotPair"));
            }

            // 4. 应用最终结果 (动画 & 数据更新)
            ApplyShuffleResult(simNodes);
            return true;
        }

        private void ShufflePositionsList(List<Vector2Int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// 强制成对策略
        /// </summary>
        private bool ApplyForcePair(List<ShuffleNode> finalNodes)
        {
            // 构建位置映射表方便查找
            Dictionary<Vector2Int, int> posToIndex = new Dictionary<Vector2Int, int>();
            for (int i = 0; i < finalNodes.Count; i++)
            {
                posToIndex[finalNodes[i].CurrentPos] = i;
            }

            // 遍历所有节点，寻找物理相邻的节点
            // 只需要找到一对即可
            foreach (var node in finalNodes)
            {
                Vector2Int[] dirs = { Vector2Int.right, Vector2Int.down };
                foreach (var dir in dirs)
                {
                    Vector2Int neighborPos = node.CurrentPos + dir;
                    if (posToIndex.TryGetValue(neighborPos, out int neighborIdx))
                    {
                        // 找到了相邻的两个棋子：node 和 finalNodes[neighborIdx]
                        // 强制修改邻居的 ConfigId 与当前节点一致
                        var neighborNode = finalNodes[neighborIdx];

                        // 这里修改的是 struct 副本，后续 ApplyShuffleResult 会应用这个 ID
                        neighborNode.ConfigId = node.ConfigId;
                        finalNodes[neighborIdx] = neighborNode;
                        return true;
                    }
                }
            }

            // 遍历完了都没找到一对相邻的 说明棋盘全是孤岛
            return false;
        }

        private void ApplyShuffleResult(List<ShuffleNode> finalNodes)
        {
            // 此时 finalNodes[i].EntityId 是原来的实体
            // finalNodes[i].CurrentPos 是新的目标位置
            // finalNodes[i].ConfigId 是最终的配置ID (可能被 ForcePair 改过)

            foreach (var node in finalNodes)
            {
                int entity = node.EntityId;
                ref var ele = ref _elePool.Get(entity);
                ref var posComp = ref _posPool.Get(entity);

                // 记录旧位置 (用于动画起点)
                Vector2Int oldPos = new Vector2Int(posComp.X, posComp.Y);
                Vector2Int newPos = node.CurrentPos;

                // 如果位置变了，处理位移逻辑
                if (oldPos != newPos)
                {
                    // 从旧格子移除引用
                    RemoveFromGrid(oldPos, entity);

                    // 更新 Position 组件
                    posComp.X = newPos.x;
                    posComp.Y = newPos.y;
                    posComp.WorldPosition = GetWorldPos(newPos);

                    // 添加到新格子引用
                    AddToGrid(newPos, entity);

                    // 添加洗牌动画组件
                    ref var anim = ref _animPool.Add(entity);
                    // 需要转换成世界坐标
                    // anim.StartPos = GetWorldPos(oldPos);
                    // anim.EndPos = GetWorldPos(newPos);
                }
                else if (ele.ConfigId != node.ConfigId)
                {
                    // 位置没变但颜色变了（ForcePair 原地修改），给个 Punch 动画
                    if (_normalPool.Has(entity))
                    {
                        ref var normal = ref _normalPool.Get(entity);
                        normal.ScaleState = ElementScaleState.Change;
                        normal.IsAnimDirty = true;
                    }
                }
            }
        }

        private void RemoveFromGrid(Vector2Int pos, int entityId)
        {
            if (_board.TryGetGridEntity(pos.x, pos.y, out int gridEnt))
            {
                ref var grid = ref _gridPool.Get(gridEnt);
                grid.StackedEntityIds?.Remove(entityId);
            }
        }

        private void AddToGrid(Vector2Int pos, int entityId)
        {
            if (_board.TryGetGridEntity(pos.x, pos.y, out int gridEnt))
            {
                ref var grid = ref _gridPool.Get(gridEnt);
                if (grid.StackedEntityIds == null) grid.StackedEntityIds = new List<int>();
                grid.StackedEntityIds.Add(entityId);
                var renderPool = _world.GetPool<ElementRenderComponent>();
                ref var renderCom = ref renderPool.Get(entityId);
                if (renderCom.ViewInstance != null)
                {
                    var gridParent = _board.GetGridInstance(pos.x, pos.y);
                    if (gridParent != null)
                    {
                        renderCom.ViewInstance.transform.SetParent(gridParent.transform);
                    }
                }
            }
        }

        private Vector3 GetWorldPos(Vector2Int gridPos)
        {
            return MatchPosUtil.CalculateWorldPosition(gridPos.x, gridPos.y, 1, 1, ElementDirection.None);
        }

        private void CollectNodes()
        {
            _nodes.Clear();
            for (int x = 0; x < _board.Width; x++)
            {
                for (int y = 0; y < _board.Height; y++)
                {
                    if (!_board.IsValid(x, y)) continue;
                    int gridEntity = _board[x, y];
                    if (!_gridPool.Has(gridEntity)) continue;
                    ref var grid = ref _gridPool.Get(gridEntity);

                    if (grid.StackedEntityIds == null) continue;
                    foreach (var id in grid.StackedEntityIds)
                    {
                        if (!_elePool.Has(id)) continue;
                        ref var ele = ref _elePool.Get(id);

                        // 只洗普通棋子 (且不是障碍/功能棋子)
                        if (IsCanCollect(grid.StackedEntityIds, out int normalEntity))
                        {
                            _nodes.Add(new ShuffleNode
                            {
                                EntityId = normalEntity,
                                CurrentPos = new Vector2Int(x, y),
                                ConfigId = ele.ConfigId,
                                Type = ele.Type
                            });
                            break; // 每格只取一个
                        }
                    }
                }
            }
        }

        private bool IsCanCollect(List<int> entities, out int normalEntity)
        {
            bool haveLock = false;
            normalEntity = -1;
            for (int i = 0; i < entities.Count; i++)
            {
                ref var ele = ref _elePool.Get(entities[i]);
                if (ele.Type == ElementType.Lock ||
                    ele.Type == ElementType.TargetBlock)
                {
                    haveLock = true;
                    break;
                }

                if (ele.Type == ElementType.Normal)
                {
                    normalEntity = entities[i];
                }

                // 也收集特殊棋子
                if (_specialPool.Has(entities[i]))
                {
                    normalEntity = entities[i];
                }
            }

            return !haveLock && normalEntity != -1;
        }

        private bool HasPotentialMatch(List<ShuffleNode> nodeList)
        {
            // 将 list 转为 map 以便快速查找
            Dictionary<Vector2Int, int> map = new Dictionary<Vector2Int, int>(nodeList.Count);
            foreach (var node in nodeList) map[node.CurrentPos] = node.ConfigId;

            foreach (var node in nodeList)
            {
                Vector2Int[] dirs = { Vector2Int.right, Vector2Int.down };
                foreach (var dir in dirs)
                {
                    Vector2Int next = node.CurrentPos + dir;
                    if (map.TryGetValue(next, out int nextConfig))
                    {
                        if (nextConfig == node.ConfigId) return true;
                    }
                }
            }

            if (_matchService.HasPotentialMatch(_world, nodeList))
            {
                return true;
            }

            return false;
        }

        private void TriggerGameFail()
        {
            G.EventModule.DispatchEvent(GameEventDefine.OnGameFailure, EventOneParam<bool>.Create(true));
        }
    }
}