using System.Collections.Generic;
using DG.Tweening;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    /// <summary>
    /// 通用阻挡类型棋子
    /// </summary>
    public class BlockElementItem : ElementBase
    {
        private Sprite _boxNormalSprite;
        private Sprite _boxFinishSprite;

        protected override void OnInitialized() {
            base.OnInitialized();


            if (Data.ConfigId == 130) {
                if (_boxNormalSprite == null) {
                    var renderer = GameObject.transform.Find("Icon").GetComponent<SpriteRenderer>();
                    InitBoxFinishSprite();
                } 

                LoadBoxSprite(false);
            }
        }

        public override void DoSelect()
        {
        }

        public override void DoDeselect()
        {
        }

        public override void DoMove(float delayTime = 0, Ease ease = Ease.OutBounce)
        {
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            bool bResult = CheckAndDoWillDestroy(context);
            return bResult;
        }

        protected virtual bool CheckAndDoWillDestroy(ElementDestroyContext context,bool isRelease = true)
        {
            if (Data.EliminateCount < 0)
                return false;
            if (Data.EliminateCount == 0)
            {
                //本身就是自动收集的，不用再扣除次数 需要飞到目标处
                if (Data.ElementType == ElementType.ColorBlockPlus)
                    return true;
                AutoCollectPlay();
                return true;
            }

            int infoIndex = context.WillDelCoords.FindIndex(x => x.Coord == Data.GridPos);
            if(infoIndex < 0)
                return false;
            var info = context.WillDelCoords[infoIndex];
            if (info.AttachCount <= 0)
            {
                return false;
            }
            List<int> attachIds = new List<int>(10);
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            if (info.AttachCount > 1)
            {
                int nextId = 0;
                db.FindAfterReduceNextId(Data.ConfigId, info.AttachCount, true, ref nextId, ref attachIds);

                if (nextId != 0 && nextId == Data.ConfigId)
                {
                    //可能是循环自动收集的元素
                    if (db.IsCircleElement(Data.ConfigId))
                    {
                        nextId = Data.NextBlockId;
                    }
                }

                //消除后随机生成其它的元素
                if (attachIds.Count > 0)
                {
                    for (int i = 0; i < attachIds.Count; i++)
                    {
                        if (ElementSystem.Instance.IsNeedPendingDelElement(db[attachIds[i]].elementType))
                        {
                            nextId = attachIds[i]; //固定好该元素，后续要自动散出其它元素
                            context.AddPendingDelCoords(Data.GridPos);
                            break;
                        }
                        //彩色元素需要逐次消除，要不然多次碰撞时会被忽略掉
                        if (db.IsMultipleColorBlock(attachIds[i]))
                        {
                            nextId = attachIds[i];
                        }

                        if (attachIds[i] == nextId)
                            continue;

                        context.AddCalAddedCount(attachIds[i], 1);
                        bool needParseTarget = ElementSystem.Instance.IsNeedParseTarget(attachIds[i], out int parseTargetId);
                        if (needParseTarget)
                        {
                            context.AddCalAddedCount(parseTargetId, 1);
                        }
                    }

                    //添加元素分数
                    for (int i = 0; i < attachIds.Count; i++)
                    {
                        if (attachIds[i] == Data.ConfigId || attachIds[i] == nextId)
                        {
                            continue;
                        }

                        AddBlockScore(attachIds[i]);
                    }
                }

                Logger.Debug($"{Data.GridPos} 会被消除 :{info.AttachCount}次，元素：{Data.ConfigId}，会变成：{nextId}");
                Data.NextBlockId = nextId;
            }

            Data.EliminateCount -= info.AttachCount;
            int realReduceCount = Mathf.Min(info.AttachCount, db.CalculateTotalEliminateCount(Data.ConfigId));
            info.ReduceAttachCount(realReduceCount);
            context.WillDelCoords[infoIndex] = info;
            bool bResult = Data.EliminateCount <= 0;
            if (isRelease && bResult)
                PlayEffect();
            return bResult;
        }

        public override bool CanMove()
        {
            return false;
        }

        protected override void SetIdleSpine(Transform spineTran, string spineName, bool loop = true,bool isEmpty = false)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            if (db[Data.ConfigId].nameFlag == "xiong")
                loop = false;
            base.SetIdleSpine(spineTran, spineName, loop, isEmpty);
        }

        private void AutoCollectPlay()
        {
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchElementMoveToTarget,
                EventTwoParam<int, Vector3>.Create(this.Data.ConfigId, GameObject.transform.position));
            // MemoryPool.Release(this);
            State = ElementState.CanRecycle;
        }

        protected override void PlayEffect(bool hideIcon = true)
        {
            AddBlockScore(Data.ConfigId);
            base.PlayEffect(hideIcon);
        }

        protected void AddBlockScore(int elementId)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[elementId];
            int baseScore = config.score;
            MatchManager.Instance.AddScore(baseScore);
        }

        public void LoadBoxSprite(bool isFinish) {
            Sprite sprite = isFinish? _boxFinishSprite : _boxNormalSprite;
            var renderer = GameObject.transform.Find("Icon").GetComponent<SpriteRenderer>();

            if (sprite == null || renderer.sprite == sprite) {
                return;
            }

            renderer.sprite = sprite;
        }

        private void InitBoxFinishSprite() {
            var path = "match/sprites/item_block_130_1";
            G.ResourceModule.LoadAssetAsync<Sprite>(path, (sprite) => {
                if (sprite == null) {
                    return;
                }
                _boxFinishSprite = sprite;
            }).Forget();
        }
    }
}