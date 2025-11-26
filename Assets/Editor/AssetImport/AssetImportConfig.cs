using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GameEditor.AssetImport
{
    [CreateAssetMenu(fileName = "AssetImportConfig", menuName = "Game/AssetImport Config")]
    public class AssetImportConfig : ScriptableObject
    {
        [Header("全局配置")] 
        [LabelText("图片大小压缩"), PropertyRange(60, 100), PropertyTooltip("数值60-100，100为最大质量")]
        public int compressLevel = 60;
        
        [Header("贴图导入规则")] public List<AssetTextureRule> textureRules = new List<AssetTextureRule>();
        [Header("音频导入规则")] public List<AssetAudioRule> audioRules = new List<AssetAudioRule>();
    }
    
    [Serializable]
    public class AssetTextureRule
    {
        [Tooltip("匹配路径的正则表达式")] public string pathRegex;
        public TextureImporterType textureType = TextureImporterType.Sprite;
        public TextureImporterShape textureShape = TextureImporterShape.Texture2D;
        public bool mipmapEnabled = false;
        public bool sRGB = true;
        public FilterMode filterMode = FilterMode.Bilinear;

        [Header("平台压缩设置")]
        [Tooltip("默认")]
        public TexturePlatformSetting defaultSetting;
        [Tooltip("Android平台设置")]
        public TexturePlatformSetting androidSetting;
        [Tooltip("iOS平台设置")] public TexturePlatformSetting iosSetting;
        [Tooltip("webGL平台设置")] public TexturePlatformSetting webGLSetting;

        public TexturePlatformSetting GetSettingsForPlatform(string platform)
        {
            switch (platform.ToLower())
            {
                case "android":
                    return androidSetting;
                case "ios":
                    return iosSetting;
                case "webgl":
                    return webGLSetting;
                default:
                    return defaultSetting;
            }
        }
    }

    [Serializable]
    public class TexturePlatformSetting
    {
        public bool overrideForPlatform = true;
        [Tooltip("压缩格式")]public TextureImporterFormat format = TextureImporterFormat.Automatic;
        [Tooltip("最大尺寸")] public int maxTextureSize = 2048;
        [Tooltip("压缩质量")] [Range(0, 100)] public int compressionQuality = 50;

        public TextureImporterPlatformSettings ToUnitySettings(string platform)
        {
            return new TextureImporterPlatformSettings
            {
                name = platform,
                maxTextureSize = maxTextureSize,
                compressionQuality = compressionQuality,
                format = format,
                overridden = overrideForPlatform
            };
        }
    }

    [Serializable]
    public class AssetAudioRule
    {
        [Tooltip("匹配路径的正则表达式")] public string pathRegex;

        public AudioCompressionFormat format = AudioCompressionFormat.AAC;
        [Range(32, 256)] public int quality = 64; //小游戏默认使用64
        public bool forceToMono = true; //小游戏推荐单声道
        public AudioClipLoadType loadType = AudioClipLoadType.CompressedInMemory;
    }
}