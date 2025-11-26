using System;
using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.Define;
using Hotfix.Logic.Match;
using HotfixCore.Extensions;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class RockElementItem : BaseElementItem
    {
        protected override void OnInitialized()
        {
            var effectTrail = this.GameObject.transform.Find("eff_trail")?.gameObject;
            if (effectTrail != null)
                effectTrail.SetVisible(false);
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            if (context != null)
            {
                if (context.IsColorBallClearAll)
                {
                    MatchTweenUtil.DoRocketTargetMove(this.GameObject, Data.GridPos, Data.Direction,
                        () =>
                        {
                            State = ElementState.CanRecycle;
                            // MemoryPool.Release(this);
                        });
                }
                else if (context.UsingItemId == (int)ItemDef.EliminateArrow ||
                    context.UsingItemId == (int)ItemDef.EliminateBullet)
                {
                    MatchTweenUtil.DoRocketTargetMove(this.GameObject, Data.GridPos, Data.Direction,
                        () =>
                        {
                            State = ElementState.CanRecycle;
                            // MemoryPool.Release(this);
                        });
                }
                else
                {
                    DelayRelease().Forget();
                }

                if (context != null && context.IsCalculateCoinState)
                {
                    context.AddCalAddedCount((int)ElementIdConst.Coin, 1);
                    
                    //金币结算
                    G.EventModule.DispatchEvent(GameEventDefine.OnMatchAddResultCoin);
                }
            }

            return true;
        }

        public override void ResetSortingLayer(string layerName, int sortingOrder = 0)
        {
            foreach (var particle in this.GameObject.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (particle != null && particle.GetComponent<Renderer>() != null)
                {
                    particle.GetComponent<Renderer>().sortingLayerName = layerName;
                    particle.GetComponent<Renderer>().sortingOrder = sortingOrder;
                }
            }
            base.ResetSortingLayer(layerName, sortingOrder);
        }

        public void SetRocketIdleEffVisible(bool visible)
        {
            var effectIdle = this.GameObject.transform.Find("eff_idle")?.gameObject;
            if (effectIdle != null)
                effectIdle.SetVisible(visible);
        }

        private async UniTask DelayRelease()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(MatchConst.RocketMoveDuration));
            State = ElementState.CanRecycle;
        }
    }

    public static class RocketUtil
    {
        public static void UpdateRocketIdleEffectVisible()
        {
            var allElements = ElementSystem.Instance.GridElements;
            foreach (var elements in allElements.Values)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i] is RockElementItem rocketItem)
                    {
                        bool haveOver = false;
                        var coordEles = ElementSystem.Instance.GetGridElements(elements[i].Data.GridPos, false);
                        if (coordEles.Count > 1)
                        {
                            for (int j = coordEles.Count - 1; j >= 0; j--)
                            {
                                if (coordEles[j].Data.ConfigId == rocketItem.Data.ConfigId)
                                    continue;
                                if (coordEles[j].Data.ElementType == ElementType.TargetBlock)
                                {
                                    haveOver = true;
                                    break;
                                }
                            }
                        }

                        rocketItem.SetRocketIdleEffVisible(!haveOver);
                    }
                }
            }
        }
    }
}