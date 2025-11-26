using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
#endif

namespace GameCore.Log
{
    /// <summary>
    /// 游戏默认日志输出
    /// </summary>
    public class DefaultLogHelper : ILogHelper
    {
        private ELogLevel _logLevel = ELogLevel.Info;
        private StreamWriter _streamWriter;
        private FileStream _fileStream;

        public void SetLogLevel(ELogLevel logLevel)
        {
            _logLevel = logLevel;
            PrepareLogFile();
        }

        public void Log(ELogLevel level, object msg)
        {
            switch (level)
            {
                case ELogLevel.Debug:
                    LogImpl(ELogLevel.Debug, msg);
                    break;
                case ELogLevel.Info:
                    LogImpl(ELogLevel.Info, msg);
                    break;
                case ELogLevel.Warning:
                    LogImpl(ELogLevel.Warning, msg);
                    break;
                case ELogLevel.Error:
                    LogImpl(ELogLevel.Error, msg);
                    break;
                case ELogLevel.Fatal:
                    LogImpl(ELogLevel.Fatal, msg);
                    break;
            }
        }

        private void LogImpl(ELogLevel level, object msg)
        {
            if (level < _logLevel)
                return;
            string logStr = FormatLog(msg, level);
            //获取C#堆栈,Warning以上级别日志才获取堆栈
            if (level == ELogLevel.Error || level == ELogLevel.Warning || level == ELogLevel.Fatal)
            {
                StackFrame[] stackFrames = new StackTrace().GetFrames();
                // ReSharper disable once PossibleNullReferenceException
                for (int i = 0; i < stackFrames.Length; i++)
                {
                    StackFrame frame = stackFrames[i];
                    // ReSharper disable once PossibleNullReferenceException
                    string declaringTypeName = frame.GetMethod().DeclaringType.FullName;
                    string methodName = stackFrames[i].GetMethod().Name;

                    logStr = $"{logStr}\n{declaringTypeName}::{methodName}";
                }
            }

            switch (level)
            {
                case ELogLevel.Debug:
                case ELogLevel.Info:
                    UnityEngine.Debug.Log(logStr);
                    break;
                case ELogLevel.Warning:
                    UnityEngine.Debug.LogWarning(logStr);
                    break;
                case ELogLevel.Error:
                    UnityEngine.Debug.LogError(logStr);
                    break;
                case ELogLevel.Fatal:
                    throw new Exception(logStr);
            }
        }

        private string FormatLog(object msg, ELogLevel level)
        {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {msg.ToString()}";
        }

        private void PrepareLogFile()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            if (_streamWriter != null)
            {
                return;
            }

            string logDir = $"{Application.dataPath}/../gamelogs/";
#if UNITY_EDITOR
            logDir = $"{Application.dataPath}/../gamelogs/";
#endif
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
            string[] logFiles = Directory.GetFiles(logDir);
            List<string> sortedLogName = new List<string>(logFiles);
            sortedLogName.Sort((a, b) =>
            {
                //根据文件名的时间进行排序,时间升序
                string aFileName = Path.GetFileNameWithoutExtension(a);
                string bFileName = Path.GetFileNameWithoutExtension(b);
                return long.Parse(aFileName).CompareTo(long.Parse(bFileName));
            });

            DateTime threeDaysAgo = DateTime.Now.AddDays(-3);
            for (int i = 0; i < sortedLogName.Count; i++)
            {
                DateTime dateTime = DateTime.ParseExact(Path.GetFileNameWithoutExtension(sortedLogName[i]),
                    "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                if (dateTime < threeDaysAgo)
                    File.Delete(sortedLogName[i]);
            }

            string logFile = $"{logDir}{DateTime.Now:yyyyMMddHHmmss}.log";
            _fileStream = new FileStream(logFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _streamWriter = new StreamWriter(_fileStream, System.Text.Encoding.UTF8);

            Application.logMessageReceivedThreaded += OnUnityLog;
#endif
        }

        private void OnUnityLog(string condition, string stacktrace, LogType type)
        {
            if (_streamWriter != null)
            {
                _streamWriter.WriteLine(
                    $"{condition} {stacktrace}");
                _streamWriter.Flush();
            }
        }
    }


#if UNITY_EDITOR
    namespace TEngine.Editor
    {
        /// <summary>
        /// 日志重定向相关的实用函数。
        /// </summary>
        internal static class LogRedirection
        {
            [OnOpenAsset(0)]
            private static bool OnOpenAsset(int instanceID, int line)
            {
                if (line <= 0)
                {
                    return false;
                }

                // 获取资源路径
                string assetPath = AssetDatabase.GetAssetPath(instanceID);

                // 判断资源类型
                if (!assetPath.EndsWith(".cs"))
                {
                    return false;
                }

                bool autoFirstMatch = assetPath.Contains("DefaultLogHelper.cs") ||
                                      assetPath.Contains("AssetsLogger.cs") ||
                                      assetPath.Contains("Logger.cs");

                var stackTrace = GetStackTrace();
                if (!string.IsNullOrEmpty(stackTrace) && (stackTrace.Contains("[Debug]") ||
                                                          stackTrace.Contains("[Info]") ||
                                                          stackTrace.Contains("[Warning]") ||
                                                          stackTrace.Contains("[Error]") ||
                                                          stackTrace.Contains("[Fatal]")))

                {
                    if (!autoFirstMatch)
                    {
                        var fullPath = UnityEngine.Application.dataPath.Substring(0,
                            UnityEngine.Application.dataPath.LastIndexOf("Assets", StringComparison.Ordinal));
                        fullPath = $"{fullPath}{assetPath}";
                        // 跳转到目标代码的特定行
                        InternalEditorUtility.OpenFileAtLineExternal(fullPath.Replace('/', '\\'), line);
                        return true;
                    }

                    // 使用正则表达式匹配at的哪个脚本的哪一行
                    var matches = Regex.Match(stackTrace, @"\(at (.+)\)",
                        RegexOptions.IgnoreCase);
                    while (matches.Success)
                    {
                        var pathLine = matches.Groups[1].Value;

                        if (!pathLine.Contains("DefaultLogHelper.cs") &&
                            !pathLine.Contains("AssetsLogger.cs") &&
                            !pathLine.Contains("Logger.cs"))
                        {
                            var splitIndex = pathLine.LastIndexOf(":", StringComparison.Ordinal);
                            // 脚本路径
                            var path = pathLine.Substring(0, splitIndex);
                            // 行号
                            line = Convert.ToInt32(pathLine.Substring(splitIndex + 1));
                            var fullPath = UnityEngine.Application.dataPath.Substring(0,
                                UnityEngine.Application.dataPath.LastIndexOf("Assets", StringComparison.Ordinal));
                            fullPath = $"{fullPath}{path}";
                            // 跳转到目标代码的特定行
                            InternalEditorUtility.OpenFileAtLineExternal(fullPath.Replace('/', '\\'), line);
                            break;
                        }

                        matches = matches.NextMatch();
                    }

                    return true;
                }

                return false;
            }

            /// <summary>
            /// 获取当前日志窗口选中的日志的堆栈信息。
            /// </summary>
            /// <returns>选中日志的堆栈信息实例。</returns>
            private static string GetStackTrace()
            {
                // 通过反射获取ConsoleWindow类
                var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
                // 获取窗口实例
                var fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow",
                    BindingFlags.Static |
                    BindingFlags.NonPublic);
                if (fieldInfo != null)
                {
                    var consoleInstance = fieldInfo.GetValue(null);
                    if (consoleInstance != null)
                        if (EditorWindow.focusedWindow == (EditorWindow)consoleInstance)
                        {
                            // 获取m_ActiveText成员
                            fieldInfo = consoleWindowType.GetField("m_ActiveText",
                                BindingFlags.Instance |
                                BindingFlags.NonPublic);
                            // 获取m_ActiveText的值
                            if (fieldInfo != null)
                            {
                                var activeText = fieldInfo.GetValue(consoleInstance).ToString();
                                return activeText;
                            }
                        }
                }

                return null;
            }
        }
    }
#endif
}