#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;
using UnityEngine.UI;

namespace HotfixLogic
{
    public partial class widget_fillElement : UIWidget
    {
        private const float ICON_SIZE = 90.0f;
        private const float GRID_SPACE = 10.0f;

        private int _eleId;
        private int _targetId;
        private int _targetNum;
        private int _wid;
        private int _height;
        private Vector2Int _coord;
        private ElementMapDB _elementMapDB;

        public Vector2Int Coord => _coord;
        public int ElementId => _eleId;

        public override void OnCreate()
        {
            _elementMapDB = ConfigMemoryPool.Get<ElementMapDB>();
            text_count.text = "";
        }

        public void SetData(int elementId, Vector2Int coord)
        {
            _eleId = elementId;
            _coord = coord;
            RectTransform imgRect = img_icon.GetComponent<RectTransform>();
            if (_elementMapDB[elementId].elementType == ElementType.VerticalExpand ||
                _elementMapDB[elementId].elementType == ElementType.FixPosExpand)
            {
                ElementDirection direction = _elementMapDB[elementId].direction;
                if (direction == ElementDirection.Up)
                {
                    imgRect.pivot = new Vector2(0.5f, 0);
                    imgRect.anchorMin = new Vector2(0.5f, 0);
                    imgRect.anchorMax = new Vector2(0.5f, 0);
                }
                else if (direction == ElementDirection.Down)
                {
                    imgRect.pivot = new Vector2(0.5f, 1);
                    imgRect.anchorMin = new Vector2(0.5f, 1);
                    imgRect.anchorMax = new Vector2(0.5f, 1);
                }
                else if (direction == ElementDirection.Left)
                {
                    imgRect.pivot = new Vector2(1, 0.5f);
                    imgRect.anchorMin = new Vector2(1, 0.5f);
                    imgRect.anchorMax = new Vector2(1, 0.5f);
                }
                else if (direction == ElementDirection.Right)
                {
                    imgRect.pivot = new Vector2(0, 0.5f);
                    imgRect.anchorMin = new Vector2(0, 0.5f);
                    imgRect.anchorMax = new Vector2(0, 0.5f);
                }

                imgRect.anchoredPosition = Vector2.zero;
            }
            else
            {
                imgRect.pivot = new Vector2(0, 1);
                imgRect.anchorMin = Vector2.one * 0.5f;
                imgRect.anchorMax = Vector2.one * 0.5f;
                imgRect.anchoredPosition = new Vector2(-37, 37);
            }

            SetIcon(elementId).Forget();
        }

        public void RefreshIconSize(int targetId, int count, int width, int height)
        {
            _targetId = targetId;
            _targetNum = count;
            _wid = width;
            _height = height;

            ref readonly ElementMap config = ref _elementMapDB[_eleId];
            text_count.text = (config.eliminateStyle == EliminateStyle.Target)
                ? $"id: {targetId}\nnum: {count}"
                : "";
            if (config.elementType == ElementType.TargetBlock)
            {
                img_icon.rectTransform.sizeDelta = new Vector2(width * ICON_SIZE + ((width - 1) * GRID_SPACE),
                    height * ICON_SIZE + height * GRID_SPACE);
            }
            else if (config.elementType == ElementType.FixedGridTargetBlock)
            {
                img_icon.SetNativeSize();
            }
            else
            {
                ElementDirection direction = config.direction;
                if (direction == ElementDirection.Up || direction == ElementDirection.Down)
                {
                    img_icon.rectTransform.sizeDelta = new Vector2(width * ICON_SIZE + ((width - 1) * GRID_SPACE), height * ICON_SIZE + height * GRID_SPACE);
                }
                else if (direction == ElementDirection.Left || direction == ElementDirection.Right)
                {
                    img_icon.rectTransform.sizeDelta = new Vector2(width * ICON_SIZE + width * GRID_SPACE, height * ICON_SIZE + ((height - 1) * GRID_SPACE));
                }
            }
        }

        private async UniTask SetIcon(int id)
        {
            img_icon.sprite = await MatchEditorUtils.GetElementIcon(id);
            ElementMap config = _elementMapDB[_eleId];
            if(config.elementType == ElementType.FixedGridTargetBlock)
                img_icon.SetNativeSize();
        }
    }
}
#endif