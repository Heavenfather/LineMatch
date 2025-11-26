using System;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Module;
using UnityEngine;
using UnityEngine.UI;

namespace HotfixLogic
{
    /// <summary>
    /// 强引导交互，自动添加点击回调
    /// 
    /// </summary>
    public class GuideInteractableComponent : MonoBehaviour
    {
        private string _selfNodePath;
        private Button _interactableButton;
        
        private void Awake()
        {
            var selfBtn = this.GetComponent<Button>();
            if (selfBtn != null)
            {
                _interactableButton = selfBtn;
            }
            else
            {
                var childBtn = this.GetComponentsInChildren<Button>();
                if (childBtn != null && childBtn.Length > 0)
                {
                    for (int i = childBtn.Length - 1; i >= 0; i--)
                    {
                        if (childBtn[i].interactable)
                        {
                            _interactableButton = childBtn[i];
                            break;
                        }
                    }
                }
            }
        }

        public void ResetButtonEvent(string nodePath)
        {
            _selfNodePath = nodePath;
            
            if (_interactableButton != null)
            {
                _interactableButton.onClick.RemoveListener(OnGuideBtnClick);
                _interactableButton.onClick.AddListener(OnGuideBtnClick);
            }
        }

        private void OnGuideBtnClick()
        {
            if (!string.IsNullOrEmpty(_selfNodePath))
                G.EventModule.DispatchEvent(GameEventDefine.OnGuideForceButtonClick,
                    EventOneParam<string>.Create(_selfNodePath));
            RemoveButtonListen();
        }

        private void RemoveButtonListen()
        {
            if (_interactableButton != null)
            {
                _selfNodePath = "";
                _interactableButton.onClick.RemoveListener(OnGuideBtnClick);
            }
        }
    }
}