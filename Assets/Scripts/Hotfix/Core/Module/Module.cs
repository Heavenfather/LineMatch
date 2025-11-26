using System;
using System.Collections.Generic;
using UnityEngine;

namespace HotfixCore.Module
{
    /// <summary>
    /// 游戏模块帮助类
    /// </summary>
    public static class Module
    {
        /// <summary>
        /// 注册模块信息
        /// </summary>
        private class ModuleWrapper
        {
            /// <summary>
            /// 模块优先级 优先级越高轮询时越先调用
            /// </summary>
            public int Priority { private set; get; }

            /// <summary>
            /// 注册模块
            /// </summary>
            public IModule Module { private set; get; }

            public ModuleWrapper(IModule module, int priority)
            {
                this.Priority = priority;
                this.Module = module;
            }
        }


        /// <summary>
        /// 入口对象
        /// </summary>
        private static MonoBehaviour _enterBehaviour;

        /// <summary>
        /// 已创建的模块
        /// </summary>
        private static List<ModuleWrapper> _createdModule = new List<ModuleWrapper>(100);

        /// <summary>
        /// 是否有新创建的模块
        /// </summary>
        private static bool _isDirty = false;

        /// <summary>
        /// 模块初始化
        /// </summary>
        /// <param name="behaviour"></param>
        /// <exception cref="Exception"></exception>
        public static void Initialize(MonoBehaviour behaviour)
        {
            if (_enterBehaviour != null)
                throw new Exception("Framework is already initialize!");

            _enterBehaviour = behaviour;
        }

        /// <summary>
        /// 各模块轮询更新
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位</param>
        public static void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (_isDirty)
            {
                _isDirty = false;
                _createdModule.Sort((a, b) =>
                {
                    if (a.Priority > b.Priority)
                        return -1;
                    else if (a.Priority == b.Priority)
                        return 0;
                    else
                        return 1;
                });
            }

            for (int i = 0; i < _createdModule.Count; i++)
            {
                if (_createdModule[i].Module is IModuleUpdate moduleUpdate)
                    moduleUpdate.Tick(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 创建模块
        /// </summary>
        /// <param name="priority"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateModule<T>(int priority = 0) where T : class, IModule
        {
            return CreateModule<T>(null, priority);
        }

        /// <summary>
        /// 创建模块
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="priority"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T CreateModule<T>(System.Object parameter, int priority = 0) where T : class, IModule
        {
            if (priority < 0)
                throw new Exception($"The priority can not be negative");
            if (Contains(typeof(T)))
                throw new Exception($"Game Module {nameof(T)} is already created");

            if (priority == 0)
            {
                int minPriority = GetMinPriority();
                priority = --minPriority;
            }

            T module = Activator.CreateInstance<T>();
            ModuleWrapper wrapper = new ModuleWrapper(module, priority);
            if (wrapper.Module is IModuleAwake moduleAwake)
                moduleAwake.Awake(parameter);
            _createdModule.Add(wrapper);
            _isDirty = true;
            return module;
        }

        /// <summary>
        /// 销毁所有模块
        /// </summary>
        public static void DestroyAll()
        {
            for (int i = 0; i < _createdModule.Count; i++)
            {
                if (_createdModule[i].Module is IModuleDestroy moduleDestroy)
                    moduleDestroy.Destroy();
            }

            _createdModule.Clear();
        }

        /// <summary>
        /// 销毁某个模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Destroy<T>()
        {
            var type = typeof(T);
            for (int i = 0; i < _createdModule.Count; i++)
            {
                if (_createdModule[i].Module.GetType() == type)
                {
                    if (_createdModule[i].Module is IModuleDestroy moduleDestroy)
                        moduleDestroy.Destroy();
                    _createdModule.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 获取已创建的模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetModule<T>() where T : class, IModule
        {
            for (int i = 0; i < _createdModule.Count; i++)
            {
                if (_createdModule[i].Module.GetType() == typeof(T))
                    return (T)_createdModule[i].Module;
            }

            return null;
        }

        /// <summary>
        /// 是否存在游戏模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Contains<T>() where T : class, IModule
        {
            return Contains(typeof(T));
        }

        /// <summary>
        /// 是否存在游戏模块
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool Contains(Type type)
        {
            for (int i = 0; i < _createdModule.Count; i++)
            {
                if (_createdModule[i].Module.GetType() == type)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 获取当前模块里最小的优先级
        /// </summary>
        private static int GetMinPriority()
        {
            int minPriority = 0;
            for (int i = 0; i < _createdModule.Count; i++)
            {
                if (_createdModule[i].Priority < minPriority)
                    minPriority = _createdModule[i].Priority;
            }

            return minPriority; //小于等于零
        }
    }
}