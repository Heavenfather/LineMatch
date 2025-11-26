using Hotfix.Define;
using HotfixCore.Extensions;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class BombElementItem : BaseElementItem
    {
        protected override void OnInitialized()
        {
            var effBig = this.GameObject.transform.Find("eff_big");
            if (effBig != null)
            {
                effBig.SetVisible(false);
            }

            var iconBig = this.GameObject.transform.Find("IconBig");
            if (iconBig != null)
            {
                iconBig.SetVisible(false);
            }
            var icon = this.GameObject.transform.Find("Icon");
            if (icon != null)
            {
                icon.SetVisible(true);
            }
            base.OnInitialized();
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            MatchManager.Instance.PlayBombAudio();
            if (context is { DoubleBombCoords: { Count: > 0 } })
            {
                if (context.DoubleBombCoords.Contains(Data.GridPos))
                {
                    //大爆炸的不要播放小爆炸特效，避免增加没必要的dc
                    if (context.DoubleBombCoords[0] == Data.GridPos)
                    {
                        HideBigIcon();
                        PlayDoubleBallEffect();
                    }
                    if (context.DoubleBombCoords[1] == Data.GridPos)
                    {
                        State = ElementState.CanRecycle;
                    }

                    if (context != null && context.IsCalculateCoinState)
                    {
                        context.AddCalAddedCount((int)ElementIdConst.Coin, 1);
                        
                        //金币结算
                        G.EventModule.DispatchEvent(GameEventDefine.OnMatchAddResultCoin);
                    }

                    return true;
                }
            }

            PlayEffect();
            return true;
        }

        public override void ResetSortingLayer(string layerName, int sortingOrder = 0)
        {
            Renderer iconBig = this.GameObject.transform.Find("IconBig")?.GetComponent<Renderer>();
            if (iconBig != null)
            {
                iconBig.sortingLayerName = layerName;
                iconBig.sortingOrder = sortingOrder;
            }
            base.ResetSortingLayer(layerName, sortingOrder);
        }

        public void SwitchToBigIcon()
        {
            this.GameObject.transform.Find("IconBig").gameObject.SetActive(true);
            this.GameObject.transform.Find("Icon").gameObject.SetActive(false);
        }

        private void PlayDoubleBallEffect()
        {
            this.GameObject.transform.localScale = Vector3.one;
            MatchEffectManager.Instance.PlayObjectEffect(MatchEffectType.BombAndBombBig,this.GameObject.transform, (b) =>
            {
                State = ElementState.CanRecycle;
                // MemoryPool.Release(this);
            });
        }

        private void HideBigIcon()
        {
            var iconBig = this.GameObject.transform.Find("IconBig");
            if (iconBig != null)
            {
                iconBig.SetVisible(false);
            }
        }
    }
}