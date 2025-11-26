using System;
using DG.Tweening;
using GameConfig;
using Hotfix.Define;
using Spine.Unity;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class DropBlockElementItem : ColorBlockElementItem
    {
        private Tween _moveTween;

        protected override void OnInitialized()
        {
            
        }
        
        public override void DoMove(float delayTime = 0,Ease ease = Ease.OutBounce)
        {
            _moveTween?.Kill();
            _moveTween = this.GameObject.transform.DOLocalMove(Vector3.zero, MatchConst.DropDuration)
                .SetEase(ease, 1.7f, 1).SetAutoKill().SetDelay(delayTime);
        }
        
        public override bool CanMove()
        {
            return true;
        }
        
        public override void Clear()
        {
            base.Clear();
            _moveTween?.Kill();
            _moveTween = null;
        }
    }
}