using System.Collections.Generic;
using DG.Tweening;
using GameConfig;
using HotfixCore.Extensions;
using HotfixCore.MemoryPool;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class VerticalExpandElementItem : BlockElementItem
    {
        private int _remainNum;

        protected override void OnInitialized()
        {
            ResizeIcon();
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            var info = context.WillDelCoords.Find(x => x.Coord == Data.GridPos);
            int remain = _remainNum;
            for (int i = 0; i < info.AttachCount; i++)
            {
                remain -= 1;
                if (remain <= 0)
                    break;
                RemoveHoldInfoPos(remain, context);
            }

            int count = _remainNum - info.AttachCount;
            SetIconSize(count,true);
            if (_remainNum <= 0)
            {
                bool needParseTarget = ElementSystem.Instance.IsNeedParseTarget(Data.ConfigId, out int parseTargetId);
                if (needParseTarget)
                {
                    context.AddCalAddedCount(parseTargetId, 1);
                }
                State = ElementState.CanRecycle;
                // MemoryPool.Release(this);
                return true;
            }

            return false;
        }

        private void RemoveHoldInfoPos(int count, ElementDestroyContext context)
        {
            var gridInfos = ElementSystem.Instance.FindCoordHoldGridInfo(Data.GridPos.x, Data.GridPos.y);
            if (gridInfos is { Count: > 0 })
            {
                int indexOf = gridInfos.FindIndex(x => x.ElementId == Data.ConfigId);
                if (indexOf >= 0)
                {
                    var info = gridInfos[indexOf];
                    Vector2Int delCoord = FindTopPos(count);
                    var sideCoords = FindSideCoords(delCoord);
                    info.RemoveHoldGridPos(delCoord);
                    ValidateManager.Instance.RemoveSideCoords(Data.ConfigId, Data.GridPos, sideCoords);
                    ElementSystem.Instance.RemoveHoldGridElementCoord(delCoord, Data);
                    context.AddExtraDelCoords(delCoord);
                    gridInfos[indexOf] = info;
                }
            }
        }

        private Vector2Int FindTopPos(int num)
        {
            if (Data.Direction == ElementDirection.Up)
                return new Vector2Int(Data.GridPos.x, Data.GridPos.y - num);
            if (Data.Direction == ElementDirection.Down)
                return new Vector2Int(Data.GridPos.x, Data.GridPos.y + num);
            if (Data.Direction == ElementDirection.Left)
                return new Vector2Int(Data.GridPos.x - num, Data.GridPos.y);
            return new Vector2Int(Data.GridPos.x + num, Data.GridPos.y);
        }

        private HashSet<Vector2Int> FindSideCoords(Vector2Int delPos)
        {
            HashSet<Vector2Int> sideCoords = new HashSet<Vector2Int>();
            if (Data.Direction == ElementDirection.Up)
            {
                //上 左 右坐标
                sideCoords.Add(new Vector2Int(delPos.x, delPos.y - 1));
                sideCoords.Add(new Vector2Int(delPos.x - 1, delPos.y));
                sideCoords.Add(new Vector2Int(delPos.x + 1, delPos.y));
            }

            if (Data.Direction == ElementDirection.Down)
            {
                //下 左 右坐标
                sideCoords.Add(new Vector2Int(delPos.x, delPos.y + 1));
                sideCoords.Add(new Vector2Int(delPos.x - 1, delPos.y));
                sideCoords.Add(new Vector2Int(delPos.x + 1, delPos.y));
            }

            if (Data.Direction == ElementDirection.Left)
            {
                //左 上 下坐标
                sideCoords.Add(new Vector2Int(delPos.x - 1, delPos.y));
                sideCoords.Add(new Vector2Int(delPos.x, delPos.y - 1));
                sideCoords.Add(new Vector2Int(delPos.x, delPos.y + 1));
            }

            if (Data.Direction == ElementDirection.Right)
            {
                //右 上 下坐标
                sideCoords.Add(new Vector2Int(delPos.x + 1, delPos.y));
                sideCoords.Add(new Vector2Int(delPos.x, delPos.y - 1));
                sideCoords.Add(new Vector2Int(delPos.x, delPos.y + 1));
            }

            return sideCoords;
        }

        private void ResizeIcon()
        {
            var gridInfos = ElementSystem.Instance.FindCoordHoldGridInfo(Data.GridPos.x, Data.GridPos.y);
            if (gridInfos is { Count: > 0 })
            {
                int indexOf = gridInfos.FindIndex(x => x.ElementId == Data.ConfigId);
                if (indexOf >= 0)
                {
                    var info = gridInfos[indexOf];
                    var root = GetIconRoot();
                    float elementSize = GridSystem.GridSize.x;
                    float offset = elementSize / 2.0f;
                    if (Data.Direction == ElementDirection.Up)
                        root.localPosition = new Vector3(0, offset * -1.0f, 0);
                    else if (Data.Direction == ElementDirection.Down)
                        root.localPosition = new Vector3(0, offset, 0);
                    else if (Data.Direction == ElementDirection.Left)
                        root.localPosition = new Vector3(offset, 0, 0);
                    else if (Data.Direction == ElementDirection.Right)
                        root.localPosition = new Vector3(offset * -1.0f, 0, 0);
                    int total = info.ElementHeight;
                    if (Data.Direction == ElementDirection.Left || Data.Direction == ElementDirection.Right)
                        total = info.ElementWidth;
                    SetIconSize(total,false);
                }
            }
        }

        private void SetIconSize(int num,bool tweenMove)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[Data.ConfigId];
            if (config.nameFlag == "xrz")
                SetXRZIconSize(num,tweenMove);
            else if (config.nameFlag == "yanzhu")
                SetYZIconSize(num,tweenMove);

            _remainNum = num;
            Data.EliminateCount = num;
        }

        private void SetXRZIconSize(int num,bool tweenMove)
        {
            var shenti = this.GameObject.transform.Find("root/shenti");
            var yan = shenti.Find("yan_root");
            var effect = this.GameObject.transform.Find("Match_eff_xrz");
            if (shenti != null)
            {
                var icon = shenti.GetComponent<SpriteRenderer>();
                float elementSize = GridSystem.GridSize.x;
                if (tweenMove)
                {
                    effect.position = yan.position;
                    effect.SetVisible(false);
                    effect.SetVisible(true);
                    const float dur = 0.5f;
                    DOTween.To(() => icon.size, vec=>
                    {
                        icon.size = vec;
                    }, new Vector2(elementSize * 1.0f, elementSize * num), dur).SetAutoKill().SetEase(Ease.OutBounce);
                    yan.transform.DOLocalMove(new Vector3(0, elementSize * (num - 1), 0), dur).SetAutoKill().SetEase(Ease.OutBounce);
                    
                    //仙人掌音效
                    ElementAudioManager.Instance.Play("XianRenZhang");
                }
                else
                {
                    icon.size = new Vector2(elementSize * 1.0f, elementSize * num);
                    yan.transform.localPosition = new Vector3(0, elementSize * (num - 1), 0);
                    effect.position = yan.position;
                    effect.SetVisible(false);
                }
            }
        }

        private void SetYZIconSize(int num,bool tweenMove)
        {
            var icon = this.GameObject.transform.Find("Icon");
            if (icon != null)
            {
                var iconRoot = icon.GetComponent<SpriteRenderer>();
                float elementSize = GridSystem.GridSize.x;
                var effect = this.GameObject.transform.Find("Match_eff_yanzhu");
                if (effect != null)
                {
                    float offset = elementSize * num;
                    if (Data.Direction == ElementDirection.Up)
                        effect.localPosition = new Vector3(0, offset, 0);
                    else if(Data.Direction == ElementDirection.Down)
                        effect.localPosition = new Vector3(0, -offset, 0);
                    else if(Data.Direction == ElementDirection.Left)
                        effect.localPosition = new Vector3(-offset, 0, 0);
                    else if(Data.Direction == ElementDirection.Right)
                        effect.localPosition = new Vector3(offset, 0, 0);
                }

                if (tweenMove)
                {
                    if (effect != null)
                    {
                        effect.SetVisible(false);
                        effect.SetVisible(true);
                    }

                    const float dur = 0.5f;
                    DOTween.To(() => iconRoot.size, vec=>
                    {
                        iconRoot.size = vec;
                    }, new Vector2(elementSize * 1.0f, elementSize * num), dur).SetAutoKill().SetEase(Ease.OutBounce);
                    ElementAudioManager.Instance.Play("yanzhu");
                }
                else
                {
                    iconRoot.size = new Vector2(elementSize * 1.0f, elementSize * num);
                    if (effect != null)
                        effect.SetVisible(false);
                }
            }
        }

        private Transform GetIconRoot()
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[Data.ConfigId];
            if (config.nameFlag == "xrz")
                return this.GameObject.transform.Find("root");
            if (config.nameFlag == "yanzhu")
                return this.GameObject.transform.Find("Icon");
            return null;
        }
    }
}