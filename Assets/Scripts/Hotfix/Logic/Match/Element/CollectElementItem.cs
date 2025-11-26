using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using UnityEngine;

namespace HotfixLogic.Match
{
    /// <summary>
    /// 收集类型棋子
    /// </summary>
    public class CollectElementItem : BlockElementItem
    {
        private Tween _moveTween;
        
        public override bool CanMove()
        {
            return true;
        }

        public override void DoMove(float delayTime = 0, Ease ease = Ease.OutBounce)
        {
            _moveTween?.Kill();
            _moveTween = this.GameObject.transform.DOLocalMove(Vector3.zero, MatchConst.DropDuration)
                .SetEase(Ease.OutBounce, 1.7f, 1).SetAutoKill().SetDelay(delayTime);
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {           
            //如果当前不是在最后一行则不算掉落
            if (this.Data.GridPos.y != context.GridSystem.FindLastNotEmptyY(this.Data.GridPos.x))
            {
                return false;
            }

            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ElementMap config = db[Data.ConfigId];
            ElementAudioManager.Instance.Play(config.Id);

            _moveTween?.Kill();
            //收集元素掉落，播放动效等
            Sequence seq = DOTween.Sequence();
            Transform iconTra = this.GameObject.transform.Find("Icon");
            Vector3 localPos = iconTra.localPosition;
            var eff = this.GameObject.transform.Find("Match_eff_xyp");
            if (eff != null)
            {
                eff.SetVisible(true);
            }
            seq.Append(iconTra.DOLocalMove(new Vector3(localPos.x, localPos.y + 0.4f, localPos.z),0.3f));
            seq.Append(iconTra.DOLocalMove(new Vector3(localPos.x, localPos.y - 0.5f, localPos.z),0.5f));
            seq.SetAutoKill(true);
            seq.OnComplete(() =>
            {
                if (eff != null)
                {
                    eff.SetVisible(false);
                }
                iconTra.localPosition = Vector3.zero;
                // MemoryPool.Release(this);
                State = ElementState.CanRecycle;
                // 飞到目标那里
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchElementMoveToTarget,
                    EventTwoParam<int, Vector3>.Create(this.Data.ConfigId, GameObject.transform.position));
            });
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