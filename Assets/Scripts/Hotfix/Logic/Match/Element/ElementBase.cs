using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using HotfixCore.Extensions;
using HotfixCore.MemoryPool;
using Spine.Unity;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    public abstract class ElementBase : IElementItem
    {
        public GameObject GameObject { get; set; }

        public ElementItemData Data { get; set; }
        
        public ElementState State { get; set; }

        private Tween _shockTween;

        public void Initialize(Transform parent, ElementItemData data)
        {
            State = ElementState.Using;
            Data = data;
            GameObject = ElementObjectPool.Instance.Spawn($"Element-{data.ConfigId}");
            GameObject.transform.localScale = Vector3.one;
            SetIconInitMethod().Forget();

            if (_shockTween != null)
            {
                _shockTween.Kill();
                _shockTween = null;
            }

            SetParent(parent);
            OnInitialized();
        }

        public void SetParent(Transform parent)
        {
            if (GameObject == null)
                return;
            GameObject.transform.SetParent(parent);
        }

        public virtual void DoSelect()
        {
        }

        public virtual void DoDeselect()
        {
        }

        public virtual void ResetSortingLayer(string layerName, int sortingOrder = 0)
        {
            Renderer icon = this.GameObject.transform.Find("Icon")?.GetComponent<Renderer>();
            if (icon != null)
            {
                icon.sortingLayerName = layerName;
                icon.sortingOrder = sortingOrder;
            }
        }

        public async UniTask AfterDestroy(ElementDestroyContext context, bool immediate = false,Action callback = null)
        {
            if (immediate)
            {
                callback?.Invoke();
                MemoryPool.Release(this);
                OnAfterDestroy(context);
            }
            else
            {
                await UniTask.WaitUntil(() => State == ElementState.CanRecycle);
                callback?.Invoke();
                MemoryPool.Release(this);
                OnAfterDestroy(context);
            }
        }

        protected virtual void OnAfterDestroy(ElementDestroyContext context)
        {
            
        }
        
        /// <summary>
        /// 元素是否可移动
        /// </summary>
        /// <returns></returns>
        public virtual bool CanMove()
        {
            return true;
        }

        public virtual bool CanSelect()
        {
            return false;
        }

        public virtual void DoMove(float delayTime = 0, Ease ease = Ease.OutBounce)
        {
        }

        protected virtual void OnInitialized()
        {
        }

        public bool DoDestroy(ElementDestroyContext context)
        {
            State = ElementState.Destroying;
            bool result = OnDestroy(context);
            if(!result)
                State = ElementState.Using;
            return result;
        }

        protected abstract bool OnDestroy(ElementDestroyContext context);

        protected virtual async UniTask SetIconInitMethod()
        {
            var icon = this.GameObject.transform.Find("Icon");
            if (icon != null)
            {
                icon.SetVisible(false);
                // ResetSortingLayer("Item", Data.SortOrder);
                ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
                ElementMap config = db[Data.ConfigId];
                if (!string.IsNullOrEmpty(config.idleSpine))
                {
                    SetIdleSpine(icon, config.idleSpine);
                }
                if(config.delayShow > 0)
                    await UniTask.Delay(TimeSpan.FromSeconds(config.delayShow));
                icon.SetVisible(true);
            }
        }

        public virtual void Clear()
        {
            if (State == ElementState.Recycle)
            {
                return;
            }

            State = ElementState.Recycle;
            StopShock();
            if (this.GameObject != null && this.GameObject.transform != null)
            {
                foreach (var child in GameObject.GetComponentsInChildren<Renderer>(true))
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Background");
                }

                ElementObjectPool.Instance.Recycle(GameObject);
            }

            MemoryPool.Release(Data);
        }

        public void PlayShock()
        {
            if (_shockTween != null) return;

            System.Random random = new System.Random();
            double randomX1 = random.NextDouble() * 0.04 - 0.02;
            double randomY1 = random.NextDouble() * 0.04 - 0.02;
            var pos1 = new Vector3((float)randomX1, (float)randomY1, 0);


            double randomX2 = random.NextDouble() * 0.04 - 0.02;
            double randomY2 = random.NextDouble() * 0.04 - 0.02;
            var pos2 = new Vector3((float)randomX2, (float)randomY2, 0);

            var seq = DOTween.Sequence();
            seq.Append(GameObject.transform.DOScale(Vector3.one * 1.1f, 0.15f).SetEase(Ease.OutBounce));
            seq.Join(GameObject.transform.DOLocalMove(pos1, 0.15f));

            seq.Append(GameObject.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBounce));
            seq.Join(GameObject.transform.DOLocalMove(pos2, 0.15f));

            seq.SetLoops(-1, LoopType.Yoyo);
            _shockTween = seq;
        }

        protected virtual void SetIdleSpine(Transform spineTran, string spineName, bool loop = true,bool isEmpty = false)
        {
            SkeletonAnimation spine = spineTran?.GetComponent<SkeletonAnimation>();
            if (spine != null)
            {
                if (isEmpty)
                {
                    spine.AnimationState.SetEmptyAnimation(0, 0);
                }
                else
                {
                    spine.AnimationState.SetAnimation(0, spineName, loop);
                }
            }
        }

        public void StopShock()
        {
            if (_shockTween != null)
            {
                _shockTween.Kill();
                _shockTween = null;
            }
        }
        
        protected virtual bool PlayHitEffect(Action<bool> callback)
        {
            SkeletonAnimation spine = this.GameObject.transform.Find("Icon")?.GetComponent<SkeletonAnimation>();
            if (spine != null)
            {
                var animation = GetHitAnimation();
                if (animation != null)
                {
                    spine.AnimationState.SetAnimation(0, animation, false).Complete += (state) =>
                    {
                        callback?.Invoke(true);
                    };
                    return true;
                }
                else
                {
                    callback?.Invoke(false);
                }
            }
            else
            {
                callback?.Invoke(false);
            }
            return false;
        }

        public Spine.Animation GetHitAnimation()
        {
            SkeletonAnimation spine = this.GameObject.transform.Find("Icon")?.GetComponent<SkeletonAnimation>();
            if (spine != null)
            {
                ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
                ref readonly ElementMap config = ref db[Data.ConfigId];
                if (!string.IsNullOrEmpty(config.hitSpine))
                {
                    var animation = spine.Skeleton.Data.FindAnimation(config.hitSpine);
                    return animation;
                }
            }

            return null;
        }

        protected virtual void PlayEffect(bool hideIcon = true)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ElementMap config = db[Data.ConfigId];
            ElementAudioManager.Instance.Play(config.Id);

            var hitAnimation = GetHitAnimation();
            if (hitAnimation == null)
            {
                MatchEffectManager.Instance.PlayObjectEffect(this.Data.ConfigId, (b) =>
                {
                    State = ElementState.CanRecycle;
                    // MemoryPool.Release(this);
                }, this.GameObject.transform);

                if (hideIcon)
                {
                    var icon = this.GameObject.transform.Find("Icon");
                    if (icon != null)
                    {
                        icon.SetVisible(false);
                    }
                }
            }
            else
            {
                bool haveHit = PlayHitEffect((b) =>
                {
                    if (hideIcon)   
                    {
                        var icon = this.GameObject.transform.Find("Icon");
                        if (icon != null)
                        {
                            icon.SetVisible(false);
                        }
                    }

                    if (State != ElementState.CanRecycle && State != ElementState.Recycle)
                    {
                        State = ElementState.CanRecycle;
                    }
                });
                if (config.delayTimePlay <= 0)
                {
                    MatchEffectManager.Instance.PlayObjectEffect(this.Data.ConfigId, (b) =>
                    {
                        if (b)
                            State = ElementState.CanRecycle;
                        else
                        {
                            if (!haveHit)
                            {
                                State = ElementState.CanRecycle;
                            }
                        }
                    }, this.GameObject.transform);
                }
                else
                {
                    DelayPlayHitEffect(config.delayTimePlay).Forget();
                }
            }
        }

        private async UniTask DelayPlayHitEffect(float delay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            MatchEffectManager.Instance.PlayObjectEffect(this.Data.ConfigId, (b) =>
            {
                State = ElementState.CanRecycle;
                // MemoryPool.Release(this);
            }, this.GameObject.transform);
        }

    }
}