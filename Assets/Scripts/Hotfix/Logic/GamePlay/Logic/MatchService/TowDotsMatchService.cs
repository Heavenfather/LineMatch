using System.Collections.Generic;
using GameConfig;
using GameCore.Localization;
using Hotfix.Define;
using Hotfix.Utils;
using HotfixLogic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// TowDots模式的消除模式
    /// </summary>
    public class TowDotsMatchService : IMatchService
    {
        private IBoardSpawnStrategy _spawnStrategy;

        public MatchServiceType MatchServiceType => MatchServiceType.TowDots;

        public int[] SpecialElements { get; } = new int[] { 9, 12, 13, 14, 15, 16 };

        public bool IsBlockingBaseElement(int elementId)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[elementId];

            // --- 扩展点：在这里处理特殊例外 ---

            // eg. if (config.elementType == ElementType.Decoration) return false;

            // 默认逻辑：占用格子大于等于1就算阻挡
            return config.holdGrid >= 1;
        }

        public bool IsSpecialElement(int elementId)
        {
            for (int i = 0; i < SpecialElements.Length; i++)
            {
                if (SpecialElements[i] == elementId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanConnect(EcsWorld world, in ElementComponent fromComponent, in ElementComponent toComponent,
            int matchConfigId, int fromEntity, int toEntity, List<int> currentSelectedEntities)
        {
            if (fromComponent.Type == ElementType.Bomb || toComponent.Type == ElementType.Bomb)
                return false;

            //星爆点和普通棋子 星爆点本身有颜色，需要同色棋子
            bool isStarBombAndNormal =
                (fromComponent.Type == ElementType.StarBomb && toComponent.Type == ElementType.Normal) ||
                (fromComponent.Type == ElementType.Normal && toComponent.Type == ElementType.StarBomb);
            if (isStarBombAndNormal)
            {
                int entity = fromComponent.Type == ElementType.StarBomb ? fromEntity : toEntity;
                var starBombPool = world.GetPool<StarBombComponent>();
                if (starBombPool.Has(entity))
                {
                    ref var starBomb = ref starBombPool.Get(entity);
                    int configId = fromComponent.Type == ElementType.Normal
                        ? fromComponent.ConfigId
                        : toComponent.ConfigId;
                    return starBomb.StarDotBaseElementId == configId;
                }
            }

            //搜寻点和普通棋子
            bool isSearchDotAndNormal =
                (fromComponent.Type == ElementType.SearchDot && toComponent.Type == ElementType.Normal) ||
                (fromComponent.Type == ElementType.Normal && toComponent.Type == ElementType.SearchDot);
            if (isSearchDotAndNormal)
            {
                int entity = fromComponent.Type == ElementType.SearchDot ? fromEntity : toEntity;
                var searchPool = world.GetPool<SearchDotComponent>();
                if (searchPool.Has(entity))
                {
                    ref var searchCom = ref searchPool.Get(entity);
                    int configId = fromComponent.Type == ElementType.Normal
                        ? fromComponent.ConfigId
                        : toComponent.ConfigId;
                    return searchCom.SearchDotBaseElementId == configId;
                }
            }

            //白色的点可以和任意点连 也就是只要任意的棋子是 IsMatchable 那就是可连的
            return fromComponent.IsMatchable && toComponent.IsMatchable;
        }

        public bool IsWhiteDot(int elementId)
        {
            return elementId == SpecialElements[3] ||
                   elementId == SpecialElements[4] ||
                   elementId == SpecialElements[5];
        }

        public int ElementType2ConfigId(ElementType elementType)
        {
            if (elementType == ElementType.Bomb)
                return SpecialElements[0];
            if (elementType == ElementType.StarBomb)
                return SpecialElements[1];
            if (elementType == ElementType.SearchDot)
                return SpecialElements[2];
            if (elementType == ElementType.HorizontalDot)
                return SpecialElements[3];
            if (elementType == ElementType.TowDotsBombDot)
                return SpecialElements[4];
            if (elementType == ElementType.TowDotsColoredDot)
                return SpecialElements[5];
            return -1;
        }

        public IBoardSpawnStrategy GetSpawnStrategy()
        {
            if (_spawnStrategy == null)
            {
                _spawnStrategy = new DirectSpawnStrategy();
            }

            return _spawnStrategy;
        }

        public void CheckOneElementRequest(EcsWorld world, int elementEntity)
        {
            //TowDots模式目前没有单个元素就触发的
        }

        public List<AtomicAction> MatchRuleAction(MatchRuleContext context, List<Vector2Int> closedLoopPaths)
        {
            if (closedLoopPaths.Count == 4)
            {
                // 4个方格，只有分数
                return new List<AtomicAction>()
                {
                    AddScoreAction(OneTakeScoreType.FourRect, context.Request.ConfigId,
                        context.Request.InvolvedEntities.Count)
                };
            }

            var actions = new List<AtomicAction>(2);

            // 1.生成功能棋子
            var genData = GetGenItems(context, closedLoopPaths);
            MatchGenerateFunctionItem items = new MatchGenerateFunctionItem()
            {
                GenItemsData = genData
            };
            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            var genItemAction = factory.CreateAtomicAction(MatchActionType.Spawn2Other, extraData: items);
            actions.Add(genItemAction);

            // 2.加分
            var scoreAction = AddScoreAction(OneTakeScoreType.BombRect, context.Request.ConfigId,
                context.Request.InvolvedEntities.Count);
            actions.Add(scoreAction);
            return actions;
        }

        public List<Vector2Int> GetBombPos(Vector2Int bombPos)
        {
            //爆炸范围3x3
            return MatchPosUtil.GetEightNeighborPos(bombPos);
        }

        public IMatchRule GetMatchRule(EcsWorld world, List<int> selectEntities)
        {
            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            var elementPool = world.GetPool<SpecialElementComponent>();
            foreach (var entity in selectEntities)
            {
                if (!elementPool.Has(entity))
                    continue;
                // TowDots的特殊棋子，统一使用 TowDotsFunctionElementRule 规则处理
                return factory.GetMatchRule(MatchRequestType.TowDotsFunctionElement);
            }

            return null;
        }

        public bool IsGeometricSquare(List<int> currentPathGridIds, int nextGridId)
        {
            if (currentPathGridIds == null) return false;

            int index = currentPathGridIds.IndexOf(nextGridId);

            // 连接到的点必须在路径中，且距离队尾至少4个点（防止回退误判）
            if (index >= 0 && (currentPathGridIds.Count - index) >= 4)
            {
                return true;
            }

            return false;
        }

        public bool IsCountSquare(int connectCount)
        {
            int threshold = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("MatchLineCCount");
            return connectCount >= threshold;
        }

        public int RandomFunctionElement()
        {
            return 9;
        }

        public bool CheckItemTarget(EcsWorld world, int itemId, List<int> elementEntities)
        {
            if (itemId == (int)ItemDef.EliminateHammer)
            {
                var elePool = world.GetPool<ElementComponent>();
                for (int i = 0; i < elementEntities.Count; i++)
                {
                    ref var elementComponent = ref elePool.Get(elementEntities[i]);
                    if (elementComponent.EliminateStyle == EliminateStyle.Drop ||
                        elementComponent.EliminateStyle == EliminateStyle.Target)
                        return false;
                }
            }

            if (itemId == (int)ItemDef.EliminateColored)
            {
                var elePool = world.GetPool<ElementComponent>();
                for (int i = 0; i < elementEntities.Count; i++)
                {
                    ref var elementComponent = ref elePool.Get(elementEntities[i]);
                    if (elementComponent.EliminateStyle == EliminateStyle.Target)
                        return false;
                }

                var normalElement = world.GetPool<NormalElementComponent>();
                bool noneNormal = true;
                for (int i = 0; i < elementEntities.Count; i++)
                {
                    if (normalElement.Has(elementEntities[i]))
                    {
                        noneNormal = false;
                        break;
                    }
                }

                if (noneNormal)
                {
                    CommonUtil.ShowCommonTips(LocalizationPool.Get("Match/SelectNormalElement"));
                    return false;
                }
            }

            return true;
        }

        private List<GenItemData> GetGenItems(MatchRuleContext context, List<Vector2Int> closedLoop)
        {
            int minY = closedLoop[0].y;
            int maxY = closedLoop[0].y;

            for (int i = 1; i < closedLoop.Count; i++)
            {
                var coord = closedLoop[i];
                minY = Mathf.Min(minY, coord.y);
                maxY = Mathf.Max(maxY, coord.y);
            }

            var boundaryCoords = new HashSet<Vector2Int>(closedLoop.Count);
            for (int i = 0; i < closedLoop.Count; i++)
            {
                boundaryCoords.Add(closedLoop[i]);
            }

            var world = context.World;
            var gridCellPool = world.GetPool<GridCellComponent>();

            //逐行扫描
            List<GenItemData> genItems = new List<GenItemData>();
            for (int y = minY; y <= maxY; y++)
            {
                var intersectionsX = new List<int>();

                // 计算当前扫描线与多边形边的交点
                for (int i = 0; i < closedLoop.Count; i++)
                {
                    Vector2Int p1 = closedLoop[i];
                    Vector2Int p2 = closedLoop[(i + 1) % closedLoop.Count];

                    if (p1.y <= y && p2.y > y && p1.y != p2.y)
                    {
                        float t = (float)(y - p1.y) / (p2.y - p1.y);
                        float ix = p1.x + t * (p2.x - p1.x);
                        intersectionsX.Add(Mathf.RoundToInt(ix));
                    }
                    else if (p2.y <= y && p1.y > y && p1.y != p2.y)
                    {
                        float t = (float)(y - p2.y) / (p1.y - p2.y);
                        float ix = p2.x + t * (p1.x - p2.x);
                        intersectionsX.Add(Mathf.RoundToInt(ix));
                    }
                    else if (p1.y == p2.y && p1.y == y)
                    {
                        int minX = Mathf.Min(p1.x, p2.x);
                        int maxX = Mathf.Max(p1.x, p2.x);
                        for (int x = minX; x <= maxX; x++)
                        {
                            Vector2Int pt = new Vector2Int(x, y);
                            boundaryCoords.Add(pt); // 标记水平段作为边界点
                        }
                    }
                }

                intersectionsX.Sort();
                for (int k = 0; k < intersectionsX.Count; k += 2)
                {
                    if (k + 1 >= intersectionsX.Count) break;

                    int startX = intersectionsX[k];
                    int endX = intersectionsX[k + 1];

                    for (int x = startX; x < endX; ++x)
                    {
                        Vector2Int pt = new Vector2Int(x, y);
                        if (!boundaryCoords.Contains(pt))
                        {
                            // 1.获取格子上的组件
                            var gridEntity = context.Board[pt.x, pt.y];
                            ref var gridCell = ref gridCellPool.Get(gridEntity);
                            if (gridCell.IsBlank)
                                continue;
                            // 2.获取堆叠元素 判断添加格子上能否添加功能棋子
                            if (TryGenGridItem(world, gridCell.StackedEntityIds, out var genItem))
                            {
                                genItems.Add(genItem);
                                TaskManager.Instance.AddTaskCalculate(TaskTag.GenBomb);
                            }
                        }
                    }
                }
            }

            return genItems;
        }

        private bool TryGenGridItem(EcsWorld world, List<int> elementEntities, out GenItemData genItem)
        {
            var elementCom = world.GetPool<ElementComponent>();
            var posPool = world.GetPool<ElementPositionComponent>();
            genItem = default;
            int normalElementEntity = -1;
            if (elementEntities.Count == 1)
            {
                // 1.单元格上只有一个元素
                ref var com = ref elementCom.Get(elementEntities[0]);
                if (com.Type != ElementType.Normal)
                    return false; //不是普通元素不会生成
                normalElementEntity = elementEntities[0];
            }
            else
            {
                // 2.单元格上有多个元素 需要确定是否可以获取到普通颜色棋子
                bool haveLock = false;
                for (int i = 0; i < elementEntities.Count; i++)
                {
                    var entity = elementEntities[i];
                    ref var com = ref elementCom.Get(entity);
                    if (com.Type == ElementType.Lock || com.Type == ElementType.TargetBlock)
                    {
                        haveLock = true;
                        break;
                    }

                    if (com.Type == ElementType.Normal)
                    {
                        normalElementEntity = entity;
                        break;
                    }
                }

                if (haveLock)
                    return false; //这个格子不允许生成功能棋子
            }

            if (normalElementEntity == -1)
                return false;
            // 3.可以在此生成功能棋子
            ref var posCom = ref posPool.Get(normalElementEntity);
            genItem = new GenItemData()
            {
                ConfigId = ElementType2ConfigId(ElementType.Bomb),
                GenCoord = new Vector2Int(posCom.X, posCom.Y),
                ElementSize = new Vector2Int(elementCom.Get(normalElementEntity).Width,
                    elementCom.Get(normalElementEntity).Height)
            };
            return true;
        }

        private AtomicAction AddScoreAction(OneTakeScoreType scoreType, int configId, int count)
        {
            var db = ConfigMemoryPool.Get<BlockDiffScoreDB>();
            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            var action = factory.CreateAtomicAction(MatchActionType.AddScore,
                value: db.CalScore(configId, count, scoreType));
            return action;
        }
    }
}