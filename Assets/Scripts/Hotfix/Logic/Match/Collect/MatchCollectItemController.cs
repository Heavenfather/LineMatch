using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixCore.Extensions;
using HotfixCore.Module;
using UnityEngine;

namespace HotfixLogic.Match
{
    public enum CollectItemEnum : int
    {
        Normal,
        Bear = 129,
        Butterfly = 190,
        Wish = 1001,
        Coin = 280,
    }

    public class MatchCollectItemController : MonoBehaviour
    {
        private int _flyTargetIndex;
        private int _coinIndex; //金币由于太多了，需要批量飞
        private Dictionary<CollectItemEnum, string> _collectItemNameMap;
        private Dictionary<CollectItemEnum, Queue<MatchCollectBase>> _collectItemListMap;

        private void Awake()
        {
            _collectItemListMap = new Dictionary<CollectItemEnum, Queue<MatchCollectBase>>();
            _collectItemNameMap = new Dictionary<CollectItemEnum, string>()
            {
                { CollectItemEnum.Normal, "NormalCollectItem" },
                { CollectItemEnum.Butterfly, "ButterflyCollectItem" },
                { CollectItemEnum.Wish, "WishCollectItem" },
                { CollectItemEnum.Bear, "BearCollectItem" },
                { CollectItemEnum.Coin, "CoinCollectItem"},
            };
        }

        public void ResetData()
        {
            _flyTargetIndex = 0;
            _coinIndex = 0;
        }

        public void ClearController()
        {
            _collectItemListMap?.Clear();
        }

        public void DoCollectItemFlyToTarget(int elementId, Vector3 startPosition, Vector3 targetPosition,
            Action callback, int index = -1)
        {
            if (index == -1)
                index = _flyTargetIndex;
            if (elementId == (int)ElementIdConst.Coin)
            {
                index = _coinIndex % 10;
                _coinIndex++;
            }

            _flyTargetIndex++;

            GetMatchCollectItem(elementId, (go) =>
            {
                if (!_collectItemListMap.ContainsKey((CollectItemEnum)elementId))
                {
                    Queue<MatchCollectBase> list = new Queue<MatchCollectBase>();
                    _collectItemListMap.Add((CollectItemEnum)elementId, list);
                }

                MatchCollectBase collectItem = go.GetComponent<MatchCollectBase>();
                collectItem.Initialize(startPosition);
                collectItem.SetVisible(true);
                collectItem.DoIconEffect(elementId, targetPosition, index, () =>
                {
                    collectItem.SetVisible(false);
                    _collectItemListMap[(CollectItemEnum)elementId].Enqueue(collectItem);
                    if (callback != null)
                    {
                        callback();
                    }
                    // G.EventModule.DispatchEvent(GameEventDefine.OnMatchCollectItemFlyComplete,EventOneParam<int>.Create(elementId));
                });
            }).Forget();
        }

        private async UniTask<GameObject> GetMatchCollectItem(int elementId, Action<GameObject> callback, bool isAwait = false)
        {
            CollectItemEnum itemEnum = CollectItemEnum.Normal;
            if (Enum.IsDefined(typeof(CollectItemEnum), elementId))
            {
                itemEnum = (CollectItemEnum)elementId;
            }
            if (_collectItemListMap.ContainsKey(itemEnum) && _collectItemListMap[itemEnum].Count > 0)
            {
                foreach (var collect in _collectItemListMap[itemEnum])
                {
                    if (collect.gameObject.activeSelf == false)
                    {
                        callback?.Invoke(collect.gameObject);
                        return null;
                    }
                }
            }

            string itemName = _collectItemNameMap[itemEnum];
            string location = $"{MatchConst.ElementAddressBase}/{itemName}".ToLower();
            if (isAwait) {
                return await G.ResourceModule.LoadGameObjectAsync(location, this.transform);
            } else {
                G.ResourceModule.LoadGameObjectAsync(location, (go) =>
                {
                    if (go == null) return;
                    callback?.Invoke(go);
                }, this.transform).Forget();
            }

            return null;
        }

        public async UniTask DoCollectResultCoinFlyTarget(Vector3 startPosition, Vector3 targetPosition, Action beginCB = null) {
            var coinList = new List<MatchCollectBase>();

            var random = new System.Random();
            var flyCoinCoint = random.Next(2, 5);
            var offsetDis = 100;

            for (int i = 0; i < flyCoinCoint; i++) {
                var count = i;

                var go = await GetMatchCollectItem((int)CollectItemEnum.Coin, null, true);
                if(go == null)
                    return;

                go.transform.localScale = Vector3.one * 1.3f;
                if (!_collectItemListMap.ContainsKey(CollectItemEnum.Coin))
                {
                    Queue<MatchCollectBase> list = new Queue<MatchCollectBase>();
                    _collectItemListMap.Add(CollectItemEnum.Coin, list);
                }

                MatchCollectBase collectItem = go.GetComponent<MatchCollectBase>();
                collectItem.Initialize(startPosition);
                collectItem.SetVisible(false);
                coinList.Add(collectItem);
            }

            var delayTime = 0.1f;
            var moveSpeed = 20;
            for (int i = 0; i < flyCoinCoint; i++) {
                var curIdx = i;
                var coinItem = coinList[curIdx];
                coinItem.gameObject.SetActive(true);
                var offsetPos = new Vector3(random.Next(-offsetDis, offsetDis) / 100f, random.Next(-offsetDis, offsetDis) / 100f, 0);
                offsetPos += new Vector3(0, -0.4f, 0);
                var bloomPos = startPosition + offsetPos;

                var seq = DOTween.Sequence();
                seq.AppendInterval(delayTime * curIdx);
                seq.Append(coinItem.transform.DOMove(bloomPos, 0.3f).SetEase(Ease.OutBack));
                seq.AppendInterval(delayTime * (flyCoinCoint - curIdx));
                seq.AppendInterval(0.07f * curIdx);
                seq.Append(coinItem.transform.DOMove(targetPosition, Vector3.Distance(startPosition, targetPosition) / moveSpeed).SetEase(Ease.InBack));
                seq.AppendCallback(() => {
                    AudioUtil.PlayGetCoin();

                    if (curIdx == 0) {
                        beginCB?.Invoke();
                    }
                    coinItem.transform.localScale = Vector3.one;
                    coinItem.gameObject.SetActive(false);
                });
            }
        }
    }
}