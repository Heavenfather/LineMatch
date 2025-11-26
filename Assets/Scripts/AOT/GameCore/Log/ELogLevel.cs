namespace GameCore.Log
{
    public enum ELogLevel : byte
    {
        /// <summary>
        /// 调试日志输出
        /// </summary>
        Debug = 0,
        /// <summary>
        /// 重要日志点输出
        /// </summary>
        Info,
        /// <summary>
        /// 警告
        /// </summary>
        Warning,
        /// <summary>
        /// 错误
        /// </summary>
        Error,
        /// <summary>
        /// 致命级别的错误 会直接抛出异常
        /// </summary>
        Fatal
    }
}