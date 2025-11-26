using GameConfig;
using Hotfix.Define;
using HotfixCore.Extensions;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class BackgroundElementItem : BlockElementItem
    {
        protected override void OnInitialized()
        {
            HideAllBorder();
            base.OnInitialized();
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            bool bResult = base.OnDestroy(context);
            if (bResult && this.Data.ConfigId == (int)ElementIdConst.SpreadGrass)
            {
                HideAllBorder();
            }
            return bResult;
        }

        private void HideAllBorder()
        {
            var border = this.GameObject.transform.Find("Border");
            if (border != null)
            {
                foreach (Transform child in border)
                {
                    child.SetVisible(false);
                }
            }
        }
    }

    public static class SpreadGrassUtil
    {
        public static void UpdateGrassBorder()
        {
            var allElements = ElementSystem.Instance.GridElements;
            foreach (var elements in allElements.Values)
            {
                foreach (var element in elements)
                {
                    if (element.Data.ConfigId == (int)ElementIdConst.SpreadGrass)
                    {
                        //上
                        Vector2Int upBorder = element.Data.GridPos + new Vector2Int(0, -1);
                        Transform upObj = element.GameObject.transform.Find("Border/Up");
                        if (upObj != null)
                        {
                            upObj.SetVisible(!IsGridHaveGrass(upBorder));
                        }
                        //下
                        Vector2Int downBorder = element.Data.GridPos + new Vector2Int(0, 1);
                        Transform downObj = element.GameObject.transform.Find("Border/Down");
                        if (downObj != null)
                        {
                            downObj.SetVisible(!IsGridHaveGrass(downBorder));
                        }
                        //左
                        Vector2Int leftBorder = element.Data.GridPos + new Vector2Int(-1, 0);
                        Transform leftObj = element.GameObject.transform.Find("Border/Left");
                        if (leftObj != null)
                        {
                            leftObj.SetVisible(!IsGridHaveGrass(leftBorder));
                        }
                        //右
                        Vector2Int rightBorder = element.Data.GridPos + new Vector2Int(1, 0);
                        Transform rightObj = element.GameObject.transform.Find("Border/Right");
                        if (rightObj != null)
                        {
                            rightObj.SetVisible(!IsGridHaveGrass(rightBorder));
                        }
                        break;
                    }
                }
            }
        }

        private static bool IsGridHaveGrass(Vector2Int coord)
        {
            var elements = ElementSystem.Instance.GetGridElements(coord, true);
            if (elements is { Count: > 0 })
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Data.ConfigId == (int)ElementIdConst.SpreadGrass)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}