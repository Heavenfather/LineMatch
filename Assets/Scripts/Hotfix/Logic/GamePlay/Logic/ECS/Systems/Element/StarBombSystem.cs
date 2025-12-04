using System.Collections.Generic;
using HotfixCore.Extensions;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// TowDots模式下的星爆点系统，负责处理星爆点的表现效果
    /// 消除范围：3x3范围+一行一列
    /// </summary>
    public class StarBombSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<ElementRenderComponent> _renderPool;
        private EcsPool<ElementPositionComponent> _positionPool;
        private EcsPool<StarBombComponent> _starBombPool;
        private EcsPool<EliminatedTag> _eliminatePool;
        private IElementFactoryService _elementService;
        private IMatchRequestService _requestService;

        public void Init(IEcsSystems systems)
        {
            _elementService = MatchBoot.Container.Resolve<IElementFactoryService>();

            _requestService = MatchBoot.Container.Resolve<IMatchRequestService>();
            _world = systems.GetWorld();
            _filter = _world.Filter<ElementRenderComponent>().Include<StarBombComponent>().End();

            _positionPool = _world.GetPool<ElementPositionComponent>();
            _renderPool = _world.GetPool<ElementRenderComponent>();
            _starBombPool = _world.GetPool<StarBombComponent>();
            _eliminatePool = _world.GetPool<EliminatedTag>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                if (!_world.IsEntityAliveInternal(entity))
                    continue;

                ref var starBombCom = ref _starBombPool.Get(entity);
                
                // 处理颜色更新
                if (starBombCom.IsColorDirty)
                {
                    SetElementColor(entity, MatchElementUtil.GetElementColor(starBombCom.StarDotBaseElementId));
                    starBombCom.IsColorDirty = false;
                }

                // 处理消除标签
                if (_eliminatePool.Has(entity))
                {
                    _eliminatePool.Del(entity);

                    ref var positionCom = ref _positionPool.Get(entity);

                    if (starBombCom.TargetEntities != null && starBombCom.TargetEntities.Count > 0)
                    {
                        // 播放星爆点爆炸效果
                        PlayStarBombEffect(entity, positionCom.WorldPosition, starBombCom.TargetEntities);
                    }
                    else
                    {
                        _requestService.RequestStarBombDot(_world, entity);
                    }
                }
            }
        }

        /// <summary>
        /// 播放星爆点爆炸效果
        /// </summary>
        private void PlayStarBombEffect(int entity, Vector3 centerPos, List<int> targetEntities)
        {
            ref var renderComponent = ref _renderPool.Get(entity);
            
            // TODO: 播放星爆点爆炸动画
            
            // 延迟销毁自身
            DelayDestroySelf(entity);
        }

        private void DelayDestroySelf(int entity)
        {
            // 延迟销毁，给动画播放时间
            _elementService.AddDestroyElementTag2Entity(_world, entity);
        }

        private void SetElementColor(int entity, Color color)
        {
            ref var renderComponent = ref _renderPool.Get(entity);
            if (renderComponent.ViewInstance != null && renderComponent.ViewInstance.Icon != null)
            {
                renderComponent.ViewInstance.Icon.color = color;
                var flashIcon = renderComponent.ViewInstance.GetPart("FlashIcon");
                if (flashIcon != null)
                {
                    flashIcon.SetVisible(false);
                }
            }
        }
    }
}
