using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameEditor.UI
{
    public class UISettings : ScriptableObject
    {
        [SerializeField]
        private string _codeAuthor = "DefaultAuthor";
        /// <summary>
        /// 代码作者
        /// </summary>
        public string CodeAuthor => _codeAuthor;

        [SerializeField]
        private string _nameSpace = "HotfixLogic";

        /// <summary>
        /// 代码命名空间
        /// </summary>
        public string NameSpace => _nameSpace;

        [SerializeField]
        private string _codePath = "Assets/Scripts/Hotfix/Logic";

        /// <summary>
        /// 代码路径
        /// </summary>
        public string CodePath => _codePath;

        [SerializeField]
        private List<UIElementGenerateKv> _uiElementKv = new List<UIElementGenerateKv>();

        public UIElementGenerateKv FindUIElementGenerateKv(string name)
        {
            return _uiElementKv.Find(x => name.StartsWith(x.NameRegex));
        }
    }

    /// <summary>
    /// 绑定的UI元素Key Value对应值
    /// </summary>
    [Serializable]
    public class UIElementGenerateKv
    {
        public string NameRegex;
        public string ComponentName;
    }
}