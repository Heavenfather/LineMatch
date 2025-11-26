using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// ECS系统管理器，负责管理所有系统的生命周期
    /// </summary>
    public class EcsSystems : IEcsSystems
    {
        readonly EcsWorld _defaultWorld;
        readonly Dictionary<string, EcsWorld> _worlds;
        readonly List<IEcsSystem> _allSystems;
        readonly List<IEcsRunSystem> _runSystems;
        readonly List<IEcsPostRunSystem> _postRunSystems;

        readonly object _shared;

        /// <summary>
        /// 初始化系统管理器
        /// </summary>
        /// <param name="defaultWorld">默认ECS世界</param>
        /// <param name="shared">共享数据对象</param>
        public EcsSystems(EcsWorld defaultWorld, object shared = null)
        {
            _defaultWorld = defaultWorld;
            _shared = shared;
            _worlds = new Dictionary<string, EcsWorld>(8);
            _allSystems = new List<IEcsSystem>(128);
            _runSystems = new List<IEcsRunSystem>(128);
            _postRunSystems = new List<IEcsPostRunSystem>(128);
        }

        /// <summary>
        /// 获取共享数据
        /// </summary>
        /// <typeparam name="T">共享数据类型</typeparam>
        /// <returns>共享数据实例</returns>
        public virtual T GetShared<T>() where T : class
        {
            return _shared as T;
        }

        /// <summary>
        /// 添加世界到系统管理器
        /// </summary>
        /// <param name="world">要添加的ECS世界</param>
        /// <param name="name">世界名称</param>
        /// <returns>系统管理器实例（支持链式调用）</returns>
        public virtual IEcsSystems AddWorld(EcsWorld world, string name)
        {
#if DEBUG
            if (world == null)
            {
                throw new System.Exception("World cant be null.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new System.Exception("World name cant be null or empty.");
            }

            if (_worlds.ContainsKey(name))
            {
                throw new System.Exception($"World with name \"{name}\" already added.");
            }
#endif
            _worlds[name] = world;
            return this;
        }

        /// <summary>
        /// 根据名称获取世界
        /// </summary>
        /// <param name="name">世界名称，null表示默认世界</param>
        /// <returns>ECS世界实例</returns>
        public virtual EcsWorld GetWorld(string name = null)
        {
            if (name == null)
            {
                return _defaultWorld;
            }

            _worlds.TryGetValue(name, out var world);
            return world;
        }

        /// <summary>
        /// 获取所有命名世界的字典
        /// </summary>
        /// <returns>世界名称到世界实例的字典</returns>
        public virtual Dictionary<string, EcsWorld> GetAllNamedWorlds()
        {
            return _worlds;
        }

        /// <summary>
        /// 添加系统到管理器
        /// </summary>
        /// <param name="system">要添加的系统实例</param>
        /// <returns>系统管理器实例（支持链式调用）</returns>
        public virtual IEcsSystems Add(IEcsSystem system)
        {
            _allSystems.Add(system);
            if (system is IEcsRunSystem runSystem)
            {
                _runSystems.Add(runSystem);
            }

            if (system is IEcsPostRunSystem postRunSystem)
            {
                _postRunSystems.Add(postRunSystem);
            }

            return this;
        }

        /// <summary>
        /// 获取所有系统列表
        /// </summary>
        /// <returns>系统列表</returns>
        public virtual List<IEcsSystem> GetAllSystems()
        {
            return _allSystems;
        }

        /// <summary>
        /// 初始化所有系统
        /// </summary>
        public virtual void Init()
        {
            foreach (var system in _allSystems)
            {
                if (system is IEcsPreInitSystem initSystem)
                {
                    initSystem.PreInit(this);
                }
            }

            foreach (var system in _allSystems)
            {
                if (system is IEcsInitSystem initSystem)
                {
                    initSystem.Init(this);
                }
            }
        }

        /// <summary>
        /// 运行所有系统
        /// </summary>
        public virtual void Run()
        {
            for (int i = 0, iMax = _runSystems.Count; i < iMax; i++)
            {
                _runSystems[i].Run(this);
            }

            for (int i = 0, iMax = _postRunSystems.Count; i < iMax; i++)
            {
                _postRunSystems[i].PostRun(this);
            }
        }


        /// <summary>
        /// 销毁所有系统
        /// </summary>
        public virtual void Destroy()
        {
            for (var i = _allSystems.Count - 1; i >= 0; i--)
            {
                if (_allSystems[i] is IEcsDestroySystem destroySystem)
                {
                    destroySystem.Destroy(this);
                }
            }

            for (var i = _allSystems.Count - 1; i >= 0; i--)
            {
                if (_allSystems[i] is IEcsPostDestroySystem postDestroySystem)
                {
                    postDestroySystem.PostDestroy(this);
                }
            }

            _worlds.Clear();
            _allSystems.Clear();
            _runSystems.Clear();
            _postRunSystems.Clear();
        }
    }
}