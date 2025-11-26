using System;
using HotfixCore.Extensions;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class WishElementCollect : MatchCollectBase
    {
        public override void DoIconEffect(int elementId,Vector3 endValue, int index, Action callback)
        {
            var icon = this.transform.Find("Icon");
            if (icon != null)
            {
                icon.SetVisible(false);
            }
            base.DoIconEffect(elementId,endValue, index, callback);
        }

        protected override float GetFlyDuration()
        {
            return 1.2f;
        }
    }
}