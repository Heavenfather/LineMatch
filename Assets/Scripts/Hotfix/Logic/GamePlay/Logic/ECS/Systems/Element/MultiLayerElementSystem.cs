using System.Collections.Generic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 多层元素系统
    /// 处理所有具有MultiLayerComponent的元素的多层消除和状态转换
    /// 
    /// 工作流程：
    /// 1. 筛选有MultiLayerComponent和EliminatedTag的实体
    /// 2. 读取EliminatedTag.EliminateCount获取累积的伤害次数
    /// 3. 根据伤害次数决定转换到哪一层或完全消除
    /// 4. 创建Transform动作
    /// </summary>
    public class MultiLayerElementSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private GameStateContext _context;
        private IElementFactoryService _elementFactory;
        private IElementTransitionRuleService _transitionRule;
        private IMatchService _matchService;

        private EcsFilter _multiLayerFilter;
        private EcsPool<MultiLayerComponent> _multiLayerPool;
        private EcsPool<ElementComponent> _elementPool;
        private EcsPool<ElementPositionComponent> _positionPool;
        private EcsPool<EliminatedTag> _eliminatedPool;
        private EcsPool<PendingActionsComponent> _pendingActionsPool;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _elementFactory = MatchBoot.Container.Resolve<IElementFactoryService>();
            _transitionRule = MatchBoot.Container.Resolve<IElementTransitionRuleService>();
            _matchService = _context.ServiceFactory.GetService(_context.CurrentMatchType);

            // 筛选有MultiLayerComponent和EliminatedTag的实体
            _multiLayerFilter = _world.Filter<MultiLayerComponent>()
                .Include<ElementComponent>()
                .Include<EliminatedTag>()
                .End();

            _multiLayerPool = _world.GetPool<MultiLayerComponent>();
            _elementPool = _world.GetPool<ElementComponent>();
            _positionPool = _world.GetPool<ElementPositionComponent>();
            _eliminatedPool = _world.GetPool<EliminatedTag>();
            _pendingActionsPool = _world.GetPool<PendingActionsComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            // 处理所有收到EliminatedTag的多层元素
            foreach (var entity in _multiLayerFilter)
            {
                ref var multiLayer = ref _multiLayerPool.Get(entity);
                ref var element = ref _elementPool.Get(entity);
                ref var eliminatedTag = ref _eliminatedPool.Get(entity);

                // 获取累积的伤害次数
                int totalDamage = eliminatedTag.EliminateCount;

                // 处理伤害
                ProcessMultiLayerDamage(entity, ref multiLayer, ref element, totalDamage);

                // 删除EliminatedTag（已处理完毕）
                // 注意：必须在这里删除，否则会影响下一帧的判断，所以后续的元素判断已经不能依赖这个 EliminatedTag 标签
                // _eliminatedPool.Del(entity);
            }
        }

        /// <summary>
        /// 处理多层元素的伤害
        /// 根据累积的伤害次数决定是转换形态还是完全消除
        /// </summary>
        private void ProcessMultiLayerDamage(int entity, ref MultiLayerComponent multiLayer,
            ref ElementComponent element, int totalDamage)
        {
            // 计算需要消除的层数
            int layersToRemove = totalDamage;
            int currentLayer = multiLayer.RemainingLayers;

            if (layersToRemove >= currentLayer)
            {
                // 伤害足以完全消除
                element.LogicState = ElementLogicalState.Dying;
                multiLayer.IsEliminate = true;
                multiLayer.IsWillTransform = false;
                DestroyElement(entity);
            }
            else
            {
                // 计算需要转换到哪一层
                int targetConfigId = CalculateTargetLayer(element.ConfigId, layersToRemove);

                if (targetConfigId > 0)
                {
                    multiLayer.IsEliminate = true;
                    multiLayer.IsWillTransform = true;
                    // 转换到目标层
                    TransformToTargetLayer(entity, targetConfigId);
                }
                else
                {
                    // 没有找到目标层，直接消除
                    element.LogicState = ElementLogicalState.Dying;
                    multiLayer.IsEliminate = true;
                    multiLayer.IsWillTransform = false;
                    DestroyElement(entity);
                }
            }
        }

        /// <summary>
        /// 计算目标层的ConfigId
        /// 根据需要消除的层数，递归查找目标形态
        /// </summary>
        private int CalculateTargetLayer(int currentConfigId, int layersToRemove)
        {
            int targetConfigId = currentConfigId;
            int transitionCount = 0;
            const int MAX_TRANSITIONS = 10;

            // 递归转换到目标层
            for (int i = 0; i < layersToRemove && transitionCount < MAX_TRANSITIONS; i++)
            {
                transitionCount++;

                if (!_transitionRule.TryTransitionToNextElement(targetConfigId, _matchService, out var nextConfigId))
                {
                    // 没有下一层了
                    return 0;
                }

                targetConfigId = nextConfigId;
            }

            return targetConfigId;
        }

        /// <summary>
        /// 转换到目标层状态
        /// </summary>
        private void TransformToTargetLayer(int oldEntity, int targetConfigId)
        {
            if (!_positionPool.Has(oldEntity))
                return;

            ref var pos = ref _positionPool.Get(oldEntity);

            // 创建Transform动作
            List<AtomicAction> actions = new List<AtomicAction>
            {
                new AtomicAction
                {
                    Type = MatchActionType.Transform,
                    GridPos = new Vector2Int(pos.X, pos.Y),
                    Value = oldEntity,
                    ExtraData = targetConfigId // 存储目标层的配置ID
                }
            };

            // 创建PendingActions实体
            int pendingEntity = _world.NewEntity();
            ref var pending = ref _pendingActionsPool.Add(pendingEntity);
            pending.Actions = actions;
        }

        /// <summary>
        /// 销毁元素
        /// </summary>
        private void DestroyElement(int entity)
        {
            _elementFactory.AddDestroyElementTag2Entity(_world, entity);
        }
    }
}