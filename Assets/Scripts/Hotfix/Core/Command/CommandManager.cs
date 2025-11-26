using System.Collections.Generic;
using GameCore.Singleton;

namespace HotfixCore.Command
{
    /// <summary>
    /// 命令管理接口
    /// </summary>
    public class CommandManager : LazySingleton<CommandManager>
    {
        private readonly Dictionary<string,CommandExecutor> _commandExecutors = new Dictionary<string, CommandExecutor>();

        /// <summary>
        /// 获取或添加命令执行者
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="executor"></param>
        public void GetOrAddCommandExecutor(string commandName, out CommandExecutor executor)
        {
            if (!_commandExecutors.TryGetValue(commandName, out executor))
            {
                executor = MemoryPool.MemoryPool.Acquire<CommandExecutor>();
                _commandExecutors.Add(commandName, executor);
            }
        }

        /// <summary>
        /// 归还管理者
        /// </summary>
        /// <param name="commandName"></param>
        public void RecycleCommandExecutor(string commandName)
        {
            if (_commandExecutors.ContainsKey(commandName))
            {
                MemoryPool.MemoryPool.Release(_commandExecutors[commandName]);
                _commandExecutors.Remove(commandName);
            }
        }
    }
}