using GameConfig;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 抽象棋盘生成的策略模式
    /// 后续可方便拓展进入棋盘时，棋子以何种形式出现
    /// </summary>
    public interface IBoardSpawnStrategy
    {
        /// <summary>
        /// 计算棋子的初始世界坐标
        /// </summary>
        Vector3 GetStartWorldPosition(int x, int y, Vector3 targetWorldPos);

        /// <summary>
        /// 获取入场动画的延迟时间 (实现交错掉落等节奏感)
        /// </summary>
        float GetSpawnDelay(int x, int y);

        /// <summary>
        /// 获取入场动画类型
        /// </summary>
        SpawnAnimType GetSpawnAnimType(in ElementMap config);
    }
}