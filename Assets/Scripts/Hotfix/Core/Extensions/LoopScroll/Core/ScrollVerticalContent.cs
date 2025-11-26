using System;
using UnityEngine;

// namespace HotfixCore.Extensions
// {
    public class ScrollVerticalContent : ScrollExtensionContent
    {
        [Tooltip("Sets the spacing between elements in content.")]
        public float spacing;
        [Tooltip("Sets the padding area on the top side of a content.")]
        public float topPadding;
        [Tooltip("Sets the padding area on the bottom side of a content.")]
        public float bottomPadding;
        public override float FrontPadding { get => topPadding; set => topPadding = value; }
        public override float BackPadding { get => bottomPadding; set => bottomPadding = value; }
        public override float Spacing => spacing;
        
        private float _previousHeight = -1f;
        
        /// <summary>
        /// Sets rendered objects from top to bottom. For more info check base 
        /// <seealso cref="ScrollExtensionContent.Refresh"/>,
        /// </summary>
        public override void Refresh()
        {
            float totalHeight = _frontSize;
            for (int i = 0; i < ContentObjects.Count; i++)
            {
                RectTransform rect = GetScrollCellView(i).RTransform;
                rect.pivot = IsRevert ? new Vector2(0.5f, 0) : new Vector2(0.5f, 1);
                rect.anchorMin = rect.anchorMax = IsRevert ? new Vector2(0.5f, 0) : new Vector2(0.5f, 1);

                rect.anchoredPosition = new Vector2(0, IsRevert ? totalHeight : -totalHeight);
                totalHeight += GetScrollCellView(i).RTransform.rect.height;
                totalHeight += spacing;
                
                // Children order match their position in editor for debug purposes.
#if UNITY_EDITOR
                GetScrollCellView(i).RTransform.SetSiblingIndex(i);
#endif
            }
            
            totalHeight += bottomPadding + _backSize;
            if (_backSize == 0)
                totalHeight -= spacing;
            
            // Do not rebuild rect when the size gonna be same for optimization purposes.
            if (Math.Abs(_previousHeight - totalHeight) < float.Epsilon)
                return;
            
            _previousHeight = totalHeight;
            // RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
        }
    }
// }

