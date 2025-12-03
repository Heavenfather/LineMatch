using System.Collections.Generic;
using GameConfig;
using HotfixLogic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 旧机制下的常用的消除模式
    /// </summary>
    public class NormalMatchService : IMatchService
    {
        private IBoardSpawnStrategy _spawnStrategy;

        public MatchServiceType MatchServiceType => MatchServiceType.Normal;

        public int[] SpecialElements { get; } = new int[] { 8, 9, 10, 11 };

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
            if (currentSelectedEntities != null && currentSelectedEntities.Count > 1)
            {
                // 已经连了功能棋子，不能再连
                var specialPool = world.GetPool<SpecialElementComponent>();
                for (int i = 0; i < currentSelectedEntities.Count; i++)
                {
                    if (specialPool.Has(currentSelectedEntities[i]))
                        return false;
                }
            }

            // 彩球可以和任意棋子相连
            if (fromComponent.Type == ElementType.ColorBall && toComponent.IsMatchable)
                return true;
            // 功能棋子可以相连
            if (IsSpecialElement(fromComponent.ConfigId) && IsSpecialElement(toComponent.ConfigId))
                return true;

            return false;
        }

        public int ElementType2ConfigId(ElementType elementType)
        {
            if (elementType == ElementType.Rocket)
                return SpecialElements[0];
            if (elementType == ElementType.RocketHorizontal)
                return SpecialElements[3];
            if (elementType == ElementType.Bomb)
                return SpecialElements[1];
            if (elementType == ElementType.ColorBlock)
                return SpecialElements[2];
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
            //普通模式，有单个火箭触发和单个炸弹触发
            var requestService = MatchBoot.Container.Resolve<IMatchRequestService>();

            var rocketComponent = world.GetPool<RocketComponent>();
            if (rocketComponent.Has(elementEntity))
            {
                requestService.RequestRocket(world, elementEntity);
                return;
            }

            var bombComponent = world.GetPool<BombComponent>();
            if (bombComponent.Has(elementEntity))
            {
                ref var bombCom = ref world.GetPool<BombComponent>().Get(elementEntity);
                bombCom.AutoBomb = true;
                // 交给炸弹系统去处理
                var stablePool = world.GetPool<BoardStableCheckTag>();
                if(!stablePool.Has(elementEntity))
                    stablePool.Add(elementEntity);
                
                // requestService.RequestBomb(world, elementEntity);
                return;
            }
        }

        public List<AtomicAction> MatchRuleAction(MatchRuleContext context, List<Vector2Int> closedLoopPaths)
        {
            var genData = GenElementData(closedLoopPaths, out bool result, out var scoreType);
            if (!result)
            {
                // 4个方格，只有分数
                return new List<AtomicAction>()
                {
                    AddScoreAction(OneTakeScoreType.FourRect, context.Request.ConfigId,
                        context.Request.InvolvedEntities.Count)
                };
            }

            var actions = new List<AtomicAction>(2);
            MatchGenerateFunctionItem items = new MatchGenerateFunctionItem()
            {
                GenItemsData = new List<GenItemData>() { genData }
            };

            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            var genItemAction =
                factory.CreateAtomicAction(MatchActionType.Spawn2Other, extraData: items);
            actions.Add(genItemAction);

            var scoreAction =
                AddScoreAction(scoreType, context.Request.ConfigId, context.Request.InvolvedEntities.Count);
            actions.Add(scoreAction);

            return actions;
        }

        public List<Vector2Int> GetBombPos(Vector2Int bombPos)
        {
            //爆炸周围5x5的格子
            List<Vector2Int> bombCoords = new List<Vector2Int>();
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dy = -2; dy <= 2; dy++)
                {
                    Vector2Int bombCoord = new Vector2Int(bombPos.x + dx, bombPos.y + dy);
                    if (bombCoord != bombPos) // 避免重复添加中心点
                    {
                        bombCoords.Add(bombCoord);
                    }
                }
            }

            return bombCoords;
        }

        public IMatchRule GetMatchRule(EcsWorld world, List<int> selectEntities)
        {
            List<SpecialElementComponent> functionElementIds = new List<SpecialElementComponent>(selectEntities.Count);
            var elementPool = world.GetPool<SpecialElementComponent>();
            for (int i = 0; i < selectEntities.Count; i++)
            {
                if (!elementPool.Has(selectEntities[i]))
                    continue;
                ref SpecialElementComponent component = ref elementPool.Get(selectEntities[i]);
                functionElementIds.Add(component);
            }

            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            if (functionElementIds.Count == 1)
            {
                // 如果是只有一个功能棋子，那必然只能是彩球。功能棋子的单发请求在 MatchInputSystem 中处理。
                // 彩球可以和普通棋子相连
                var first = functionElementIds[0];
                if (first.ElementType == ElementType.ColorBall) // 按理说在 CanConnect 函数里就需要处理好连线的规则，所以这个必定是彩球才对
                {
                    return factory.GetMatchRule(MatchRequestType.ColorBallAndNormal);
                }
            }
            else if (functionElementIds.Count >= 2)
            {
                // 根据 Priority 排序，越大越靠前
                functionElementIds.Sort((a, b) =>
                {
                    if (a.Priority < b.Priority)
                        return 1;
                    if (a.Priority > b.Priority)
                        return -1;
                    return 0;
                });

                var first = functionElementIds[0];
                var second = functionElementIds[1];
                if (first.ElementType == ElementType.Rocket && second.ElementType == ElementType.RocketHorizontal)
                {
                    return factory.GetMatchRule(MatchRequestType.RocketAndRocket);
                }

                if (first.ElementType == ElementType.Bomb && (second.ElementType == ElementType.Rocket ||
                                                              second.ElementType == ElementType.RocketHorizontal))
                {
                    return factory.GetMatchRule(MatchRequestType.RocketAndBomb);
                }

                if (first.ElementType == ElementType.Bomb && second.ElementType == ElementType.Bomb)
                {
                    return factory.GetMatchRule(MatchRequestType.BombAndBomb);
                }

                if (first.ElementType == ElementType.ColorBall && (second.ElementType == ElementType.Rocket ||
                                                                   second.ElementType == ElementType.RocketHorizontal))
                {
                    return factory.GetMatchRule(MatchRequestType.RocketAndColorBall);
                }

                if (first.ElementType == ElementType.ColorBall && second.ElementType == ElementType.ColorBall)
                {
                    return factory.GetMatchRule(MatchRequestType.ColorBallAndColorBall);
                }

                if (first.ElementType == ElementType.ColorBall && second.ElementType == ElementType.Bomb)
                    return factory.GetMatchRule(MatchRequestType.BombAndColorBall);
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
            return false;
        }

        public int RandomFunctionElement()
        {
            int ran = Random.Range(0, 2);
            if (ran == 0)
                return 8;
            if (ran == 1)
                return 9;
            return 11;
        }

        private GenItemData GenElementData(List<Vector2Int> closedLoop, out bool result, out OneTakeScoreType scoreType)
        {
            result = true;
            scoreType = OneTakeScoreType.None;
            int minX = closedLoop[0].x;
            int minY = closedLoop[0].y;
            int maxX = closedLoop[0].x;
            int maxY = closedLoop[0].y;

            for (int i = 1; i < closedLoop.Count; i++)
            {
                var coord = closedLoop[i];
                minX = Mathf.Min(minX, coord.x);
                minY = Mathf.Min(minY, coord.y);
                maxX = Mathf.Max(maxX, coord.x);
                maxY = Mathf.Max(maxY, coord.y);
            }

            int width = maxX - minX + 1; // 包围盒宽度 (列数)
            int height = maxY - minY + 1; // 包围盒高度 (行数)
            Vector2Int genCoord = closedLoop[0];
            var ruleDb = ConfigMemoryPool.Get<SpecialEleRuleDB>();
            var matchRule = ruleDb.Match(width, height);
            if (matchRule.HasValue == false)
            {
                result = false;
                return default;
            }

            var rule = matchRule.Value;
            ElementType finalType = rule.resultElement;
            if (rule.resultElement == ElementType.Rocket)
                finalType = ruleDb.GetFinalRocketType(width, height, rule);

            //任务统计
            TaskManager.Instance.AddTaskCalculate((TaskTag)rule.taskTag);
            return new GenItemData()
                { ConfigId = ElementType2ConfigId(finalType), GenCoord = genCoord, ElementSize = Vector2Int.one };
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