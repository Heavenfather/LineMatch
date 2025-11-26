using DG.Tweening;
using GameConfig;
using Hotfix.Define;
using HotfixCore.Extensions;
using HotfixCore.Module;
using UnityEngine;

namespace HotfixLogic.Match
{
    /// <summary>
    /// 基础消除元素棋子
    /// </summary>
    public class BaseElementItem : ElementBase
    {
        private Tween _popTween;
        private Tween _moveTween;
        private Tween _tipsTween;
        private Tween _shakeTween;
        private Tween _useItemFlash;
        private Sequence _sequenceFlash;
        private bool _isInPop;

        protected override void OnInitialized()
        {
            if (Data.ElementType == ElementType.Normal)
            {
                SetElementColor();
            }
        }

        public override bool CanSelect()
        {
            return true;
        }

        public override void DoSelect()
        {
            // _oriScale = _pulseScale;
            StopPopTween();
            DoPopScale(1.2f, 0.3f);

            PlaySelectFlash();

            // AudioUtil.PlayMatchLine();
        }

        public override void DoDeselect()
        {
            StopPopTween();
            DoPopScale(1.0f, 0.3f);
        }

        public override void DoMove(float delayTime = 0, Ease ease = Ease.OutBounce)
        {
            _moveTween?.Kill();
            _moveTween = null;
            _moveTween = this.GameObject.transform.DOLocalMove(Vector3.zero, MatchConst.DropDuration)
                .SetEase(ease, 1.7f, 1).SetDelay(delayTime);
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            --Data.EliminateCount;
            if (Data.EliminateCount <= 0)
            {
                MatchManager.Instance.PlayEliminateAudio();

                PlayEffect();
                if (context != null && context.IsCalculateCoinState)
                {
                    context.AddCalAddedCount((int)ElementIdConst.Coin, 1);
                    
                    //金币结算
                    G.EventModule.DispatchEvent(GameEventDefine.OnMatchAddResultCoin);
                }
                
                return true;
            }

            return false;
        }

        public void SetElementColor()
        {
            Color color = ElementSystem.Instance.GetElementColor(Data.ConfigId);
            var flashIcon = this.GameObject.transform.Find("FlashIcon");
            if (flashIcon != null)
            {
                flashIcon.GetComponent<SpriteRenderer>().color = color;
            }
            var icon = this.GameObject.transform.Find("Icon");
            if (icon != null && icon.GetComponent<SpriteRenderer>() != null)
                icon.GetComponent<SpriteRenderer>().color = color;
        }
        
        public void PopRectangleEffect()
        {
            if (_isInPop)
                return;

            _isInPop = true;
            _popTween?.Kill();
            _popTween = null;
            _popTween = DOTween
                .To(() => 1.0f, x => { this.GameObject.transform.localScale = Vector3.one * x; }, 1.2f, 0.5f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

            PlaySelectFlash();
        }

        public void StopPopTween(bool resetScale = true)
        {
            if (!_isInPop)
                return;
            if (_popTween != null)
            {
                _popTween?.Kill();
                _popTween = null;
            }

            if (resetScale)
                DoPopScale(1.0f, 0.3f);
            else
                this.GameObject.transform.localScale = Vector3.one * 1.2f;
            _isInPop = false;
        }

        public void DoTipsTween(float endValue,bool isX)
        {
            _tipsTween?.Kill(true);
            _tipsTween = null;

            _tipsTween = isX
                ? this.GameObject.transform.DOLocalMoveX(endValue, 0.3f).SetLoops(4, LoopType.Yoyo).OnComplete(() =>
                {
                    _tipsTween.Rewind();
                })
                : this.GameObject.transform.DOLocalMoveY(endValue, 0.3f).SetLoops(4, LoopType.Yoyo).OnComplete(() =>
                {
                    _tipsTween.Rewind();
                });
        }

        public void StopTipsTween()
        {
            _tipsTween?.Kill();
            _tipsTween = null;
        }

        public void DoShake(bool isPlay)
        {
            if (isPlay)
            {
                _shakeTween?.Kill(true);
                _shakeTween = null;
                _useItemFlash?.Kill(true);
                _useItemFlash = null;
                _shakeTween = this.GameObject.transform.DOShakePosition(0.15f, 0.05f).SetLoops(-1,LoopType.Yoyo).OnKill(
                    () =>
                    {
                        this.GameObject.transform.localPosition = Vector3.zero;
                    });
            }
            else
            {
                _shakeTween?.Kill(true);
                _shakeTween = null;
            }
        }

        public void DoUseItemFlashIcon(bool isPlay)
        {
            if(this.GameObject == null || this.GameObject.transform == null)
                return;
            var flashIcon = this.GameObject.transform.Find("FlashIcon");
            if (flashIcon == null)
                return;
            _sequenceFlash?.Kill(true);
            _sequenceFlash = null;
            _shakeTween?.Kill(true);
            _shakeTween = null;
            if (isPlay)
            {
                _useItemFlash?.Kill(true);
                _useItemFlash = null;
                flashIcon.SetVisible(true);
                flashIcon.transform.localScale = Vector3.one;
                var flashIconRenderer = flashIcon.GetComponent<SpriteRenderer>();
                var curColor = flashIconRenderer.color;
                curColor.a = 0.7f;
                flashIconRenderer.color = curColor;
                _useItemFlash = flashIcon.DOScale(Vector3.one * 1.3f, 1f).SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.Linear).OnKill(() =>
                    {
                        flashIcon.transform.localScale = Vector3.one;
                        flashIcon.SetVisible(false);
                    });
            }
            else
            {
                _useItemFlash?.Kill(true);
                _useItemFlash = null;
                flashIcon.SetVisible(false);
            }
        }

        public override void Clear()
        {
            base.Clear();
            _popTween?.Kill();
            _popTween = null;
            _moveTween?.Kill();
            _moveTween = null;
            _tipsTween?.Kill();
            _tipsTween = null;
            _shakeTween?.Kill();
            _shakeTween = null;
            _useItemFlash?.Kill();
            _useItemFlash = null;
            _sequenceFlash?.Kill();
            _sequenceFlash = null;
            _isInPop = false;
            if (this.GameObject != null && this.GameObject.transform != null)
            {
                var flashIcon = this.GameObject.transform.Find("FlashIcon");
                if (flashIcon != null)
                {
                    flashIcon.gameObject.transform.localScale = Vector3.one;
                }
            }
        }

        public void DoPopScale(float scale, float duration)
        {
            _popTween?.Kill();
            _popTween = this.GameObject.transform.DOScale(scale, duration);
        }

        protected override void PlayEffect(bool hideIcon = true)
        {
            this.GameObject.transform.Find("FlashIcon")?.SetVisible(false);
            base.PlayEffect(hideIcon);
        }

        public void PlaySelectFlash(float duration = 0.8f, float startScale = 2.0f,float endScale = 1.5f)
        {
            var flashIcon = this.GameObject.transform.Find("FlashIcon");
            if (flashIcon == null)
                return;

            if (GameObject.transform.localScale.x > 1)
            {
                return;
            }

            if (_useItemFlash != null)
            {
                _useItemFlash.Kill();
                _useItemFlash = null;
            }

            flashIcon.gameObject.SetActive(true);
            flashIcon.transform.localScale = Vector3.one * startScale;

            if (_sequenceFlash != null)
            {
                _sequenceFlash.Kill(true);
                _sequenceFlash = null;
            }

            _sequenceFlash = DOTween.Sequence();
            var flashIconRenderer = flashIcon.GetComponent<SpriteRenderer>();

            var curColor = flashIconRenderer.color;
            curColor.a = 0.7f;
            flashIconRenderer.color = curColor;
            
            _sequenceFlash.Append(flashIcon.transform.DOScale(endScale, duration));
            _sequenceFlash.Join(flashIconRenderer.DOFade(0f, duration));
            _sequenceFlash.OnComplete(() =>
            {
                _sequenceFlash = null;
                flashIcon.transform.localScale = Vector3.one;
                flashIcon.gameObject.SetActive(false);
            });
        }
        
    }
}