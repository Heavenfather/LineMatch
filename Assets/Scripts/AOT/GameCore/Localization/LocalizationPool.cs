using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;
using Logger = GameCore.Log.Logger;

namespace GameCore.Localization
{
    /// <summary>
    /// 多语言缓存池
    /// </summary>
    public static class LocalizationPool
    {
        private static readonly Dictionary<string, string> _allTexts = new Dictionary<string, string>();

        /// <summary>
        /// 加载多语言配置文件
        /// </summary>
        /// <param name="dataGenerator">数据生成器方法</param>
        /// <param name="clearExisting">是否清空现有数据(当切换语言时建议清空)</param>
        public static async UniTask LanguageFactory(Func<Action<string, string>, UniTask> dataGenerator,
            bool clearExisting = false)
        {
            if (clearExisting)
            {
                _allTexts.Clear();
            }

            void AddEntry(string key, string value)
            {
                if (_allTexts.TryGetValue(key, out var existingValue))
                {
                    Logger.Warning(
                        $"Localization key conflict: {key} oldValue: {existingValue} newValue: {value}");
                    return;
                }

                var interKey = string.Intern(key);
                var interValue = string.Intern(value.Replace("\\n","\n"));
                _allTexts.Add(interKey, interValue);
            }

            await dataGenerator(AddEntry);
        }

        /// <summary>
        /// 读取多语言
        /// </summary>
        /// <param name="locations">资源地址列表</param>
        /// <param name="packageName">资源包名</param>
        /// <param name="clearExisting">是否清空现有数据(当切换语言时建议清空)</param>
        /// <returns></returns>
        public static UniTask ReadI18N(string[] locations, string packageName,bool clearExisting = false)
        {
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            var package = YooAssets.GetPackage(packageName);
            if (package == null)
                return UniTask.CompletedTask;
            if(clearExisting)
                _allTexts.Clear();
            int loadedCount = 0;
            for (int i = 0; i < locations.Length; i++)
            {
                int index = i;
                LanguageFactory(async addEntry =>
                {
                    var handle = package.LoadAssetAsync<TextAsset>(locations[index]);
                    handle.Completed += async (assetHandle) =>
                    {
                        TextAsset asset = assetHandle.AssetObject as TextAsset;
                        if (asset != null)
                        {
                            using (StringReader sr = new StringReader(asset.text))
                            {
                                string line = "";
                                while ((line = await sr.ReadLineAsync()) != null)
                                {
                                    var keyValue = line.Split('=', 2);
                                    if (keyValue.Length == 2)
                                    {
                                        addEntry(keyValue[0].Trim(), keyValue[1].Trim());
                                    }
                                }

                                sr.Close();
                            }
                            loadedCount++;
                            //立即释放资源
                            handle.Release();
                            if (loadedCount >= locations.Length)
                                tcs.TrySetResult();
                        }
                        else
                        {
                            Logger.Error($"Fail Load I18N {packageName} : {locations[index]}");
                            tcs.TrySetResult();
                        }
                    };
                }).Forget();
            }

            return tcs.Task;
        }

        /// <summary>
        /// 获取多语言翻译
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Get(string key)
        {
            if (!_allTexts.TryGetValue(key, out var value))
                value = $"MissingKey:{key}";
            return value;
        }
    }
}