using HotfixCore.Extensions;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 元素生成系统，只负责创建元素的视图实例
    /// </summary>
    public class ElementSpawnSystem : IEcsInitSystem,IEcsRunSystem
    {
        private EcsWorld _world;
        private GameStateContext _context;
        private EcsFilter _filter;
        private IBoard _board;
        private EcsPool<ElementRenderComponent> _renderPool;
        private EcsPool<ElementPositionComponent> _positionPool;
        private Transform _defaultElementRoot;
        
        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _board = _context.Board;
            // 筛选有RenderComponent 但还没有View的实体
            _filter = _world.Filter<ElementRenderComponent>().Include<ElementPositionComponent>().End();
            _renderPool = _world.GetPool<ElementRenderComponent>();
            _positionPool = _world.GetPool<ElementPositionComponent>();
            _defaultElementRoot = _context.SceneView.GetSceneRootTransform("MatchCanvas", "EleRoot");
        }
        
        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                ref var renderComponent = ref _renderPool.Get(entity);
                if (renderComponent.ViewInstance == null)
                {
                    GameObject go = ElementObjectPool.Instance.Spawn(renderComponent.PrefabKey);
                    if (go != null)
                    {
                        ref var positionComponent = ref _positionPool.Get(entity);
                        var parent = _board.GetGridInstance(positionComponent.X, positionComponent.Y);
                        go.transform.SetParent(parent == null ? _defaultElementRoot : parent.transform);
                        // go.name = $"{renderComponent.PrefabKey}_{entity}";
                        // 绑定View视图
                        var view = go.GetOrAddComponent<ElementView>();
                        view.SetVisible(renderComponent.IsVisible);
                        
                        //初始化位置
                        if (_positionPool.Has(entity))
                        {
                            view.transform.position = positionComponent.WorldPosition;
                        }
                        
                        renderComponent.ViewInstance = view;
                        renderComponent.IsDirty = true; //标记为脏，后续会更新视图
                    }
                }
                
            }
        }
    }
}