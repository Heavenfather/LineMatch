using System.Collections.Generic;
using GameConfig;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 棋盘视图配置
    /// </summary>
    public class BoardViewConfig : MonoBehaviour
    {
        public GameObject GridCellPrefab; // 普通格子
        public GameObject HolePrefab; // 镂空格子
        public LayerMask TouchHitMask; // 触摸检查层级
        [Header("------边缘装饰线 严格按枚举顺序添加-----")]
        public List<GameObject> LinePrefabs;

        public GameObject GetLinePrefab(MatchLineType type)
        {
            int index = (int)type;
            if (index >= 0 && index < LinePrefabs.Count)
                return LinePrefabs[index];
            return null;
        }
    }
}