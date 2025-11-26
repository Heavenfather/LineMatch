using System;
using Spine.Unity;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class CoinCollectItem : MatchCollectBase
    {
        public override void DoIconEffect(int elementId, Vector3 endValue, int index, Action callback)
        {
            base.DoIconEffect(elementId, endValue, index, callback);
        }
    }
}