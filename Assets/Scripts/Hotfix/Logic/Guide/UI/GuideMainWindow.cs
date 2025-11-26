using GameConfig;
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;
using Logger = GameCore.Log.Logger;
using Hotfix.Define;

namespace HotfixLogic
{
    [Window(UILayer.UI, "uiprefab/guide/guidemainwindow")]
    public partial class GuideMainWindow : UIWindow
    {
        private GuideHoleController _holeController;
        private GuideConfig _guideData;

        public override void OnCreate()
        {
            _holeController = img_mask.GetComponent<GuideHoleController>();
            // btn_touch.gameObject.SetActive(false);
            btn_touch.AddClick(() => { 
                Logger.Debug("GuideMainWindow btn_touch clicked: " + GuideManager.Instance.CurrentGuideId);
                GuideManager.Instance.FinishCurrentGuide(); 
            });

            G.EventModule.DispatchEvent(GameEventDefine.OnGuideShowGuide);
        }

        public override void OnRefresh()
        {
            _guideData = (GuideConfig)UserDatas[0];
            GuideManager.Instance.SendLog(_guideData.id, 1);
            _holeController.SetUp(new HoleConfig(_guideData.id, _guideData.holeData, _guideData.nodePath,_guideData.endNodePath, _guideData.holeShape));

            SetContent(_guideData);
            if (_guideData.guideType == GuideType.Force)
            {
                SetFinger();
                SetForceGuideButtonEvent();
                //强引导由外部自行触发完成
                btn_touch.SetVisible(false);
            }
            else
            {
                HideFingerGuide();
                btn_touch.SetVisible(true);
            }
        }

        public void HideFingerGuide()
        {
            tf_finger.SetVisible(false);
        }

        public void HideLine()
        {
            img_mask.raycastTarget = false;
        }

        public void ActiveMaskRaycast()
        {
            img_mask.raycastTarget = true;
        }

        private void SetForceGuideButtonEvent()
        {
            GameObject target = GameObject.Find(_guideData.nodePath);
            if(target == null)
                return;
            var component = target.GetOrAddComponent<GuideInteractableComponent>();
            component.ResetButtonEvent(_guideData.nodePath);
        }

        private void SetContent(GuideConfig config)
        {
            text_content.text = config.content;
            if (config.stickYTarget && !string.IsNullOrEmpty(config.nodePath))
            {
                var target = GameObject.Find(config.nodePath);
                if (target != null)
                {
                    var targetRect = target.GetComponent<RectTransform>();
                    var contentRect = go_contentRoot.GetComponent<RectTransform>();
                    if (targetRect != null)
                    {
                        go_contentRoot.transform.position = targetRect.position;
                        contentRect.anchoredPosition = new Vector2(0, contentRect.anchoredPosition.y + config.contentPos.y);
                    }
                    else
                    {
                        Vector3 targetWorldPos = target.transform.position;
                        
                        // 将世界位置转换为屏幕位置
                        Camera mainCamera = G.UIModule.CurrentCamera;
                        Vector2 screenPos = mainCamera.WorldToScreenPoint(targetWorldPos);
                        
                        // 将屏幕位置转换为UI本地位置
                        Vector2 localPos;
                        Camera uiCamera = G.UIModule.UICamera;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            contentRect,
                            screenPos,
                            uiCamera,
                            out localPos
                        );
                        go_contentRoot.transform.localPosition = localPos;
                        contentRect.anchoredPosition = new Vector2(0, contentRect.anchoredPosition.y + config.contentPos.y);
                    }
                    return;
                }
            }

            go_contentRoot.GetComponent<RectTransform>().anchoredPosition = config.contentPos;

        }

        private void SetFinger()
        {
            if (_guideData.fingerType == GuideFingerType.Touch)
            {
                SetFingerTouch(_guideData.nodePath);
                HideLine();
                ActiveMaskRaycast();
            }
            else
            {
                tf_finger.SetVisible(false);
            }
        }

        public void RefreshHole(HoleConfig holeConfig)
        {
            _holeController.SetUp(holeConfig);
        }
        
        public void SetFingerTouch(string nodePath)
        {
            if(string.IsNullOrEmpty(nodePath))
                return;
            GameObject targetNode = GameObject.Find(nodePath);
            if (targetNode != null)
            {
                tf_finger.SetVisible(true);
                RectTransform targetRect = targetNode.GetComponent<RectTransform>();
                if (targetRect != null)
                {
                    tf_finger.position = targetRect.transform.position;
                }
                else
                {
                    Camera currentCamera = G.UIModule.CurrentCamera == null ? Camera.main : G.UIModule.CurrentCamera;
                    if (currentCamera != null)
                    {
                        Vector3 targetViewPoint = currentCamera.WorldToViewportPoint(targetNode.transform.position);
                        Vector3 targetScreenPos = G.UIModule.UICamera.ViewportToScreenPoint(targetViewPoint);
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            this.gameObject.GetComponentInParent<Canvas>().GetComponent<RectTransform>(), targetScreenPos,
                            G.UIModule.UICamera, out Vector2 localPos);
                        tf_finger.localPosition = localPos;
                    }
                }
            }
        }
    }
}