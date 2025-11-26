#if UNITY_WEBGL && DOUYINMINIGAME && !UNITY_EDITOR
using TMPro;
using TTSDK;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TTClickableInputField : EventTrigger
{
    public string confirmType = "done"; // 可选值有: "done", "next", "search", "go", "send"
    public int maxInputLength = 100; // 最大输入长度
    public bool multiple = false; // 是否多行输入
    private InputField _inputField;
    private TMP_InputField _tmpInputField;
    
    private void Start()
    {
        _inputField = GetComponent<InputField>();
        _tmpInputField = GetComponent<TMP_InputField>();
    }
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (_inputField != null)
        {
            if (_inputField.isFocused)
            {
                TT.ShowKeyboard(new()
                {
                    maxLength = maxInputLength,
                    multiple = multiple,
                    defaultValue = _inputField.text,
                    confirmType = confirmType
                });
            }
        }else if (_tmpInputField != null)
        {
            if (_tmpInputField.isFocused)
            {
                TT.ShowKeyboard(new()
                {
                    maxLength = maxInputLength,
                    multiple = multiple,
                    defaultValue = _inputField.text,
                    confirmType = confirmType
                });
            }
        }
    }
}
#endif