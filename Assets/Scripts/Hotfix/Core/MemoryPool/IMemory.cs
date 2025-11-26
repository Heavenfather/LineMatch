namespace HotfixCore.MemoryPool
{
    public interface IMemory
    {
        /// <summary>
        /// 清理内存对象回收入池。
        /// </summary>
        void Clear();
    }
}