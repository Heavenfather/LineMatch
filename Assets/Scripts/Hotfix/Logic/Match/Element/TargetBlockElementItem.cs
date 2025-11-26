using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using Hotfix.Define;
using HotfixCore.Extensions;
using HotfixCore.MemoryPool;
using TMPro;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class TargetBlockElementItem : BlockElementItem
    {
        protected int _targetId;
        protected int _targetNum;
        protected TextMeshPro _targetNumText;
        private Sequence _sequence;

        // 用于做收集动画统计
        private int _collectCount;

        protected override void OnInitialized()
        {
            SetElementInfo();
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            if (context.CalAddedDelTargets == null)
                return false;
            if (context.CalAddedDelTargets.TryGetValue(_targetId, out var count))
            {
                //在这里减掉目标数量会有风险，当配置目标里面也有这个Target的配置时，就会无法正确统计到
                if (_targetNum >= count)
                {
                    context.CalAddedDelTargets[_targetId] = 0;
                }
                else
                {
                    context.CalAddedDelTargets[_targetId] = count - _targetNum;
                }

                int remainCount = Mathf.Max(0, _targetNum - count);
                UpdateNum(remainCount,true);
            }

            if (_targetNum <= 0)
            {
                PlayCloseEffect(context);
                return true;
            }

            return false;
        }

        public int GetTargetId()
        {
            return _targetId;
        }

        public int GetRemainCount()
        {
            return _targetNum;
        }

        public Vector3 GetTextNumPosition()
        {
            if (_targetNumText != null)
                return _targetNumText.transform.position;
            return Vector3.zero;
        }

        private void PlayCloseEffect(ElementDestroyContext context)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[Data.ConfigId];
            if (config.nameFlag == "chuanglian")
            {
                UniTaskCompletionSource cts = new UniTaskCompletionSource();
                context.AddWaitElementDestroyTask(cts.Task);
                //卷帘收起
                _sequence = DOTween.Sequence();
                SpriteRenderer icon = this.GameObject.transform.Find("Icon").GetComponent<SpriteRenderer>();
                float size = GridSystem.GridSize.x;
                _sequence.Append(DOTween.To(x => icon.size = new Vector2(icon.size.x, x), icon.size.y, size * 2,
                    MatchConst.ChuanglianCloseDuration));
                SpriteRenderer lianzi = this.GameObject.transform.Find("lianzi").GetComponent<SpriteRenderer>();
                _sequence.Join(DOTween
                    .To(x => lianzi.size = new Vector2(lianzi.size.x, x), lianzi.size.y, size * 2,
                        MatchConst.ChuanglianCloseDuration / 2));
                Transform targetRoot = lianzi.transform.Find("target");
                _sequence.Join(targetRoot.DOLocalMove(new Vector3(0, -1.0f * size, 0), MatchConst.ChuanglianCloseDuration / 2));
                _sequence.SetAutoKill().OnComplete(() =>
                {
                    if (lianzi != null)
                    {
                        lianzi.SetVisible(false);
                    }

                    var effTag = this.GameObject.transform.Find("eff_tag");
                    MatchEffectManager.Instance.PlayObjectEffect(MatchEffectType.JuanLianBroke,
                        effTag.transform.position);
                    State = ElementState.CanRecycle;
                    // MemoryPool.Release(this);
                    cts.TrySetResult();
                }).SetDelay(0.25f);
                
                ElementAudioManager.Instance.Play(config.Id);
            }
            else
            {
                State = ElementState.CanRecycle;
                // MemoryPool.Release(this);
            }
        }

        private void SetElementInfo()
        {
            var gridInfos = ElementSystem.Instance.FindCoordHoldGridInfo(Data.GridPos.x, Data.GridPos.y);
            if (gridInfos is { Count: > 0 })
            {
                int indexOf = gridInfos.FindIndex(x => x.ElementId == Data.ConfigId);
                if (indexOf >= 0)
                {
                    var info = gridInfos[indexOf];
                    _targetId = info.TargetElementId;
                    SetElementComponent(info);
                    UpdateNum(info.TargetElementNum);
                }
            }
        }

        protected virtual void SetElementComponent(GridHoldInfo info)
        {
            SpriteRenderer icon = this.GameObject.transform.Find("Icon").GetComponent<SpriteRenderer>();
            float elementSize = GridSystem.GridSize.x;
            var lianzi = this.GameObject.transform.Find("lianzi").GetComponent<SpriteRenderer>();
            icon.transform.localPosition = new Vector3(elementSize / 2.0f * -1, elementSize / 2.0f, 0);
            icon.size = new Vector2(elementSize * info.ElementWidth, elementSize * info.ElementHeight);
            Transform targetRoot = lianzi.transform.Find("target");
            SpriteRenderer targetSp = targetRoot.GetComponent<SpriteRenderer>();
            targetSp.color = ElementSystem.Instance.GetElementColor(info.TargetElementId);
            float lianziHeight = elementSize;
            float targetOffsetY = 0;
            if (info.ElementHeight > 3)
            {
                lianziHeight = elementSize * info.ElementHeight / 2.0f + elementSize;
                targetOffsetY = elementSize * info.ElementHeight / 2.0f;
            }
            else
            {
                if (info.ElementHeight == 3)
                {
                    lianziHeight = elementSize * info.ElementHeight - elementSize / 2;
                    targetOffsetY = elementSize + elementSize / 2;
                }
                else
                {
                    lianziHeight = elementSize * info.ElementHeight;
                    if (info.ElementHeight == 2)
                        targetOffsetY = elementSize;
                    else
                        targetOffsetY = elementSize / 2;
                }
            }
            lianzi.size = new Vector2(elementSize * 2,lianziHeight);
            lianzi.transform.localPosition =
                new Vector3(elementSize * (info.ElementWidth / 2.0f) - elementSize / 2.0f, 0.4f, 0);
            targetRoot.transform.localPosition = new Vector3(0, -1.0f * targetOffsetY + 0.01f, 0);
            _targetNumText = targetRoot.GetComponentInChildren<TextMeshPro>();
            var effRoot = this.GameObject.transform.Find("eff_tag");
            if (effRoot != null)
            {
                effRoot.transform.localPosition = new Vector3(lianzi.transform.localPosition.x - elementSize / 2,
                    lianzi.transform.localPosition.y, 0);
            }
        }

        protected void UpdateNum(int num,bool playEff = false)
        {
            if (_targetNumText != null)
            {
                if (playEff)
                {
                    ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
                    ref readonly ElementMap config = ref db[Data.ConfigId];
                    if (config.nameFlag == "chuanglian")
                    {
                        var eff = this.GameObject.transform.Find("lianzi/target/Match_eff_chuanglian_shuzi");
                        if (eff != null)
                        {
                            eff.SetVisible(false);
                            eff.SetVisible(true);
                        }
                    }
                }

                _targetNumText.text = $"{num}";
                _targetNum = num;
                _collectCount = num;
            }
        }

        public void AddCollectCount(int count = -1)
        {
            _collectCount += count;
        }

        public int GetCollectCount()
        {
            return _collectCount;
        }

        public override void Clear()
        {
            base.Clear();
            if (_sequence != null)
            {
                _sequence.Kill();
                _sequence = null;
            }

        }
    }
}