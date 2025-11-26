using System.Collections.Generic;
using GameConfig;
using Hotfix.Define;
using HotfixCore.Extensions;
using Spine.Unity;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class ColorBlockElementItem : BlockElementItem
    {
        private Dictionary<int, string> _hitNamMap = new Dictionary<int, string>()
        {
            [1] = "hit3",
            [2] = "hit2",
            [3] = "hit5",
            [4] = "hit4"
        };
        
        protected override void OnInitialized()
        {
            foreach (Transform child in GameObject.transform)
            {
                if (child.name.StartsWith("item_"))
                {
                    child.SetVisible(true);
                }

                if (child.name.StartsWith("Match_eff_ylp"))
                {
                    child.SetVisible(false);
                }
            }
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            if (Data.EffElementIds == null || Data.EffElementIds.Count <= 0)
            {
                return base.OnDestroy(context);
            }

            bool hasSuccessDel = DoColorElementDestroy(context);
            
            if (hasSuccessDel)
            {
                bool bResult = CheckAndDoWillDestroy(context, false);
                if (bResult)
                {
                    PlayEffect();
                }
                return bResult;
            }
            return false;
        }

        protected bool DoColorElementDestroy(ElementDestroyContext context)
        {
            bool hasSuccessDel = false;
            List<int> randomIds = new List<int>(Data.EffElementIds);
            var delInfo = context.WillDelCoords.Find(x => x.Coord == Data.GridPos);
            bool needParseTarget = ElementSystem.Instance.IsNeedParseTarget(Data.ConfigId, out int parseTargetId);
            if (delInfo.DelStyle.Contains(EliminateStyle.Bomb))
            {
                //含有功能棋子消除，根据次数随机消除多个
                for (int i = 0; i < delInfo.AttachCount; i++)
                {
                    if(randomIds.Count <= 0)
                        break;
                    int randomIndex = Random.Range(0, randomIds.Count);
                    DelElement(randomIndex, randomIds[randomIndex]);
                    if (needParseTarget)
                    {
                        context.AddCalAddedCount(parseTargetId, 1);
                    }
                    randomIds.RemoveAt(randomIndex);
                    hasSuccessDel = true;
                }
            }
            else
            {
                foreach (var effId in context.CurrentEffectId)
                {
                    int index = Data.EffElementIds.FindIndex(x => x == effId);
                    if (index >= 0)
                    {
                        if (needParseTarget)
                        {
                            context.AddCalAddedCount(parseTargetId, 1);
                        }
                        DelElement(index, Data.EffElementIds[index]);
                        hasSuccessDel = true;
                        break;
                    }
                }
            }
            return hasSuccessDel;
        }

        protected virtual void DelElement(int index, int effId)
        {
            Data.EffElementIds.RemoveAt(index);
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[Data.ConfigId];
            if (config.nameFlag == "ylp_05")
            {
                foreach (var kp in _hitNamMap)
                {
                    var obj = FindColorElementObject(kp.Key);
                    if(obj == null)
                        continue;
                    var effObj = FindColorElementEffObject(kp.Key);
                    if (kp.Key == effId)
                    {
                        obj.SetVisible(false);
                        //饮料瓶音效
                        ElementAudioManager.Instance.Play("YinLiaoPing");
                        if (effObj != null)
                            effObj.SetVisible(true);
                    }
                    else
                    {
                        SkeletonAnimation spine = obj.GetComponent<SkeletonAnimation>();
                        if (spine != null)
                        {
                            spine.AnimationState.SetAnimation(0, kp.Value, false);
                        }
                        PlayHitEffect(null);
                    }
                }

            }
            else if(config.nameFlag == "binggan")
            {
                var go = FindColorElementObject(effId);
                if (go != null)
                {
                    go.SetVisible(false);
                    ElementAudioManager.Instance.Play("BingGanPoSui");
                }

                var effect = FindColorElementEffObject(effId);
                if (effect != null)
                {
                    effect.SetVisible(true);
                }
            }

        }

        private GameObject FindColorElementEffObject(int effId)
        {
            foreach (Transform child in GameObject.transform)
            {
                if(child.gameObject.name == $"Match_eff_ylp_0{effId}" && child.gameObject.activeSelf == false)
                    return child.gameObject;
            }

            return null;
        }

        private GameObject FindColorElementObject(int id)
        {
            foreach (Transform child in GameObject.transform)
            {
                if(child.gameObject.name == $"item_{id}" && child.gameObject.activeSelf)
                    return child.gameObject;
            }

            return null;
        }
    }
}