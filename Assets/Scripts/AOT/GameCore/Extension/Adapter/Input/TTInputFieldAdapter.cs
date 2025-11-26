#if UNITY_WEBGL && DOUYINMINIGAME && !UNITY_EDITOR
using TTSDK;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class TTInputFieldAdapter : MonoBehaviour
{
    private InputField inputField;

    private void Awake()
    {
        inputField = GetComponent<InputField>();
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