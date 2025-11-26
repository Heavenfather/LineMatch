using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 元素入场组件
    /// 记录元素的入场信息
    /// </summary>
    public struct ElementSpawnComponent
    {
        /// <summary>
        /// 最终要去的目的地
        /// </summary>
        public Vector3 TargetWorldPosition;

        /// <summary>
        /// 动画延迟
        /// </summary>
        public float Delay;

        /// <summary>
        /// 入场动画类型
        /// </summary>
        public SpawnAnimType AnimType;
        
        /// <summary>
        /// 标记是否已做完入场动画
        /// </summary>
        public bool IsDirty;
    }
}