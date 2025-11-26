namespace HotfixCore.Module
{
    public interface IModule
    {
    }

    /// <summary>
    /// 当模块创建时
    /// </summary>
    public interface IModuleAwake : IModule
    {
        void Awake(System.Object parameter);
    }

    /// <summary>
    /// 轮询模块
    /// </summary>
    public interface IModuleUpdate : IModule
    {
        /// <summary>
        /// 每帧更新轮询
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位</param>
        void Tick(float elapseSeconds, float realElapseSeconds);
    }

    /// <summary>
    /// 当模块销毁时
    /// </summary>
    public interface IModuleDestroy : IModule
    {
        void Destroy();
    }
}