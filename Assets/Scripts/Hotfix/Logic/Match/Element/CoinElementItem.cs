using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Module;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class CoinElementItem : DropBlockElementItem
    {
        protected override bool OnDestroy(ElementDestroyContext context)
        {
            bool bResult = base.OnDestroy(context);
            if (bResult)
            {
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchElementMoveToTarget,
                    EventTwoParam<int, Vector3>.Create(this.Data.ConfigId, GameObject.transform.position));
            }
            return bResult;
        }
    }
}