using GameConfig;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 掉落入场策略
    /// 棋子从上方掉落进入棋盘
    /// </summary>
    public class FallDownSpawnStrategy : IBoardSpawnStrategy
    {
        private const float DROP_HEIGHT_OFFSET = 10f; // 从上方 10 单位处掉落
        private const float DELAY_PER_ROW = 0.05f; // 每行延迟 0.05s

        private ElementMap _currentExeElement;

        public Vector3 GetStartWorldPosition(int x, int y, Vector3 targetWorldPos)
        {
            // 只有可移动的棋子才需要掉落
            if (_currentExeElement.isMovable == false)
                return targetWorldPos;
            
            // 初始位置在目标位置的正上方
            return targetWorldPos + new Vector3(0, DROP_HEIGHT_OFFSET, 0);
        }

        public float GetSpawnDelay(int x, int y)
        {
            // 简单的节奏：下方的先掉，上方的后掉
            return y * DELAY_PER_ROW;
        }

        public SpawnAnimType GetSpawnAnimType(in ElementMap config)
        {
            _currentExeElement = config;
            // 区分不同棋子，有的棋子可能直接入场，有的可能需要掉落
            if (config.isMovable)
                return SpawnAnimType.FallDown;
            return SpawnAnimType.None;
        }
    }
}