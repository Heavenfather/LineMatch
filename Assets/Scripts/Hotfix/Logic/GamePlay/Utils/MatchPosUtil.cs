using System.Collections.Generic;
using GameConfig;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public static class MatchPosUtil
    {
        //四方向检测
        public static readonly Vector2Int[] NeighborDirs = new[]
        {
            new Vector2Int(0, 1), // 上
            new Vector2Int(-1, 0), // 左
            new Vector2Int(1, 0), // 右
            new Vector2Int(0, -1), // 下
        };
        
        public static readonly Vector2Int[] EightNeighborDirs = new[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
            new Vector2Int(1, 1), // 右上
            new Vector2Int(1, -1), // 右下
            new Vector2Int(-1, 1), // 左上
            new Vector2Int(-1, -1) // 左下
        };

        /// <summary>
        /// 两个单元格是否相邻
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static bool IsNeighbor(int x1, int y1, int x2, int y2)
        {
            return (x1 == x2 - 1 && y1 == y2) ||
                   (x1 == x2 + 1 && y1 == y2) ||
                   (x1 == x2 && y1 == y2 - 1) ||
                   (x1 == x2 && y1 == y2 + 1);
        }


        /// <summary>
        /// 计算棋盘网格中的世界坐标位置
        /// </summary>
        /// <returns></returns>
        public static Vector3 CalculateWorldPosition(float x, float y, int elementWidth, int elementHeight,
            ElementDirection direction)
        {
            float cellSize = MatchElementUtil.GridSize.x;
            float centerX = x;
            float centerY = y;

            switch (direction)
            {
                case ElementDirection.Right:
                    centerX = x + (elementWidth - 1) / 2.0f;
                    break;
                case ElementDirection.Left:
                    centerX = x - (elementWidth - 1) / 2.0f;
                    break;
                case ElementDirection.Down:
                    centerY = y + (elementHeight - 1) / 2.0f;
                    break;
                case ElementDirection.Up:
                    centerY = y - (elementHeight - 1) / 2.0f;
                    break;
                case ElementDirection.None:
                    centerX = x + (elementWidth - 1) / 2.0f;
                    centerY = y + (elementHeight - 1) / 2.0f;
                    break;
            }

            var worldX = centerX * cellSize - 4.8f;
            var worldY = 5 - centerY * cellSize;

            return new Vector3(worldX, worldY, 0);
        }

        /// <summary>
        /// 获取棋盘网格中的八方向邻居位置
        /// </summary>
        /// <param name="gridPos"></param>
        /// <returns></returns>
        public static List<Vector2Int> GetEightNeighborPos(Vector2Int gridPos)
        {
            List<Vector2Int> neighbor = new List<Vector2Int>();
            for (int i = 0; i < EightNeighborDirs.Length; i++)
            {
                Vector2Int pos = gridPos + EightNeighborDirs[i];
                neighbor.Add(pos);
            }

            return neighbor;
        }
        
        // public static 
    }
}