using DG.Tweening;
using Hotfix.Define;
using HotfixCore.Extensions;
using HotfixCore.Module;
using Spine.Unity;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class ColorBallElementItem : BaseElementItem
    {
        private int _normalElementId;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            PlayBallAnimation("Idle");
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            State = ElementState.CanRecycle;

            if (context != null && context.IsCalculateCoinState)
            {
                context.AddCalAddedCount((int)ElementIdConst.Coin, 1);
                
                //金币结算
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchAddResultCoin);
            }

            return true;
        }

        public void PlayBallAnimation(string animationName)
        {
            var eff = this.GameObject.transform.Find("eff");
            if (animationName == "Idle02")
            {
                if (eff != null)
                {
                    eff.SetVisible(true);
                }

                ResetSortingLayer("OverLine", 2);
            }
            else
            {
                eff.SetVisible(false);
            }

            var spine = this.GameObject.transform.Find("Icon").GetComponent<SkeletonAnimation>();
            if (spine != null)
            {
                spine.AnimationState.SetAnimation(0, animationName, true);
            }
        }

        public void DoPopScale()
        {
            this.GameObject.transform.DOScale(Vector3.one * 2.0f, 0.3f).SetEase(Ease.OutBounce).SetAutoKill(true);
        }
    }
}