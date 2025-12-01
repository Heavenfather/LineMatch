using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public struct NormalElementComponent : IEcsAutoReset<NormalElementComponent>
    {
        /// <summary>
        /// 是否需要更新棋子颜色
        /// </summary>
        public bool IsColorDirty;
        public bool IsAnimDirty;

        /// <summary>
        /// 是否被其他元素处理，如果被其它元素处理，必须要管理它的生命周期
        /// </summary>
        public bool IsOtherElementHandleThis;
        /// <summary>
        /// 棋子视图颜色
        /// </summary>
        public Color ElementColor;

        /// <summary>
        /// 棋子视图的闪图类型
        /// </summary>
        public NormalFlashIconAniType FlashIconAniType;
        public ElementScaleState ScaleState;

        public bool IsFlashIconDirty;
        public float FlashStartScale;
        public float FlashEndScale;
        public float FlashDuration;
        
        public void AutoReset(ref NormalElementComponent c)
        {
            IsOtherElementHandleThis = false;
        }
    }
}