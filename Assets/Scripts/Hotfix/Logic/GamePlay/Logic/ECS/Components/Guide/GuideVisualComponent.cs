using DG.Tweening;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 引导视觉组件
    /// 存储引导相关的视觉元素引用和状态
    /// </summary>
    public struct GuideVisualComponent
    {
        /// <summary>
        /// 引导手指GameObject
        /// </summary>
        public GameObject GuideFinger;
        
        /// <summary>
        /// 引导相机
        /// </summary>
        public Camera GuideCamera;
        
        /// <summary>
        /// 引导关卡背景
        /// </summary>
        public SpriteRenderer GuideLevelBg;
        
        /// <summary>
        /// 引导关卡道具背景
        /// </summary>
        public SpriteRenderer GuideLevelItemBg;
        
        /// <summary>
        /// 手指动画Tween
        /// </summary>
        public Sequence FingerTween;
        
        /// <summary>
        /// 是否显示手指
        /// </summary>
        public bool IsFingerVisible;
        
        /// <summary>
        /// 引导图层名称
        /// </summary>
        public string GuideMaskLayerName;
    }
}
