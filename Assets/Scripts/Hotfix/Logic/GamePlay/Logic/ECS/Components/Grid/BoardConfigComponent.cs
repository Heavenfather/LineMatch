namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 棋盘配置组件，存储棋盘整体信息
    /// </summary>
    public struct BoardConfigComponent
    {
        /// <summary>
        /// 棋盘行数
        /// </summary>
        public int GridRows;

        /// <summary>
        /// 棋盘列数
        /// </summary>
        public int GridColumns;

        /// <summary>
        /// 关卡数
        /// </summary>
        public int LevelId;
    }
}