using System.IO;
using System.Text.RegularExpressions;
using GameEditor.BuildPipeline;
using UnityEditor;
using UnityEngine;

namespace GameEditor.AssetImport
{
    /// <summary>
    /// 自定义资产导入管线
    /// </summary>
    public class CustomAssetPostProcess : AssetPostprocessor
    {
        private static AssetImportConfig _config;

        private static AssetImportConfig LoadConfig()
        {
            if (_config == null)
            {
                _config = AssetDatabase.LoadAssetAtPath<AssetImportConfig>(
                    "Assets/Editor/AssetImport/AssetImportConfig.asset");
            }

            return _config;
        }
        
        void OnPreprocessTexture()
        {
            var config = LoadConfig();
            if(config == null)
                return;
            
            var importer = assetImporter as TextureImporter;
            if(importer == null)
                return;
            // string commandPath = $"{Application.dataPath}/../Publish/tools/pingo.exe";
            // if (File.Exists(commandPath))
            // {
            //     var fullPath = Path.GetFullPath(importer.assetPath);
            //     var args = $"-quality={config.compressLevel} *.png {fullPath}";
            //     BuildUtils.LaunchProcess(commandPath, args,false);
            // }

            void ApplyPlatformSettings(AssetTextureRule rule)
            {
                importer.textureType = rule.textureType;
                importer.textureShape = rule.textureShape;
                importer.mipmapEnabled = rule.mipmapEnabled;
                importer.sRGBTexture = rule.sRGB;
                importer.filterMode = rule.filterMode;
                
                importer.ClearPlatformTextureSettings("Android");
                importer.ClearPlatformTextureSettings("iOS");
                importer.ClearPlatformTextureSettings("WebGL");

                var androidSettings = rule.GetSettingsForPlatform("Android");
                if (androidSettings.overrideForPlatform)
                {
                    importer.SetPlatformTextureSettings(androidSettings.ToUnitySettings("Android"));
                }
                
                var iosSettings = rule.GetSettingsForPlatform("iOS");
                if (iosSettings.overrideForPlatform)
                {
                    importer.SetPlatformTextureSettings(iosSettings.ToUnitySettings("iOS"));
                }
                
                var webglSettings = rule.GetSettingsForPlatform("WebGL");
                if (webglSettings.overrideForPlatform)
                {
                    importer.SetPlatformTextureSettings(webglSettings.ToUnitySettings("WebGL"));
                }
            }
            
            string path = importer.assetPath;
            foreach (var rule in config.textureRules)
            {
                if(!Regex.IsMatch(path,rule.pathRegex))
                    continue;

                importer.textureType = rule.textureType;
                importer.textureShape = rule.textureShape;
                ApplyPlatformSettings(rule);
                break;
            }
        }

        private void OnPreprocessAudio()
        {
            var config = LoadConfig();
            if(config == null)
                return;
            
            var importer = assetImporter as AudioImporter;
            if(importer == null)
                return;

            string path = importer.assetPath;
            foreach (var rule in config.audioRules)
            {
                if(!Regex.IsMatch(path,rule.pathRegex))
                    continue;
                
                importer.forceToMono = rule.forceToMono;
                importer.loadInBackground = true;
                
                AudioImporterSampleSettings defaultSettings = importer.defaultSampleSettings;
                defaultSettings.quality = Mathf.Clamp01(rule.quality / 255.0f);
                defaultSettings.loadType = rule.loadType;
                defaultSettings.compressionFormat = rule.format;
                defaultSettings.preloadAudioData = false;
                importer.defaultSampleSettings = defaultSettings;

                importer.SetOverrideSampleSettings("WebGL", defaultSettings);
                break;
            }
            
            EditorUtility.SetDirty(importer);
        }
    }
}