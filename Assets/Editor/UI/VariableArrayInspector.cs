using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameEditor.Common;
using Hotfix.Define;
using HotfixCore.Module;
using UnityEditor;
using UnityEngine;

namespace GameEditor.UI
{
    [CustomEditor(typeof(VariableArray))]
    public class VariableArrayInspector : UnityEditor.Editor
    {
        private VariableArray m_target;
        private UISettings m_settings;

        private SerializedProperty m_BindDatas;
        private SerializedProperty m_BindComs;
        private SerializedProperty m_codeSubPathProperty;
        private SerializedProperty m_isWindow;
        private SerializedProperty m_widgetName;
        private List<VariableArray.BindData> m_TempList = new List<VariableArray.BindData>();
        private List<string> m_TempFiledNames = new List<string>();
        private List<string> m_TempComponentTypeNames = new List<string>();
        private string m_codePath;
        private MVCEnum m_mvcEnum = MVCEnum.None;

        private void OnEnable()
        {
            m_target = (VariableArray)target;
            m_BindDatas = serializedObject.FindProperty("BindDatas");
            m_BindComs = serializedObject.FindProperty("Components");
            m_settings = EditorUtil.GetScriptableAsset<UISettings>("UISettings", true);

            m_isWindow = serializedObject.FindProperty("_isWindow");
            m_widgetName = serializedObject.FindProperty("_widgetName");
            m_codeSubPathProperty = serializedObject.FindProperty("_codeSubPath");
            m_codePath = $"{m_settings.CodePath}/{m_codeSubPathProperty.stringValue}";
            if(!Enum.TryParse(m_codeSubPathProperty.stringValue, true, out m_mvcEnum))
                m_mvcEnum = MVCEnum.None;

            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawTopButton();

            DrawSetting();

            DrawKvData();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 绘制顶部按钮
        /// </summary>
        private void DrawTopButton()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("排序"))
            {
                Sort();
            }

            if (GUILayout.Button("全部删除"))
            {
                RemoveAll();
            }

            if (GUILayout.Button("删除空引用"))
            {
                RemoveNull();
            }

            if (GUILayout.Button("自动绑定组件"))
            {
                AutoBindComponent();
            }

            if (GUILayout.Button("生成绑定代码"))
            {
                // if (string.IsNullOrEmpty(m_codeSubPathProperty.stringValue))
                if(m_mvcEnum == MVCEnum.None)
                {
                    Debug.LogError("模块名不能为空!");
                    return;
                }
                GenLogicCode();

                // 通用模块不需要MCV框架
                if (m_mvcEnum != MVCEnum.Common) {
                    GenModelCode();
                    GenControllerCode();
                }

                GenBindingCode();
                AssetDatabase.Refresh();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSetting()
        {
            EditorGUILayout.BeginHorizontal();
            m_mvcEnum = (MVCEnum)EditorGUILayout.EnumPopup("模块名", m_mvcEnum);
            m_codeSubPathProperty.stringValue = m_mvcEnum.ToString();
            
            bool isWidget = EditorGUILayout.Toggle("是否窗口", m_isWindow.boolValue);
            if (isWidget != m_isWindow.boolValue)
            {
                m_isWindow.boolValue = isWidget;
            }

            EditorGUILayout.EndHorizontal();
            if (!m_isWindow.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                m_widgetName.stringValue = EditorGUILayout.TextField(new GUIContent("WidgetName"), m_widgetName.stringValue);
                if (string.IsNullOrEmpty(m_widgetName.stringValue))
                    m_widgetName.stringValue = m_target.gameObject.name;
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 绘制键值对数据
        /// </summary>
        private void DrawKvData()
        {
            //绘制key value数据
            int needDeleteIndex = -1;

            EditorGUILayout.BeginVertical();
            SerializedProperty property;

            for (int i = 0; i < m_BindDatas.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"[{i}]", GUILayout.Width(25));
                property = m_BindDatas.GetArrayElementAtIndex(i).FindPropertyRelative("Name");
                property.stringValue = EditorGUILayout.TextField(property.stringValue, GUILayout.Width(150));
                property = m_BindDatas.GetArrayElementAtIndex(i).FindPropertyRelative("BindCom");
                property.objectReferenceValue =
                    EditorGUILayout.ObjectField(property.objectReferenceValue, typeof(Component), true);

                if (GUILayout.Button("X"))
                {
                    //将元素下标添加进删除list
                    needDeleteIndex = i;
                }

                EditorGUILayout.EndHorizontal();
            }

            //删除data
            if (needDeleteIndex != -1)
            {
                m_BindDatas.DeleteArrayElementAtIndex(needDeleteIndex);
                SyncBindComs();
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 排序
        /// </summary>
        private void Sort()
        {
            m_TempList.Clear();
            foreach (VariableArray.BindData data in m_target.BindDatas)
            {
                m_TempList.Add(new VariableArray.BindData(data.Name, data.BindCom, data.IsGameObject));
            }

            m_TempList.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

            m_BindDatas.ClearArray();
            foreach (VariableArray.BindData data in m_TempList)
            {
                AddBindData(data.Name, data.BindCom, data.IsGameObject);
            }

            SyncBindComs();
        }

        /// <summary>
        /// 全部删除
        /// </summary>
        private void RemoveAll()
        {
            if (EditorUtility.DisplayDialog("提示", "确定删除UI全部引用吗?", "确定"))
            {
                m_BindDatas.ClearArray();

                SyncBindComs();
            }
        }

        /// <summary>
        /// 删除空引用
        /// </summary>
        private void RemoveNull()
        {
            for (int i = m_BindDatas.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty element = m_BindDatas.GetArrayElementAtIndex(i).FindPropertyRelative("BindCom");
                if (element.objectReferenceValue == null)
                {
                    m_BindDatas.DeleteArrayElementAtIndex(i);
                }
            }

            SyncBindComs();
        }

        /// <summary>
        /// 自动绑定
        /// </summary>
        private void AutoBindComponent()
        {
            m_BindDatas.ClearArray();

            var transform = m_target.transform;
            Ergodic(transform, transform);

            SyncBindComs();
        }

        /// <summary>
        /// 同步绑定数据
        /// </summary>
        private void SyncBindComs()
        {
            m_BindComs.ClearArray();

            for (int i = 0; i < m_BindDatas.arraySize; i++)
            {
                SerializedProperty property = m_BindDatas.GetArrayElementAtIndex(i).FindPropertyRelative("BindCom");
                m_BindComs.InsertArrayElementAtIndex(i);
                m_BindComs.GetArrayElementAtIndex(i).objectReferenceValue = property.objectReferenceValue;
            }
        }

        private void Ergodic(Transform rootTransform, Transform currentTransform)
        {
            for (int i = 0; i < currentTransform.childCount; ++i)
            {
                Transform child = currentTransform.GetChild(i);

                m_TempFiledNames.Clear();
                m_TempComponentTypeNames.Clear();

                if (IsValidBind(child, m_TempFiledNames, m_TempComponentTypeNames))
                {
                    for (int index = 0; index < m_TempFiledNames.Count; index++)
                    {
                        string componentName = m_TempComponentTypeNames[index];
                        bool isGameObject = componentName.Equals("GameObject");
                        componentName = isGameObject ? "Transform" : componentName;
                        Component com = child.GetComponent(componentName);
                        if (com == null)
                        {
                            Debug.LogError($"{child.name}上不存在{componentName}的组件");
                        }
                        else
                        {
                            AddBindData(m_TempFiledNames[index], child.GetComponent(componentName), isGameObject);
                        }
                    }
                }

                //已挂载绑定脚本的将不再遍历下面的节点
                if (child.GetComponent<VariableArray>() != null)
                {
                    continue;
                }

                Ergodic(rootTransform, child);
            }
        }

        /// <summary>
        /// 添加绑定数据
        /// </summary>
        private void AddBindData(string name, Component bindCom, bool isGameObject = false)
        {
            int index = m_BindDatas.arraySize;
            m_BindDatas.InsertArrayElementAtIndex(index);
            SerializedProperty element = m_BindDatas.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("Name").stringValue = name;
            element.FindPropertyRelative("BindCom").objectReferenceValue = bindCom;
            element.FindPropertyRelative("IsGameObject").boolValue = isGameObject;
        }

        private bool IsValidBind(Transform transform, List<string> filedNames, List<string> componentTypeNames)
        {
            string uiElementName = transform.name;
            var rule = m_settings.FindUIElementGenerateKv(uiElementName);

            if (rule != null)
            {
                filedNames.Add($"{uiElementName}");
                componentTypeNames.Add(rule.ComponentName);
                return true;
            }

            return false;
        }

        private void GenBindingCode()
        {
            GameObject go = m_target.gameObject;
            string codePath = $"{m_codePath}/UI/";
            PrepareDirectory(codePath);

            string className = go.name;
            if (!m_target.IsWindow && !string.IsNullOrEmpty(m_target.WidgetName))
            {
                className = $"{m_target.WidgetName}";
            }
            string fileName = $"{className}.BindComponent";
            string realGuid = FindRealPath(fileName);
            string bindFile = $"{codePath}{fileName}.cs";
            if (!string.IsNullOrEmpty(realGuid))
            {
                bindFile = realGuid;
            }

            using (StreamWriter sw = new StreamWriter(bindFile, false, Encoding.UTF8))
            {
                sw.WriteLine("/*-------------------------------------");
                sw.WriteLine($"Author:{m_settings.CodeAuthor}");
                sw.WriteLine($"Time:{DateTime.Now}");
                sw.WriteLine("--------------------------------------*/");
                sw.WriteLine("");

                sw.WriteLine("using UnityEngine;");
                sw.WriteLine("using UnityEngine.UI;");
                sw.WriteLine("using TMPro;");
                sw.WriteLine("using Spine.Unity;");
                sw.WriteLine("");

                if (!string.IsNullOrEmpty(m_settings.NameSpace))
                {
                    //命名空间
                    sw.WriteLine("namespace " + m_settings.NameSpace);
                    sw.WriteLine("{");
                }

                sw.WriteLine($"\tpublic partial class {className}");
                sw.WriteLine("\t{");

                //组件字段
                foreach (VariableArray.BindData data in m_target.BindDatas)
                {
                    if (data.IsGameObject)
                    {
                        if (data.Name.StartsWith("widget_"))
                        {
                            VariableArray variable = data.BindCom.GetComponent<VariableArray>();
                            if (variable != null)
                            {
                                if (!string.IsNullOrEmpty(variable.WidgetName))
                                {
                                    sw.WriteLine($"\t\tprivate {variable.WidgetName} {data.Name};");
                                    continue;
                                }
                            }
                        }

                        sw.WriteLine($"\t\tprivate GameObject {data.Name};");
                    }
                    else
                    {
                        sw.WriteLine($"\t\tprivate {data.BindCom.GetType().Name} {data.Name};");
                    }
                }

                sw.WriteLine("");

                //组件引用
                sw.WriteLine("\t\tpublic override void ScriptGenerate()");
                sw.WriteLine("\t\t{");
                for (int i = 0; i < m_target.BindDatas.Count; i++)
                {
                    VariableArray.BindData data = m_target.BindDatas[i];
                    string filedName = $"{data.Name}";
                    if (data.IsGameObject)
                    {
                        if (data.Name.StartsWith("widget_"))
                        {
                            VariableArray variable = data.BindCom.GetComponent<VariableArray>();
                            if (variable != null)
                            {
                                if (!string.IsNullOrEmpty(variable.WidgetName))
                                {
                                    string widgetGoBind =
                                        $"VariableArray.Get<{data.BindCom.GetType().Name}>({i}).gameObject";
                                    sw.WriteLine(
                                        $"\t\t\t{data.Name} = base.CreateWidget<{variable.WidgetName}>({widgetGoBind},{widgetGoBind}.activeSelf);");
                                    continue;
                                }
                            }
                        }

                        sw.WriteLine(
                            $"\t\t\t{filedName} = VariableArray.Get<{data.BindCom.GetType().Name}>({i}).gameObject;");
                    }
                    else
                    {
                        sw.WriteLine($"\t\t\t{filedName} = VariableArray.Get<{data.BindCom.GetType().Name}>({i});");
                    }
                }

                sw.WriteLine("\t\t}");

                sw.WriteLine("\t}");

                if (!string.IsNullOrEmpty(m_settings.NameSpace))
                {
                    sw.WriteLine("}");
                }
            }

            Debug.Log($"代码生成成功:{bindFile}");
        }

        private void GenLogicCode()
        {
            GameObject go = m_target.gameObject;
            string codePath = $"{m_codePath}/UI/";
            PrepareDirectory(codePath);

            string className = $"{go.name}";
            if (!m_target.IsWindow && !string.IsNullOrEmpty(m_target.WidgetName))
            {
                className = $"{m_target.WidgetName}";
            }
            string logicFile = $"{codePath}{className}.cs";
            string realPath = FindRealPath(className);
            if (!string.IsNullOrEmpty(realPath))
                logicFile = realPath;

            if (File.Exists(logicFile))
                return;

            using (StreamWriter sw = new StreamWriter(logicFile, true, Encoding.UTF8))
            {
                sw.WriteLine("using UnityEngine;");
                sw.WriteLine("using HotfixCore.Module;");
                sw.WriteLine("");

                if (!string.IsNullOrEmpty(m_settings.NameSpace))
                {
                    //命名空间
                    sw.WriteLine("namespace " + m_settings.NameSpace);
                    sw.WriteLine("{");
                }

                if (m_target.IsWindow)
                {
                    string location = $"uiprefab/{m_codeSubPathProperty.stringValue}/{go.name}";
                    sw.WriteLine($"\t[Window(UILayer.UI,\"{location.ToLower()}\")]");
                }

                className = m_target.IsWindow ? $"{className} : UIWindow" : $"{className} : UIWidget";
                sw.WriteLine($"\tpublic partial class {className}");
                sw.WriteLine("\t{");

                //基类重写
                sw.WriteLine("\t\tpublic override void OnCreate()");
                sw.WriteLine("\t\t{");
                sw.WriteLine("\t\t}");

                sw.WriteLine("\t}");

                if (!string.IsNullOrEmpty(m_settings.NameSpace))
                {
                    sw.WriteLine("}");
                }
            }

            Debug.Log($"代码生成成功:{logicFile}");
        }

        private void GenModelCode()
        {
            if (!m_target.IsWindow)
            {
                return;
            }

            string modelPath = $"{m_codePath}/Model/";
            PrepareDirectory(modelPath);
            string moduleName = m_codeSubPathProperty.stringValue;
            string modelFile = $"{modelPath}/{moduleName}Data.cs";
            string realPath = FindRealPath($"{moduleName}Data");
            if (!string.IsNullOrEmpty(realPath))
            {
                modelFile = realPath;
            }

            if (!File.Exists(modelFile))
            {
                using (StreamWriter sw = new StreamWriter(modelFile, true, Encoding.UTF8))
                {
                    sw.WriteLine("using HotfixCore.MVC;");
                    sw.WriteLine("");

                    if (!string.IsNullOrEmpty(m_settings.NameSpace))
                    {
                        //命名空间
                        sw.WriteLine("namespace " + m_settings.NameSpace);
                        sw.WriteLine("{");
                    }

                    sw.WriteLine($"\tpublic class {moduleName}Data : BaseModel");
                    sw.WriteLine("\t{");
                    sw.WriteLine("\t}");

                    if (!string.IsNullOrEmpty(m_settings.NameSpace))
                    {
                        sw.WriteLine("}");
                    }
                }
            }
            
        }

        private void GenControllerCode()
        {
            string modelPath = $"{m_codePath}";
            PrepareDirectory(modelPath);
            string moduleName = m_codeSubPathProperty.stringValue;
            string controllerFile =  $"{m_codePath}/{moduleName}Controller.cs";
            string realPath = FindRealPath($"{moduleName}Controller");
            if (!string.IsNullOrEmpty(realPath))
            {
                controllerFile = realPath;
            }

            if (!File.Exists(controllerFile))
            {
                using (StreamWriter sw = new StreamWriter(controllerFile, true, Encoding.UTF8))
                {
                    sw.WriteLine("using System;");
                    sw.WriteLine("using HotfixCore.MVC;");
                    sw.WriteLine("");

                    if (!string.IsNullOrEmpty(m_settings.NameSpace))
                    {
                        //命名空间
                        sw.WriteLine("namespace " + m_settings.NameSpace);
                        sw.WriteLine("{");
                    }

                    sw.WriteLine($"\t[MVCDefine(\"{moduleName}\")]");
                    sw.WriteLine($"\tpublic class {moduleName}Controller : BaseController");
                    sw.WriteLine("\t{");
                    sw.WriteLine("\t\tpublic override Type MainView { get; } = "+ $"typeof({m_target.gameObject.name});");
                    sw.WriteLine("\t}");

                    if (!string.IsNullOrEmpty(m_settings.NameSpace))
                    {
                        sw.WriteLine("}");
                    }
                }
            }
        }

        private string FindRealPath(string fileName)
        {
            string[] paths = AssetDatabase.FindAssets($"{fileName} t:Script");
            for (int i = 0; i < paths.Length; i++)
            {
                string assetName = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(paths[i]));
                if(assetName == fileName)
                    return AssetDatabase.GUIDToAssetPath(paths[i]);
            }
            return null;
        }
        
        private void PrepareDirectory(string dirPath)
        {
            if (!Directory.Exists($"{dirPath}/"))
            {
                Directory.CreateDirectory($"{dirPath}/");
            }
        }
    }
}