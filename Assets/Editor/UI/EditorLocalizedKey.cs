using System.Collections.Generic;
using System.Linq;
using GameCore.Localization;
using HotfixCore.Module;
using Sirenix.OdinInspector.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace GameEditor.UI
{
    [CustomEditor(typeof(LocalizedKey))]
    public class EditorLocalizedKey : UnityEditor.Editor
    {
        private const string CUSTOM_LOCALIZED_KEY_TEXT = "Assets/ArtLoad/Localization/CustomLocalization_CN.bytes";
        private Dictionary<string, string> _keyValueDic = new Dictionary<string, string>();

        private LocalizedKey _localizedKey;
        private SerializedProperty _localizeData;
        private SerializedProperty _languageKey;
        private SerializedProperty _fontAsset;

        private void OnEnable()
        {
            _localizedKey = target as LocalizedKey;
            InitLocalizedFont();
            // ReloadLocalizeText();

            _localizeData = serializedObject.FindProperty("LocalizedData");
            _languageKey = serializedObject.FindProperty("LanguageKey");
            _fontAsset = _localizeData.FindPropertyRelative("FontAsset");
        }

        /// <summary>
        /// 获取字体
        /// </summary>
        private void InitLocalizedFont()
        {
            var localizedData = _localizedKey.LocalizedData;
            if (localizedData == null)
            {
                _localizedKey.LocalizedData = new LocalizedData();
                localizedData = _localizedKey.LocalizedData;
            }

            if (localizedData.FontAsset == null)
            {
                localizedData.FontAsset = _localizedKey.GetComponent<TextMeshProUGUI>().font;
            }
        }

        private void ReloadLocalizeText()
        {
            TextAsset ta = AssetDatabase.LoadAssetAtPath<TextAsset>(CUSTOM_LOCALIZED_KEY_TEXT);
            if (ta != null)
            {
                _keyValueDic.Clear();
                string contents = ta.text;
                string[] lines = contents.Split('\n');
                foreach (var line in lines)
                {
                    var keyValue = line.Split('=');
                    if (keyValue.Length == 2)
                    {
                        _keyValueDic.Add(keyValue[0], keyValue[1]);
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (_languageKey == null)
                return;
            if (string.IsNullOrEmpty(_languageKey.stringValue))
            {
                _languageKey.stringValue = string.Empty;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("多语言Key:", GUILayout.Width(100), GUILayout.Height(20));

            _languageKey.stringValue = EditorGUILayout.TextField(_localizedKey.LanguageKey, GUILayout.Width(200), GUILayout.Height(20));
            if (GUILayout.Button("选择", GUILayout.Width(40), GUILayout.Height(20)))
            {
                ReloadLocalizeText();
                LanguageSelector selector = new LanguageSelector(_keyValueDic);
                selector.SelectionCancelled += () =>
                {
                    string key = ((KeyValuePair<string, string>)selector.SelectionTree.Selection.SelectedValue).Key;
                    string value = ((KeyValuePair<string, string>)selector.SelectionTree.Selection.SelectedValue).Value;
                    if (!string.IsNullOrEmpty(key))
                    {
                        _localizedKey.LanguageKey = key;
                        _localizedKey.gameObject.GetComponent<TextMeshProUGUI>().text = value;
                        EditorUtility.SetDirty(_localizedKey);
                    }
                };
                selector.ShowInPopup();
            }
            EditorGUILayout.EndHorizontal();
            DrawLocalizedElement();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLocalizedElement()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("字体选择:", GUILayout.Width(100), GUILayout.Height(20));
            EditorGUILayout.PropertyField(_fontAsset, GUIContent.none, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();
        }
    }

    public class LanguageSelector : OdinSelector<IEnumerator<KeyValuePair<string, string>>>
    {
        class MenuItem : OdinMenuItem
        {
            bool bDrawing;

            public override string SmartName => bDrawing ? null : base.SmartName;

            public string StringValue => ((KeyValuePair<string, string>)Value).Value;

            public MenuItem(OdinMenuTree tree, KeyValuePair<string, string> value) : base(tree, value.ToString(), value)
            {
            }

            public override void DrawMenuItem(int indentLevel)
            {
                bDrawing = true;

                var style = this.Style;
                var height = style.Height;
                style.Height = 40;
                base.DrawMenuItem(indentLevel);
                GUI.Label(Rect, Name, style.DefaultLabelStyle);
                style.Height = height;

                bDrawing = false;
            }
        }

        private IEnumerable<KeyValuePair<string, string>> collection;

        public LanguageSelector(Dictionary<string, string> values)
        {
            collection = values;
        }

        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Config.DrawSearchToolbar = true;
            tree.Selection.SupportsMultiSelect = false;
            tree.DefaultMenuStyle = OdinMenuStyle.TreeViewStyle;

            foreach (var v in collection)
            {
                tree.AddMenuItemAtPath("", new MenuItem(tree, v));
            }
        }


        protected override float DefaultWindowWidth()
        {
            return 500f;
        }
    }
}