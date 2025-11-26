using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 生成棋子请求
    /// </summary>
    public struct ProjectileRequestComponent
    {
        /// <summary>
        /// 棋子配置Id
        /// </summary>
        public int ConfigId;
        
        /// <summary>
        /// 棋子生成起始位置
        /// </summary>
        public Vector2Int StartPos;
    }
}