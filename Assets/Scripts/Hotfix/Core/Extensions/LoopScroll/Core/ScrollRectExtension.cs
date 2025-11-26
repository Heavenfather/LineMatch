using System;
using System.Collections.Generic;
using HotfixCore.Module;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// namespace HotfixCore.Extensions
// {
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectExtension : MonoBehaviour, IScrollHandler, IBeginDragHandler, IEndDragHandler
    {
        /// <summary>
        /// Possible alignments of elements. Used into jump to element feature.  
        /// </summary>
        public enum Alignment
        {
            TopOrLeft,
            Center,
            BottomOrRight,
        }

        /// <summary>
        /// Possible directions of scroll. 
        /// </summary>
        public enum Direction
        {
            Vertical,
            Horizontal,
        }

        private ScrollRect scrollRect;

        [SerializeField, Tooltip("default template")]
        private GameObject tempGo;

        [SerializeField, Tooltip("revert scroll")]
        private bool isRevert;

        public bool IsRevert => isRevert;

        private Action<Vector2> _onChangePosAction;
        private Action<PointerEventData> _onBeginDragAction;
        private Action<PointerEventData> _onEndDragAction;

        private ScrollExtensionContent scrollContent;

        private Direction direction
        {
            get
            {
                if (ScrollContent is ScrollHorizontalContent)
                {
                    return Direction.Horizontal;
                }

                return Direction.Vertical;
            }
        }

        /// <summary>
        /// Exposed scroll content component.
        /// </summary>
        public ScrollExtensionContent ScrollContent
        {
            get
            {
                if (ScrollRect == null)
                {
                    Debug.LogError($"[Scroll Rect] component is null.");
                }

                if (scrollContent != null) return scrollContent;
                
                scrollContent = ScrollRect.content.GetComponent<ScrollExtensionContent>();
                if (scrollContent == null)
                {
                    Debug.LogError("[ScrollExtensionContent] component is null.");
                }
                return scrollContent;
            }
        }

        /// <summary>
        /// Exposed scroll rect component.
        /// </summary>
        public ScrollRect ScrollRect
        {
            get
            {
                if (scrollRect == null)
                {
                    scrollRect = this.GetComponent<ScrollRect>();
                }

                return scrollRect;
            }
        }
        
        /// <summary>
        /// Exposed spacing from <seealso cref="ScrollVerticalContent.spacing"/>,
        /// changing it directly will not cause reloading of existing view.
        /// </summary>
        public float Spacing
        {
            get => ScrollContent.Spacing;
            set
            {
                ScrollContent.Spacing = value;
                _needToReload = true;
            }
        }

        /// <summary>
        /// Exposed top padding from <seealso cref="ScrollVerticalContent.topPadding"/>,
        /// changing it directly will not cause reloading of existing view.
        /// </summary>
        public float PaddingTop
        {
            get => ScrollContent.FrontPadding;
            set
            {
                ScrollContent.FrontPadding = value;
                _needToReload = true;
            }
        }

        /// <summary>
        /// Exposed bottom padding from <seealso cref="ScrollVerticalContent.bottomPadding"/>,
        /// changing it directly will not cause reloading of existing view.
        /// </summary>
        public float PaddingBottom
        {
            get => ScrollContent.BackPadding;
            set
            {
                ScrollContent.BackPadding = value;
                _needToReload = true;
            }
        }

        /// <summary>
        /// Full size of content.
        /// </summary>
        public float ContentSize => Vertical
            ? ScrollContent.RTransform.rect.height
            : ScrollContent.RTransform.rect.width;

        /// <summary>
        /// The absolute position in pixels from the start of the scroller.
        /// </summary>
        public float ScrollPosition
        {
            get
            {
                if (Horizontal)
                    return (IsRevert ? (1f - NormalizedScrollPosition) : NormalizedScrollPosition) * ScrollSize;
                
                return (IsRevert ? NormalizedScrollPosition : (1f - NormalizedScrollPosition)) * ScrollSize;
            }
            set
            {
                if (0 < _cellViewSizeArray.Count)
                {
                    var min = -ScrollRectSize;
                    var max = GetScrollPositionFromIndex(_cellViewSizeArray.Count - 1) + ScrollRectSize;
                    if (value < min || max < value)
                    {
                        value = Mathf.Clamp(value, min, max);
                        ScrollRect.velocity = Vector2.zero;
                    }
                }

                if (0.01f < Mathf.Abs(ScrollPosition - value))
                {
                    _needToRefresh = true;
                    NormalizedScrollPosition = value / ScrollSize;
                }
            }
        }

        /// <summary>
        /// Normalized scroll position.
        /// </summary>
        private float NormalizedScrollPosition
        {
            get => direction == Direction.Vertical
                ? scrollRect.verticalNormalizedPosition
                : scrollRect.horizontalNormalizedPosition;

            set
            {
                if (direction == Direction.Vertical)
                    scrollRect.verticalNormalizedPosition = IsRevert ? value : (1f - value);
                else
                    scrollRect.horizontalNormalizedPosition = IsRevert ? (1f - value) : value;
            }
        }

        public int DataCount
        {
            get
            {
                var total = _getDataCount?.Invoke() ?? 0;
                if (total <= 0) return 0;
                if (_isTempList && this.tempGo != null)
                {
                    var listCount = this.tempGo.transform.childCount;
                    int tempCount = Mathf.CeilToInt(total / (float) listCount);
                    return tempCount;
                }

                return total;
            }
        }

        /// <summary>
        /// Full size of visible part of scroll.
        /// </summary>
        public float ScrollRectSize => direction == Direction.Vertical
            ? ScrollRectTransform.rect.height
            : ScrollRectTransform.rect.width;

        private RectTransform ScrollRectTransform => ScrollRect.transform as RectTransform;

        private float ScrollSize => ContentSize - ScrollRectSize;

        /// <summary>
        /// Size in pixels, of the first element to be moved from the start of beginning of rect.  
        /// </summary>
        private float UpperSize
        {
            get => ScrollContent._frontSize;
            set
            {
                ScrollContent._frontSize = value;
                _needToReload = true;
            }
        }

        /// <summary>
        /// Size in pixels, from the last element to be moved from it to the bottom of rect.  
        /// </summary>
        private float BottomSize
        {
            get => ScrollContent._backSize;
            set
            {
                ScrollContent._backSize = value;
                _needToReload = true;
            }
        }

        /// <summary>
        /// Data index of first visible cell>.
        /// </summary>
        private int _activeStartCellIndex;

        /// <summary>
        /// Data index of last visible cell>.
        /// </summary>
        private int _activeEndCellIndex;

        private ICellViewPool Pool => _pool ??= new CellViewPool(ScrollContent.RTransform);
        public bool Horizontal => direction == Direction.Horizontal;
        public bool Vertical => direction == Direction.Vertical;

        private readonly List<float> _cellViewSizeArray = new List<float>();
        private readonly List<float> _cellViewOffsetArray = new List<float>();
        private readonly List<int> _remainingCellIndices = new List<int>();

        private Func<int> _getDataCount;
        private Func<int, Vector2> _getCellSize;
        private Func<int, GameObject> _getTempFunc;
        private Action<ScrollCellView> _refreshFunc; //使用UIWidget，Window类型不允许创建成列表型
        private UIBase _controllerUI;
        private Type _uiCellWidgetType;

        private bool _isTempList = false;
        private Action<ScrollCellView, ScrollCellView> _tempListRefreshFunc;

        private ICellViewPool _pool;
        private bool _needToReload = false;
        private bool _needToRefresh;

        #region Unity Methods

        protected virtual void Awake()
        {
            // Force the scroller to scroll in the direction we want set the containers anchor and pivot
            RectTransform content = ScrollContent.RTransform;
            ScrollContent.IsRevert = IsRevert;

            if (direction == Direction.Vertical)
            {
                ScrollRect.horizontal = false;
                ScrollRect.vertical = true;
                content.anchorMin = IsRevert ? Vector2.zero : Vector2.up;
                content.anchorMax = IsRevert ? Vector2.right : Vector2.one;
                content.pivot = IsRevert ? new Vector2(0.5f, 0) : new Vector2(0.5f, 1);

                content.offsetMax = Vector2.zero;
                content.offsetMin = Vector2.zero;
                content.localRotation = Quaternion.identity;
                content.localScale = Vector3.one;

                content.anchoredPosition = Vector2.zero;
            }
            else if (direction == Direction.Horizontal)
            {
                ScrollRect.horizontal = true;
                ScrollRect.vertical = false;
                content.anchorMin = IsRevert ? Vector2.right : Vector2.zero;
                content.anchorMax = IsRevert ? Vector2.one : Vector2.up;
                content.pivot = IsRevert? new Vector2(1, 0.5f) : new Vector2(0, 0.5f);

                content.offsetMax = Vector2.zero;
                content.offsetMin = Vector2.zero;
                content.localRotation = Quaternion.identity;
                content.localScale = Vector3.one;

                content.anchoredPosition = Vector2.zero;
            }

            if (tempGo != null)
            {
                tempGo.SetActive(false);
            }

            ScrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        protected virtual void LateUpdate()
        {
            // if the reload flag is true, then reload the data
            if (_needToReload)
            {
                ReloadData();
            }

            if (_needToRefresh)
            {
                RefreshActive();
            }
        }

        private void OnDestroy()
        {
            ScrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }

        /// <summary>
        /// Called by the EventSystem when a OnScroll event occurs.
        /// </summary>
        public virtual void OnScroll(PointerEventData eventData)
        {
        }

        /// <summary>
        /// Called by the EventSystem when a OnBeginDrag event occurs.
        /// </summary>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            _onBeginDragAction?.Invoke(eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a OnEndDrag event occurs.
        /// </summary>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            _onEndDragAction?.Invoke(eventData);
        }

        #endregion

        /// <summary>
        /// 设置Cell的参数回调
        /// </summary>
        /// <param name="getCountFunc">必传，获取列表总数</param>
        /// <param name="getViewSizeFunc">可选，根据数据索引获取Cell大小，不传将获取模板宽高</param>
        /// <param name="getTempGo">可选，根据索引获取Cell的模板，不传将使用面板设置的模板</param>
        public void SetDelegate(Func<int> getCountFunc, Func<int, Vector2> getViewSizeFunc = null,
            Func<int, GameObject> getTempGo = null)
        {
            this._getDataCount = getCountFunc;
            this._getCellSize = getViewSizeFunc;
            this._getTempFunc = getTempGo;
        }

        /// <summary>
        /// 设置刷新函数
        /// </summary>
        /// <param name="controllerUI">操作模板的UIBase(通常是对象本身this)</param>
        /// <param name="cellWidget">实际控制Widget的类型</param>
        /// <param name="refreshFunc">刷新函数</param>
        public void SetRefreshFunc(UIBase controllerUI,Type cellWidget, Action<ScrollCellView> refreshFunc)
        {
            this._isTempList = false;
            _controllerUI ??= controllerUI;
            _uiCellWidgetType ??= cellWidget;
            this._refreshFunc = refreshFunc;
        }

        /// <summary>
        /// 设置模板列表类型的刷新回调
        /// </summary>
        /// <param name="controllerUI">控制Cell节点的超类,通常传this</param>
        /// <param name="cellWidget">模板里子节点所需创建的Widget类型</param>
        /// <param name="refreshFunc">刷新回调</param>
        public void SetTempListRefreshFunc(UIBase controllerUI, Type cellWidget, Action<ScrollCellView,ScrollCellView> refreshFunc)
        {
            this._isTempList = true;
            _controllerUI = controllerUI;
            _uiCellWidgetType = cellWidget;
            this._tempListRefreshFunc = refreshFunc;
        }

        /// <summary>
        /// Callback to build in scroll <seealso cref="UnityEngine.UI.ScrollRect.onValueChanged"/>,
        /// listening to position change.
        /// </summary>
        private void OnScrollValueChanged(Vector2 x)
        {
            RefreshActive();
            _onChangePosAction?.Invoke(x);
        }

        /// <summary>
        /// Reloads scroll and moves it to given normalized position.
        /// </summary>
        public void ReloadData(float scrollNormalized = 0)
        {
            _needToReload = false;

            while (ScrollContent.Count > 0)
                ReturnCellView(ScrollContent.First);

            _activeStartCellIndex = 0;
            _activeEndCellIndex = 0;

            Resize(false);

            NormalizedScrollPosition = scrollNormalized;
        }

        public void Reload(bool keepPos = false)
        {
            float pos = keepPos ? (ScrollPosition / ScrollSize) : 0;
            ReloadData(pos);
        }

        /// <summary>
        /// 仅刷新数据 回调回去刷新函数
        /// </summary>
        public void RefreshData()
        {
            int i = 0;
            while (i < ScrollContent.Count)
            {
                var cell = ScrollContent.GetScrollCellView(i);
                cell.RefreshCellWidget();
                i++;
            }
        }

        public bool TryJumpTo(int dataIndex, Alignment align)
        {
            bool hasCell = CanJumpTo(dataIndex);
            if (!hasCell)
            {
                Debug.LogWarning("[Scroll Rect Extension] Given data index to jump is outside data count.");
            }
            else
            {
                JumpTo(dataIndex, align);
            }

            return hasCell;
        }

        /// <summary>
        /// Checks if scroll has enough data to jump to given index.
        /// </summary>
        /// <param name="index">Cell data index.</param>
        public bool CanJumpTo(int index)
        {
            return (0 <= index && index < DataCount);
        }

        /// <summary>
        /// Moves scroll to given data index.  
        /// </summary>
        /// <param name="index">Cell data index.</param>
        /// <param name="align">Alignment to which element gonna be set.</param>
        public void JumpTo(int index, Alignment align)
        {
            ScrollPosition = GetCellPosition(index, align);
        }

        /// <summary>
        /// Returns absolute cell position for given data index and align.
        /// </summary>
        /// <param name="index">Cell data index.</param>
        /// <param name="align">Alignment to which element gonna be set.</param>
        public float GetCellPosition(int index, Alignment align)
        {
            int count = DataCount;
            if (count == 0)
                return 0;

            index = index % count;
            if (index < 0)
                index += count;

            float normalizedOffset = (int)align * 0.5f;

            var cellOffsetPosition = 0f;

            if (normalizedOffset < 0 || 0 < normalizedOffset)
            {
                var cellSize = GetCellSize(index);
                cellOffsetPosition = cellSize * normalizedOffset;
            }

            var offset = -(normalizedOffset * ScrollRectSize) + cellOffsetPosition;
            var newScrollPosition = GetScrollPositionFromIndex(index) + offset;

            if (ScrollRect.movementType != ScrollRect.MovementType.Unrestricted)
            {
                newScrollPosition = Mathf.Clamp(newScrollPosition, 0, ScrollSize);
            }

            return newScrollPosition;
        }

        public int GetActiveIndex(Alignment alignment)
        {
            // Check for extremes first.
            if (DataCount <= 0 || _cellViewOffsetArray.Count <= 0)
                return -1;

            if (ScrollPosition < 1)
                return 0;

            if (ScrollSize - 1 < ScrollPosition)
                return DataCount - 1;

            var pos = ScrollPosition + (ScrollRectSize * Mathf.Clamp01((int)alignment * 0.5f));
            var cellViewIndex = GetIndexFromScrollPosition(pos, 0, _cellViewOffsetArray.Count - 1);
            return cellViewIndex % DataCount;
        }

        /// <summary>
        /// Takes corresponding to direction size of cell view for given data index.
        /// </summary>
        private float GetCellSize(int dataIndex)
        {
            var cellSize = Vector2.zero /*Controller.GetCellViewSize(dataIndex)*/;
            if (dataIndex < DataCount)
            {
                if (_getCellSize == null)
                {
                    if (_getTempFunc != null)
                    {
                        GameObject temp = this._getTempFunc(dataIndex);
                        if (temp != null)
                        {
                            cellSize = temp.GetComponent<RectTransform>().sizeDelta;
                        }
                    }
                    else
                    {
                        //获取默认预制体的尺寸
                        if (tempGo != null)
                        {
                            cellSize = tempGo.GetComponent<RectTransform>().sizeDelta;
                        }
                    }
                }
                else
                {
                    cellSize = _getCellSize(dataIndex);
                }
            }

            return direction == Direction.Vertical ? cellSize.y : cellSize.x;
        }

        private float GetScrollPositionFromIndex(int cellViewIndex, bool beforeCell = true)
        {
            if (DataCount == 0 || _cellViewOffsetArray.Count == 0)
                return 0;

            if (cellViewIndex == 0 && beforeCell)
                return PaddingTop;

            if (_cellViewOffsetArray.Count <= cellViewIndex)
                return _cellViewOffsetArray[_cellViewOffsetArray.Count - 1];

            if (beforeCell)
                return _cellViewOffsetArray[cellViewIndex - 1] + Spacing;

            return _cellViewOffsetArray[cellViewIndex];
        }

        /// <summary>
        /// Calculates visible points for each cell, based on data stored in
        /// </summary>
        /// <param name="keepPosition">Recalculate data but keep current scroll position,
        /// can be used when data is loaded dynamically.</param>
        private void Resize(bool keepPosition)
        {
            var originalNormalizedPosition = NormalizedScrollPosition;
            _cellViewSizeArray.Clear();

            for (var i = 0; i < DataCount; i++)
            {
                var viewSize = GetCellSize(i) + (i == DataCount - 1 ? 0 : ScrollContent.Spacing);
                _cellViewSizeArray.Add(viewSize);
            }
            
            _cellViewOffsetArray.Clear();
            var offset = ScrollContent.FrontPadding;
            for (var i = 0; i < _cellViewSizeArray.Count; i++)
            {
                offset += _cellViewSizeArray[i];
                _cellViewOffsetArray.Add(offset);
            }

            if (0 < _cellViewOffsetArray.Count)
            {
                float lastOffset = _cellViewOffsetArray[^1];
                SetSize(lastOffset + ScrollContent.BackPadding);
            }

            ResetVisibleCellViews();

            if (keepPosition)
                NormalizedScrollPosition = originalNormalizedPosition;
            else
                NormalizedScrollPosition = 0;
        }

        /// <summary>
        /// Sets content size corresponding to scroll direction. 
        /// </summary>
        private void SetSize(float size)
        {
            if (direction == Direction.Vertical)
                SetContentVerticalSize(size);
            else
                SetContentHorizontalSize(size);
        }

        /// <summary>
        /// Set height of content rect.
        /// </summary>
        private void SetContentVerticalSize(float y)
        {
            ScrollContent.RTransform.sizeDelta = new Vector2(ScrollContent.RTransform.sizeDelta.x, y);
        }

        /// <summary>
        /// Set width of content rect.
        /// </summary>
        private void SetContentHorizontalSize(float x)
        {
            ScrollContent.RTransform.sizeDelta = new Vector2(x, ScrollContent.RTransform.sizeDelta.y);
        }

        /// <summary>
        /// Gonna update all visible cells views and rebuild the view. 
        /// </summary>
        private void ResetVisibleCellViews()
        {
            CalculateCurrentActiveCellRange(out var startIndex, out var endIndex);
            ResetVisibleCellViews(startIndex, endIndex);
        }

        private void ResetVisibleCellViews(int startIndex, int endIndex)
        {
            var i = 0;
            _remainingCellIndices.Clear();
            while (i < ScrollContent.Count)
            {
                if (ScrollContent.GetScrollCellView(i).ContentIndex < startIndex ||
                    ScrollContent.GetScrollCellView(i).ContentIndex > endIndex)
                {
                    ReturnCellView(ScrollContent.ContentObjects[i]);
                }
                else
                {
                    _remainingCellIndices.Add(ScrollContent.GetScrollCellView(i).ContentIndex);
                    i++;
                }
            }

            if (_remainingCellIndices.Count == 0)
            {
                for (i = startIndex; i <= endIndex; i++)
                    AddCellViewAtEnd(i);
            }
            else
            {
                int index = _remainingCellIndices[0];
                for (i = endIndex; i >= startIndex; i--)
                {
                    if (i < index)
                        AddCellViewAtStart(i);
                }

                index = _remainingCellIndices[_remainingCellIndices.Count - 1];
                for (i = startIndex; i <= endIndex; i++)
                {
                    if (i > index)
                        AddCellViewAtEnd(i);
                }
            }

            _activeStartCellIndex = startIndex;
            _activeEndCellIndex = endIndex;

            // Adjusting padding gonna also refresh content. 
            AdjustPaddingSize(startIndex, endIndex);

            OnChangeActiveCellPosition();
        }

        /// <summary>
        /// Adjust padding size of scroll content rect based on visible cell data indexes. 
        /// </summary>
        private void AdjustPaddingSize(int startIndex, int endIndex)
        {
            if (DataCount == 0)
                return;

            var firstSize = _cellViewOffsetArray[startIndex] - _cellViewSizeArray[startIndex];
            var lastSize = _cellViewOffsetArray[_cellViewOffsetArray.Count - 1] - _cellViewOffsetArray[endIndex];

            // Do not use properties to not rebuild vertical content twice, instead use refresh after that.
            ScrollContent._frontSize = firstSize;
            ScrollContent._backSize = lastSize;

            ScrollContent.Refresh();
        }

        /// <summary>
        /// Pools corresponding cell view to given prefab type.
        /// </summary>
        /// <param name="cellView"></param>
        private void ReturnCellView(GameObject cellView)
        {
            ScrollContent.RemoveObjectWithoutRefreshing(cellView);
            ReturnCellViewToPool(cellView);
            var scrollCellView = cellView.GetComponent<ScrollCellView>();
            scrollCellView.ContentIndex = -1;
            scrollCellView.Active = false;
        }

        /// <summary>
        /// Short cut to add cell view at the beginning of content object.
        /// </summary>
        private void AddCellViewAtStart(int cellIndex)
        {
            AddCellView(cellIndex, 0);
        }

        /// <summary>
        /// Short cut to add cell view at the end of content object.
        /// </summary>
        private void AddCellViewAtEnd(int cellIndex)
        {
            AddCellView(cellIndex, ScrollContent.Count);
        }

        /// <summary>
        /// Adds cell view to content object.
        /// </summary>
        private void AddCellView(int cellIndex, int at)
        {
            if (DataCount == 0)
                return;

            var dataIndex = cellIndex % DataCount;
            GameObject cellView = GetCellTemplate(dataIndex);
            var scrollCellView = cellView.GetComponent<ScrollCellView>();

            // Set up Rect transform to be sure that visible object gonna be rebuild.
            RectTransform c = scrollCellView.RTransform;
            var size = _cellViewSizeArray[cellIndex] - (cellIndex == DataCount - 1 ? 0 : ScrollContent.Spacing); 
            if (direction == Direction.Horizontal)
            {
                c.sizeDelta = new Vector2(size, c.sizeDelta.y);
            }
            else
            {
                c.sizeDelta = new Vector2(c.sizeDelta.x, size);
            }
            c.localRotation = Quaternion.identity;
            c.localScale = Vector3.one;

            scrollCellView.ContentIndex = cellIndex;
            scrollCellView.DataCount = _getDataCount?.Invoke() ?? 0;
            scrollCellView.WidgetType = _uiCellWidgetType;
            scrollCellView.IsTempList = _isTempList;
            scrollCellView.ControllerUI = _controllerUI;
            scrollCellView.RefreshCallback = _refreshFunc;
            scrollCellView.TempListCallback = _tempListRefreshFunc;

            scrollCellView.RefreshCellWidget();

            // For debug purposes in editor.
            c.gameObject.name = cellIndex.ToString();
            ScrollContent.AddObjectWithoutRefreshing(cellView, at);
        }

        /// <summary>
        /// Takes template and requests pooling system for instance created from same prefab. 
        /// </summary>
        private GameObject RentCellViewFromPool(GameObject template)
        {
            return Pool.RentCellView(template);
        }

        /// <summary>
        /// Returns instance of template to pool. 
        /// </summary>
        private void ReturnCellViewToPool(GameObject cellView)
        {
            Pool.ReturnCellView(cellView);
        }

        /// <summary>
        /// Gets cell template for given data index.
        /// </summary>
        private GameObject GetCellTemplate(int dataIndex)
        {
            var template =
                _getTempFunc == null ? tempGo : _getTempFunc(dataIndex) /*Controller.GetCellTemplateFor(dataIndex)*/;
            return RentCellViewFromPool(template);
        }

        /// <summary>
        /// Handles updating the active list of cells.
        /// </summary>
        private void RefreshActive()
        {
            _needToRefresh = false;

            // Get the range of visible cells
            CalculateCurrentActiveCellRange(out var startIndex, out var endIndex);

            // if the index hasn't changed, ignore and return
            if (startIndex == _activeStartCellIndex && endIndex == _activeEndCellIndex)
            {
                OnChangeActiveCellPosition();
                return;
            }

            // Recreate the visible cells
            ResetVisibleCellViews(startIndex, endIndex);
        }

        /// <summary>
        /// Updates cell view with current local position.
        /// </summary>
        private void OnChangeActiveCellPosition()
        {
            var pos = ScrollPosition;
            var rectSize = ScrollRectSize;
            if (rectSize <= 0)
                return;

            for (int i = 0; i < ScrollContent.Count; i++)
            {
                var cell = ScrollContent.GetScrollCellView(i);
                var cellPosition = GetScrollPositionFromIndex(cell.ContentIndex);
                cellPosition += GetCellSize(i) / 2;
                cell.OnPositionChangedCallback?.Invoke((cellPosition - pos) / rectSize);
            }
        }

        /// <summary>
        /// Returns visible range of cells shown in scroll. 
        /// </summary>
        private void CalculateCurrentActiveCellRange(out int startIndex, out int endIndex)
        {
            int lastIndex = _cellViewOffsetArray.Count - 1;
            startIndex = GetIndexFromScrollPosition(ScrollPosition, 0, lastIndex);
            endIndex = GetIndexFromScrollPosition(ScrollPosition + ScrollRectSize, 0, lastIndex);
        }

        /// <summary>
        /// Allows to estimate which index cell has at given scroll position. 
        /// </summary>
        /// <param name="scrollPosition">The absolute position in pixels from the start of the scroller.</param>
        /// <param name="indexFrom">Index to look from, used to minimize the number of calls.</param>
        /// <param name="indexTo">Index to look to, used to minimize the number of calls.</param>
        /// <returns></returns>
        private int GetIndexFromScrollPosition(float scrollPosition, int indexFrom, int indexTo)
        {
            if (indexFrom >= indexTo)
                return indexFrom;

            var middleIndex = (indexFrom + indexTo) / 2;
            return scrollPosition <= (_cellViewOffsetArray[middleIndex])
                ? GetIndexFromScrollPosition(scrollPosition, indexFrom, middleIndex)
                : GetIndexFromScrollPosition(scrollPosition, middleIndex + 1, indexTo);
        }

        public void RegisterChangePosAction(Action<Vector2> action) {
            _onChangePosAction = action;
        }

        public void RegisterBeginDragAction(Action<PointerEventData> action) {
            _onBeginDragAction = action;
        }

        public void RegisterEndDragAction(Action<PointerEventData> action) {
            _onEndDragAction = action;
        }
    }
// }