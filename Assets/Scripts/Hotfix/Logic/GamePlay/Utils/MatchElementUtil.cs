using System.Collections.Generic;
using GameConfig;
using HotfixLogic;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public static class MatchElementUtil
    {
        private static Dictionary<int, Color> _elementColorMap = null;

        private static string _matchBgColor = "";

        /// <summary>
        /// 消除关卡背景颜色
        /// </summary>
        public static string MatchBgColor
        {
            get
            {
                if (_elementColorMap == null)
                {
                    _elementColorMap = new Dictionary<int, Color>();
                    BuildElementColorMap();
                }

                return _matchBgColor;
            }
            private set { _matchBgColor = value; }
        }

        /// <summary>
        /// 元素单网格大小
        /// </summary>
        public static readonly Vector2 GridSize = new Vector2(0.8f, 0.8f);

        public static void DynamicModifyColorMap(BoardColorStruck data)
        {
            MatchBgColor = $"#{ColorUtility.ToHtmlStringRGB(data.BgColor).ToLower()}";
            _elementColorMap[1] = data.Green;
            _elementColorMap[2] = data.Blue;
            _elementColorMap[3] = data.Yellow;
            _elementColorMap[4] = data.Red;
            _elementColorMap[5] = data.Purple;
            _elementColorMap[6] = data.Orange;
            _elementColorMap[7] = data.Cycan;
            // foreach (var elements in _gridElements.Values)
            // {
            //     for (int i = 0; i < elements.Count; i++)
            //     {
            //         if (elements[i] is BaseElementItem elementItem)
            //         {
            //             elementItem.SetElementColor();
            //         }
            //     }
            // }

            // 发送GameEventDefine.OnOkChangeBoardColor事件 实时刷新棋盘上棋子的颜色 TODO....
        }

        public static Color GetElementColor(int elementId)
        {
            if (_elementColorMap == null)
            {
                _elementColorMap = new Dictionary<int, Color>();
                BuildElementColorMap();
            }

            Color color = Color.white;
            if (_elementColorMap.TryGetValue(elementId, out var value))
                color = value;
            return color;
        }

        /// <summary>
        /// 判断两个元素是否变体的关系
        /// target->source target能否通过一步一步消除后形成source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsContributingToTarget(int source, int target)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            if (db.IsCircleElement(source))
                return false;
            if (source == target)
                return true;

            List<int> nextIds = new();
            db.RefElementNextList(target, ref nextIds);
            if (nextIds.Count > 0 && nextIds.Contains(source))
                return true;
            
            return false;
        }

        /// <summary>
        /// 为所有待消除的棋子添加“旁消”检测
        /// </summary>
        /// <param name="ctx">规则上下文</param>
        /// <param name="actions">当前的指令列表</param>
        public static void AppendSideEliminationActions(this MatchRuleContext ctx, ref List<AtomicAction> actions)
        {
            IMatchServiceFactory factory = MatchBoot.Container.Resolve<IMatchServiceFactory>();
            // 1. 收集所有即将受到伤害/销毁的位置
            HashSet<Vector2Int> damagePositions = new HashSet<Vector2Int>();

            foreach (var action in actions)
            {
                // 只有 Damage 类型的指令会触发旁消
                if (action.Type == MatchActionType.Damage)
                {
                    damagePositions.Add(action.GridPos);
                }
            }

            // 2. 遍历这些位置的邻居
            var gridPool = ctx.World.GetPool<GridCellComponent>();
            var elePool = ctx.World.GetPool<ElementComponent>();

            foreach (var centerPos in damagePositions)
            {
                foreach (var dir in MatchPosUtil.NeighborDirs)
                {
                    Vector2Int neighborPos = centerPos + dir;

                    // 检查邻居是否有障碍物
                    if (ctx.Board.TryGetGridEntity(neighborPos.x, neighborPos.y, out int gridEntity))
                    {
                        ref var grid = ref gridPool.Get(gridEntity);
                        if (grid.StackedEntityIds.Count == 0) continue;

                        // 检查格子上是否有支持“旁消”的元素
                        foreach (var entityId in grid.StackedEntityIds)
                        {
                            if (elePool.Has(entityId))
                            {
                                ref var ele = ref elePool.Get(entityId);
                                if (ele.EliminateStyle == EliminateStyle.Side)
                                {
                                    // 生成对障碍物的伤害指令
                                    actions.Add(factory.CreateAtomicAction(MatchActionType.Damage, neighborPos, 1,
                                        entityId));
                                    break; //可以退出了，避免同一次产生了多次伤害
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void AddSingleScore(in ElementComponent element)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[element.ConfigId];
            MatchManager.Instance.AddScore(config.score);
        }

        public static void AddSingleScore(EcsWorld world, int entityId)
        {
            ref var element = ref world.GetPool<ElementComponent>().Get(entityId);
            AddSingleScore(element);
        }
        
        public static void AddSingleScore(int configId)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[configId];
            MatchManager.Instance.AddScore(config.score);
        }

        /// <summary>
        /// 是否循环类型元素
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsCycleElement(this ElementComponent element)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            return db.IsCircleElement(element.ConfigId);
        }

        private static void BuildElementColorMap()
        {
            LevelMapImageDB db = ConfigMemoryPool.Get<LevelMapImageDB>();
            MatchBgColor = db.GetMatchBgColor(MatchManager.Instance.CurLevelID, MatchManager.Instance.MaxLevel);

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
    }
}