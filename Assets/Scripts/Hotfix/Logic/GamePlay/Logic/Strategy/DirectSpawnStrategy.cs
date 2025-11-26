using GameConfig;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 直接入场策略
    /// 棋子直接出现在目标位置
    /// </summary>
    public class DirectSpawnStrategy : IBoardSpawnStrategy
    {
        public Vector3 GetStartWorldPosition(int x, int y, Vector3 targetWorldPos)
        {
            return targetWorldPos; // 初始位置就是目标位置
        }

        public float GetSpawnDelay(int x, int y) => 0f;

        public SpawnAnimType GetSpawnAnimType(in ElementMap config) => SpawnAnimType.None;
    }
}