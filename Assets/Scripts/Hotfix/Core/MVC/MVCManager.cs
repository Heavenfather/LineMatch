using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Singleton;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.MVC
{
    public class MVCManager : MonoSingleton<MVCManager>
    {
        private readonly object _lock = new object();

        private readonly Dictionary<string, MVCModule> _modules = new Dictionary<string, MVCModule>(32);
        private readonly LinkedList<MVCModule> _activeModules = new LinkedList<MVCModule>();
        private readonly List<MVCModule> _gcBuffer = new List<MVCModule>(10);
        private MVCSettings _settings = new MVCSettings();

        private void Start()
        {
            StartCoroutine(AutoMemoryManagement());
        }

        /// <summary>
        /// 显示并激活模块
        /// </summary>
        /// <param name="mvcName">模块名称</param>
        /// <param name="sleepOther">是否隐藏其它模块</param>
        /// <param name="initData">模块初始化时传递的数据</param>
        /// <param name="callback">激活模块完成回调</param>
        public async UniTask ActiveModule(string mvcName, bool sleepOther = false,Action<bool> callback = null, params object[] initData)
        {
            var module = GetOrCreateModule(mvcName);
            try
            {
                await ExecuteModuleActivation(module, initData);
                await ProcessActivationPolicy(module, sleepOther);
                callback?.Invoke(true);
            }
            catch (Exception e)
            {
                Logger.Error($"Open module [{mvcName}] error: {e}");
                UnloadModule(mvcName);
                callback?.Invoke(false);
            }
        }

        /// <summary>
        /// 获取当前已打开的模块,包括睡眠中
        /// </summary>
        /// <param name="mvcName"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public bool TryGetActiveModule(string mvcName, out MVCModule module)
        {
            lock (_lock)
            {
                foreach (var m in _activeModules)
                {
                    if (m.ModuleDefine.MVCName == mvcName)
                    {
                        module = m;
                        return true;
                    }
                }

                module = null;
                return false;
            }
        }

        /// <summary>
        /// 睡眠（隐藏）其它模块
        /// </summary>
        /// <param name="mvcName"></param>
        public void SleepModule(string mvcName)
        {
            if (TryGetActiveModule(mvcName, out var module))
            {
                module.Sleep().Forget();
            }
        }

        /// <summary>
        /// 关闭当前模块
        /// </summary>
        public void UnloadCurrentModule()
        {
            var current = _activeModules.First;
            if(current == null)
                return;
            if(current.Value.ModuleDefine.NeverAutoCollect)
                return;
            current.Value.Unload();
            _modules.Remove(current.Value.ModuleDefine.MVCName);
            _activeModules.Remove(current);
        }
        
        /// <summary>
        /// 关闭指定模块
        /// </summary>
        /// <param name="mvcName"></param>
        public void UnloadModule(string mvcName)
        {
            if (TryGetActiveModule(mvcName, out var module))
            {
                if (!module.ModuleDefine.NeverAutoCollect)
                {
                    module.Unload();
                    _modules.Remove(mvcName);
                    _activeModules.Remove(module);
                }
            }
        }

        private async UniTask ProcessActivationPolicy(MVCModule target, bool sleepOther)
        {
            var toDeactivate = new List<MVCModule>(4);
            lock (_lock)
            {
                if (sleepOther)
                {
                    var current = _activeModules.First;
                    while (current != null)
                    {
                        var next = current.Next;
                        if (current.Value != target)
                        {
                            toDeactivate.Add(current.Value);
                            _activeModules.Remove(current);
                        }

                        current = next;
                    }
                }
            }

            // 并行停用模块
            var deactivateTasks = new UniTask[toDeactivate.Count];
            for (int i = 0; i < toDeactivate.Count; i++)
            {
                deactivateTasks[i] = toDeactivate[i].Sleep();
            }

            await UniTask.WhenAll(deactivateTasks);
        }

        private async UniTask ExecuteModuleActivation(MVCModule module,  params object[] initData)
        {
            lock (_lock)
            {
                if (!_activeModules.Contains(module))
                    _activeModules.AddLast(module);
            }
            await module.Activate(initData);
        }

        private MVCModule GetOrCreateModule(string mvcName)
        {
            if (_modules.TryGetValue(mvcName, out var module))
                return module;
            module = new MVCModule(MVCConfig.GetMVCDefine(mvcName));
            lock (_lock)
            {
                _modules[mvcName] = module;
            }

            return module;
        }

        private IEnumerator AutoMemoryManagement()
        {
            while (!IsDestroyed)
            {
                yield return new WaitForSecondsRealtime(_settings.gcInterval);
                PerformGarbageCollection();
            }
        }

        private void PerformGarbageCollection()
        {
            lock (_lock)
            {
                _gcBuffer.Clear();
                foreach (var pair in _modules)
                {
                    if (pair.Value.ModuleDefine.NeverAutoCollect)
                        continue;
                    //模块长时间处于休眠状态自动释放掉
                    if (pair.Value.State == MVCModule.ModuleState.Sleeping &&
                        (DateTime.Now - pair.Value.LastActiveTime).TotalSeconds > 300)
                    {
                        _gcBuffer.Add(pair.Value);
                    }
                }

                // LRU淘汰算法
                _gcBuffer.Sort((a, b) => a.LastActiveTime.CompareTo(b.LastActiveTime));
                var removeCount = _modules.Count - _settings.maxModuleCache;
                if (removeCount <= 0) return;

                for (int i = 0; i < removeCount && i < _gcBuffer.Count; i++)
                {
                    var module = _gcBuffer[i];
                    module.Unload();
                    _modules.Remove(module.ModuleDefine.MVCName);
                }
            }
        }
    }
}