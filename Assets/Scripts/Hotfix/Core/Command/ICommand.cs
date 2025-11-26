namespace HotfixCore.Command
{
    /// <summary>
    /// 命令接口
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 命令执行
        /// </summary>
        void Execute();
        
        /// <summary>
        /// 撤销
        /// </summary>
        void Undo();

        /// <summary>
        /// 撤销回做
        /// </summary>
        void Redo();
    }
} 