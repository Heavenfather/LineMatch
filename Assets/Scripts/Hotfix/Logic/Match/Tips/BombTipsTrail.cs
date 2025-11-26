using System.Collections.Generic;
using DG.Tweening;
using Hotfix.Define;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    public class BombTipsTrail : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer _fuseLine;
        [SerializeField]
        private Transform _flameTarget;
        
        private const int SEGMENT_COUNT = 5;
        private float _burnProgress;
        private List<Vector3> _pathPoints = new List<Vector3>();
        private int _currentPointCount = 0;
        private bool _isRecycle;
        private Tween _igniteTween;

        // 设置引线路径
        public void DrawTrail(List<Vector3> path)
        {
            ClearTween();
            
            _isRecycle = false;
            _pathPoints = SmoothPath(path);
            _currentPointCount = _pathPoints.Count;
            _burnProgress = 0;
            Ignite();
        }

        public void StopTrail()
        {
            if(_isRecycle)
                return;
            _isRecycle = true;
            ClearTween();
        }

        private List<Vector3> SmoothPath(List<Vector3> path)
        {
            if (path.Count < 2)
                return path;
            List<Vector3> smoothedPath = new List<Vector3>();
            for (int i = 0; i < path.Count - 1; i++)
            {
                smoothedPath.Add(path[i]);
                //在当前点和下一个点之间插入中间点以实现线段的平滑
                for (int j = 1; j <= SEGMENT_COUNT; j++)
                {
                    float t = (j * 1.0f) / (SEGMENT_COUNT + 1);
                    Vector3 point = Vector3.Lerp(path[i], path[i + 1], t);
                    smoothedPath.Add(point);
                }
            }
            //添加最后一个点
            smoothedPath.Add(path[^1]);
            smoothedPath.Reverse();
            return smoothedPath;
        }
        
        // 开始燃烧
        private void Ignite()
        {
            UpdateFuseLine(_pathPoints.Count);
            _igniteTween = DOTween.To(() => 0f, x =>
            {
                if(_pathPoints.Count <= 0)
                    return;
                _burnProgress = x;
                //计算当前应该显示的点数
                int targetPointCount = Mathf.FloorToInt(_pathPoints.Count * (1 - _burnProgress));
                targetPointCount = Mathf.Max(2, targetPointCount);
                // Logger.Debug($"当前点数：{targetPointCount} 进度:{_burnProgress}");
                if (targetPointCount != _currentPointCount)
                {
                    UpdateFuseLine(targetPointCount);
                    _currentPointCount = targetPointCount;
                }
                UpdateFlamePosition();

            }, 1f, MatchConst.TipsMoveTimes).SetEase(Ease.Linear).OnComplete(OnFuseComplete);
        }
        
        // 更新火光位置
        private void UpdateFlamePosition()
        {
            if (_flameTarget == null || _pathPoints.Count == 0) return;
        
            // 根据燃烧进度计算火光在路径上的位置
            float progress = 1f - _burnProgress;
            int pointIndex = Mathf.FloorToInt(progress * (_pathPoints.Count - 1));
            pointIndex = Mathf.Min(pointIndex, _pathPoints.Count - 1);
        
            _flameTarget.position = _pathPoints[pointIndex];
        }
        
        // 更新引线线段
        private void UpdateFuseLine(int targetCount)
        {
            int pointCount = Mathf.Min(targetCount, _pathPoints.Count);
            _fuseLine.positionCount = pointCount;
            _fuseLine.startWidth = 0.25f;
            _fuseLine.endWidth = 0.25f;
            for (int i = 0; i < pointCount; i++)
            {
                _fuseLine.SetPosition(i, _pathPoints[i]);
            }
        }

        // 引线燃烧完成回调
        private void OnFuseComplete()
        {
            _pathPoints.Clear();
            MatchEffectManager.Instance.Recycle(this.gameObject);
        }
        
        private void ClearTween()
        {
            if (_igniteTween != null)
            {
                _igniteTween.Kill(true);
                _igniteTween = null;
            }
        }
    }
}