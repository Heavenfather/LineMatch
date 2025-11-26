using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace GameEditor.AssetImport
{
    /// <summary>
    /// 精灵导入自动生成图集文件
    /// </summary>
    public class AtlasImportProcess : AssetPostprocessor
    {
        private const string _atlasPath = "Assets/ArtRaw/UI/Atlas";
        private const string _spritePath = "Assets/ArtLoad/UI/Sprites";
        private static readonly List<string> _dirtyAtlasList = new List<string>();
        private static readonly Dictionary<string, List<string>> _allSprites = new Dictionary<string, List<string>>();
        private static readonly Dictionary<string, string> _atlasMap = new Dictionary<string, string>();
        private static bool _isInit = false;
        private static bool _dirty = false;
        
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var s in importedAssets)
            {
                ImportSprite(s);
            }

            foreach (var s in deletedAssets)
            {
                DeleteSprite(s);
            }

            foreach (var s in movedFromAssetPaths)
            {
                DeleteSprite(s);
            }

            foreach (var s in movedAssets)
            {
                ImportSprite(s);
            }
        }

        private static void Init()
        {
            if (_isInit)
                return;

            EditorApplication.update += CheckDirty;

            //读取所有图集信息
            string[] findAssets = AssetDatabase.FindAssets("t:SpriteAtlas", new[] {_atlasPath});
            foreach (var findAsset in findAssets)
            {
                var path = AssetDatabase.GUIDToAssetPath(findAsset);
                SpriteAtlas sa = AssetDatabase.LoadAssetAtPath(path, typeof(SpriteAtlas)) as SpriteAtlas;
                if (sa == null)
                {
                    Debug.LogError($"加载图集{path}失败");
                    continue;
                }

                string atlasName = Path.GetFileNameWithoutExtension(path);
                var objects = sa.GetPackables();
                foreach (var o in objects)
                {
                    if (!_allSprites.TryGetValue(atlasName, out var list))
                    {
                        list = new List<string>();
                        _allSprites.Add(atlasName, list);
                    }

                    list.Add(AssetDatabase.GetAssetPath(o));
                }
            }

            _isInit = true;
        }

        private static void CheckDirty()
        {
            if (!_dirty) return;

            _dirty = false;
            AssetDatabase.Refresh();
            float lastProgress = -1;
            for (int i = 0; i < _dirtyAtlasList.Count; i++)
            {
                string atlasName = _dirtyAtlasList[i];
                Debug.Log($"更新图集:{atlasName}");
                var curProgress = (float) i / _dirtyAtlasList.Count;
                if (curProgress > lastProgress + 0.01f)
                {
                    lastProgress = curProgress;
                    var progressText = $"当前进度:{i}/{_dirtyAtlasList.Count} {atlasName}";
                    bool cancel =
                        EditorUtility.DisplayCancelableProgressBar($"刷新图集:{atlasName}", progressText, curProgress);
                    if (cancel)
                    {
                        break;
                    }
                }

                SaveAtlas(atlasName);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _dirtyAtlasList.Clear();
        }

        private static void ImportSprite(string assetPath)
        {
            if (!assetPath.StartsWith(_spritePath)) return;

            TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (ti != null)
            {
                var modify = false;
                if (ti.textureType == TextureImporterType.Sprite)
                {
                    ProcessSprite(assetPath);
                }
            }
        }

        private static void ProcessSprite(string assetPath)
        {
            if (!assetPath.StartsWith("Assets")) return;
            //Texture大图不进图集
            if(IsTextureSprite(assetPath)) return;
            if(!IsUISprite(assetPath)) return;
            
            Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture2D != null)
            {
                if (Math.Max(texture2D.width, texture2D.height) > 1024)
                {
                    Debug.LogError($"The texture size is over 1024,please remove to texture path:{assetPath}");
                    return;
                }
            }

            Init();

            var spriteName = Path.GetFileNameWithoutExtension(assetPath);
            var spritePath = GetSpritePath(assetPath);
            if (!_atlasMap.TryGetValue(spriteName, out string oldAssetPath) || spritePath == oldAssetPath)
            {
                _atlasMap[spriteName] = spritePath;
                _dirty = true;
            }
            else
            {
                Debug.LogError($"有重名的图片:{spriteName}\n旧图集:{oldAssetPath}\n新图集:{spritePath}");
                _atlasMap[spriteName] = spritePath;
                _dirty = true;
            }

            string atlasName = GetPackAtlasName(assetPath);
            if (string.IsNullOrEmpty(atlasName))
                return;

            List<string> ret;
            if (!_allSprites.TryGetValue(atlasName, out ret))
            {
                ret = new List<string>();
                _allSprites.Add(atlasName, ret);
            }

            if (!ret.Contains(assetPath))
            {
                ret.Add(assetPath);
                _dirty = true;
                if (!_dirtyAtlasList.Contains(atlasName))
                {
                    _dirtyAtlasList.Add(atlasName);
                }
            }
        }

        private static void DeleteSprite(string assetPath)
        {
            if (!assetPath.StartsWith(_spritePath))
                return;

            Init();
            string atlasName = GetPackAtlasName(assetPath);
            if (!_allSprites.TryGetValue(atlasName, out var ret))
                return;

            if (!ret.Exists(x => Path.GetFileName(x) == Path.GetFileName(assetPath)))
                return;

            var spriteName = Path.GetFileNameWithoutExtension(assetPath);
            if (_atlasMap.ContainsKey(spriteName))
            {
                _atlasMap.Remove(spriteName);
            }

            ret.Remove(assetPath);
            _dirty = true;
            if (!_dirtyAtlasList.Contains(atlasName))
                _dirtyAtlasList.Add(atlasName);
        }

        private static string GetSpritePath(string assetPath)
        {
            string path = assetPath.Substring(0, assetPath.LastIndexOf(".", StringComparison.Ordinal));
            path = path.Replace("Assets/ArtLoad/UI/", "");
            return path;
        }

        private static string GetPackAtlasName(string fullName)
        {
            fullName = fullName.Replace("\\", "/");
            int idx = fullName.LastIndexOf("Sprites", StringComparison.Ordinal);
            if (idx == -1)
            {
                return "";
            } 

            if (IsTextureSprite(fullName))
            {
                return "";
            }

            string subPath = fullName.Substring(idx + 7);
            string[] splits = subPath.Split('/');
            if (splits.Length <= 0)
                return "Common";
            var atlasPath = splits[1];

            return atlasPath;
        }

        private static bool IsTextureSprite(string assetPath)
        {
            return assetPath.Contains("Texture");
        }

        private static bool IsUISprite(string assetPath)
        {
            return assetPath.Contains("Sprites");
        }
        
        private static void SaveAtlas(string atlasName)
        {
            List<Object> spriteList = new List<Object>();
            if (_allSprites.TryGetValue(atlasName, out var list))
            {
                list.Sort(StringComparer.Ordinal);

                foreach (var s in list)
                {
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(s);
                    if (sprite != null)
                    {
                        spriteList.Add(sprite);
                    }
                }
            }

            string savePath = $"{_atlasPath}/{atlasName}.spriteatlasv2";
            Debug.Log($"save atlas path:{savePath}");
            //删除空目录图集
            if (spriteList.Count == 0)
            {
                if (File.Exists(savePath))
                {
                    AssetDatabase.DeleteAsset(savePath);
                }

                return;
            }

            var atlas = new SpriteAtlasAsset();
            atlas.name = atlasName;
            atlas.Add(spriteList.ToArray());
            EditorUtility.SetDirty(atlas);
            SpriteAtlasAsset.Save(atlas, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var atlasImpt = AssetImporter.GetAtPath(savePath) as SpriteAtlasImporter;
            if (atlasImpt == null)
            {
                return;
            }

            var setting = new SpriteAtlasPackingSettings()
            {
                blockOffset = 1,
                padding = 4,
                enableRotation = true,
                enableAlphaDilation = true,
            };

            var textureSetting = new SpriteAtlasTextureSettings()
            {
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear
            };
            atlasImpt.includeInBuild = true;
            atlasImpt.packingSettings = setting;
            atlasImpt.textureSettings = textureSetting;

            var iosSetting = atlasImpt.GetPlatformSettings("iPhone");
            if (!iosSetting.overridden)
            {
                iosSetting.overridden = true;
                iosSetting.format = TextureImporterFormat.ASTC_6x6;
                iosSetting.compressionQuality = 50;
                atlasImpt.SetPlatformSettings(iosSetting);
            }

            var androidPlatformSetting = atlasImpt.GetPlatformSettings("Android");
            if (!androidPlatformSetting.overridden)
            {
                androidPlatformSetting.overridden = true;
                androidPlatformSetting.format = TextureImporterFormat.ASTC_6x6;
                androidPlatformSetting.compressionQuality = 50;
                atlasImpt.SetPlatformSettings(androidPlatformSetting);
            }

            var webglSettings = atlasImpt.GetPlatformSettings("WebGL");
            if (!webglSettings.overridden)
            {
                webglSettings.overridden = true;
                webglSettings.format = TextureImporterFormat.ASTC_6x6;
                webglSettings.compressionQuality = 50;
                atlasImpt.SetPlatformSettings(webglSettings);
            }

            atlasImpt.SaveAndReimport();
        }

        private static readonly Dictionary<string, List<string>> _tempAllASprites =
            new Dictionary<string, List<string>>();

        [MenuItem("Tools/Atlas/重新生成UI图集", false, 90)]
        static void ForceGenAtlas()
        {
            Init();
            List<string> needSaveAtlas = new List<string>();
            _tempAllASprites.Clear();
            _allSprites.Clear();
            var findAssets = AssetDatabase.FindAssets("t:sprite", new[] {_spritePath});
            foreach (var findAsset in findAssets)
            {
                var path = AssetDatabase.GUIDToAssetPath(findAsset);
                var atlasName = GetPackAtlasName(path);
                if (!_tempAllASprites.TryGetValue(atlasName, out var spriteList))
                {
                    spriteList = new List<string>();
                    _tempAllASprites[atlasName] = spriteList;
                }

                if (!spriteList.Contains(path))
                {
                    spriteList.Add(path);
                }
            }

            //有变化的才刷
            var iter = _tempAllASprites.GetEnumerator();
            while (iter.MoveNext())
            {
                bool needSave = false;
                var atlasName = iter.Current.Key;
                var newSpritesList = iter.Current.Value;

                if (_allSprites.TryGetValue(atlasName, out var existSprites))
                {
                    if (existSprites.Count != newSpritesList.Count)
                    {
                        needSave = true;
                        existSprites.Clear();
                        existSprites.AddRange(newSpritesList);
                    }
                    else
                    {
                        for (int i = 0; i < newSpritesList.Count; i++)
                        {
                            if (!existSprites.Contains(newSpritesList[i]))
                            {
                                needSave = true;
                                break;
                            }
                        }

                        if (needSave)
                        {
                            existSprites.Clear();
                            existSprites.AddRange(newSpritesList);
                        }
                    }
                }
                else
                {
                    needSave = true;
                    _allSprites.Add(atlasName, new List<string>(newSpritesList));
                }

                if (needSave && !needSaveAtlas.Contains(atlasName))
                {
                    needSaveAtlas.Add(atlasName);
                }
            }

            iter.Dispose();
            foreach (var atlas in needSaveAtlas)
            {
                Debug.LogFormat("Gen atlas:{0}", atlas);
                SaveAtlas(atlas);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);
            Debug.Log("Gen end");
        }
    }
}