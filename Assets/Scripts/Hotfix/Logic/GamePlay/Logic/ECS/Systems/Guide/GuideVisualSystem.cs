using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using HotfixCore.Extensions;
using HotfixCore.MemoryPool;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 引导视觉系统
    /// 负责引导的视觉表现：手指动画、高亮效果、图层切换等
    /// </summary>
    public class GuideVisualSystem : IEcsRunSystem, IEcsInitSystem, IEcsDestroySystem
    {
        private EcsWorld _world;
        private GameStateContext _context;
        private IBoard _board;
        
        private EcsFilter _guideFilter;
        private EcsFilter _gridFilter;
        private EcsFilter _elementFilter;
        
        private EcsPool<GuideStateComponent> _guideStatePool;
        private EcsPool<GuidePathComponent> _guidePathPool;
        private EcsPool<GuideVisualComponent> _guideVisualPool;
        private EcsPool<GridCellComponent> _gridPool;
        private EcsPool<ElementComponent> _elePool;
        private EcsPool<ElementRenderComponent> _renderPool;
        private EcsPool<ElementPositionComponent> _posPool;
        
        private int _guideEntity = -1;
        private const string GUIDE_MASK_LAYER = "GuideMaskItem";
        private const string DEFAULT_LAYER = "Background";
        
        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _board = _context.Board;
            
            _guideFilter = _world.Filter<GuideStateComponent>().End();
            _gridFilter = _world.Filter<GridCellComponent>().End();
            _elementFilter = _world.Filter<ElementComponent>().Include<ElementRenderComponent>().End();
            
            _guideStatePool = _world.GetPool<GuideStateComponent>();
            _guidePathPool = _world.GetPool<GuidePathComponent>();
            _guideVisualPool = _world.GetPool<GuideVisualComponent>();
            _gridPool = _world.GetPool<GridCellComponent>();
            _elePool = _world.GetPool<ElementComponent>();
            _renderPool = _world.GetPool<ElementRenderComponent>();
            _posPool = _world.GetPool<ElementPositionComponent>();
            
            // 创建引导实体
            CreateGuideEntity();
        }
        
        private void CreateGuideEntity()
        {
            _guideEntity = _world.NewEntity();
            
            ref var guideState = ref _guideStatePool.Add(_guideEntity);
            guideState.IsGuiding = false;
            guideState.IsGuideLevel = false;
            guideState.CurrentGuideId = 0;
            guideState.GuideType = GuideType.None;
            
            ref var guidePath = ref _guidePathPool.Add(_guideEntity);
            guidePath.PathCoords = new List<Vector2Int>();
            guidePath.HighlightElementIds = new List<int>();
            
            ref var guideVisual = ref _guideVisualPool.Add(_guideEntity);
            guideVisual.GuideMaskLayerName = GUIDE_MASK_LAYER;
            guideVisual.IsFingerVisible = false;
            
            // 从场景中获取引导相关的GameObject引用
            // 这些需要在场景初始化时设置
            // guideVisual.GuideFinger = ...
            // guideVisual.GuideCamera = ...
        }
        
        public void Run(IEcsSystems systems)
        {
            if (_guideEntity < 0) return;
            
            ref var guideState = ref _guideStatePool.Get(_guideEntity);
            if (!guideState.IsGuiding) return;
            
            ref var guideVisual = ref _guideVisualPool.Get(_guideEntity);
            
            // 根据引导类型更新视觉效果
            switch (guideState.GuideType)
            {
                case GuideType.Weak:
                    UpdateWeakGuideVisual(ref guideState, ref guideVisual);
                    break;
                case GuideType.Force:
                    UpdateForceGuideVisual(ref guideState, ref guideVisual);
                    break;
            }
        }
        
        /// <summary>
        /// 更新弱引导视觉效果
        /// </summary>
        private void UpdateWeakGuideVisual(ref GuideStateComponent guideState, ref GuideVisualComponent guideVisual)
        {
            if (!_guidePathPool.Has(_guideEntity)) return;
            
            ref var guidePath = ref _guidePathPool.Get(_guideEntity);
            
            // 高亮指定的元素
            HighlightElements(guidePath.HighlightElementIds, guidePath.PathCoords);
        }
        
        /// <summary>
        /// 更新强引导视觉效果
        /// </summary>
        private void UpdateForceGuideVisual(ref GuideStateComponent guideState, ref GuideVisualComponent guideVisual)
        {
            if (!_guidePathPool.Has(_guideEntity)) return;
            
            ref var guidePath = ref _guidePathPool.Get(_guideEntity);
            
            // 如果手指动画还没开始，启动它
            if (guideVisual.FingerTween == null || !guideVisual.FingerTween.IsPlaying())
            {
                StartFingerAnimation(ref guidePath, ref guideVisual);
            }
        }
        
        /// <summary>
        /// 高亮指定元素
        /// </summary>
        private void HighlightElements(List<int> elementIds, List<Vector2Int> coords)
        {
            // 先重置所有元素的图层
            ResetAllLayers();
            
            // 高亮指定坐标的格子和元素
            foreach (var coord in coords)
            {
                // 找到对应坐标的格子实体
                foreach (var gridEntity in _gridFilter)
                {
                    ref var grid = ref _gridPool.Get(gridEntity);
                    if (grid.Position == coord)
                    {
                        // 切换图层到引导遮罩层
                        var gridInstance = _board.GetGridInstance(coord.x, coord.y);
                        if (gridInstance != null)
                        {
                            SetGameObjectLayer(gridInstance, GUIDE_MASK_LAYER);
                        }
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// 启动手指动画
        /// </summary>
        private void StartFingerAnimation(ref GuidePathComponent guidePath, ref GuideVisualComponent guideVisual)
        {
            if (guidePath.PathCoords == null || guidePath.PathCoords.Count == 0) return;
            if (guideVisual.GuideFinger == null) return;
            
            // 清理旧动画
            guideVisual.FingerTween?.Kill();
            
            // 设置手指图层
            SetGameObjectLayer(guideVisual.GuideFinger, GUIDE_MASK_LAYER);
            
            var fingerSp = guideVisual.GuideFinger.GetComponent<SpriteRenderer>();
            var fingerChildSp = guideVisual.GuideFinger.transform.GetChild(0).GetComponent<SpriteRenderer>();
            
            // 创建手指动画序列
            var seq = DOTween.Sequence();
            seq.AppendInterval(0.5f);
            // seq.AppendCallback(() =>
            // {
            //     guideVisual.IsFingerVisible = true;
            // });
            seq.Append(fingerSp.DOFade(1, 0.3f));
            seq.Join(fingerChildSp.DOFade(1, 0.3f));
            
            // 添加路径动画
            for (int i = 0; i < guidePath.PathCoords.Count; i++)
            {
                Vector2Int coord = guidePath.PathCoords[i];
                Vector3 targetPos = GetWorldPositionByCoord(coord);
                
                if (i == 0)
                {
                    guideVisual.GuideFinger.transform.position = targetPos;
                }
                else
                {
                    seq.Append(guideVisual.GuideFinger.transform.DOMove(targetPos, 0.2f).SetEase(Ease.Linear));
                }
            }
            
            // 循环播放
            seq.AppendInterval(0.35f);
            seq.Append(fingerSp.DOFade(0, 0.2f));
            seq.Join(fingerChildSp.DOFade(0, 0.2f));
            // seq.AppendCallback(() => { guideVisual.IsFingerVisible = false; });
            seq.AppendInterval(0.5f);
            
            seq.SetLoops(-1, LoopType.Restart);
            guideVisual.FingerTween = seq;
        }
        
        /// <summary>
        /// 重置所有图层
        /// </summary>
        private void ResetAllLayers()
        {
            foreach (var gridEntity in _gridFilter)
            {
                ref var grid = ref _gridPool.Get(gridEntity);
                var gridInstance = _board.GetGridInstance(grid.Position.x, grid.Position.y);
                if (gridInstance != null)
                {
                    SetGameObjectLayer(gridInstance, DEFAULT_LAYER);
                }
            }
        }
        
        /// <summary>
        /// 设置GameObject及其子物体的图层
        /// </summary>
        private void SetGameObjectLayer(GameObject go, string layerName)
        {
            if (go == null) return;
            
            int layer = LayerMask.NameToLayer(layerName);
            foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                renderer.gameObject.layer = layer;
            }
        }
        
        /// <summary>
        /// 根据坐标获取世界位置
        /// </summary>
        private Vector3 GetWorldPositionByCoord(Vector2Int coord)
        {
            foreach (var gridEntity in _gridFilter)
            {
                ref var grid = ref _gridPool.Get(gridEntity);
                if (grid.Position == coord)
                {
                    return grid.WorldPosition;
                }
            }
            return Vector3.zero;
        }
        
        /// <summary>
        /// 开始引导
        /// </summary>
        public void StartGuide(int guideId, GuideType guideType, string parameters, string parameters2)
        {
            if (_guideEntity < 0) return;
            
            ref var guideState = ref _guideStatePool.Get(_guideEntity);
            guideState.IsGuiding = true;
            guideState.CurrentGuideId = guideId;
            guideState.GuideType = guideType;
            
            ref var guidePath = ref _guidePathPool.Get(_guideEntity);
            guidePath.GuideParameters = parameters;
            guidePath.GuideParameters2 = parameters2;
            guidePath.PathCoords.Clear();
            guidePath.HighlightElementIds.Clear();
            
            // 解析引导参数
            ParseGuideParameters(ref guidePath, guideType, parameters);
            
            Logger.Debug($"开始引导: ID={guideId}, Type={guideType}");
        }
        
        /// <summary>
        /// 停止引导
        /// </summary>
        public void StopGuide()
        {
            if (_guideEntity < 0) return;
            
            ref var guideState = ref _guideStatePool.Get(_guideEntity);
            guideState.IsGuiding = false;
            guideState.GuideType = GuideType.None;
            
            ref var guideVisual = ref _guideVisualPool.Get(_guideEntity);
            guideVisual.FingerTween?.Kill();
            guideVisual.FingerTween = null;
            guideVisual.IsFingerVisible = false;
            
            // 重置所有图层
            ResetAllLayers();
            
            Logger.Debug("停止引导");
        }
        
        /// <summary>
        /// 解析引导参数
        /// </summary>
        private void ParseGuideParameters(ref GuidePathComponent guidePath, GuideType guideType, string parameters)
        {
            if (string.IsNullOrEmpty(parameters)) return;
            
            if (guideType == GuideType.Weak)
            {
                // 弱引导：参数是元素ID列表，用|分隔
                string[] elementIds = parameters.Split('|');
                foreach (var idStr in elementIds)
                {
                    if (int.TryParse(idStr, out int elementId))
                    {
                        guidePath.HighlightElementIds.Add(elementId);
                    }
                }
            }
            else if (guideType == GuideType.Force)
            {
                // 强引导：参数是坐标列表，格式：x,y|x,y|...
                string[] coords = parameters.Split('|');
                foreach (var coordStr in coords)
                {
                    string[] xy = coordStr.Split(',');
                    if (xy.Length == 2 && int.TryParse(xy[0], out int x) && int.TryParse(xy[1], out int y))
                    {
                        guidePath.PathCoords.Add(new Vector2Int(x, y));
                    }
                }
                guidePath.IsRestrictPath = true;
            }
        }
        
        public void Destroy(IEcsSystems systems)
        {
            StopGuide();
        }
    }
}
