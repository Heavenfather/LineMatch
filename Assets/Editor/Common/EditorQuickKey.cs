using System.Collections.Generic;
using System.IO;
using GameCore.LitJson;
using GameCore.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Logger = GameCore.Log.Logger;
namespace GameEditor.Common
{
    /// <summary>
    /// 编辑器下的快捷键定义
    /// </summary>
    public static class EditorQuickKey
    {
        [MenuItem("Tools/Jump/跳转到拼UI场景 &u")]
        public static void QuickUIEditor()
        {
            JumpScene("UIEditor");
        }

        [MenuItem("Tools/Jump/跳转到启动场景 &g")]
        public static void QuickGameEditor()
        {
            JumpScene("GameEnter");
        }

        // [MenuItem("Tools/Jump/跳转到消除关卡编辑")]
        // public static void QuickMatchEditorScene()
        // {
        //     JumpScene("MatchLevelEditor");
        // }

        private static void JumpScene(string sceneName)
        {
            string[] sceneGuids = AssetDatabase.FindAssets($"{sceneName} t:Scene");
            if(sceneGuids.Length == 0)
                return;
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[0]);
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        [MenuItem("Tools/删除数据")]
        private static void DeleteData()
        {
            PlayerPrefsUtil.DeleteAll();
            // EditorPrefs.DeleteAll();
            if (Directory.Exists(Application.persistentDataPath))
            {
                Directory.Delete(Application.persistentDataPath, true);
                Logger.Debug($"[PresistentFileUtility] DeleteAllFile file");
            }
            else
            {
                Logger.Warning($"[PresistentFileUtility] DeleteAllFile file not exist");
            }
        }

        [MenuItem("Tools/Spine点击动画导出数据")]
        private static void WriteSpineTouchJsonData() {
            Dictionary<int, List<string>> spineItemList = new Dictionary<int, List<string>>();

            var checkPath = "Assets/ArtRaw/Spine/Puzzle";

            // 获取指定路径下的所有AudioClip资源
            string[] spineGuids = AssetDatabase.FindAssets("t:TextAsset", new[] {checkPath});
            foreach (string guid in spineGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                TextAsset text = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                if (text != null && text.text.Contains("Touch"))
                {
                    Debug.Log("assetPath = " + assetPath);
                    var prefabPath = assetPath.Replace(checkPath + "/", "");

                    var strArray = prefabPath.Split('/');
                    if (strArray.Length != 3) continue;

                    int mapID = int.TryParse(strArray[0], out int id) ? id : 0;
                    string itemName = strArray[1];
                    if (mapID != 0) {
                        if (!spineItemList.ContainsKey(mapID)) {
                            spineItemList.Add(mapID, new List<string>());
                        }
                        spineItemList[mapID].Add(itemName);
                    }
                }
            }

            foreach (var item in spineItemList) {
                item.Value.Sort();
            }

            // 定义当前文件夹路径
            string currentFolderPath = "./";

            // 创建 DirectoryInfo 对象
            DirectoryInfo directoryInfo = new DirectoryInfo(currentFolderPath);

            // 获取并打印父目录路径
            string parentFolderPath = directoryInfo.Parent.FullName;
            if (!Directory.Exists(parentFolderPath + "/PuzzleBackUp")) {
                Directory.CreateDirectory(parentFolderPath + "/PuzzleBackUp");
            }


            var fileName = "SpineTouchData";
            var filePath = parentFolderPath + "/PuzzleBackUp/" + fileName + ".json";

            var jsonStr = JsonMapper.ToJson(spineItemList);
            File.WriteAllText(filePath, jsonStr);
        }

        [MenuItem("GameObject/MyTools/Copy节点路径")]
        public static void CopyNodePath()
        {
            string nodePath = "";
            GetNodePath(Selection.activeGameObject.transform, ref nodePath);
            UnityEngine.Debug.Log($"拷贝节点路径:{nodePath}");
            TextEditor editor = new TextEditor();
            editor.text = nodePath;
            editor.SelectAll();
            editor.Copy();
        }

        private static void GetNodePath(Transform trans, ref string path)
        {
            if (path == "")
            {
                path = trans.name;
            }
            else
            {
                path = $"{trans.name}/{path}";
            }

            if (trans.parent != null)
            {
                GetNodePath(trans.parent, ref path);
            }
        }

    }
}