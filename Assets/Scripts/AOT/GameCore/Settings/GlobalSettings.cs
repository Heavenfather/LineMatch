using System;
using System.Collections.Generic;
using System.IO;
using GameCore.Logic;
using UnityEngine;
using YooAsset;

namespace GameCore.Settings
{
    /// <summary>
    /// 游戏全局相关配置
    /// </summary>
    [CreateAssetMenu(fileName = "GlobalSettings", menuName = "Game/Global Config")]
    public class GlobalSettings : ScriptableObject
    {
        /// <summary>
        /// 项目构建类型
        /// </summary>
        public EAppMode AppMode;

        /// <summary>
        /// 资源加载方式
        /// </summary>
        public EPlayMode PlayMode;

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProductName;

        /// <summary>
        /// 默认资源包
        /// </summary>
        public string DefaultPackageName = "AssetMain";

        /// <summary>
        /// 多语言使用代码 CN/EN 等等
        /// </summary>
        public string Language = "CN";

        /// <summary>
        /// 主CDN
        /// </summary>
        public string CDN;

        /// <summary>
        /// 备用CDN
        /// </summary>
        public string FallBackCDN;

        /// <summary>
        /// 游戏服域名
        /// </summary>
        public string GameAddress;
        
        /// <summary>
        /// 资源包名
        /// </summary>
        public string[] PackageNames = new string[] { "AssetMain" };

        /// <summary>
        /// 热更程序集
        /// </summary>
        public List<string> HotUpdateAssemblies = new List<string>() { "GameHotfix.dll" };

        /// <summary>
        /// 热更程序集-HotUpdateAssemblies依赖于HotUpdateReferenceAssemblies 需要优先加载
        /// </summary>
        public List<string> HotUpdateReferenceAssemblies = new List<string>() { "GameConfig.dll" };

        /// <summary>
        /// 补充元数据程序集
        /// </summary>
        public List<string> AOTMetaAssemblies = new List<string>()
            { "mscorlib.dll", "System.dll", "System.Core.dll", "GameCore.dll","YooAsset.dll","UniTask.dll","UnityEngine.CoreModule.dll" };

        /// <summary>
        /// 主CDN资源下载路径
        /// </summary>
        public string GetCDNDownLoadPath()
        {
            return Path.Combine(CDN, ProductName, AppMode.ToString(), GetPlatformName()).Replace("\\", "/");
        }

        public string GetFallbackCDNDownLoadPath()
        {
            return Path.Combine(FallBackCDN, ProductName, AppMode.ToString(), GetPlatformName()).Replace("\\", "/");
        }

        /// <summary>
        /// 主CDN资源下载路径
        /// </summary>
        public string GetCDNDownLoadPath(string platform)
        {
            return Path.Combine(CDN, ProductName, AppMode.ToString(), platform).Replace("\\", "/");
        }

        public string GetFallbackCDNDownLoadPath(string platform)
        {
            return Path.Combine(FallBackCDN, ProductName, AppMode.ToString(), platform).Replace("\\", "/");
        }
        
        /// <summary>
        /// 获取当前的平台名称。
        /// </summary>
        /// <returns>平台名称。</returns>
        public string GetPlatformName()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "win";
                case RuntimePlatform.IPhonePlayer:
                    return "ios";
                case RuntimePlatform.Android:
                    return "android";
                case RuntimePlatform.WebGLPlayer:
                    return "webgl";
                default:
                    throw new NotSupportedException($"Platform '{Application.platform.ToString()}' is not supported.");
            }
        }
    }
}