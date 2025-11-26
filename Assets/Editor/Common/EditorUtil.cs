using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameEditor.Common
{
    public static class EditorUtil
    {
        /// <summary>
        /// 获取Scriptable资产
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="createIfMissing"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetScriptableAsset<T>(string assetName, bool createIfMissing = false)
            where T : ScriptableObject
        {
            string[] paths = AssetDatabase.FindAssets($"t:{assetName}");
            string assetPath = "";
            if (paths.Length <= 0)
            {
                if (createIfMissing)
                {
                    assetPath = $"Assets/Editor/{assetName}.asset";
                    CreateScriptable<T>(assetPath);
                }
                else return null;
            }
            else
            {
                assetPath = AssetDatabase.GUIDToAssetPath(paths[0]);
            }

            T source = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            return source;
        }

        /// <summary>
        /// 资源是否存在
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static bool AssetExists(string assetName)
        {
            string[] paths = AssetDatabase.FindAssets(assetName);
            return paths.Length >= 1;
        }

        static void CreateScriptable<T>(string path) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.Refresh();
        }
    }
}