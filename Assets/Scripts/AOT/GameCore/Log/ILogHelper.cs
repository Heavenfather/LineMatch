namespace GameCore.Log
{
    /// <summary>
    /// 日志帮助类
    /// </summary>
    public interface ILogHelper
    {
        void Log(ELogLevel level, object msg);

        void SetLogLevel(ELogLevel logLevel);
    }
}