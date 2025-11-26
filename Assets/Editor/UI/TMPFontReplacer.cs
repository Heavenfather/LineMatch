using UnityEditor;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace GameEditor.UI
{
    public class TMPFontReplacer : EditorWindow
    {
        private TMP_FontAsset _oldFont;
        private TMP_FontAsset _newFont;
        private Dictionary<string, Material> _materialMap = new Dictionary<string, Material>();
        private int _modifiedCount;
        private Vector2 _scrollPos;
        private Object _oldMaterialsFolder;

        [MenuItem("Tools/TextMeshPro/TMP Font Batch Replacer")]
        public static void ShowWindow()
        {
            GetWindow<TMPFontReplacer>("TMP Font Replacer");
        }

        private void ProcessSelectedFolders(string[] folderPaths)
        {
            try
            {
                var allPrefabs = new List<string>();

                foreach (var folder in folderPaths)
                {
                    // 递归获取所有预制体
                    var guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
                    var paths = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                        .Where(p => p.EndsWith(".prefab"))
                        .Where(IsValidPrefabWithTMP)
                        .ToArray();

                    allPrefabs.AddRange(paths);
                }

                if (allPrefabs.Count == 0)
                {
                    Debug.Log("所选文件夹中没有需要处理的预制体");
                    return;
                }

                EditorUtility.DisplayProgressBar("准备处理", "收集预制体中...", 0);
                ProcessAssets(allPrefabs.Distinct().ToList(), "处理选中文件夹", 0, 1);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }

        void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("1. 设置新旧字体\n2. 自动建立材质映射\n3. 执行批量替换", MessageType.Info);

            GUILayout.Space(20);
            _oldFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Old Font Asset", _oldFont, typeof(TMP_FontAsset),
                false);
            _newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New Font Asset", _newFont, typeof(TMP_FontAsset),
                false);
            if (GUILayout.Button("执行批量替换", GUILayout.Height(40)))
            {
                BuildMaterialMapping();
                ExecuteFullReplacement();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("处理选中文件夹"))
            {
                ProcessSelectedFolders(GetSelectedFolders());
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField($"Modified Objects: {_modifiedCount}");
        }

        private string[] GetSelectedFolders()
        {
            return Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets)
                .Select(AssetDatabase.GetAssetPath).ToArray().Where(AssetDatabase.IsValidFolder).ToArray();
        }

        void BuildMaterialMapping()
        {
            if (!ValidateFonts()) return;

            _materialMap.Clear();
            var allMaterials = AssetDatabase.FindAssets("t:Material")
                .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)));

            foreach (var mat in allMaterials)
            {
                if (mat.name.StartsWith(_newFont.name))
                {
                    var oldMaterialName = mat.name.Replace(_newFont.name, _oldFont.name);
                    _materialMap[oldMaterialName] = mat;
                }
            }

            Debug.Log($"Material mapping built. Found {_materialMap.Count} material pairs.");
        }

        void ExecuteFullReplacement()
        {
            if (!ValidateFonts() || !ValidateMaterialMapping()) return;

            _modifiedCount = 0;
            try
            {
                EditorUtility.DisplayProgressBar("Processing", "Starting replacement...", 0);

                // 处理预制体
                ProcessAssets(
                    AssetDatabase
                        .FindAssets("t:Prefab",
                            new[] { "Assets/MatchProject" })
                        .Select(guid => AssetDatabase.GUIDToAssetPath(guid)),
                    "Prefabs", 0f, 0.4f);

                // 处理场景
                ProcessScenes(AssetDatabase.FindAssets("t:Scene").Select(guid => AssetDatabase.GUIDToAssetPath(guid)),
                    "Scenes", 0.4f, 0.4f);
                AssetDatabase.SaveAssets();
                Debug.Log($"Replacement complete! Modified {_modifiedCount} objects.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        void ProcessAssets(IEnumerable<string> assetPaths, string category, float progressStart, float progressRange)
        {
            var paths = assetPaths
                .Where(p => !IsExcludedPath(p))
                .Where(IsValidPrefabWithTMP)
                .ToArray();

            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                try
                {
                    EditorUtility.DisplayProgressBar("Processing", $"{category}: {path}",
                        progressStart + (float)i / paths.Length * progressRange);

                    using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
                    {
                        var prefabRoot = editingScope.prefabContentsRoot;
                        bool modified = ProcessObject(prefabRoot);

                        if (modified)
                        {
                            _modifiedCount++;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"处理失败：{path}\n错误详情：{e}");
                }
            }
        }

        bool IsExcludedPath(string path)
        {
            // 排除规则：Package目录、隐藏文件、临时文件
            return path.StartsWith("Packages/") ||
                   Path.GetFileName(path).StartsWith("~") ||
                   Path.GetExtension(path).Contains(".tmp");
        }

        bool IsValidPrefabWithTMP(string path)
        {
            // 双重验证机制
            return HasTMPReferenceInDependencies(path) &&
                   HasActualTMPComponent(path);
        }

        bool HasTMPReferenceInDependencies(string path)
        {
            // 快速依赖检查
            var dependencies = AssetDatabase.GetDependencies(path, false);
            return dependencies.Any(d => AssetDatabase.GetMainAssetTypeAtPath(d) == typeof(TMP_FontAsset));
        }

        bool HasActualTMPComponent(string path)
        {
            // 精确组件检查（不实例化预制体）
            var obj = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
            return obj != null &&
                   obj.GetComponentsInChildren<TMP_Text>(true).Any(t => t.font == _oldFont);
        }

        bool IsSafeToProcess(string path)
        {
            // 安全清单检查
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null) return false;

            // 检查物理组件
            var hasPhysics = ((GameObject)asset).GetComponentsInChildren<Component>(true)
                .Any(c => c is Collider || c is Rigidbody);

            // 检查网格引用
            var hasMesh = ((GameObject)asset).GetComponentsInChildren<MeshFilter>(true)
                .Any(m => m.sharedMesh != null);

            return !(hasPhysics && hasMesh); // 同时包含物理和网格的视为高风险
        }


        void ProcessScenes(IEnumerable<string> scenePaths, string category, float progressStart, float progressRange)
        {
            var paths = scenePaths
                .Where(p => !p.StartsWith("Packages/"))
                .ToArray();

            var originalScene = SceneManager.GetActiveScene();

            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                EditorUtility.DisplayProgressBar("Processing", $"{category}: {path}",
                    progressStart + (float)i / paths.Length * progressRange);

                var scene = EditorSceneManager.OpenScene(path);
                var allTexts = scene.GetRootGameObjects()
                    .SelectMany(go => go.GetComponentsInChildren<TMP_Text>(true))
                    .Where(t => t.font == _oldFont);

                bool modified = false;
                foreach (var tmp in allTexts)
                {
                    Undo.RecordObject(tmp, "Replace TMP Font");
                    tmp.font = _newFont;
                    modified |= ReplaceMaterials(tmp);
                    EditorUtility.SetDirty(tmp);
                }

                if (modified)
                {
                    EditorSceneManager.SaveScene(scene);
                    _modifiedCount++;
                }
            }

            EditorSceneManager.OpenScene(originalScene.path);
        }

        bool ProcessObject(GameObject target)
        {
            bool modified = false;
            var tmpComponents = target.GetComponentsInChildren<TMP_Text>(true)
                .Where(t => t.font == _oldFont); // 提前过滤
            foreach (var tmp in tmpComponents)
            {
                if (tmp.font == _oldFont)
                {
                    Undo.RecordObject(tmp, "Replace TMP Font");
                    // 材质替换 必须先替换材质，要不然设置完字体后TMP会默认给新的材质
                    if (ReplaceMaterials(tmp))
                    {
                        EditorUtility.SetDirty(tmp);
                    }

                    tmp.font = _newFont;
                    modified = true;
                }
            }

            return modified;
        }

        bool ReplaceMaterials(TMP_Text tmp)
        {
            bool modified = false;

            // 替换主材质
            if (tmp.fontSharedMaterial != null)
            {
                // Debug.LogError($"{tmp.gameObject.name}: Replace materials for {tmp.fontSharedMaterial.name}");
                if (_materialMap.TryGetValue(tmp.fontSharedMaterial.name, out var newMat))
                {
                    tmp.fontSharedMaterial = newMat;
                    modified = true;
                }
                else
                {
                    Debug.LogError($"Failed to replace materials for {tmp.fontSharedMaterial.name}");
                }
            }

            return modified;
        }

        bool ValidateFonts()
        {
            if (_oldFont == null || _newFont == null)
            {
                Debug.LogError("Please assign both old and new fonts!");
                return false;
            }

            return true;
        }

        bool ValidateMaterialMapping()
        {
            if (_materialMap.Count == 0)
            {
                Debug.LogError("Material mapping is empty! Click 'Analyze Material Mapping' first.");
                return false;
            }

            return true;
        }
    }
}