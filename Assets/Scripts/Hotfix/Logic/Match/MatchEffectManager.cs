using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using GameCore.Singleton;
using HotfixCore.Extensions;
using HotfixCore.Module;
using HotfixCore.Utils;
using UnityEngine;

namespace HotfixLogic.Match
{
    public enum MatchEffectType
    {
        BlockBombPanel,
        DoubleBallBombPanel,
        ColorBallTipsTrail,
        BombTipsTrail,
        RocketTipsTrail,
        BombAndBombBig,
        JuanLianBroke,
    }

    public class MatchEffectManager : LazySingleton<MatchEffectManager>
    {
        private Dictionary<MatchEffectType, string> _effectTypeMap;
        private Dictionary<string, float> _cachedParticleDuration;
        private HashSet<string> _allEffectKeys = new HashSet<string>(100);

        protected override void OnInitialized()
        {
            _effectTypeMap = new Dictionary<MatchEffectType, string>()
            {
                [MatchEffectType.BlockBombPanel] = "effect/Match_eff_legao_baokai",
                [MatchEffectType.DoubleBallBombPanel] = "effect/Match_eff_caiqiu_jh",
                [MatchEffectType.ColorBallTipsTrail] = "effect/Match_eff_xcq_trail",
                [MatchEffectType.BombTipsTrail] = "effect/Match_eff_yx_trail",
                [MatchEffectType.RocketTipsTrail] = "effect/Match_eff_hj_trail",
                [MatchEffectType.BombAndBombBig] = "effect/Match_eff_zhadan_big",
                [MatchEffectType.JuanLianBroke] = "effect/Match_eff_chuanglian"
            };
            _cachedParticleDuration = new Dictionary<string, float>();
        }

        public void PlayObjectEffect(GameObject effectRoot)
        {
            if (effectRoot == null)
                return;
            ParticleSystem[] particles = effectRoot.GetComponentsInChildren<ParticleSystem>(true);
            if (particles.Length == 0)
                return;
            effectRoot.SetVisible(true);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Play();
            }
        }

        public void PlayObjectEffect(MatchEffectType effectType, Transform parent = null, Action<bool> complete = null)
        {
            var effectGo = Get(effectType, parent);
            if (effectGo == null)
            {
                complete?.Invoke(false);
                return;
            }
            InternalPlayEffect(effectGo, $"Effect-{effectType}", complete);
        }

        public void PlayObjectEffect(MatchEffectType effectType, Vector3 worldPosition, Action<bool> complete = null)
        {
            var effectGo = Get(effectType);
            if (effectGo == null)
            {
                complete?.Invoke(false);
                return;
            }

            effectGo.transform.position = worldPosition;
            InternalPlayEffect(effectGo, $"Effect-{effectType}", complete);
        }
        
        public void PlayObjectEffect(MatchEffectType effectType,Transform parent, Vector3 localPos, Action<bool> complete = null)
        {
            var effectGo = Get(effectType, parent, localPos);
            if (effectGo == null)
            {
                complete?.Invoke(false);
                return;
            }

            InternalPlayEffect(effectGo, $"Effect-{effectType}", complete);
        }
        
        public void PlayObjectEffect(int elementId,Action<bool> complete, Transform parent = null)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[elementId];
            if (string.IsNullOrEmpty(config.loadEffect))
            {
                complete?.Invoke(false);
                return;
            }
            string key = $"ElementEffect-{config.loadEffect}";
            var effectGo = ElementObjectPool.Instance.Spawn(key);
            if (effectGo == null)
            {
                complete?.Invoke(false);
                return;
            }

            if (parent != null)
            {
                effectGo.transform.SetParent(parent);
                effectGo.transform.localPosition = config.effectOffset;
            }

            InternalPlayEffect(effectGo, key, complete);
        }

        public UniTask PrewarmEffects()
        {
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            int count = _effectTypeMap.Count;
            int loaded = 0;
            foreach (var typeMap in _effectTypeMap)
            {
                string key = $"Effect-{typeMap.Key}";
                if(_allEffectKeys.Contains(key))
                    continue;
                _allEffectKeys.Add(key);
                ElementObjectPool.Instance.CreatePool(key, typeMap.Value, () =>
                {
                    loaded++;
                    if (loaded == count)
                    {
                        tcs.TrySetResult();
                    }
                }, 2).Forget();
            }

            return tcs.Task;
        }

        public UniTask PrewarmElementEffects(int elementId, int warmCnt, int poolMax)
        {
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[elementId];
            if (string.IsNullOrEmpty(config.loadEffect))
            {
                return UniTask.CompletedTask;
            }

            string location = $"effect/{config.loadEffect}";
            string key = $"ElementEffect-{config.loadEffect}";
            if (!_allEffectKeys.Add(key))
            {
                return UniTask.CompletedTask;
            }

            return ElementObjectPool.Instance.CreatePool(key, location,null, warmCnt / 2, poolMax);
        }
        
        public void ClearCacheKey()
        {
            _allEffectKeys.Clear();
        }
        
        public GameObject Get(MatchEffectType type, Transform parent = null,Vector3 localPos = default)
        {
            if (_effectTypeMap.TryGetValue(type, out string _))
            {
                GameObject obj = ElementObjectPool.Instance.Spawn($"Effect-{type}");

                if (parent != null)
                {
                    obj.transform.SetParent(parent);
                    obj.transform.localPosition = localPos;
                }

                return obj;
            }

            return null;
        }

        public void Recycle(GameObject obj)
        {
            ElementObjectPool.Instance.Recycle(obj);
        }

        private void InternalPlayEffect(GameObject effectGo,string cacheKey,Action<bool> complete = null)
        {
            ParticleSystem[] particles = effectGo.GetComponentsInChildren<ParticleSystem>(true);
            if (particles.Length == 0)
            {
                complete?.Invoke(false);
                return;
            }
            effectGo.SetVisible(true);
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i].emission.enabled)
                    particles[i].Play();
            }

            bool haveCache = _cachedParticleDuration.TryGetValue(cacheKey, out float duration);
            if (!haveCache)
            {
                duration = effectGo.GetParticleSystemLength();
                _cachedParticleDuration.Add(cacheKey, duration);
            }

            if (duration > 0)
            {
                UniTask.Create(async () =>
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(duration));
                    complete?.Invoke(true);
                    ElementObjectPool.Instance.Recycle(effectGo);
                }).Forget();
            }
            else
            {
                complete?.Invoke(false);
                ElementObjectPool.Instance.Recycle(effectGo);
            }
        }
    }
}