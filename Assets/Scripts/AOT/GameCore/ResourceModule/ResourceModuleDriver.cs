using System.Collections.Generic;
using GameCore.Settings;
using UnityEngine;
using YooAsset;

namespace GameCore.Resource
{
    public delegate void OnLowMemoryListener();
    
    public class ResourceModuleDriver : MonoBehaviour
    {
        private static ResourceModuleDriver _instance;
        
        public static ResourceModuleDriver Instance => _instance;
        
        public OnLowMemoryListener OnLowMemory { get; private set; }
        
        /// <summary>
        /// 资源包列表。
        /// </summary>
        public Dictionary<string, ResourcePackage> PackageMap { get; private set; } = new Dictionary<string, ResourcePackage>();
        
        private void Awake()
        {
            _instance = this;
        }

        public void AddLowMemoryListen(OnLowMemoryListener listener)
        {
            OnLowMemory += listener;
        }

        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        public bool TryGetValue(string packageName, out ResourcePackage package)
        {
            if (string.IsNullOrEmpty(packageName))
                packageName = GameSettings.Instance.ProjectSetting.DefaultPackageName;
            return PackageMap.TryGetValue(packageName, out package);
        }

        /// <summary>
        /// 获取默认资源包名
        /// </summary>
        /// <returns></returns>
        public ResourcePackage GetDefaultPackage()
        {
            PackageMap.TryGetValue(GameSettings.Instance.ProjectSetting.DefaultPackageName, out ResourcePackage package);
            return package;
        }
        
        /// <summary>
        /// 添加资源包
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="package"></param>
        public void AddPackage(string packageName, ResourcePackage package)
        {
            if(packageName == GameSettings.Instance.ProjectSetting.DefaultPackageName)
                YooAssets.SetDefaultPackage(package);
            PackageMap.TryAdd(packageName, package);
        }

        /// <summary>
        /// 移除资源包
        /// </summary>
        /// <param name="packageName"></param>
        public void RemovePackage(string packageName)
        {
            PackageMap.Remove(packageName);
        }
    }
}