using System.Collections.Generic;
using GameConfig;
using HotfixCore.Extensions;
using Spine.Unity;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class ColoredLightBlockElementItem : BlockElementItem
    {
        private int _currentId;
        private Dictionary<int, string> _colorIdToIdleMap;
        private Dictionary<int, string> _colorIdToHitMap;
        private SkeletonAnimation _spine;
        private GridColorLightBlock _lightBlockConfig;

        protected override void OnInitialized()
        {
            _colorIdToIdleMap = new Dictionary<int, string>(4)
            {
                [1] = "Idle_green",
                [2] = "Idle_blue",
                [3] = "Idle_yellow",
                [4] = "Idle_red"
            };

            _colorIdToHitMap = new Dictionary<int, string>(4)
            {
                [1] = "hit_green",
                [2] = "hit_blue",
                [3] = "hit_yellow",
                [4] = "hit_red"
            };

            _spine = this.GameObject.transform.Find("Icon")?.GetComponent<SkeletonAnimation>();
            if (_spine != null)
            {
                foreach (var slot in _spine.Skeleton.Slots)
                {
                    slot.A = 1;
                }
            }

            for (int i = 1; i <= 6; i++)
            {
                var eff = this.GameObject.transform.Find($"Match_eff_dp_{i}");
                if (eff != null)
                {
                    eff.SetVisible(false);
                }
            }

            _lightBlockConfig = this.GameObject.GetComponent<GridColorLightBlock>();

            InitSetColor();
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            if (Data.EliminateCount < 0)
                return false;
            bool needParseTarget = ElementSystem.Instance.IsNeedParseTarget(Data.ConfigId, out int parseTargetId);
            bool hasSuccessDel = false;

            var delInfo = context.WillDelCoords.Find(x => x.Coord == Data.GridPos);
            if (delInfo.DelStyle.Contains(EliminateStyle.Bomb))
            {
                //含有功能棋子消除，根据次数随机消除多个
                if (needParseTarget)
                {
                    int count = Mathf.Min(delInfo.AttachCount, Data.EliminateCount);
                    context.AddCalAddedCount(parseTargetId, count);
                }

                hasSuccessDel = true;
            }
            else
            {
                if (context.CurrentEffectId.Contains(_currentId))
                {
                    if (needParseTarget)
                    {
                        context.AddCalAddedCount(parseTargetId, 1);
                    }

                    hasSuccessDel = true;
                }
            }

            if (hasSuccessDel)
            {
                bool bResult = base.OnDestroy(context);
                UpdateLightVisible();
                return bResult;
            }

            return false;
        }

        private void UpdateLightVisible()
        {
            int delColorId = _currentId;
            var hitAnimation = GetSpineHitAnimation(_currentId);
            if (hitAnimation != null)
            {
                _spine.AnimationState.SetAnimation(0, hitAnimation, false).Complete+= (entry) =>
                {
                    SkeletonAnimation spine = this.GameObject.transform.Find("Icon")?.GetComponent<SkeletonAnimation>();
                    if (spine != null)
                    {
                        ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
                        ref readonly ElementMap config = ref db[Data.ConfigId];
                        int oriCount = config.eliminateCount;
                        int reduce = oriCount - Data.EliminateCount;
                        foreach (var slot in spine.Skeleton.Slots)
                        {
                            if (slot.Data.Name.StartsWith("light"))
                            {
                                int.TryParse(slot.Data.Name.Substring(5), out int index);
                                if (index == 0)
                                    index = 1;
                                float oriA = slot.A;
                                slot.A = index <= reduce ? 0 : 1;
                                if (!Mathf.Approximately(oriA, slot.A) && slot.A <= 0)
                                {
                                    SetLightEffectColors(index,delColorId);
                                    
                                    ElementAudioManager.Instance.Play("caidengpo");
                                }
                            }
                        }
                    }
                };
            }
        }

        private void InitSetColor()
        {
            //初始化时，从初始化棋子的颜色中获取
            int colorId = ElementSystem.Instance.LightBlockInitId;
            UpdateSetColor(colorId);
        }

        public void UpdateColor(int colorId)
        {
            if (this.GameObject == null || this.GameObject.transform == null)
                return;
            if (Data.EliminateCount <= 0)
                return;
            
            UpdateSetColor(colorId);
        }

        private void UpdateSetColor(int colorId)
        {
            _currentId = colorId;
            ElementSystem.Instance.LightBlockInitId = colorId;
            var animation = GetSpineIdleAnimation(colorId);
            if (animation != null)
            {
                _spine.AnimationState.SetAnimation(0, animation, false);
            }
        }

        private Spine.Animation GetSpineIdleAnimation(int colorId)
        {
            if (_spine != null)
            {
                if (_colorIdToIdleMap.TryGetValue(colorId, out string idleName))
                    return _spine.Skeleton.Data.FindAnimation(idleName);
            }

            return null;
        }

        private Spine.Animation GetSpineHitAnimation(int colorId)
        {
            if (_spine != null)
            {
                if (_colorIdToHitMap.TryGetValue(colorId, out string hitName))
                    return _spine.Skeleton.Data.FindAnimation(hitName);
            }

            return null;
        }

        private void SetLightEffectColors(int index,int colorId)
        {
            if(_lightBlockConfig == null)
                return;
            var effectRoot = this.GameObject.transform.Find($"Match_eff_dp_{index}");
            ColorLightReferenceParticleColor config = _lightBlockConfig.GetConfigColor(colorId);
            if (config != null && effectRoot != null)
            {
                var fang = effectRoot.Find("eff/fang");
                if (fang != null)
                {
                    var mainModule = fang.GetComponent<ParticleSystem>().main;
                    mainModule.startColor = config._fang;
                }
                var glow_a = effectRoot.Find("eff/glow_a");
                if (glow_a != null)
                {
                    var mainModule = glow_a.GetComponent<ParticleSystem>().main;
                    mainModule.startColor = config.glow_a;
                }
                var glow_g = effectRoot.Find("eff/glow_g");
                if (glow_g != null)
                {
                    var mainModule = glow_g.GetComponent<ParticleSystem>().main;
                    mainModule.startColor = config.glow_g;
                }
                var shikuai_b = effectRoot.Find("eff/shikuai_b");
                if (shikuai_b != null)
                {
                    var mainModule = shikuai_b.GetComponent<ParticleSystem>().main;
                    mainModule.startColor = config.shikuai_b;
                }
                var shikuai_s = effectRoot.Find("eff/shikuai_s");
                if (shikuai_s != null)
                {
                    var mainModule = shikuai_s.GetComponent<ParticleSystem>().main;
                    mainModule.startColor = config.shikuai_s;
                }
                var shikuai = effectRoot.Find("eff/shikuai");
                if (shikuai != null)
                {
                    var mainModule = shikuai.GetComponent<ParticleSystem>().main;
                    mainModule.startColor = config.shikuai;
                }
                
                effectRoot.SetVisible(true);
                var rootPar = effectRoot.Find("eff")?.GetComponent<ParticleSystem>();
                if (rootPar != null)
                {
                    rootPar.Play(true);
                }
            }
        }
    }
}