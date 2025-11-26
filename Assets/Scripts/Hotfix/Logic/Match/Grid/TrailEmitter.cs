using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Hotfix.Define;
using UnityEngine;

namespace HotfixLogic.Match
{
    public enum TrailEmitterType
    {
        Trail,
        StepTrail
    }
    
    public class TrailEmitter : MonoBehaviour
    {
        [SerializeField] private GameObject _trail;
        [SerializeField] private GameObject _stepTrail;

        // private Sequence _emitterSequence;
        private Queue<GameObject> _trailPool = new Queue<GameObject>();
        private Queue<GameObject> _stepTrailPool = new Queue<GameObject>();

        public void Emitter(Vector3 startPos, List<Vector3> endPositions, Action<int> onStepComplete, Action onComplete, TrailEmitterType type = TrailEmitterType.Trail)
        {
            if (endPositions == null || endPositions.Count <= 0)
            {
                onComplete?.Invoke();
                return;
            }
            ClearSequence();

            var emitterSequence = DOTween.Sequence();
            int poolCount = type == TrailEmitterType.Trail ? _trailPool.Count : _stepTrailPool.Count;
            bool enough = poolCount >= endPositions.Count;
            int loadedCount = 0;
            int total = endPositions.Count;
            emitterSequence.Pause();
            for (int i = 0; i < endPositions.Count; i++)
            {
                int index = i;
                emitterSequence.AppendInterval(MatchConst.EmitterInterval);
                emitterSequence.AppendCallback(() =>
                {
                    GameObject obj = null;
                    if (enough)
                    {
                        obj = PopTrail(type);
                    }
                    else
                    {
                        obj = Instantiate(type == TrailEmitterType.Trail ? _trail : _stepTrail, this.transform);
                    }
                    
                    obj.transform.position = startPos;
                    obj.transform.DOKill();
                    obj.transform.DOMove(endPositions[index], MatchConst.TrailEmitterDuration).SetEase(Ease.OutQuad).OnComplete(() =>
                    {
                        try
                        {
                            if (onStepComplete != null)
                            {
                                onStepComplete(index);
                            }
                        }
                        finally
                        {
                            PushTrail(obj, type);
                            loadedCount++;
                            if (loadedCount >= total)
                            {
                                onComplete?.Invoke();
                            }
                        }
                    });
                });
            }
            emitterSequence.Play();
        }

        private GameObject PopTrail(TrailEmitterType type)
        {
            if (type == TrailEmitterType.Trail)
            {
                if (_trailPool.Count <= 0)
                {
                    var obj = Instantiate(_trail, this.transform);
                    return obj;
                }

                var trail = _trailPool.Dequeue();
                trail.SetActive(true);
                return trail;
            }
            if (type == TrailEmitterType.StepTrail)
            {
                if (_stepTrailPool.Count <= 0)
                {
                    var obj = Instantiate(_stepTrail, this.transform);
                    return obj;
                }

                var trail = _stepTrailPool.Dequeue();
                trail.SetActive(true);
                return trail;
            }

            return null;
        }

        private void PushTrail(GameObject trail, TrailEmitterType type)
        {
            trail.SetActive(false);
            if (type == TrailEmitterType.Trail)
                _trailPool.Enqueue(trail);
            else if(type == TrailEmitterType.StepTrail)
                _stepTrailPool.Enqueue(trail);
        }

        private void OnDestroy()
        {
            ClearSequence();
        }

        private void ClearSequence()
        {
            // if (_emitterSequence != null)
            // {
            //     _emitterSequence.Kill();
            //     _emitterSequence = null;
            // }
        }
    }
}