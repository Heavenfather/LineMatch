using System;
using UnityEngine;

// namespace HotfixCore.Extensions
// {
    public class ScrollHorizontalContent : ScrollExtensionContent
    {
        [Tooltip("Sets the spacing between elements in content.")]
        public float spacing;
        [Tooltip("Sets the padding area on the left side of a content.")]
        public float leftPadding;
        [Tooltip("Sets the padding area on the right side of a content.")]
        public float rightPadding;
        
        public override float FrontPadding { get => leftPadding; set => leftPadding = value; }
        public override float BackPadding { get => rightPadding; set => rightPadding = value; }
        public override float Spacing => spacing;
        
        private float _previousWidth = -1f;
        
        /// <summary>
        /// Sets rendered objects from left to right. For more info check base 
        /// <seealso cref="ScrollExtensionContent.Refresh"/>,
        /// </summary>
        public override void Refresh()
        {
            float totalWidth = _frontSize;
            for (int i = 0; i < ContentObjects.Count; i++)
            {
                RectTransform rect = GetScrollCellView(i).RTransform;
                rect.pivot = IsRevert ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f);
                rect.anchorMin = rect.anchorMax = IsRevert ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f);

                rect.anchoredPosition = new Vector2(IsRevert ? -totalWidth: totalWidth, 0);
                totalWidth += GetScrollCellView(i).RTransform.rect.width;
                totalWidth += spacing;

#if UNITY_EDITOR
                GetScrollCellView(i).RTransform.SetSiblingIndex(i);
#endif
            }
            
            totalWidth += rightPadding + _backSize;
            if (_backSize == 0)
                totalWidth -= spacing;
            
            // Do not rebuild rect when the size gonna be same for optimization purposes.
            if (Math.Abs(_previousWidth - totalWidth) < float.Epsilon)
                return;
            
            _previousWidth = totalWidth;
            // RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
        }
    }
// }