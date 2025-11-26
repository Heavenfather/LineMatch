using System;
using System.Collections.Generic;
using System.Reflection;
using GameCore.Localization;
using GameCore.Logic;
using GameCore.Settings;
using HybridCLR;
using UnityEngine;
using YooAsset;
using Logger = GameCore.Log.Logger;

namespace GameCore.Process
{
    /// <summary>
    /// 加载热更代码
    /// </summary>
    public class ProcessLoadAssembly : IProcess
    {
        private int _loadAssetCount;
        private int _loadedReferenceCount;
        private int _loadMetadataAssetCount;
        private int _failureAssetCount;
        private int _failureMetadataAssetCount;
        private bool _loadAssemblyComplete;
        private bool _loadMetadataAssemblyComplete;
        private bool _loadAssemblyWait;
        private bool _loadMetadataAssemblyWait;
        private bool _loadAssemblyFailTag;
        private const string _hotfixAssemblyName = "GameHotfix.dll";
        private Assembly _mainLogicAssembly;
        private List<Assembly> _hotfixAssemblyList;
        private GlobalSettings _setting;


        public void Init()
        {
            _setting = GameSettings.Instance.ProjectSetting;
        }

        public void Enter()
        {
            Logger.Debug("Hybrid CLR Load Assembly");
            HotUpdateManager.Instance.UpdateProgress(EUpdateState.LoadAssembly);
            _loadAssetCount = _setting.HotUpdateAssemblies.Count + _setting.HotUpdateReferenceAssemblies.Count;
            _loadedReferenceCount = _setting.HotUpdateReferenceAssemblies.Count;
            LoadAssembly();
        }

        public void Leave()
        {
        }

        public void Update()
        {
            if (!_loadAssemblyComplete)
                return;
            if (!_loadMetadataAssemblyComplete)
                return;

            AllAssemblyLoadComplete();
        }
        
        private void AllAssemblyLoadComplete()
        {
            ProcessManager.Instance.Enter<ProcessStartGame>();
#if UNITY_EDITOR
            _mainLogicAssembly = GetMainLogicAssembly();
#endif
            if (_mainLogicAssembly == null)
            {
                Logger.Fatal($"Main logic assembly missing.");
                return;
            }
            
            var appType = _mainLogicAssembly.GetType("HotFixStart");
            if (appType == null)
            {
                Logger.Fatal($"Main logic type 'HotFixStart' missing.");
                return;
            }
            var entryMethod = appType.GetMethod("Entry");
            if (entryMethod == null)
            {
                Logger.Fatal($"HotFixStart logic entry method 'Entry' missing.");
                return;
            }
            entryMethod.Invoke(appType, null);
        }
        
        private void LoadAssembly()
        {
            _loadAssemblyComplete = false;
            _hotfixAssemblyList = new List<Assembly>();

            //AOT Assembly加载原始metadata
#if !UNITY_EDITOR
                _loadMetadataAssemblyComplete = false;
                LoadMetadataForAOTAssembly();
#else
            _loadMetadataAssemblyComplete = true;
#endif

            if (_setting.PlayMode == EPlayMode.EditorSimulateMode)
            {
                _mainLogicAssembly = GetMainLogicAssembly();
                _loadAssemblyComplete = true;
            }
            else
            {
                //优先加载引用依赖程序集
                foreach (string referenceDllName in _setting.HotUpdateReferenceAssemblies)
                {
                    var assetLocation = $"dll/{referenceDllName}";
                    Logger.Debug($"LoadReferenceDLLAsset: [ {assetLocation} ]");
                    var handle = YooAssets.LoadAssetAsync<TextAsset>(assetLocation.ToLower());
                    handle.Completed += LoadReferenceAssemblyComplete;
                }

                _loadAssemblyWait = true;
            }
        }

        private void LoadReferenceAssemblyComplete(AssetHandle handle)
        {
            if (handle.Status != EOperationStatus.Succeed)
            {
                Logger.Error($"Failed to load reference assembly: {handle.LastError}");
                return;
            }

            _loadAssetCount--;
            _loadedReferenceCount--;

            try
            {
                TextAsset textAsset = (TextAsset)handle.AssetObject;
                var assembly = Assembly.Load(textAsset.bytes);
                _hotfixAssemblyList.Add(assembly);
                Logger.Info($"Assembly [ {assembly.GetName().Name} ] loaded");
            }
            catch (Exception e)
            {
                _failureAssetCount++;
                Logger.Error($"Load Reference Assembly failed: {e.Message}");
                LoadAssemblyFail();
            }
            finally
            {
                if (_loadedReferenceCount == 0)
                {
                    //开始加载热更程序集
                    foreach (string referenceDllName in _setting.HotUpdateAssemblies)
                    {
                        var assetLocation = $"dll/{referenceDllName}";
                        Logger.Debug($"LoadHotFixAsset: [ {assetLocation} ]");
                        var hotfixHandle = YooAssets.LoadAssetAsync<TextAsset>(assetLocation.ToLower());
                        hotfixHandle.Completed += LoadAssetAssemblyComplete;
                    }
                }
            }

            handle.Release();
        }

        private void LoadAssetAssemblyComplete(AssetHandle handle)
        {
            if (handle.Status != EOperationStatus.Succeed)
            {
                Logger.Error($"Failed to load hotfix assembly: {handle.LastError}");
                return;
            }

            _loadAssetCount--;
            try
            {
                TextAsset textAsset = (TextAsset)handle.AssetObject;
                var assembly = Assembly.Load(textAsset.bytes);
                if (string.Compare(_hotfixAssemblyName, textAsset.name, StringComparison.Ordinal) == 0)
                {
                    _mainLogicAssembly = assembly;
                }

                _hotfixAssemblyList.Add(assembly);
                Logger.Info($"Assembly [ {assembly.GetName().Name} ] loaded");
                _loadAssemblyComplete = _loadAssemblyWait && 0 == _loadedReferenceCount && 0 == _loadAssetCount;
            }
            catch (Exception e)
            {
                _failureAssetCount++;
                Logger.Error($"Load Asset Assembly fail : {e.Message}");
                LoadAssemblyFail();
            }
            finally
            {
                handle.Release();
            }
        }

        private Assembly GetMainLogicAssembly()
        {
            _hotfixAssemblyList.Clear();
            Assembly mainLogicAssembly = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.Compare(_hotfixAssemblyName, $"{assembly.GetName().Name}.dll",
                        StringComparison.Ordinal) == 0)
                {
                    mainLogicAssembly = assembly;
                }

                foreach (var hotUpdateDllName in _setting.HotUpdateReferenceAssemblies)
                {
                    if (hotUpdateDllName == $"{assembly.GetName().Name}.dll")
                    {
                        _hotfixAssemblyList.Add(assembly);
                    }
                }

                foreach (var hotUpdateDllName in _setting.HotUpdateAssemblies)
                {
                    if (hotUpdateDllName == $"{assembly.GetName().Name}.dll")
                    {
                        _hotfixAssemblyList.Add(assembly);
                    }
                }

                if (mainLogicAssembly != null && _hotfixAssemblyList.Count ==
                    _setting.HotUpdateAssemblies.Count + _setting.HotUpdateReferenceAssemblies.Count)
                {
                    break;
                }
            }

            return mainLogicAssembly;
        }

        /// <summary>
        /// 为Aot Assembly加载原始metadata， 这个代码放Aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行。
        /// </summary>
        private void LoadMetadataForAOTAssembly()
        {
            // 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
            // 我们在BuildProcessor_xxx里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。

            // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            if (_setting.AOTMetaAssemblies.Count == 0)
            {
                _loadMetadataAssemblyComplete = true;
                return;
            }

            foreach (string aotDllName in _setting.AOTMetaAssemblies)
            {
                var assetLocation = $"dll/{aotDllName}";
                Logger.Debug($"LoadMetadataAsset: [ {assetLocation} ]");
                _loadMetadataAssetCount++;
                var handle = YooAssets.LoadAssetAsync<TextAsset>(assetLocation.ToLower());
                handle.Completed += LoadMetadataAssetSuccess;
            }

            _loadMetadataAssemblyWait = true;
        }

        private void LoadMetadataAssetSuccess(AssetHandle handle)
        {
            if (handle.Status != EOperationStatus.Succeed)
            {
                Logger.Error($"LoadMetadataAssetSuccess:Load Metadata failed.: {handle.LastError}");
                return;
            }

            _loadMetadataAssetCount--;
            TextAsset textAsset = (TextAsset)handle.AssetObject;
            Logger.Info($"LoadMetadataAssetSuccess, assetName: [ {textAsset.name} ]");
            try
            {
                byte[] dllBytes = textAsset.bytes;
                // 加载assembly对应的dll，会自动为它hook。一旦Aot泛型函数的native函数不存在，用解释器版本代码
                HomologousImageMode mode = HomologousImageMode.SuperSet;
                LoadImageErrorCode err =
                    (LoadImageErrorCode)HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Logger.Warning($"LoadMetadataForAOTAssembly:{textAsset.name}. mode:{mode} ret:{err}");
                _loadMetadataAssemblyComplete = _loadMetadataAssemblyWait && 0 == _loadMetadataAssetCount;
            }
            catch (Exception e)
            {
                _failureMetadataAssetCount++;
                Logger.Error($"Load MetadataAsset Fail : {e.Message}");
                LoadAssemblyFail();
            }
            finally
            {
                handle.Release();
            }
        }

        private void LoadAssemblyFail()
        {
            if(_loadAssemblyFailTag)
                return;
            _loadAssemblyFailTag = true;
            HotUpdateManager.Instance.ShowRetryInfo(LocalizationPool.Get("LoadAssemblyFail"), () =>
            {
                _loadAssemblyFailTag = false;
                HotUpdateManager.Instance.UpdateProgress(EUpdateState.RetryState);
                //可以考虑回到资源预加载流程或者更早之前 TODO...
                this.Enter();
            });
        }
    }
}