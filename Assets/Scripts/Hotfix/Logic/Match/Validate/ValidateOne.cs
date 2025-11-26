using System;
using System.Collections.Generic;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Module;

namespace HotfixLogic.Match
{
    public class ValidateOne : IValidate
    {
        public void Validate(ElementDestroyContext context, List<GridItem> gridItems, Action<bool> callback)
        {
            if (gridItems == null || gridItems.Count == 0)
            {
                callback(false);
                return;
            }

            //校验特殊道具 单个可触发
            var elements = ElementSystem.Instance.GetGridElements(gridItems[0].Data.Coord, false);
            ElementBase element = null;
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                var ele = elements[i];
                //如果使用道具，按照实际的占格找出实际要消除的元素
                if (context.UsingItemId == (int)ItemDef.EliminateHammer)
                {
                    element = elements[^1];
                    context.AddWillDelCoord(element.Data.GridPos, EliminateStyle.Bomb, element.Data.UId,
                        element.Data.GridPos);
                    context.AddGridDelCoord(gridItems[0].Data.Coord);
                    context.AddEffGridCoord(element.Data.GridPos);
                    break;
                }

                if (ElementSystem.Instance.IsSpecialElement(ele.Data.ElementType))
                {
                    element = ele;
                    break;
                }
            }

            if (element == null)
            {
                callback(false);
                return;
            }

            if (!ElementSystem.Instance.IsSpecialElement(element.Data.ElementType) && context.UsingItemId < 0)
            {
                callback(false);
                return;
            }

            if (element.Data.ElementType == ElementType.Bomb)
            {
                ValidateManager.Instance.AttachBomb(context, gridItems[0].Data.Coord, element.Data.UId);
            }
            else if (element.Data.ElementType == ElementType.ColorBall)
            {
                //使用道具，就是指定某个颜色
                if (context.UsingItemId > 0)
                {
                    int pickElementId = element.Data.ColorBallDestroyId;
                    if (pickElementId <= 0)
                        pickElementId = ElementSystem.Instance.RandomPickBoardBaseElementId();
                    //漏掉统计当前位置的基础棋子
                    context.AddCalAddedCount(pickElementId, 1);
                    context.IsColorBallLineNormal = true;
                    context.LinkSpecialBallCoord = element.Data.GridPos;
                    ValidateManager.Instance.AttachColorBall(context, pickElementId,
                        gridItems[0].Data.Coord, element.Data.UId);
                    
                    AddSideEffectBlock(context);
                }
                else
                {
                    // 需求变更成单点彩球不会消除了，必须要连接基础棋子
                    callback(false);
                    return;
                }
            }
            else if (element.Data.ElementType == ElementType.Rocket ||
                     element.Data.ElementType == ElementType.RocketHorizontal)
            {
                ValidateManager.Instance.AttachRocket(context, gridItems[0].Data.Coord, element.Data.Direction,
                    element.Data.UId);
            }
            else if (element.Data.ElementType == ElementType.BombBlock)
            {
                ValidateManager.Instance.AttachBombBlock(context, gridItems[0].Data.Coord, element.Data.UId);
            }
            else
            {
                //使用锤子敲掉单个
                // context.AddWillDelCoord(element.Data.GridPos, element.Data.EliminateStyle, element.Data.UId);
                // context.AddEffGridCoord(element.Data.GridPos);
            }

            if (element.Data.ElementType != ElementType.ColorBall)
            {
                AddSideEffectBlock(context);
            }

            G.EventModule.DispatchEvent(GameEventDefine.OnDestroyTargetListElement,
                EventOneParam<ElementDestroyContext>.Create(context));
            callback?.Invoke(true);
        }

        private void AddSideEffectBlock(ElementDestroyContext context)
        {
            var effBlockCoords = ValidateManager.Instance.FindEffBlockCoordsOnlyWater(context.EffGridCoords);
            if (effBlockCoords.Count > 0)
            {
                foreach (var coord in effBlockCoords)
                {
                    var sideElements = ElementSystem.Instance.GetGridElements(coord, true);
                    if (sideElements is { Count: > 0 })
                    {
                        context.AddEffGridCoord(coord);
                        context.AddWillDelCoord(coord, EliminateStyle.Side, sideElements[^1].Data.UId);
                    }
                }
            }
        }
    }
}