using DG.Tweening;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public class ShuffleAnimationSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<ShuffleAnimationComponent> _animPool;
        private EcsPool<ElementRenderComponent> _renderPool;
        private EcsPool<VisualBusyComponent> _busyPool;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<ShuffleAnimationComponent>().End();
            _animPool = _world.GetPool<ShuffleAnimationComponent>();
            _renderPool = _world.GetPool<ElementRenderComponent>();
            _busyPool = _world.GetPool<VisualBusyComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                ref var anim = ref _animPool.Get(entity);
                ref var render = ref _renderPool.Get(entity);

                if (render.ViewInstance == null)
                {
                    _animPool.Del(entity);
                    continue;
                }

                // 1. 挂锁
                if (!_busyPool.Has(entity)) _busyPool.Add(entity);

                Transform trans = render.ViewInstance.transform;

                // 2. 强制设置到起点
                // trans.position = anim.StartPos;

                // 3. 播放缓动动画
                Sequence seq = DOTween.Sequence();

                float delay = Random.Range(0f, 0.15f);
                float duration = 0.6f;

                seq.AppendInterval(delay);
                // 飞向终点
                seq.Join(trans.DOLocalMove(Vector3.zero, duration).SetEase(Ease.InOutQuart));
                seq.OnComplete(() =>
                {
                    // 动画结束，解锁
                    if (_world.IsEntityAliveInternal(entity))
                    {
                        _busyPool.Del(entity);
                    }
                });

                // 4. 移除动画组件
                _animPool.Del(entity);
            }
        }
    }
}