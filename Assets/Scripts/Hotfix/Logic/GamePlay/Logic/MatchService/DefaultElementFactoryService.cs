using System;
using System.Collections.Generic;
using System.Reflection;
using GameConfig;
using HotfixCore.Utils;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 默认元素工厂服务
    /// </summary>
    public class DefaultElementFactoryService : IElementFactoryService
    {
        // 注入策略
        private IBoardSpawnStrategy _spawnStrategy;

        // 构建器字典
        private Dictionary<ElementType, IElementBuilder> _builders = new Dictionary<ElementType, IElementBuilder>(100);

        public DefaultElementFactoryService()
        {
            //---------- 反射添加不同类型的构建器 ----------------
            Assembly assembly = AssemblyUtil.GetAssembly("GameHotfix");
            if (assembly != null)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetInterface("IElementBuilder") != null)
                    {
                        var instance = (IElementBuilder)Activator.CreateInstance(type);
                        RegisterBuilder(instance);
                    }
                }
            }
            else
            {
                Logger.Error("无法获取程序集:GameHotfix,元素构建器初始化失败!");
            }
        }

        private void RegisterBuilder(IElementBuilder builder)
        {
            _builders[builder.TargetType] = builder;
        }

        public void SetSpawnStrategy(IBoardSpawnStrategy strategy)
        {
            _spawnStrategy = strategy;
        }

        public int CreateElementEntity(GameStateContext context, IMatchService matchService, int configId, int x, int y,
            int width = 1, int height = 1)
        {
            EcsWorld world = context.World;
            int entity = world.NewEntity();

            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[configId];

            // 通用组件
            var elePool = world.GetPool<ElementComponent>();
            ref var eleComp = ref elePool.Add(entity);

            eleComp.ConfigId = config.Id;
            eleComp.Type = config.elementType;
            eleComp.EliminateStyle = config.eliminateStyle;
            eleComp.EliminateCount = config.eliminateCount;
            eleComp.MaxEliminateCount = config.eliminateCount;
            eleComp.Layer = config.sortOrder;
            eleComp.LogicState = ElementLogicalState.Idle;
            MatchBoot.Container.Resolve<IElementTransitionRuleService>()
                .TryTransitionToNextElement(config.Id, matchService, out var nextId);
            eleComp.NextConfigId = nextId;

            // 动态设置宽高和原点
            eleComp.Width = width;
            eleComp.Height = height;
            eleComp.OriginGridPosition = new Vector2Int(x, y);
            eleComp.IsMovable = config.isMovable;
            eleComp.IsMatchable = false;
            eleComp.IsDamageDirty = false;

            // [PositionComponent]
            var posPool = world.GetPool<ElementPositionComponent>();
            ref var posComp = ref posPool.Add(entity);
            posComp.X = x;
            posComp.Y = y;
            posComp.LocalPosition = Vector3.zero;

            // 计算世界坐标
            Vector3 targetWorldPos = MatchPosUtil.CalculateWorldPosition(x, y, width, height, config.direction);
            Vector3 startPos = targetWorldPos;
            float delay = 0;
            SpawnAnimType animType = SpawnAnimType.None;
            if (_spawnStrategy != null)
            {
                animType = _spawnStrategy.GetSpawnAnimType(in config);
                startPos = _spawnStrategy.GetStartWorldPosition(x, y, targetWorldPos);
                delay = _spawnStrategy.GetSpawnDelay(x, y);
            }

            posComp.WorldPosition = startPos;

            // [ElementSpawnComponent] 告诉 View 层怎么播放入场
            if (animType != SpawnAnimType.None)
            {
                // 这里先不添加，现在还没这方面的需求，要不然这里会跟消除后掉落的动画冲突
                // var spawnPool = world.GetPool<ElementSpawnComponent>();
                // ref var spawnComp = ref spawnPool.Add(entity);
                // spawnComp.TargetWorldPosition = targetWorldPos; // 记录目的地
                // spawnComp.Delay = delay;
                // spawnComp.AnimType = animType;
                // spawnComp.IsDirty = true;
            }

            // [RenderComponent] 渲染组件
            var renderPool = world.GetPool<ElementRenderComponent>();
            ref var renderComp = ref renderPool.Add(entity);
            renderComp.PrefabKey = $"Element-{config.Id}";
            renderComp.IsVisible = false;
            renderComp.IsDirty = false;
            renderComp.IsSelected = false;
            renderComp.ViewInstance = null;

            // 添加特定的元素组件
            if (_builders.TryGetValue(ParseLinkElementType(config.elementType), out var builder))
            {
                builder.Build(context, entity, config);
            }

            return entity;
        }

        public bool IsElementCanSelected(ElementType elementType, EcsWorld world, int elementEntity)
        {
            return _builders.TryGetValue(ParseLinkElementType(elementType), out var builder) &&
                   builder.IsElementCanSelected(world, elementEntity);
        }

        /// <summary>
        /// 为了减少构建器的创建，这里将不同的元素类型指向特定的构建器
        /// 由构建器内部再进一步判断
        /// </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        private ElementType ParseLinkElementType(ElementType elementType)
        {
            switch (elementType)
            {
                // 功能棋子统一链接到同一个构建器进行处理
                case ElementType.Rocket:
                case ElementType.Bomb:
                case ElementType.RocketHorizontal:
                case ElementType.ColorBall:
                case ElementType.StarBomb:
                case ElementType.SearchDot:
                case ElementType.HorizontalDot:
                case ElementType.TowDotsBombDot:
                case ElementType.TowDotsColoredDot:
                    return ElementType.Bomb;
                case ElementType.Collect:
                case ElementType.JumpCollect:
                    return ElementType.Collect;
                default:
                    return elementType;
            }
        }
    }
}