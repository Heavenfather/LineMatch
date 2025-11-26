using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GameCore.Logic
{
    public class HotUpdateConfirmUI : MonoBehaviour
    {
        [SerializeField] private Button btn_confirm;
        [SerializeField] private Button btn_close;
        [SerializeField] private TextMeshProUGUI text_confirm;
        [SerializeField] private TextMeshProUGUI text_content;
        
        private UnityAction _confirmAction;
        private string _content;
        private string _confirmText;
        
        public void SetView(UnityAction confirmAction,string content, string confirmText,bool hideClose = true)
        {
            _confirmAction = confirmAction;
            _content = content;
            _confirmText = confirmText;
            
            if (_confirmAction != null)
            {
                btn_confirm.onClick.RemoveAllListeners();
                btn_confirm.onClick.AddListener(() =>
                {
                    _confirmAction?.Invoke();
                    Close();
                });
                if (!string.IsNullOrEmpty(_confirmText))
                {
                    text_confirm.text = _confirmText;
                }
            }
            else
            {
                btn_confirm.gameObject.SetActive(false);
            }
        
            if (text_content != null)
            {
                text_content.text = _content;
            }

            btn_close.gameObject.SetActive(!hideClose);
        }

        private void Close()
        {
            this.gameObject.SetActive(false);
        }
    }
}