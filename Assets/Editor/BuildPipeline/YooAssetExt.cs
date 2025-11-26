using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

namespace GameEditor.BuildPipeline
{
    [DisplayName("定位地址: 分组名+文件名(全部转小写)")]
    public class GameAddressRule : IAddressRule
    {
        public string GetAssetAddress(AddressRuleData data)
        {
            string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
            return $"{data.GroupName}_{fileName.ToLower()}";
        }
    }

    [DisplayName("定位地址: 文件名(全部转小写)")]
    public class GameAddressRuleFileName : IAddressRule
    {
        public string GetAssetAddress(AddressRuleData data)
        {
            string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
            return $"{fileName.ToLower()}";
        }
    }

    [DisplayName("定位地址: 正则表达式(全部转小写)")]
    public class GameRegexRuleToLower : IAddressRule
    {
        public string GetAssetAddress(AddressRuleData data)
        {
            var userDataArr = data.UserData.Split(';');
            string matchPattern = string.Empty;
            string addressPattern = string.Empty;
            if (userDataArr.Length > 1)
            {
                matchPattern = userDataArr[0];
                addressPattern = userDataArr[1];
            }

            var match = new Regex(matchPattern);

            var address = match.IsMatch(data.AssetPath)
                ? match.Replace(data.AssetPath, addressPattern).ToLower()
                : string.Empty;
            return address;
        }
    }

    [DisplayName("UI寻址")]
    public class GameRegexUIRule : IAddressRule
    {
        private readonly Dictionary<string, string> _convertMapping = new Dictionary<string, string>()
        {
            ["Prefab"] = "uiprefab",
            ["Sprites"] = "uisprites",
            ["Texture"] = "uitexture",
        };
        
        private const string _regex = @"^Assets/ArtLoad/UI/(?<type>Prefab|Sprites|Texture)/(?<path>.*?)(?:\\.\\w+)?$";

        public string GetAssetAddress(AddressRuleData data)
        {
            var match = Regex.Match(data.AssetPath, _regex, RegexOptions.IgnoreCase);
            if (!match.Success) return string.Empty;
            var type = match.Groups["type"].Value;
            var subPath = match.Groups["path"].Value;
            return _convertMapping.TryGetValue(type,out var target) ? $"{target}/{subPath}".Replace(Path.GetExtension($"{target}/{subPath}"),"").ToLower() : string.Empty;
        }
    }

    [DisplayName("预制体排除预加载资源")]
    public class GameFilterPreloadPrefab : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            return !GamePreloadEditor.IsPreloadAsset(data.AssetPath) && Path.GetExtension(data.AssetPath) == ".prefab";
        }
    }

    [DisplayName("精灵图排除预加载资源")]
    public class GameFilterPreloadTexture : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            bool isSprite = false;
            var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(data.AssetPath);
            if (mainAssetType == typeof(Texture2D))
            {
                var texImporter = AssetImporter.GetAtPath(data.AssetPath) as TextureImporter;
                if (texImporter != null && texImporter.textureType == TextureImporterType.Sprite)
                    isSprite = true;
            }

            return !GamePreloadEditor.IsPreloadAsset(data.AssetPath) && isSprite;
        }
    }

    [DisplayName("音频排除预加载资源")]
    public class GameFilterPreloadAudio : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            bool isAudio = false;
            string suffix = Path.GetExtension(data.AssetPath).ToLower();
            if(suffix == ".mp3" || suffix == ".ogg" || suffix == ".wav")
                isAudio = true;
            return !GamePreloadEditor.IsPreloadAsset(data.AssetPath) && isAudio;
        }
    }

    [DisplayName("定位地址: 正则表达式")]
    public class GameRegexRule : IAddressRule
    {
        public string GetAssetAddress(AddressRuleData data)
        {
            var userDataArr = data.UserData.Split(';');
            string matchPattern = string.Empty;
            string addressPattern = string.Empty;
            if (userDataArr.Length > 1)
            {
                matchPattern = userDataArr[0];
                addressPattern = userDataArr[1];
            }

            var match = new Regex(matchPattern);
            var address = match.IsMatch(data.AssetPath) ? match.Replace(data.AssetPath, addressPattern) : string.Empty;
            return address;
        }
    }
}