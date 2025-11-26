using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameCore.LitJson;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

namespace GameEditor.BuildPipeline
{
    /// <summary>
    /// 预加载配置文件结构
    /// </summary>
    [Serializable]
    public class PreloadList
    {
        public List<string> Preload;

        public bool IsContain(string assetPath)
        {
            for (int i = 0; i < Preload.Count; i++)
            {
                if (assetPath.Contains(Preload[i]))
                    return true;
            }

            return false;
        }
    }

    public static class GamePreloadEditor
    {
        private static PreloadList _preloadList = new PreloadList();
        private static HashSet<string> _preloadHashList = new HashSet<string>();

        private struct PreloadBundleInfo
        {
            public long FileSize;
            public string FileHash;
        }
        
        static GamePreloadEditor()
        {
            TextAsset ta =
                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Editor/BuildPipeline/PreloadPackBundle.json");
            if (ta != null)
            {
                _preloadList = JsonMapper.ToObject<PreloadList>(ta.text);
            }
        }

        public static void BuildBundleMap(string bundleVersion, BuildTarget buildTarget)
        {
            _preloadHashList.Clear();
            const string defaultPackageName = "AssetMain";
            string bundlePath =
                $"{AssetBundleBuilderHelper.GetDefaultBuildOutputRoot()}/{buildTarget}/{defaultPackageName}/{bundleVersion}";
            if (Directory.Exists(bundlePath))
            {
                string versionHashFile = $"{bundlePath}/{defaultPackageName}_{bundleVersion}.json";
                if (File.Exists(versionHashFile))
                {
                    long preloadTotalSize = 0;
                    List<PreloadBundleInfo> preloadBundleList = new List<PreloadBundleInfo>();
                    PackageManifest manifest = ManifestTools.DeserializeFromJson(File.ReadAllText(versionHashFile));
                    foreach (var bundle in manifest.BundleList)
                    {
                        for (int i = 0; i < bundle.Tags.Length; i++)
                        {
                            if (bundle.Tags[i] == "Preload")
                            {
                                preloadTotalSize += bundle.FileSize;
                                preloadBundleList.Add(new PreloadBundleInfo
                                {
                                    FileSize = bundle.FileSize,
                                    FileHash = bundle.FileHash
                                });
                            }
                        }
                    }
                    preloadBundleList.Sort((x, y) => y.FileSize.CompareTo(x.FileSize));
                    Debug.Log("============= PreloadInfo Start ==============");
                    for (int i = 0; i < preloadBundleList.Count; i++)
                    {
                        Debug.Log($"{preloadBundleList[i].FileHash} : {preloadBundleList[i].FileSize / 1024.0f / 1024.0f}M");
                        _preloadHashList.Add(preloadBundleList[i].FileHash);
                    }
                    Debug.Log("============= PreloadInfo End ==============");

                    long sizeM = preloadTotalSize / 1024 / 1024;
                    Debug.Log($"PreloadTotalSize : {sizeM} M");
                }
            }
        }

        public static bool IsPreloadAsset(string assetPath)
        {
            return _preloadList.IsContain(assetPath);
        }
        
        public static string GetFormatPreloadList(string cdnPath)
        {
            //将_preloadHashList里面的每个字符加上cdnPath，然后拼接 ; 符号返回
            return string.Join(";", _preloadHashList.Select(x => cdnPath + "/" + x + ".bundle"));
        }
    }
}