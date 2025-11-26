using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using HotfixCore.Module;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class AdjustCollectElementItem : BlockElementItem
    {
        private bool _lockState = false;
        
        protected override void OnInitialized()
        {
            Transform closeObject = this.GameObject.transform.Find("Close");
            if (closeObject != null)
                SetLockState(false);
            
            Transform openObject = this.GameObject.transform.Find("Open");
            if (openObject != null)
                SetLockState(true);
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            if (IsCanDestroy())
            {
                bool bResult = base.OnDestroy(context);
                if (bResult)
                {
                    Transform openObject = this.GameObject.transform.Find("Open");
                    if (openObject != null)
                        openObject.SetVisible(false);
                    if (!string.IsNullOrEmpty(Data.Extra) && int.TryParse(Data.Extra, out int targetId))
                    {
                        context.AddCalAddedCount(targetId, 1);
                        // 飞到目标那里
                        G.EventModule.DispatchEvent(GameEventDefine.OnMatchElementMoveToTarget,
                            EventTwoParam<int, Vector3>.Create(targetId, GameObject.transform.position));
                    }
                }
                return bResult;
            }

            return false;
        }

        public void UpdateElementState()
        {
            if(State != ElementState.Using)
                return;
            SetLockState(!_lockState,true);
        }
        
        private bool IsCanDestroy()
        {
            Transform closeObject = this.GameObject.transform.Find("Close");
            if (closeObject != null)
                return !closeObject.gameObject.activeSelf;
            Transform openObject = this.GameObject.transform.Find("Open");
            if (openObject != null)
                return openObject.gameObject.activeSelf;

            return false;
        }

        private void SetLockState(bool isLock,bool playAudio = false)
        {
            _lockState = isLock;
            Transform closeObject = this.GameObject.transform.Find("Close");
            Transform openObject = this.GameObject.transform.Find("Open");

            if (closeObject != null)
            {
                var icon = this.GameObject.transform.Find("Icon");
                if (icon != null)
                {
                    icon.SetVisible(!isLock);
                }
                closeObject.SetVisible(isLock);
            }
            else if (openObject != null)
            {
                var icon = this.GameObject.transform.Find("Icon");
                if (icon != null)
                {
                    icon.SetVisible(isLock);
                }
                openObject.SetVisible(!isLock);
            }

            if (playAudio)
                ElementAudioManager.Instance.Play(isLock ? "zuanshiheguan" : "zuanshihekai");
        }
    }
}