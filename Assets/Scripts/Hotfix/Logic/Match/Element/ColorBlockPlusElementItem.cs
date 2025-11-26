using HotfixCore.Extensions;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class ColorBlockPlusElementItem : ColorBlockElementItem
    {
        protected override void OnInitialized()
        {
            foreach (Transform child in GameObject.transform)
            {
                if (child.name.StartsWith("item_"))
                {
                    child.GetComponent<SpriteRenderer>().enabled = true;
                    if (child.childCount > 0)
                    {
                        child.GetChild(0).SetVisible(false);
                    }
                }
            }
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            if (context.BombPanelElementCoord != null && context.BombPanelElementCoord.Contains(Data.GridPos))
            {
                context.BombPanelElementCoord.Remove(Data.GridPos);
                PlayEffect();
                return true;
            }

            bool hasSuccessDel = DoColorElementDestroy(context);
            if (hasSuccessDel)
            {
                ElementAudioManager.Instance.Play("LeGao");
                bool bResult = CheckAndDoWillDestroy(context, false);
                if (bResult)
                {
                    context.AddBombPanelElement(Data.GridPos);
                }
            }
            
            return false;
        }
        
        protected override void DelElement(int index, int effId)
        {
            Data.EffElementIds.RemoveAt(index);
            var targetGo = GetColorElementObject(effId);
            if (targetGo != null)
            {
                targetGo.GetComponent<SpriteRenderer>().enabled = false;
                if (targetGo.transform.childCount > 0)
                {
                    targetGo.transform.GetChild(0).SetVisible(true);
                }
                // targetGo.SetVisible(false);
            }
        }

        private GameObject GetColorElementObject(int effId)
        {
            foreach (Transform child in GameObject.transform)
            {
                if (child.gameObject.name == $"item_{effId}")
                {
                    if(child.GetComponent<SpriteRenderer>().enabled)
                        return child.gameObject;
                }
            }

            return null;
        }
    }
}