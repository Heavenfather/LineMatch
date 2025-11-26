using System;
using System.Collections.Generic;
using GameCore.LRU;

namespace GameConfig
{
    /// <summary>
    /// 所有配置对象缓存池
    /// 统一获取统一管理配置对象，方便动态释放
    /// </summary>
    public static class ConfigMemoryPool
    {
        //最大保留50份配置，可以按照具体的配置体量调整，每10分钟清理一次
        // private static readonly AutoLru<string, ConfigBase> _configPool = new AutoLru<string, ConfigBase>(50, TimeSpan.FromMinutes(10));
        private static readonly Dictionary<string, ConfigBase> _configPool = new Dictionary<string, ConfigBase>(100);

        public static T Get<T>() where T : ConfigBase, new()
        {
            var key = typeof(T).FullName;
            if(_configPool.TryGetValue(key, out var config))
                return (T)config;
            _configPool.TryAdd(key, new T());
            return (T)_configPool[key];
        }
        
        internal static void MarkUsage(ConfigBase config)
        {
            // _configPool.MarkUsage(config);
        }
    }
}