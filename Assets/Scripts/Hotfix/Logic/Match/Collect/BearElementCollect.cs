using System;
using DG.Tweening;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class BearElementCollect : MatchCollectBase
    {
        public override void DoIconEffect(int elementId,Vector3 endValue, int index, Action callback)
        {
            var iconTarget = this.transform.Find("Icon");
            iconTarget.localScale = Vector3.one;
            endValue = new Vector3(endValue.x, endValue.y, 0);

            var beginPos = new Vector3(this.transform.position.x, this.transform.position.y, 0);

            var xMinus = (endValue.x - beginPos.x) * 0.5f;

            var ease = Ease.Linear;

            Vector3 offsetDir = new Vector3(xMinus, 0, 0);

            var pathPos1 = beginPos + offsetDir;
            Vector3[] path = { pathPos1, endValue };

            Sequence targetTwn = GetTweenSequence();
            iconTarget.localScale = Vector3.one * 0.5f;
            targetTwn.Append(this.transform.DOLocalMoveY(this.transform.localPosition.y + 1, 0.35f)
                .SetEase(Ease.OutBounce));
            targetTwn.Join(iconTarget.DOScale(Vector3.one * 1.0f, 0.3f));

            targetTwn.AppendInterval(0.1f);

            float flyDur = GetFlyDuration();
            targetTwn.AppendInterval(index * 0.05f);
            targetTwn.Append(this.transform.DOPath(path, flyDur, PathType.CatmullRom).SetEase(ease));
            targetTwn.Join(iconTarget.DOScale(Vector3.one * 1.2f, flyDur / 1.5f));
            targetTwn.AppendInterval(flyDur / 2);
            targetTwn.Join(iconTarget.DOScale(Vector3.one * 0.7f, flyDur / 1.5f));


            targetTwn.SetAutoKill(true).OnComplete(() => { callback?.Invoke(); });
            //熊猫拖尾音效
            ElementAudioManager.Instance.Play("XiongMaoTuoWei");
        }
    }
}