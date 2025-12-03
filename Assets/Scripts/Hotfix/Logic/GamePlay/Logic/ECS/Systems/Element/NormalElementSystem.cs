using DG.Tweening;
using HotfixCore.Extensions;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic.GamePlay
{
    public class NormalElementSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private IElementFactoryService _elementService;
        private EcsPool<NormalElementComponent> _normalElementPool;
        private EcsPool<ElementRenderComponent> _elementRenderPool;
        private EcsPool<ElementComponent> _elementPool;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _elementService = MatchBoot.Container.Resolve<IElementFactoryService>();
            _filter = _world.Filter<NormalElementComponent>().Include<ElementComponent>()
                .Include<ElementRenderComponent>().End();
            _normalElementPool = _world.GetPool<NormalElementComponent>();
            _elementRenderPool = _world.GetPool<ElementRenderComponent>();
            _elementPool = _world.GetPool<ElementComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                ref var normalElementComponent = ref _normalElementPool.Get(entity);
                // 如果棋子正处于 Acting 状态，又没有被其它元素处理的话，它就自己标签自己死亡
                ref var elementComponent = ref _elementPool.Get(entity);
                if (elementComponent.LogicState == ElementLogicalState.Acting && !normalElementComponent.IsOtherElementHandleThis)
                {
                    elementComponent.LogicState = ElementLogicalState.Dying;
                    _elementService.AddDestroyElementTag2Entity(_world, entity);
                    continue;
                }
                
                if (normalElementComponent.IsColorDirty)
                {
                    // 更新视图棋子颜色
                    SetElementColor(entity, normalElementComponent.ElementColor);
                    normalElementComponent.IsColorDirty = false;
                }

                ref var renderComp = ref _elementRenderPool.Get(entity);
                if (normalElementComponent.IsAnimDirty)
                {
                    // 更新视图闪图的表现
                    UpdateAnimation(renderComp.ViewInstance, normalElementComponent);
                    normalElementComponent.IsAnimDirty = false;
                }
            }
        }

        private void SetElementColor(int entity, Color color)
        {
            ref var renderComponent = ref _elementRenderPool.Get(entity);
            if (renderComponent.ViewInstance != null && renderComponent.ViewInstance.Icon != null)
            {
                renderComponent.ViewInstance.Icon.color = color;
                var flashIcon = renderComponent.ViewInstance.GetPart("FlashIcon");
                if (flashIcon != null)
                {
                    var sp = flashIcon.GetComponent<SpriteRenderer>();
                    sp.color = color;
                }
            }
        }


        private void UpdateAnimation(ElementView view, NormalElementComponent normal)
        {
            if (view == null) return;
            GameObject go = view.gameObject;
            Transform flashIcon = view.GetPart("FlashIcon")?.transform;

            // 1. 清理旧动画
            go.transform.DOKill();
            if (flashIcon) flashIcon.DOKill();

            // 2. 根据状态执行新动画
            switch (normal.ScaleState)
            {
                case ElementScaleState.None:
                    // 恢复原状
                    go.transform.DOScale(1.0f, 0.1f);
                    if (flashIcon) flashIcon.SetVisible(false);
                    break;

                case ElementScaleState.PunchOnce:
                    // 单线选中：放大并保持，不循环
                    go.transform.DOScale(1.2f, 0.2f);

                    // 闪图处理：SelectedFlash 通常只闪一下
                    if (normal.FlashIconAniType == NormalFlashIconAniType.SelectedFlash && flashIcon)
                    {
                        PlayOneShotFlash(flashIcon, normal);
                    }
                    else if (flashIcon)
                    {
                        flashIcon.SetVisible(false);
                    }

                    break;

                case ElementScaleState.Breathing:
                    // 闪图处理：循环闪烁
                    if (normal.FlashIconAniType == NormalFlashIconAniType.UseItemFlash && flashIcon)
                    {
                        PlayLoopFlash(flashIcon, normal);
                    }
                    else
                    {
                        // 闭环模式：呼吸循环
                        go.transform.localScale = Vector3.one * 1.2f; // 先设置到底
                        go.transform.DOScale(1.0f, 0.5f).SetLoops(-1, LoopType.Yoyo);

                    }

                    break;
                case ElementScaleState.Shake:
                    // 抖动
                    if (flashIcon) flashIcon.SetVisible(false);
                    go.transform.DOShakePosition(0.15f, 0.05f).SetLoops(-1, LoopType.Yoyo).OnKill(() =>
                    {
                        go.transform.localPosition = Vector3.zero;
                    });
                    break;
                case ElementScaleState.Change:
                    go.transform.DOShakePosition(0.2f, 0.05f).OnKill(() =>
                    {
                        go.transform.localPosition = Vector3.zero;
                    }).SetAutoKill();
                    break;
            }
        }

        private void PlayOneShotFlash(Transform flashIcon, NormalElementComponent normal)
        {
            flashIcon.SetVisible(true);
            flashIcon.localScale = Vector3.one * normal.FlashStartScale;
            var sp = flashIcon.GetComponent<SpriteRenderer>();
            if (sp != null)
            {
                sp.color = new Color(sp.color.r, sp.color.g, sp.color.b, 0.7f);
                sp.DOFade(0f, normal.FlashDuration);
            }
            flashIcon.DOScale(normal.FlashEndScale, normal.FlashDuration)
                .OnComplete(() => flashIcon.SetVisible(false));
        }

        private void PlayLoopFlash(Transform flashIcon, NormalElementComponent normal)
        {
            flashIcon.SetVisible(true);
            flashIcon.localScale = Vector3.one * 0.7f;
            var sp = flashIcon.GetComponent<SpriteRenderer>();
            if (sp) sp.color = new Color(sp.color.r, sp.color.g, sp.color.b, 0.7f);

            flashIcon.DOScale(Vector3.one * 1.3f, 1).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear).OnKill(() =>
            {
                flashIcon.SetVisible(false);
            });
        }
    }
}