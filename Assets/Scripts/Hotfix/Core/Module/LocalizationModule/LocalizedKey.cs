using System;
using TMPro;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.Module
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedKey : MonoBehaviour
    {
        public string LanguageKey;
        public LocalizedData LocalizedData;
        private TextMeshProUGUI _text;

        private LocalizationModule _localizationModule;

        private void Awake()
        {
            _localizationModule = G.LocalizationModule;
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            ChangeLanguage();
        }

        private void OnEnable()
        {
            _localizationModule.AddLocalizeKey(this);
        }

        private void OnDisable()
        {
            _localizationModule.RemoveLocalizeKey(this);
        }

        public void ChangeLanguage()
        {
            if (string.IsNullOrEmpty(LanguageKey))
            {
                Logger.Warning($"Language content is null {LanguageKey}");
                return;
            }

            string content = _localizationModule.GetLanguageTextValue(LanguageKey);
            _text.text = content;
            _text.font = LocalizedData.FontAsset;
        }
    }
}