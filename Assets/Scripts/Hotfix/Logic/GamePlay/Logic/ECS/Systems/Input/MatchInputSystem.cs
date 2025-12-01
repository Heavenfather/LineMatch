using System.Collections.Generic;
using System.Linq;
using GameConfig;
using Hotfix.Utils;
using HotfixCore.Module;
using HotfixLogic.Match;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hotfix.Logic.GamePlay
{
    public class MatchInputSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;
        private GameStateContext _context;
        private Camera _camera;
        private IConnectionRuleService _ruleService;
        private IMatchRequestService _requestService;
        private BoardViewConfig _viewConfig;
        private IMatchService _matchService;

        private EcsFilter _normalElementFilter;
        private EcsPool<MatchInputComponent> _inputPool;
        private EcsPool<GridCellComponent> _gridPool;
        private EcsPool<ElementComponent> _elePool;
        private EcsPool<ElementRenderComponent> _renderPool;
        private EcsPool<ElementPositionComponent> _posPool;
        private EcsPool<NormalElementComponent> _normalPool;

        // 缓存射线检测结果，避免GC
        private RaycastHit2D[] _linecastResults = new RaycastHit2D[20];

        // 临时列表用于排序
        private List<int> _sortedHitEntities = new List<int>(10);
        private int _inputEntity; // 存储单例组件的实体ID
        private int _gridLayerMask;
        private int _vibrationForce = 0;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _matchService = _context.ServiceFactory.GetService(_context.CurrentMatchType);
            _camera = _context.SceneView.GetSceneRootComponent<Camera>("MainCamera", "");
            _viewConfig = _context.SceneView.GetSceneRootComponent<BoardViewConfig>("MatchCanvas", "GridBoard");
            _gridLayerMask = _viewConfig.TouchHitMask.value;
            _ruleService = MatchBoot.Container.Resolve<IConnectionRuleService>();
            _requestService = MatchBoot.Container.Resolve<IMatchRequestService>();
            _normalElementFilter = _world.Filter<NormalElementComponent>().Include<ElementComponent>()
                .Include<ElementRenderComponent>().End();
            _normalPool = _world.GetPool<NormalElementComponent>();

            _inputPool = _world.GetPool<MatchInputComponent>();
            _gridPool = _world.GetPool<GridCellComponent>();
            _elePool = _world.GetPool<ElementComponent>();
            _posPool = _world.GetPool<ElementPositionComponent>();
            _renderPool = _world.GetPool<ElementRenderComponent>();

            // 创建唯一的输入状态实体
            _inputEntity = _world.NewEntity();
            ref var input = ref _inputPool.Add(_inputEntity);
            input.SelectedEntityIds = new List<int>(40);
            input.SelectedGridIds = new List<int>(40);
            input.IsDragging = false;

            var constDB = ConfigMemoryPool.Get<ConstConfigDB>();
            string devicePlatform = CommonUtil.GetDevicePlatform();
            if (devicePlatform == "ios")
                _vibrationForce = constDB.GetConfigIntVal("VibrationForceIOS");
            else
                _vibrationForce = constDB.GetConfigIntVal("VibrationForce");
        }

        public void Run(IEcsSystems systems)
        {
            if (!G.TouchModule.TouchIsValid()) return;

            ProcessTouchPhase(G.TouchModule.TouchPhase, G.TouchModule.InputPos);
        }

        private void ProcessTouchPhase(TouchPhase phase, Vector2 screenPos)
        {
            if (IsPointerOverUI(screenPos))
                return;

            ref var input = ref _inputPool.Get(_inputEntity);
            Vector3 mousePos = _camera.ScreenToWorldPoint(screenPos);
            mousePos.z = 0;

            switch (phase)
            {
                case TouchPhase.Began:
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    // 区分“刚开始点”和“拖拽中”
                    if (!input.IsDragging)
                    {
                        if (phase == TouchPhase.Began)
                        {
                            // 刚按下时，只需要检测鼠标当前这一个点
                            int hitGridEntity = RaycastSingleGrid(mousePos);
                            if (hitGridEntity >= 0)
                            {
                                int hitElementEntity = GetTopElement(hitGridEntity);
                                if (hitElementEntity >= 0)
                                    TryStartDrag(ref input, hitGridEntity, hitElementEntity);
                            }
                        }
                    }
                    else
                    {
                        // 拖拽中
                        ProcessDragUpdate(ref input, mousePos);
                    }

                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (input.IsDragging)
                    {
                        OnDragEnd(ref input);
                    }

                    break;
            }
        }

        private bool IsPointerOverUI(Vector2 screenPos)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = screenPos;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            // 过滤完全透明的UI
            return results.Any(r =>
                r.gameObject.GetComponent<Graphic>()?.color.a > 0.1f);
        }

        private void TryStartDrag(ref MatchInputComponent input, int gridEntity, int elementEntity)
        {
            if (_ruleService.IsSelectable(_world, elementEntity))
            {
                input.IsDragging = true;
                input.IsRectangle = false;
                input.IsInputComplete = false;
                input.SelectedEntityIds.Clear();
                input.SelectedGridIds.Clear();

                ref var ele = ref _elePool.Get(elementEntity);
                input.FirstConfigId = ele.ConfigId;

                AddToSelection(ref input, gridEntity, elementEntity);
            }
        }

        private int GetTopElement(int gridEntity)
        {
            ref var grid = ref _gridPool.Get(gridEntity);
            if (grid.StackedEntityIds == null || grid.StackedEntityIds.Count == 0) return -1;
            return grid.StackedEntityIds[^1];
        }

        /// <summary>
        /// 处理拖拽时的核心逻辑
        /// </summary>
        private void ProcessDragUpdate(ref MatchInputComponent input, Vector3 currentMousePos)
        {
            if (input.SelectedEntityIds.Count == 0) return;

            // 获取上一个选中的格子位置
            int lastElementEntity = input.SelectedEntityIds[^1];
            ref var lastPosComp = ref _posPool.Get(lastElementEntity);
            Vector3 lastWorldPos = lastPosComp.WorldPosition;

            // --- 回退检测 ---
            // 如果鼠标回到了“倒数第二个”选中的格子，则执行回退
            int currentHoverGrid = RaycastSingleGrid(currentMousePos);
            if (currentHoverGrid >= 0 && input.SelectedGridIds.Count >= 2)
            {
                int lastIdx = input.IsRectangle ? 1 : 2;
                if (input.SelectedGridIds[^lastIdx] == currentHoverGrid)
                {
                    RemoveLastSelection(ref input);
                    // 回退后更新“上一个位置”为新的队尾，以便后续 Linecast 正确
                    lastElementEntity = input.SelectedEntityIds[^1];
                    lastPosComp = ref _posPool.Get(lastElementEntity);
                    lastWorldPos = lastPosComp.WorldPosition;
                }
            }

            // 如果已经形成闭环，且没有回退操作，不再允许添加新连线
            if (input.IsRectangle) return;

            // --- 连线检测 ---
            // 从“上一个选中位置”到“当前鼠标位置”发射射线，找出路径上所有的格子
            _sortedHitEntities.Clear();
            int hitCount = Physics2D.LinecastNonAlloc(lastWorldPos, currentMousePos, _linecastResults, _gridLayerMask);

            for (int i = 0; i < hitCount; i++)
            {
                var collider = _linecastResults[i].collider;
                if (collider != null && collider.TryGetComponent<EntityLink>(out var link))
                {
                    int gridEntity = link.EntityId;
                    // 排除已经选中的
                    // 注意：这里不能排除倒数第二个，因为上面已经处理了回退，这里主要防止添加自身
                    if (gridEntity != input.SelectedGridIds[^1])
                    {
                        _sortedHitEntities.Add(gridEntity);
                    }
                }
            }

            // --- 按距离排序 ---
            if (_sortedHitEntities.Count > 1)
            {
                _sortedHitEntities.Sort((a, b) =>
                {
                    ref var posA = ref _gridPool.Get(a);
                    ref var posB = ref _gridPool.Get(b);
                    float distA = Vector3.SqrMagnitude(posA.WorldPosition - lastWorldPos);
                    float distB = Vector3.SqrMagnitude(posB.WorldPosition - lastWorldPos);
                    return distA.CompareTo(distB);
                });
            }

            // --- 依次尝试连接 ---
            foreach (var gridEntity in _sortedHitEntities)
            {
                int elementEntity = GetTopElement(gridEntity);
                if (elementEntity >= 0)
                {
                    // 尝试将当前队尾连接到这个新格子
                    TryConnectNext(ref input, gridEntity, elementEntity);
                }
            }
        }

        private void TryConnectNext(ref MatchInputComponent input, int nextGridEntity, int nextElementEntity)
        {
            // 1. 防止重复添加
            if (input.SelectedGridIds.Contains(nextGridEntity))
            {
                int index = input.SelectedGridIds.IndexOf(nextGridEntity);
                // 排除倒数第一（自己）和倒数第二（回退），剩下的都是“更早的点”
                // 只要连到更早的点，且是邻居，就是闭环
                if (index >= 0 && (input.SelectedGridIds.Count - index) >= 4)
                {
                    // 还需要检查是否是邻居
                    int lastGrid = input.SelectedGridIds[^1];
                    if (IsNeighbor(lastGrid, nextGridEntity) && !IsSelectedHaveFunctionElement(in input.SelectedEntityIds))
                    {
                        input.IsRectangle = true;
                        input.LoopTargetEntityId = nextElementEntity;
                        // 触发全屏同类型选中效果
                        SetSquareHighlight(input.FirstConfigId, true, input.SelectedEntityIds);

                        // 触发闭环震动/音效
                        CommonUtil.DeviceVibration(_vibrationForce + 1, 0.1f);
                        ElementAudioManager.Instance.PlayMatchLink(input.SelectedGridIds.Count);
                    }
                }

                return;
            }

            // 2. 邻居检测
            int currentTailGrid = input.SelectedGridIds[^1];
            if (!IsNeighbor(currentTailGrid, nextGridEntity)) return;

            // 3. 规则检测
            int currentTailElement = input.SelectedEntityIds[^1];
            if (_ruleService.CanConnect(_world, currentTailElement, nextElementEntity, input.FirstConfigId,
                    _matchService, input.SelectedEntityIds))
            {
                AddToSelection(ref input, nextGridEntity, nextElementEntity);
            }
        }

        private bool IsSelectedHaveFunctionElement(in List<int> selectedEntityIds)
        {
            bool result = false;
            foreach (var entity in selectedEntityIds)
            {
                ref var element = ref _elePool.Get(entity);
                if (_matchService.IsSpecialElement(element.ConfigId))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        private int RaycastSingleGrid(Vector3 worldPos)
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 100f, _gridLayerMask);
            if (hit.collider != null && hit.collider.TryGetComponent<EntityLink>(out var link))
            {
                return link.EntityId;
            }

            return -1;
        }

        /// <summary>
        /// 设置方块高亮状态
        /// </summary>
        private void SetSquareHighlight(int configId, bool isSquareMode, List<int> currentPath)
        {
            foreach (var entity in _normalElementFilter)
            {
                ref var ele = ref _elePool.Get(entity);
                if (ele.ConfigId != configId) continue;

                ref var normal = ref _normalPool.Get(entity);
                ref var render = ref _renderPool.Get(entity);
                if (isSquareMode)
                {
                    // 所有同色棋子
                    render.IsSelected = true;

                    UpdateNormalState(ref normal, ElementScaleState.Breathing, NormalFlashIconAniType.SelectedFlash);
                }
                else
                {
                    // 退出闭环：
                    if (currentPath.Contains(entity))
                    {
                        // 停止循环
                        render.IsSelected = true;
                        UpdateNormalState(ref normal, ElementScaleState.PunchOnce, NormalFlashIconAniType.None);
                    }
                    // 2. 如果不在路径里 -> 恢复原样
                    else
                    {
                        render.IsSelected = false;
                        UpdateNormalState(ref normal, ElementScaleState.None, NormalFlashIconAniType.None);
                    }
                }
            }
        }

        private void AddToSelection(ref MatchInputComponent input, int gridEntity, int elementEntity)
        {
            input.SelectedGridIds.Add(gridEntity);
            input.SelectedEntityIds.Add(elementEntity);

            // 更新元素渲染选中效果
            ref var normal = ref _normalPool.Get(elementEntity);
            UpdateNormalState(ref normal, ElementScaleState.PunchOnce, NormalFlashIconAniType.SelectedFlash);

            // 播放选中音效/震动
            CommonUtil.DeviceVibration(_vibrationForce, 0.1f);
            PlayLinkAudio(input.SelectedGridIds.Count);
        }

        private void RemoveLastSelection(ref MatchInputComponent input)
        {
            int removedEntity = input.SelectedEntityIds[^1];
            input.SelectedGridIds.RemoveAt(input.SelectedGridIds.Count - 1);
            input.SelectedEntityIds.RemoveAt(input.SelectedEntityIds.Count - 1);

            // 取消该棋子的高亮
            ref var normal = ref _normalPool.Get(removedEntity);
            UpdateNormalState(ref normal, ElementScaleState.None, NormalFlashIconAniType.None);

            // 如果之前是闭环，现在打破了 -> 恢复全盘状态
            if (input.IsRectangle)
            {
                input.IsRectangle = false;
                input.LoopTargetEntityId = -1; // 重置闭合点
                // 取消全屏高亮，恢复路径高亮
                SetSquareHighlight(input.FirstConfigId, false, input.SelectedEntityIds);
            }

            PlayLinkAudio(input.SelectedGridIds.Count);
        }

        private void OnDragEnd(ref MatchInputComponent input)
        {
            input.IsDragging = false;

            // 拖拽逻辑，必然是2个以上才会产生消除请求
            if (input.SelectedEntityIds.Count >= 2)
            {
                if (input.IsRectangle)
                {
                    _requestService.RequestPlayerSquare(_world, input.SelectedEntityIds, input.LoopTargetEntityId,
                        input.FirstConfigId);
                }
                else
                {
                    _requestService.RequestPlayerLine(_world, input.SelectedEntityIds, input.SelectedEntityIds[^1],
                        input.FirstConfigId);
                }
            }
            else
            {
                // 取消选中状态
                if (input.SelectedEntityIds.Count == 1)
                {
                    int oneEntity = input.SelectedEntityIds[0];
                    _matchService.CheckOneElementRequest(_world, oneEntity);
                }
                
                ClearSelectionVisuals(input.SelectedEntityIds);
            }

            input.IsInputComplete = true; // 标记本轮输入结束，等待消除系统处理
            input.SelectedEntityIds.Clear();
            input.SelectedGridIds.Clear();
            input.IsRectangle = false;
            input.LoopTargetEntityId = -1;
            
            _context.MatchStateContext.RoundClear();
        }

        private bool IsNeighbor(int gridA, int gridB)
        {
            ref var a = ref _gridPool.Get(gridA);
            ref var b = ref _gridPool.Get(gridB);
            // 曼哈顿距离为 1 即为邻居
            return MatchPosUtil.IsNeighbor(a.Position.x, a.Position.y, b.Position.x, b.Position.y);
        }

        private void UpdateNormalState(ref NormalElementComponent normal, ElementScaleState animState,
            NormalFlashIconAniType flashType)
        {
            if (normal.ScaleState != animState || normal.FlashIconAniType != flashType)
            {
                normal.ScaleState = animState;
                normal.FlashIconAniType = flashType;
                normal.IsAnimDirty = true; // 通知 View 系统更新 Tween
            }
        }

        private void ClearSelectionVisuals(List<int> entities)
        {
            foreach (var entity in entities)
            {
                if (_normalPool.Has(entity))
                {
                    ref var normal = ref _normalPool.Get(entity);
                    // 恢复由 NormalElementSystem 或其它功能棋子的System 处理
                    normal.ScaleState = ElementScaleState.None;
                    normal.FlashIconAniType = NormalFlashIconAniType.None;
                    normal.IsAnimDirty = true;

                    if (_renderPool.Has(entity))
                    {
                        ref var render = ref _renderPool.Get(entity);
                        render.IsSelected = false;
                    }
                }
            }
        }

        private void PlayLinkAudio(int count)
        {
            ElementAudioManager.Instance.PlayMatchLink(count);
        }
    }
}