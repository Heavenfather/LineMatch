using System.Collections.Generic;
using GameCore.Localization;
using Hotfix.Define;
using Hotfix.EventParameter;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.Module
{
    public class LocalizationModule : IModuleAwake, IModuleDestroy
    {
        private HashSet<LocalizedKey> _localizedKeys;

        private LanguageType _language;

        /// <summary>
        /// 当前语言
        /// </summary>
        public LanguageType Language => _language;

        public void Awake(object parameter)
        {
            _localizedKeys = new HashSet<LocalizedKey>();

            G.EventModule.AddEventListener<EventLocalizationChanged>(GameEventDefine.OnGameLanguageChanged, OnChangeLanguage, this);
        }

        public void Destroy()
        {
            G.EventModule.RemoveEventListener<EventLocalizationChanged>(GameEventDefine.OnGameLanguageChanged, OnChangeLanguage, this);
        }

        /// <summary>
        /// 获取多语言文本
        /// </summary>
        /// <param name="key">多语言文本id</param>
        /// <returns></returns>
        public string GetLanguageTextValue(string key)
        {
            return LocalizationPool.Get(key);
        }

        /// <summary>
        /// 注册多语言key
        /// </summary>
        public void AddLocalizeKey(LocalizedKey key)
        {
            _localizedKeys.Add(key);
        }

        /// <summary>
        /// 移除多语言key
        /// </summary>
        public void RemoveLocalizeKey(LocalizedKey key)
        {
            _localizedKeys.Remove(key);
        }

        private void OnChangeLanguage(EventLocalizationChanged args)
        {
            if (_localizedKeys == null || _localizedKeys.Count == 0)
                return;

            _language = args.Language;

            foreach (var key in _localizedKeys)
            {
                key.ChangeLanguage();
            }

            Logger.Info($"Changed language to {_language.ToString()}");
        }
    }
}