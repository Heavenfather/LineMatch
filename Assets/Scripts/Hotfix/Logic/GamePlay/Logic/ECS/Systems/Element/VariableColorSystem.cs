using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 通用可变颜色组件视觉更新系统
    /// </summary>
    public class VariableColorSystem : IEcsInitSystem,IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<VariableColorComponent> _variableColorPool;
        private EcsPool<ElementRenderComponent> _elementRenderPool;
        
        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<VariableColorComponent>().End();
            _variableColorPool = _world.GetPool<VariableColorComponent>();
            _elementRenderPool = _world.GetPool<ElementRenderComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                ref var variableColorCom = ref _variableColorPool.Get(entity);
                if (variableColorCom.IsColorDirty)
                {
                    Color targetColor = Color.white;
                    if (variableColorCom.CurrentColorId != 0)
                    {
                        targetColor = MatchElementUtil.GetElementColor(variableColorCom.CurrentColorId);
                    }
                    SetElementColor(entity, targetColor);
                    variableColorCom.IsColorDirty = false;
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
    }
}