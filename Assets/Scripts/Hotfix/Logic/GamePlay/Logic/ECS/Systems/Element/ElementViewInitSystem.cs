using GameConfig;
using HotfixCore.Extensions;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 元素视图初始化系统，负责初始化元素的视图实例
    /// </summary>
    public class ElementViewInitSystem : IEcsInitSystem, IEcsRunSystem
    {
        private ElementMapDB _elementMapDB;
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<ElementRenderComponent> _renderComponentPool;
        private EcsPool<ElementComponent> _componentPool;

        public void Init(IEcsSystems systems)
        {
            _elementMapDB = ConfigMemoryPool.Get<ElementMapDB>();
            _world = systems.GetWorld();
            _filter = _world.Filter<ElementRenderComponent>().Include<ElementComponent>().End();
            _renderComponentPool = _world.GetPool<ElementRenderComponent>();
            _componentPool = _world.GetPool<ElementComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                ref var render = ref _renderComponentPool.Get(entity);
                if (!render.IsDirty || render.ViewInstance == null) continue;

                ref var elementData = ref _componentPool.Get(entity);
                ref readonly ElementMap config = ref _elementMapDB[elementData.ConfigId];

                //------- 棋子通用的处理 ----------
                // 1.设置层级排序  这里会和查找实体堆叠的逻辑冲突，先不要设置，由预制体设置固定值
                // render.ViewInstance.SetSortingOrder(elementData.Layer);

                // 2.Spine Idle处理
                if (!string.IsNullOrEmpty(config.idleSpine) && render.ViewInstance.Spine != null)
                {
                    render.ViewInstance.SetSpineAnimation(config.idleSpine, true);
                }

                render.IsVisible = true;
                render.ViewInstance.SetVisible(render.IsVisible); // 可以显示了

                render.IsDirty = false;
            }
        }
    }
}