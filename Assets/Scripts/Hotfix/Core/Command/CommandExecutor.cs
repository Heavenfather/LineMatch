using System.Collections.Generic;
using GameCore.Log;
using HotfixCore.MemoryPool;

namespace HotfixCore.Command
{
    public class CommandExecutor : IMemory
    {
        // 撤销栈（存储已执行命令）
        private Stack<ICommand> _undoStack = new Stack<ICommand>(200);

        // 重做栈（存储已撤销命令）
        private Stack<ICommand> _redoStack = new Stack<ICommand>(200);

        /// <summary>
        /// 执行新命令
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            // 执行命令
            command.Execute();

            // 压入撤销栈
            _undoStack.Push(command);

            // 清空重做栈（新命令会破坏重做链）
            _redoStack.Clear();
        }

        /// <summary>
        /// 撤销上一个操作
        /// </summary>
        public void Undo()
        {
            if (_undoStack.Count == 0)
            {
                Logger.Warning("无可撤销操作!");
                return;
            }

            // 从撤销栈取出命令
            var cmd = _undoStack.Pop();

            // 执行撤销
            cmd.Undo();

            // 存入重做栈
            _redoStack.Push(cmd);
        }

        /// <summary>
        /// 重做最后一个被撤销的操作
        /// </summary>
        public void Redo()
        {
            if (_redoStack.Count == 0)
            {
                Logger.Warning("无撤销重做操作!");
                return;
            }

            // 从重做栈取出命令
            var cmd = _redoStack.Pop();

            // 执行重做（通常与Execute相同）
            cmd.Redo();

            // 重新存入撤销栈
            _undoStack.Push(cmd);
        }

        /// <summary>
        /// 清空所有历史记录
        /// </summary>
        public void ClearHistory()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        public void Clear()
        {
            ClearHistory();
        }
    }
}