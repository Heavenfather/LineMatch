using System;
using DG.Tweening;
using HotfixCore.Extensions;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class MatchCollectBase : MonoBehaviour
    {
        [SerializeField]
        public GameObject _effectObject;

        private Sequence _flySequence;

        private void Awake()
        {
            if (_effectObject != null)
            {
                SetEffectVisible(true);
            }
        }

        public void Initialize(Vector3 startPosition)
        {
            this.transform.position = startPosition;
        }

        public virtual void DoIconEffect(int elementId,Vector3 endValue,int index,Action callback)
        {
            var iconTarget = this.transform.Find("Icon");
            if (iconTarget != null)
                iconTarget.localScale = Vector3.one;
            endValue = new Vector3(endValue.x, endValue.y, 0);

            var beginPos = new Vector3(this.transform.position.x, this.transform.position.y, 0);
            var xMinus = (endValue.x - beginPos.x) * 0.5f;
            Vector3 offsetDir = new Vector3(xMinus, -0.2f, 0);
            var pathPos1 = beginPos + offsetDir;

            Vector3[] path = new Vector3[] { pathPos1, endValue };

            GetTweenSequence();
            float flyDur = GetFlyDuration();
            // _flySequence.AppendInterval(0.1f);

            _flySequence.AppendInterval(index * 0.05f);
            _flySequence.Append(this.transform.DOPath(path, flyDur, PathType.CatmullRom).SetEase(Ease.Linear));
            if (iconTarget != null)
            {
                _flySequence.Join(iconTarget.DOScale(Vector3.one * 1.2f, flyDur / 1.5f));
                _flySequence.AppendInterval(flyDur / 2);
                _flySequence.Join(iconTarget.DOScale(Vector3.one * 0.7f, flyDur / 1.5f));
            }

            _flySequence.SetAutoKill(true).OnComplete(() =>
            {
                callback?.Invoke();
            });
        }
        
        private void OnDisable()
        {
            ClearTween();
        }

        private void OnDestroy()
        {
            ClearTween();
        }

        public void SetEffectVisible(bool visible)
        {
            if(_effectObject == null)
                return;
            _effectObject.SetVisible(visible);
        }
        
        protected Sequence GetTweenSequence()
        {
            ClearTween(true);
            return _flySequence = DOTween.Sequence();
        }

        protected virtual float GetFlyDuration()
        {
            return 0.6f;
        }
        
        private void ClearTween(bool killComplete = false)
        {
            if (_flySequence != null)
            {
                _flySequence.Kill(killComplete);
                _flySequence = null;
            }
        }

        public void SetTweenSequence(Sequence sequence) {
            ClearTween();
            _flySequence = sequence;
        }
    }
}