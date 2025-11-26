using System;
using DG.Tweening;
using GameConfig;
using HotfixCore.Extensions;
using HotfixCore.Module;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class NormalElementCollect : MatchCollectBase
    {
        public override void DoIconEffect(int elementId,Vector3 endValue, int index, Action callback)
        {
            var iconTarget = this.transform.Find("Icon");
            LoadSpriteObject(iconTarget, elementId, () =>
            {
                ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
                if (db[elementId].elementType == ElementType.Normal)
                {
                    iconTarget.localScale = Vector3.one;
                    endValue = new Vector3(endValue.x, endValue.y, 0);

                    var beginPos = new Vector3(this.transform.position.x, this.transform.position.y, 0);

                    var xMinus = (endValue.x - beginPos.x) * 0.5f;

                    if (xMinus > 1)
                    {
                        xMinus = 1;
                    }
                    else if (xMinus < -1)
                    {
                        xMinus = -1;
                    }

                    var pathPos1 = endValue - new Vector3(xMinus, 0, 0);
                    pathPos1.y -= 0.4f;

                    Vector3[] path = new Vector3[] { pathPos1, endValue };

                    Sequence targetTwn = GetTweenSequence();
                    targetTwn.AppendInterval(0.1f);

                    float flyDur = GetFlyDuration();
                    targetTwn.AppendInterval(index * 0.05f);
                    targetTwn.Append(this.transform.DOPath(path, flyDur, PathType.CatmullRom).SetEase(Ease.Linear));
                    targetTwn.SetAutoKill(true).OnComplete(() => { callback?.Invoke(); });
                }
                else
                {
                    base.DoIconEffect(elementId, endValue, index, callback);
                }
            });
        }

        private void LoadSpriteObject(Transform icon, int elementId,Action callback)
        {
            string spLocation = MatchManager.Instance.GetElementIconLocation(elementId);
            icon.SetVisible(false);
            G.ResourceModule.LoadAssetAsync<Sprite>(spLocation, (sprite) =>
            {
                var sp = icon.GetComponent<SpriteRenderer>();
                sp.color = ElementSystem.Instance.GetElementColor(elementId);
                sp.sprite = sprite;
                icon.SetVisible(true);
                
                callback?.Invoke();
            }).Forget();
        }
    }
}