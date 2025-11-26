
namespace GameCore.Log
{
    public static class Logger
    {
        private static ILogHelper _log;
        private static bool _enableDebug;

        /// <summary>
        /// 设置日志帮助类
        /// </summary>
        public static void SetLogHelper(ILogHelper log, ELogLevel logLevel,bool enableDebug = true)
        {
            _log = log;
            _log.SetLogLevel(logLevel);
            _enableDebug = enableDebug;
        }

        /// <summary>
        /// 调试级别的日志输出
        /// </summary>
        public static void Debug(object message)
        {
            if (_log == null || !_enableDebug)
                return;
            _log.Log(ELogLevel.Debug, message);
        }

        /// <summary>
        /// 调试级别的日志输出
        /// </summary>
        public static void Debug(string message)
        {
            if (_log == null || !_enableDebug)
                return;
            _log.Log(ELogLevel.Debug, message);
        }

        /// <summary>
        /// 调试级别的日志输出
        /// </summary>
        public static void DebugFormat(string format, params object[] args)
        {
            if (_log == null || !_enableDebug)
                return;
            _log.Log(ELogLevel.Debug, string.Format(format, args));
        }

        /// <summary>
        /// 重要日志点输出
        /// </summary>
        public static void Info(object message)
        {
            if (_log == null)
                return;
            _log.Log(ELogLevel.Info, message);
        }

        /// <summary>
        /// 重要日志点输出
        /// </summary>
        public static void Info(string message)
        {
            if (_log == null)
                return;
            _log.Log(ELogLevel.Info, message);
        }

        /// <summary>
        /// 重要日志点输出
        /// </summary>
        public static void InfoFormat(string format, params object[] args)
        {
            if (_log == null)
                return;
            _log.Log(ELogLevel.Info, string.Format(format, args));
        }

        /// <summary>
        /// 警告级别日志
        /// </summary>
        public static void Warning(object message)
        {
            if (_log == null)
                return;
            _log.Log(ELogLevel.Warning, message);
        }

        /// <summary>
        /// 警告级别日志
        /// </summary>
        public static void Warning(string message)
        {
            if (_log == null)
                return;
            _log.Log(ELogLevel.Warning, message);
        }

        /// <summary>
        /// 警告级别日志
        /// </summary>
        public static void WarningFormat(string format, params object[] args)
        {
            if (_log == null)
                return;
            _log.Log(ELogLevel.Warning, string.Format(format, args));
        }

        /// <summary>
        /// 错误级别日志
        /// </summary>
        public static void Error(object message)
        {
            if (_log == null)
                return;
            _log.Log(ELogLevel.Error, message);
        }

        /// <summary>
        /// 错误级别日志
        /// </summary>
        public static void Error(string message)
        {
            if (_log == null)
                return;
            _log.Log(ELogLevel.Error, message);
        }

        /// <summary>
        /// 错误级别日志
        /// </summary>
        public static void ErrorFormat(string format, params object[] args)
        {
            if (_log == null)
                return;
            _log.Log(ELogLevel.Error, string.Format(format, args));
        }

        /// <summary>
        /// 抛出异常级别日志
        /// </summary>
        public static void Fatal(object message)
        {
            if (_log == null)
                return;
            _log.Log(ELogLevel.Fatal, message);
        }

        /// <summary>
        /// 抛出异常级别日志
        /// </summary>
        public static void Fatal(string message)
        {
            if (_log == null)
                return;
            _log.Log(ELogLevel.Fatal, message);
        }

        /// <summary>
        /// 抛出异常级别日志
        /// </summary>
        public static void FatalFormat(string format, params object[] args)
        {
            if (_log == null)
                return;
            _log.Log(ELogLevel.Fatal, string.Format(format, args));
        }
    }
}