using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using Hotfix.Define;
using Hotfix.Logic.Match;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using TMPro;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class FixedGridTargetBlockElementItem : TargetBlockElementItem
    {
        private float MoveDuration = 0.5f;

        protected override void SetElementComponent(GridHoldInfo info)
        {
            SpriteRenderer icon = this.GameObject.transform.Find("Icon").GetComponent<SpriteRenderer>();
            if (info.TargetElementId != 4)
            {
                G.ResourceModule.LoadAssetAsync<Sprite>($"match/sprites/diamond-cone_{info.TargetElementId}", sp =>
                {
                    icon.sprite = sp;
                }).Forget();
            }
            _targetNumText = this.GameObject.transform.Find("Icon/num").GetComponent<TextMeshPro>();
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
                UpdateNum(remainCount);
            }

            if (_targetNum <= 0)
            {
                WaitPlayEffect(context);
                return true;
            }

            return false;
        }

        private void WaitPlayEffect(ElementDestroyContext context)
        {
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            context.AddWaitElementDestroyTask(tcs.Task);
            
            MatchEffectManager.Instance.PlayObjectEffect(this.Data.ConfigId, null, this.GameObject.transform);
            
            var realDir = GetElementDirection();
            int last = 10;
            Vector3 startPos = Vector3.zero;
            if (realDir == ElementDirection.Up)
                startPos = GridSystem.GetGridPositionByCoord(Data.GridPos.x, last + 2);
            else if (realDir == ElementDirection.Down)
                startPos = GridSystem.GetGridPositionByCoord(Data.GridPos.x, -2);
            else if (realDir == ElementDirection.Left)
                startPos = GridSystem.GetGridPositionByCoord(last + 2, Data.GridPos.y);
            else if (realDir == ElementDirection.Right)
                startPos = GridSystem.GetGridPositionByCoord(-2, Data.GridPos.y);
            
            this.GameObject.transform.position = startPos;
            Vector3 endPos = Vector3.zero;
            if (realDir == ElementDirection.Up)
                endPos = GridSystem.GetGridPositionByCoord(Data.GridPos.x, -5);
            else if (realDir == ElementDirection.Down)
                endPos = GridSystem.GetGridPositionByCoord(Data.GridPos.x, last + 5);
            else if (realDir == ElementDirection.Left)
                endPos = GridSystem.GetGridPositionByCoord(-3, Data.GridPos.y);
            else if (realDir == ElementDirection.Right)
                endPos = GridSystem.GetGridPositionByCoord(last + 3, Data.GridPos.y);
            this.GameObject.transform.DOMove(endPos, MoveDuration).SetAutoKill(true);

            List<Vector2Int> delList = MatchTweenUtil.GetRocketDelPos(Data.GridPos, realDir);
            List<Vector2Int> realDelPos = new List<Vector2Int>(delList.Count);
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            for (int i = 0; i < delList.Count; i++)
            {
                var elements = ElementSystem.Instance.GetGridElements(delList[i], false);
                bool canAttach = ElementSystem.Instance.IsCanAttachElements(elements);
                if (canAttach)
                {
                    context.AddGridDelCoord(delList[i]);
                    //先计算出总的消除次数
                    int attachCount = 0;
                    for (int j = 0; j < elements.Count; j++)
                    {
                        if (db[elements[j].Data.ConfigId].eliminateCount == -1 && elements[j].Data.EliminateCount > 0)
                        {
                            attachCount += elements[j].Data.EliminateCount;
                        }
                        else
                            attachCount += db.CalculateTotalEliminateCount(elements[j].Data.ConfigId);
                    }

                    foreach (var element in elements)
                    {
                        if (!realDelPos.Contains(element.Data.GridPos))
                        {
                            realDelPos.Add(element.Data.GridPos);
                        }

                        if (ElementSystem.Instance.IsSpecialElement(element.Data.ElementType))
                        {
                            ValidateManager.Instance.CollectAutoElement(context, element);
                        }
                        else
                        {
                            context.AddWillDelCoord(element.Data.GridPos, EliminateStyle.Bomb, element.Data.UId,
                                Data.GridPos, attachCount: attachCount);
                            context.AddWillDelCoord(element.Data.GridPos, element.Data.EliminateStyle, element.Data.UId,
                                attachCount: attachCount);
                            if (element.Data.ElementType == ElementType.Normal)
                            {
                                ValidateManager.Instance.AddEffectBaseElementScore(element.Data.ConfigId);
                            }
                        }
                    }
                }
            }

            if (realDelPos.Count > 0)
            {
                List<DeleteGridInfo> delGridInfos = new List<DeleteGridInfo>(context.WillDelCoords);
                var seq = DOTween.Sequence();
                float startDelay = 0.0f;
                for (int i = 0; i < realDelPos.Count; i++)
                {
                    var idx = i;
                    float delayTime = startDelay + (i * MatchConst.DelElementInterval);
                    seq.InsertCallback(delayTime, () =>
                    {
                        var pos = realDelPos[idx];
                        context.GridSystem.DoElement(pos, context, delGridInfos, true).Forget();
                    });
                }

                ElementAudioManager.Instance.Play("zuanshizhuiyidong");
                seq.SetAutoKill().OnComplete(async () =>
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(MoveDuration));
                    // MemoryPool.Release(this);
                    State = ElementState.CanRecycle;
                    tcs.TrySetResult();
                });
            }
            else
            {
                State = ElementState.CanRecycle;
                tcs.TrySetResult();
                // MemoryPool.Release(this);
            }
        }

        private ElementDirection GetElementDirection()
        {
            Transform colorIcon = this.GameObject.transform.Find("Icon");
            if (colorIcon != null)
            {
                int angle = (int)colorIcon.transform.localEulerAngles.z;
                if (angle == 180)
                    return ElementDirection.Down;
                if (angle == 270)
                    return ElementDirection.Right;
                if (angle == 90)
                    return ElementDirection.Left;
            }

            return ElementDirection.Up;
        }
    }
}