using System;
using GameCore.Log;
using YooAsset;

namespace GameCore.Process
{
    internal sealed class AssetsLogger : ILogger
    {
        public void Log(string message)
        {
            Logger.Info(message);
        }

        public void Warning(string message)
        {
            Logger.Warning(message);
        }

        public void Error(string message)
        {
            Logger.Error(message);
        }

        public void Exception(Exception exception)
        {
            Logger.Fatal(exception.ToString());
        }
    }
}