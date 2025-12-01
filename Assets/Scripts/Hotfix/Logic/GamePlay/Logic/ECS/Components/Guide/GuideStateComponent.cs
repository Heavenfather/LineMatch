using System.Collections.Generic;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 引导状态组件
    /// 存储当前引导的状态信息
    /// </summary>
    public struct GuideStateComponent
    {
        /// <summary>
        /// 是否正在引导中
        /// </summary>
        public bool IsGuiding;
        
        /// <summary>
        /// 是否在引导关卡中
        /// </summary>
        public bool IsGuideLevel;
        
        /// <summary>
        /// 当前引导ID
        /// </summary>
        public int CurrentGuideId;
        
        /// <summary>
        /// 引导类型：0=无，1=弱引导，2=强引导
        /// </summary>
        public GuideType GuideType;
        
        /// <summary>
        /// 是否强制方形消除
        /// </summary>
        public bool IsForceSquare;
        
        /// <summary>
        /// 方形消除计数（用于引导关卡统计）
        /// </summary>
        public int ForceSquareCount;
        
        /// <summary>
        /// 引导关卡步骤
        /// </summary>
        public int GuideLevelStep;
        
        /// <summary>
        /// 引导尝试次数（用于统计）
        /// </summary>
        public int GuideAttemptCount;
    }
    
    /// <summary>
    /// 引导类型枚举
    /// </summary>
    public enum GuideType
    {
        None = 0,
        Weak = 1,    // 弱引导：只高亮，不限制操作
        Force = 2,   // 强引导：显示手指，限制操作
    }
}
