using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HotfixCore.Module
{
    public class VariableArray : MonoBehaviour
    {
#if UNITY_EDITOR
        //编辑器绑定组件
        [Serializable]
        public class BindData
        {
            public BindData()
            {
            }

            public BindData(string name, Component bindCom, bool isGameObject = false)
            {
                Name = name;
                BindCom = bindCom;
                IsGameObject = isGameObject;
            }

            public string Name;
            public Component BindCom;
            public bool IsGameObject;
        }

        [SerializeField] private string _codeSubPath;
        [SerializeField] private bool _isWindow;
        [SerializeField] private string _widgetName;

        public List<BindData> BindDatas = new List<BindData>();
        public string CodeSubPath => _codeSubPath;
        public bool IsWindow => _isWindow;
        public string WidgetName => _widgetName;
#endif

        [SerializeField] public List<Component> Components = new List<Component>();

        public T Get<T>(int index) where T : Component
        {
            if (index >= Components.Count)
            {
                Debug.LogError($"索引无效:{index}");
                return null;
            }

            T bindCom = Components[index] as T;

            if (bindCom == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{this.gameObject.name}] 节点 {BindDatas[index].Name} 类型无效:{typeof(T).Name}");
#else
                Debug.LogError($"[{this.gameObject.name}] 类型无效:{typeof(T).Name}");
#endif
                return null;
            }

            return bindCom;
        }

        private void OnEnable()
        {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
        var childs = this.GetComponentsInChildren<TMP_InputField>(true);
        foreach (var child in childs)
        {
            if (child.GetComponent<WXInputFieldTmpAdapter>() == null)
            {
                child.gameObject.AddComponent<WXInputFieldTmpAdapter>();
            }
        }

        foreach (var child in this.GetComponentsInChildren<InputField>(true))
        {
            if (child.GetComponent<WXInputFieldAdapter>() == null)
            {
                child.gameObject.AddComponent<WXInputFieldAdapter>();
            }
        }
#elif UNITY_WEBGL && DOUYINMINIGAME && !UNITY_EDITOR
        var childs = this.GetComponentsInChildren<TMP_InputField>();
        foreach (var child in childs)
        {
            if (child.GetComponent<TTInputFieldTmpAdapter>() == null)
            {
                child.gameObject.AddComponent<TTInputFieldTmpAdapter>();
            }
        }

        foreach (var child in this.GetComponentsInChildren<InputField>())
        {
            if (child.GetComponent<TTInputFieldAdapter>() == null)
            {
                child.gameObject.AddComponent<TTInputFieldAdapter>();
            }
        }
#endif
        }
    }
}