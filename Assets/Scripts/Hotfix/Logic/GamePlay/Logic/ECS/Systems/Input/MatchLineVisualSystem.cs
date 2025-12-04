using HotfixCore.Module;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 连线可视化系统
    /// </summary>
    public class MatchLineVisualSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<MatchInputComponent> _inputPool;
        private EcsPool<ElementPositionComponent> _posPool;
        private EcsPool<SearchDotComponent> _searchPool;
        private EcsPool<StarBombComponent> _starBombPool;
        private EcsPool<NormalElementComponent> _normalPool;
        private EcsPool<ElementComponent> _elePool;
        private Camera _mainCamera;
        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _inputPool = _world.GetPool<MatchInputComponent>();
            _posPool = _world.GetPool<ElementPositionComponent>();
            _elePool = _world.GetPool<ElementComponent>();
            _searchPool = _world.GetPool<SearchDotComponent>();
            _starBombPool = _world.GetPool<StarBombComponent>();
            _normalPool = _world.GetPool<NormalElementComponent>();
            _filter = _world.Filter<MatchInputComponent>().End();

            var context = systems.GetShared<GameStateContext>();
            _mainCamera = context.SceneView.GetSceneRootComponent<Camera>("MainCamera", "");
        }

        public void Run(IEcsSystems systems)
        {
            int entity = _filter[0]; // 获取唯一的输入组件
            ref var input = ref _inputPool.Get(entity);
            UpdateVisuals(in input);
        }

        private void UpdateVisuals(in MatchInputComponent input)
        {
            // 如果不在拖拽且没有选中，清除线
            if (!input.IsDragging)
            {
                LineController.Instance.ClearAllLines();
                return;
            }

            if (input.SelectedEntityIds == null || input.SelectedEntityIds.Count == 0)
                return;

            // 1. 设置颜色
            int colorId = 0;
            for (int i = 0; i < input.SelectedEntityIds.Count; i++)
            {
                if (_normalPool.Has(input.SelectedEntityIds[i]))
                {
                    ref var eleCom = ref _elePool.Get(input.SelectedEntityIds[i]);
                    colorId = eleCom.ConfigId;
                    break;
                }

                if (_starBombPool.Has(input.SelectedEntityIds[i]))
                {
                    ref var starBombCom = ref _starBombPool.Get(input.SelectedEntityIds[i]);
                    colorId = starBombCom.StarDotBaseElementId;
                    break;
                }

                if (_searchPool.Has(input.SelectedEntityIds[i]))
                {
                    ref var searchCom = ref _searchPool.Get(input.SelectedEntityIds[i]);
                    colorId = searchCom.SearchDotBaseElementId;
                    break;
                }
            }
            
            Color c = MatchElementUtil.GetElementColor(colorId);
            LineController.Instance.SetLineColor(c);

            // 2. 更新 LineRenderer 点
            LineController.Instance.ClearAllLines(); // 先清空再重画

            for (int j = 0; j < input.SelectedEntityIds.Count; j++)
            {
                int entityId = input.SelectedEntityIds[j];
                ref var pos = ref _posPool.Get(entityId);
                LineController.Instance.AddUnderPoint(pos.WorldPosition);
            }

            if (input.IsRectangle && input.SelectedEntityIds.Count > 0)
            {
                if (input.LoopTargetEntityId >= 0 && _posPool.Has(input.LoopTargetEntityId))
                {
                    ref var loopTargetPos = ref _posPool.Get(input.LoopTargetEntityId);
                    LineController.Instance.AddUnderPoint(loopTargetPos.WorldPosition);
                }

                // 闭环时不再显示手指牵引线
                LineController.Instance.ClearOverLine();
            }
            // 3. 非闭环时，处理手指牵引线
            else if (input.IsDragging && input.SelectedEntityIds.Count > 0)
            {
                Vector3 lastPos = _posPool.Get(input.SelectedEntityIds[^1]).WorldPosition;
                Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(G.TouchModule.InputPos);
                mouseWorldPos.z = 0;

                LineController.Instance.SetOverLinePoint(1, lastPos);
                LineController.Instance.SetOverLinePoint(2, mouseWorldPos);
            }
        }
    }
}