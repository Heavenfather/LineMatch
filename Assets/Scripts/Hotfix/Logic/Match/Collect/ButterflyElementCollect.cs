using System;
using Spine.Unity;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class ButterflyElementCollect : MatchCollectBase
    {
        public override void DoIconEffect(int elementId, Vector3 endValue, int index, Action callback)
        {
            PlaySpineAnimation();
            base.DoIconEffect(elementId, endValue, index, callback);
        }

        private void PlaySpineAnimation()
        {
            // var iconTarget = this.transform.Find("Icon");
            // if (iconTarget != null)
            // {
            //     SkeletonAnimation spine = iconTarget.GetComponent<SkeletonAnimation>();
            //     if (spine != null)
            //     {
            //         spine.AnimationState.SetAnimation(0, "fly", true);
            //     }
            // }
        }

        protected override float GetFlyDuration()
        {
            return 1f;
        }
    }
}