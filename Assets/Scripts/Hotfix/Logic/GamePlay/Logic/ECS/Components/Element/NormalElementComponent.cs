using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public struct NormalElementComponent
    {
        /// <summary>
        /// 是否需要更新棋子颜色
        /// </summary>
        public bool IsColorDirty;
        public bool IsAnimDirty;
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
    }
}