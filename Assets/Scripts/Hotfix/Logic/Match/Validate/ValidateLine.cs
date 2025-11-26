using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Module;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class ValidateLine : IValidate
    {
        private BlockDiffScoreDB _db => ConfigMemoryPool.Get<BlockDiffScoreDB>();
        private int _matchLineRectangleCount => ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("MatchLineCCount");
        private MatchGameType _matchGameType => MatchManager.Instance.CurrentMatchGameType;
        
        public void Validate(ElementDestroyContext context, List<GridItem> gridItems, Action<bool> callback)
        {
            int total = 0;
            int matchElementId = gridItems[0].GetSelectElement(true).Data.ConfigId;
            if (_matchGameType == MatchGameType.TowDots && gridItems.Count >= _matchLineRectangleCount)
            {
                var allKeys = ElementSystem.Instance.GridElements.Keys;
                foreach (var coord in allKeys)
                {
                    var elements = ElementSystem.Instance.GetGridElements(coord, false);
                    bool result = ElementSystem.Instance.TryGetBaseElement(elements, out var index);
                    if (result && index >= 0 && elements[index].Data.ConfigId == matchElementId)
                    {
                        total++;
                        context.AddEffGridCoord(coord);
                        context.AddWillDelCoord(coord, EliminateStyle.Match, elements[index].Data.UId);
                    }
                }
            }
            else
            {
                //添加直接影响的元素
                for (int i = 0; i < gridItems.Count; i++)
                {
                    context.AddEffGridCoord(gridItems[i].Data.Coord);
                    context.AddWillDelCoord(gridItems[i].Data.Coord, EliminateStyle.Match,
                        gridItems[i].Data.GetTopElement().UId);
                }
                total = gridItems.Count;
            }

            //检测功能棋子的连线
            bool isSpecial = CheckSpecialElement(context, gridItems);
            HashSet<Vector2Int> effBlockCoords = null;
            if (!isSpecial)
            {
                //不是功能棋子连线，在这里直接算分
                int score = _db.CalScoreNotRect(matchElementId, total);
                MatchManager.Instance.AddScore(score);
                effBlockCoords = ValidateManager.Instance.FindEffBlockCoords(context.EffGridCoords,context);
            }
            else
            {
                effBlockCoords = ValidateManager.Instance.FindEffBlockCoordsOnlyWater(context.EffGridCoords);
            }
            
            if (effBlockCoords is {Count: > 0})
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

            G.EventModule.DispatchEvent(GameEventDefine.OnDestroyTargetListElement,
                EventOneParam<ElementDestroyContext>.Create(context));
            callback?.Invoke(true);
        }

        private bool CheckSpecialElement(ElementDestroyContext context, List<GridItem> gridItems)
        {
            bool isAllSpecialElement = true;
            int baseElementId = -1;
            ColorBallElementItem colorBallElement = null;
            foreach (var gridItem in gridItems)
            {
                var element = gridItem.GetSelectElement(true);
                if(element == null)
                    continue;
                if (element.Data.ElementType == ElementType.Normal)
                {
                    isAllSpecialElement = false;
                    baseElementId = element.Data.ConfigId;
                    context.AddCurrentEffectId(element.Data.ConfigId);
                }

                if (element.Data.ElementType == ElementType.ColorBall)
                {
                    colorBallElement = element as ColorBallElementItem;
                }
            }
            if (!isAllSpecialElement && colorBallElement == null)
            {
                return false;
            }

            if (colorBallElement != null && !isAllSpecialElement)
            {
                //彩色球连着基础棋子
                ValidateManager.Instance.AttachColorBall(context, baseElementId, colorBallElement.Data.GridPos,colorBallElement.Data.UId);
                context.LinkSpecialBallCoord = colorBallElement.Data.GridPos;
                colorBallElement.Data.ColorBallDestroyId = baseElementId;
                context.IsColorBallLineNormal = true;
                return true;
            }

            //按照配置id排序 最终消除效果只取级别最高的两个 级别：彩球>炸弹>火箭
            if (gridItems.Count >= 2)
            {
                gridItems.Sort((a, b) =>
                {
                    var aElement = a.GetSelectElement(true);
                    var bElement = b.GetSelectElement(true);

                    if (aElement.Data.Priority > bElement.Data.Priority)
                        return -1;
                    if (aElement.Data.Priority < bElement.Data.Priority)
                        return 1;
                    return 0;
                });
                foreach (var item in gridItems)
                {
                    var ele = item.GetSelectElement(true);
                    context.AddCurrentEffectId(ele.Data.ConfigId);
                }
            }

            var firstElement = gridItems[0].GetSelectElement(true);
            var secondElement = gridItems[1].GetSelectElement(true);
            ValidateManager.Instance.InvokeComposeElement(context, firstElement, secondElement);
            return true;
        }

    }
}