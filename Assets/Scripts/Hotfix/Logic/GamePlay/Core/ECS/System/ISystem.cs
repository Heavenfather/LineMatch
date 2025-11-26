using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// ECS系统基础接口，所有系统都应实现此接口
    /// </summary>
    public interface IEcsSystem
    {
    }

    /// <summary>
    /// 预初始化系统接口，在初始化之前执行
    /// </summary>
    public interface IEcsPreInitSystem : IEcsSystem
    {
        /// <summary>
        /// 预初始化方法，在系统初始化前调用
        /// </summary>
        /// <param name="systems">系统管理器实例</param>
        void PreInit(IEcsSystems systems);
    }

    /// <summary>
    /// 初始化系统接口，在系统启动时执行
    /// </summary>
    public interface IEcsInitSystem : IEcsSystem
    {
        /// <summary>
        /// 初始化方法，在系统启动时调用
        /// </summary>
        /// <param name="systems">系统管理器实例</param>
        void Init(IEcsSystems systems);
    }

    /// <summary>
    /// 运行系统接口，在每帧更新时执行
    /// </summary>
    public interface IEcsRunSystem : IEcsSystem
    {
        /// <summary>
        /// 运行方法，在每帧更新时调用
        /// </summary>
        /// <param name="systems">系统管理器实例</param>
        void Run(IEcsSystems systems);
    }

    /// <summary>
    /// 运行后系统接口，在每帧更新后执行
    /// </summary>
    public interface IEcsPostRunSystem : IEcsSystem
    {
        /// <summary>
        /// 运行后方法，在每帧更新后调用
        /// </summary>
        /// <param name="systems">系统管理器实例</param>
        void PostRun(IEcsSystems systems);
    }

    /// <summary>
    /// 销毁系统接口，在系统销毁时执行
    /// </summary>
    public interface IEcsDestroySystem : IEcsSystem
    {
        /// <summary>
        /// 销毁方法，在系统销毁时调用
        /// </summary>
        /// <param name="systems">系统管理器实例</param>
        void Destroy(IEcsSystems systems);
    }

    /// <summary>
    /// 销毁后系统接口，在系统销毁后执行
    /// </summary>
    public interface IEcsPostDestroySystem : IEcsSystem
    {
        /// <summary>
        /// 销毁后方法，在系统销毁后调用
        /// </summary>
        /// <param name="systems">系统管理器实例</param>
        void PostDestroy(IEcsSystems systems);
    }

    /// <summary>
    /// ECS系统管理器接口，用于管理和协调多个系统
    /// </summary>
    public interface IEcsSystems
    {
        /// <summary>
        /// 获取共享数据
        /// </summary>
        /// <typeparam name="T">共享数据类型</typeparam>
        /// <returns>共享数据实例</returns>
        T GetShared<T>() where T : class;

        /// <summary>
        /// 添加世界到系统管理器
        /// </summary>
        /// <param name="world">要添加的ECS世界</param>
        /// <param name="name">世界名称</param>
        /// <returns>系统管理器实例（支持链式调用）</returns>
        IEcsSystems AddWorld(EcsWorld world, string name);

        /// <summary>
        /// 根据名称获取世界
        /// </summary>
        /// <param name="name">世界名称，null表示默认世界</param>
        /// <returns>ECS世界实例</returns>
        EcsWorld GetWorld(string name = null);

        /// <summary>
        /// 获取所有命名世界的字典
        /// </summary>
        /// <returns>世界名称到世界实例的字典</returns>
        Dictionary<string, EcsWorld> GetAllNamedWorlds();

        /// <summary>
        /// 添加系统到管理器
        /// </summary>
        /// <param name="system">要添加的系统实例</param>
        /// <returns>系统管理器实例（支持链式调用）</returns>
        IEcsSystems Add(IEcsSystem system);

        /// <summary>
        /// 获取所有系统列表
        /// </summary>
        /// <returns>系统列表</returns>
        List<IEcsSystem> GetAllSystems();

        /// <summary>
        /// 初始化所有系统
        /// </summary>
        void Init();

        /// <summary>
        /// 运行所有系统
        /// </summary>
        void Run();

        /// <summary>
        /// 销毁所有系统
        /// </summary>
        void Destroy();
    }
}