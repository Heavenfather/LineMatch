#if UNITY_WEBGL && DOUYINMINIGAME && !UNITY_EDITOR
using TMPro;
using TTSDK;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class TTInputFieldTmpAdapter : MonoBehaviour
{
    private TMP_InputField inputField;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    private void OnEnable()
    {
        var com = this.gameObject.GetComponent<TTClickableInputField>();
        if (com == null)
        {
            this.gameObject.AddComponent<TTClickableInputField>();
        }

        RegisterKeyboardEvents();
    }

    private void OnDestroy()
    {
        UnregisterKeyboardEvents();
    }

    private void RegisterKeyboardEvents()
    {
        TT.OnKeyboardInput += OnTTKeyboardInput;
        TT.OnKeyboardConfirm += OnTTKeyboardConfirm;
    }

    private void UnregisterKeyboardEvents()
    {
        TT.OnKeyboardInput -= OnTTKeyboardInput;
        TT.OnKeyboardConfirm -= OnTTKeyboardConfirm;
    }

    private void OnTTKeyboardConfirm(string value)
    {
        
    }

    private void OnTTKeyboardInput(string value)
    {
        if (inputField != null && inputField.isFocused)
        {
            inputField.text = value;
        }
    }
}
#endif