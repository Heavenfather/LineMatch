using System;
using System.Collections.Generic;
using DG.Tweening;
using Hotfix.Define;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class ColorBallTipsTrail : MonoBehaviour
    {
        [SerializeField]
        private List<TrailRenderer> _trailRenderers = new List<TrailRenderer>();

        private Tween _moveTween;
        private bool _isRecycle;
        
        public void DrawTrailRender(List<Vector3> positions)
        {
            ClearTween();
            this.transform.position = positions[0];
            for (int i = 0; i < _trailRenderers.Count; i++)
            {
                var trailRenderer = _trailRenderers[i];
                trailRenderer.Clear();
                trailRenderer.time = MatchConst.TipsMoveTimes + 5.5f;
            }
            
            _isRecycle = false;
            _moveTween = this.transform.DOPath(positions.ToArray(), MatchConst.TipsMoveTimes).SetEase(Ease.Linear).OnComplete(
                () =>
                {
                    MatchEffectManager.Instance.Recycle(this.gameObject);
                });
        }

        public void StopTrail()
        {
            if(_isRecycle)
                return;
            _isRecycle = true;
            ClearTween();
        }
        
        private void OnDestroy()
        {
            ClearTween();
        }

        private void ClearTween()
        {
            if (_moveTween != null)
            {
                _moveTween.Kill(true);
                _moveTween = null;
            }
        }
    }
}