using System;
using UnityEngine;

namespace GameCore.Localization
{
    /// <summary>
    /// 资源包内的多语言配置
    /// </summary>
    [CreateAssetMenu(fileName = "NativeLocalization", menuName = "Game/Language Config")]
    public class NativeLocalization : ScriptableObject
    {
        [Serializable]
        public class LanguageEntry
        {
            public string Key;

            [TextArea(3, 5)] public string Value;
        }

        public LanguageEntry[] languageEntries;
    }
}