using System.Collections.Generic;
using DG.Tweening;
using Hotfix.Define;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class RocketTipsTrail : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem _rootParticle;

        private Tween _moveTween;
        private bool _isRecycle;
        
        public void PlayTrail(List<Vector3> positions)
        {
            this.transform.position = positions[0];
            ClearTween();
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