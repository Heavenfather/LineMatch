using System.Collections.Generic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 引导路径组件
    /// 存储引导的路径信息和限制条件
    /// </summary>
    public struct GuidePathComponent
    {
        /// <summary>
        /// 引导路径坐标列表
        /// </summary>
        public List<Vector2Int> PathCoords;
        
        /// <summary>
        /// 引导高亮的元素ID列表（用于弱引导）
        /// </summary>
        public List<int> HighlightElementIds;
        
        /// <summary>
        /// 引导的目标元素ConfigId（用于强引导）
        /// </summary>
        public int TargetElementConfigId;
        
        /// <summary>
        /// 是否限制只能按引导路径操作
        /// </summary>
        public bool IsRestrictPath;
        
        /// <summary>
        /// 引导参数（从配置读取）
        /// </summary>
        public string GuideParameters;
        
        /// <summary>
        /// 引导参数2（从配置读取，用于下一步引导判断）
        /// </summary>
        public string GuideParameters2;
    }
}
