using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.Logic
{
    public class HotUpdateVersion : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _version;
        [SerializeField] private Button _enter;

        private Action _clickCallback;
        
        private void Awake()
        {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
            if (_version != null)
            {
                _version.gameObject.AddComponent<WXInputFieldTmpAdapter>();
            }
#endif
            _enter.onClick.AddListener(OnVersionClick);
        }

        public void SetEnterCallback(Action callback)
        {
            _clickCallback = callback;
        }

        private void OnVersionClick()
        {
            if (!string.IsNullOrEmpty(_version.text))
            {
                HotUpdateManager.Instance.TargetVersion = _version.text;
            }
            this.gameObject.SetActive(false);
            if(_clickCallback != null)
                _clickCallback();
        }
    }
}