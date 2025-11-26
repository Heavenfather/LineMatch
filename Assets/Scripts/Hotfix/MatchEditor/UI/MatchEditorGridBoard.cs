#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using HotfixCore.Module;
using HotfixLogic.Match;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic
{
    public class MatchEditorGridBoard : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private class CellInfo : MonoBehaviour
        {
            public int x;
            public int y;

            private const float _offset = 20.0f;
            private Image _elementIcon;
            private Image _gridBg;
            private Button _dropFlagBtn;
            private MatchEditorWindow _editorWindow;
            private List<widget_fillElement> _fillElements = new List<widget_fillElement>();
            private List<Image> _elementIcons = new List<Image>();
            private List<int> _eleIds = new List<int>();

            public async UniTask UpdateElement(LevelElement element,MatchEditorWindow window, Transform parent)
            {
                Init();
                _editorWindow = window;
                ClearAllElements();
                if (element.isWhite)
                {
                    var color = _gridBg.color;
                    color.a = 0;
                    _gridBg.color = color;
                }
                else
                {
                    var color = _gridBg.color;
                    color.a = 1;
                    _gridBg.color = color;

                    for (int i = 0; i < element.elements.Count; i++)
                    {
                        if (MatchEditorUtils.IsFillElement(element.elements[i].id))
                        {
                            AddFillElement(element.elements[i].id,
                                    _editorWindow.CreateFillElement(element.elements[i].id, new Vector2Int(x, y)),
                                    parent,false);
                        }
                        else
                        {
                            GameObject iconGo = Instantiate(_elementIcon.gameObject, parent);
                            _elementIcons.Add(iconGo.GetComponent<Image>());
                            _eleIds.Add(element.elements[i].id);
                            await UpdateElementIcon(_elementIcons[i], element.elements[i].id);
                            _elementIcons[i].SetVisible(true);
                            iconGo.transform.position = this.transform.position;
                            FixAnchorPos(element.elements[i].id, _elementIcons[i]);
                        }
                    }
                }
            }

            public async UniTask AddElement(int elementId, Transform parent)
            {
                if (elementId == MatchEditorConst.WhiteElementId)
                {
                    //空格子
                    this._gridBg.color = new Color(1, 1, 1, 0);
                    //应该清空格子上面的元素？
                    for (int i = 0; i < _elementIcons.Count; i++)
                    {
                        DestroyImmediate(_elementIcons[i].gameObject);
                    }

                    _elementIcons.Clear();
                    _eleIds.Clear();

                    DispatchEvent(MatchEditorConst.OnClearGridElement);
                }
                else if (elementId == MatchEditorConst.RectangleElementId)
                {
                    this._gridBg.color = new Color(1, 1, 1, 1);
                    DispatchEvent(MatchEditorConst.OnGridNotWhite);
                }
                else
                {
                    if (HasBaseElement(elementId))
                    {
                        Logger.Error("一个格子只能有一个基础棋子");
                        return;
                    }

                    this._gridBg.color = new Color(1, 1, 1, 1);
                    GameObject iconGo = Instantiate(_elementIcon.gameObject, parent);
                    Image icon = iconGo.GetComponent<Image>();
                    _elementIcons.Add(icon);
                    _eleIds.Add(elementId);
                    await UpdateElementIcon(icon, elementId);
                    iconGo.transform.position = this.transform.position;
                    FixAnchorPos(elementId, icon);
                    iconGo.SetVisible(true);
                    DispatchEvent(MatchEditorConst.OnFillElementToGrid);
                }
            }

            public void AddFillElement(int eleId,widget_fillElement widget, Transform parent,bool dispatchEvent = true)
            {
                this._gridBg.color = new Color(1, 1, 1, 1);
                widget.transform.SetParent(parent);
                _fillElements.Add(widget);
                _eleIds.Add(eleId);
                UniTask.Create(async () =>
                {
                    await UniTask.DelayFrame(1);
                    widget.transform.position = this.transform.position;
                }).Forget();
                
                if (dispatchEvent)
                    G.EventModule.DispatchEvent(MatchEditorConst.OnFirstAddFillElementToGrid,
                        EventThreeParam<int, int, int>.Create(eleId, x, y));
            }

            public void RemoveTopElement()
            {
                if (_elementIcons.Count > 0)
                {
                    DestroyImmediate(_elementIcons[^1].gameObject);
                    _elementIcons.RemoveAt(_elementIcons.Count - 1);
                }

                if (_fillElements.Count > 0)
                {
                    _fillElements[^1].Destroy();
                    _fillElements.RemoveAt(_fillElements.Count - 1);
                }

                DispatchEvent(MatchEditorConst.OnDelTopElement);
                _eleIds.RemoveAt(_eleIds.Count - 1);
            }

            public void ResizeFillElement(FillElementData fillData)
            {
                for (int i = 0; i < _fillElements.Count; i++)
                {
                    if (_fillElements[i].Coord.x == fillData.X && _fillElements[i].Coord.y == fillData.Y && fillData.ElementId == _fillElements[i].ElementId)
                    {
                        _fillElements[i].RefreshIconSize(fillData.TargetId, fillData.TargetNum, fillData.Width,
                            fillData.Height);
                        break;
                    }
                }
            }

            private void Init()
            {
                if (_gridBg == null || _elementIcon == null)
                {
                    _gridBg = this.GetComponent<Image>();
                    _elementIcon = this.transform.Find("icon").GetComponent<Image>();
                }

                if (_dropFlagBtn == null)
                {
                    var btnTran = this.transform.Find("flagDropIcon");
                    if (y == 0)
                    {
                        btnTran.SetVisible(true);
                        _dropFlagBtn = btnTran.GetComponent<Button>();
                        _dropFlagBtn.AddClick(() =>
                        {
                            G.EventModule.DispatchEvent(MatchEditorConst.OnOpenEditDropFlag, EventOneParam<int>.Create(x));
                        });
                    }
                    else
                    {
                        btnTran.SetVisible(false);
                    }
                }
            }

            private bool HasBaseElement(int nowElementId)
            {
                ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
                bool nowIsBaseElement = db[nowElementId].elementType == ElementType.Normal;
                if (!nowIsBaseElement)
                    return false;
                for (int i = 0; i < _eleIds.Count; i++)
                {
                    if (db[_eleIds[i]].elementType == ElementType.Normal)
                        return true;
                }

                return false;
            }

            private async UniTask UpdateElementIcon(Image img, int elementId)
            {
                if (img != null && img.gameObject != null)
                {
                    img.sprite = await MatchEditorUtils.GetElementIcon(elementId);
                    var db = ConfigMemoryPool.Get<ElementMapDB>();
                    ElementMap config = db[elementId];
                    if (config.elementType == ElementType.Normal)
                    {
                        img.color = ElementSystem.Instance.GetElementColor(elementId);
                    }
                    else
                    {
                        if (elementId == 150)
                            img.color = Color.gray;
                        else
                            img.color = Color.white;
                    }
                    img.SetNativeSize();
                }
            }

            private void DispatchEvent(int eventId)
            {
                G.EventModule.DispatchEvent(eventId, EventTwoParam<int, int>.Create(x, y));
            }

            private void FixAnchorPos(int eleId, Image icon)
            {
                var db = ConfigMemoryPool.Get<ElementMapDB>();
                ref readonly ElementMap config = ref db[eleId];
                if (config.holdGrid <= 1)
                    return;
                int textureWidth = icon.mainTexture.width;
                int textureHeight = icon.mainTexture.height;
                float offsetConst = 4.0f;
                if(config.holdGrid >= 9)
                    offsetConst = 3.0f;
                float offsetX = textureWidth / offsetConst + _offset;
                float offsetY = textureHeight / offsetConst + _offset;
                var anchorPos = icon.GetComponent<RectTransform>().anchoredPosition;
                if (config.direction == ElementDirection.None)
                    icon.GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(anchorPos.x + offsetX, anchorPos.y - offsetY);
                if (config.direction == ElementDirection.Right)
                    icon.GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(anchorPos.x + offsetX, anchorPos.y);
                if (config.direction == ElementDirection.Down)
                    icon.GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(anchorPos.x, anchorPos.y - offsetY);
            }

            public void ClearAllElements()
            {
                _eleIds?.Clear();
                if (_elementIcons.Count > 0)
                {
                    for (int i = 0; i < _elementIcons.Count; i++)
                    {
                        DestroyImmediate(_elementIcons[i].gameObject);
                    }
                    _elementIcons.Clear();
                }

                if (_fillElements.Count > 0)
                {
                    for (int i = _fillElements.Count - 1; i >= 0; i--)
                    {
                        _fillElements[i].Destroy();
                    }
                    _fillElements.Clear();
                }
            }
        }

        public GameObject colPrefab;
        public GameObject cellPrefab;
        public Transform eleRoot;

        private int _currentRow = 0;
        private int _currentColumn = 0;
        private bool _isDragging = false;
        private LevelData _levelData;
        private MatchEditorData _data;
        private MatchEditorWindow _editorWindow;
        private List<GameObject> _cols = new List<GameObject>();
        private List<CellInfo> _trackedCells = new List<CellInfo>();
        
        public void Init(MatchEditorWindow editorWindow, MatchEditorData data)
        {
            _editorWindow = editorWindow;
            this._data = data;
        }
        
        public void UpdateGrid(LevelData levelData)
        {
            _levelData = levelData;
            int newRows = levelData.gridRow;
            int newColumns = levelData.gridCol;
            //调整列数
            if (newColumns != _currentColumn)
            {
                if (newColumns > _currentColumn)
                {
                    for (int i = _currentColumn; i < newColumns; i++)
                    {
                        AddCol(newRows, i);
                    }
                }
                else
                {
                    for (int i = _currentColumn - 1; i >= newColumns; i--)
                    {
                        RemoveCol(i);
                    }
                }

                _currentColumn = newColumns;
            }

            //调整行数
            if (newRows != _currentRow)
            {
                int colIndex = 0;
                foreach (var col in _cols)
                {
                    AdjustRow(col, colIndex, newRows);
                    colIndex++;
                }

                _currentRow = newRows;
            }

            UpdateCellElement();
        }

        public void UpdateFillElement(Vector2Int coord,FillElementData fillData)
        {
            CellInfo cellInfo = FindCellByCoord(coord.x, coord.y);
            cellInfo.ResizeFillElement(fillData);
        }

        public void DelElement(Vector2Int coord)
        {
            CellInfo cellInfo = FindCellByCoord(coord.x, coord.y);
            if (cellInfo != null)
            {
                cellInfo.RemoveTopElement();
            }
        }

        private void UpdateCellElement()
        {
            foreach (var row in _cols)
            {
                foreach (Transform cellTran in row.transform)
                {
                    CellInfo cellInfo = cellTran.GetComponent<CellInfo>();
                    cellInfo.UpdateElement(_levelData.grid[cellInfo.x][cellInfo.y],_editorWindow, eleRoot).Forget();
                }
            }
        }

        private void AddCol(int row, int colIndex)
        {
            GameObject col = Instantiate(colPrefab, transform);
            for (int i = 0; i < row; i++)
            {
                GameObject cell = Instantiate(cellPrefab, col.transform);
                CellInfo cellInfo = cell.GetOrAddComponent<CellInfo>();
                cellInfo.x = colIndex;
                cellInfo.y = i;
                cellInfo.gameObject.name = $"{cellInfo.x}-{cellInfo.y}";
            }

            _cols.Add(col);
        }

        private void RemoveCol(int index)
        {
            if (index < 0 || index >= _cols.Count)
                return;
            GameObject col = _cols[index];
            foreach (Transform child in col.transform)
            {
                CellInfo cellInfo = child.GetComponent<CellInfo>();
                if (cellInfo != null)
                {
                    cellInfo.ClearAllElements();
                }
            }
            DestroyImmediate(col);
            _cols.RemoveAt(index);
        }

        private void AdjustRow(GameObject col, int colIndex, int targetRow)
        {
            int currentChildCount = col.transform.childCount;
            if (targetRow > currentChildCount)
            {
                for (int i = currentChildCount; i < targetRow; i++)
                {
                    GameObject cell = Instantiate(cellPrefab, col.transform);
                    CellInfo cellInfo = cell.GetOrAddComponent<CellInfo>();
                    cellInfo.x = colIndex;
                    cellInfo.y = i;
                    cellInfo.gameObject.name = $"{cellInfo.x}-{cellInfo.y}";
                }
            }
            else if (targetRow < currentChildCount)
            {
                for (int i = currentChildCount - 1; i >= targetRow; i--)
                {
                    CellInfo cellInfo = col.transform.GetChild(i).gameObject.GetOrAddComponent<CellInfo>();
                    cellInfo.ClearAllElements();
                    DestroyImmediate(cellInfo.gameObject);
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_data.CurFillElementId == -1 && _data.FillState != ElementFillState.Delete)
            {
                Logger.Error("当前未选择任何元素进行填充！");
                return;
            }

            // if (_data.FillState != ElementFillState.Delete)
            // {
            //     float holdGrid = MatchEditorUtils.GetElementHoldGridCount(_data.CurFillElementId);
            //     if (holdGrid > 1 && _data.FillState == ElementFillState.Scroll)
            //     {
            //         Logger.Error("当前元素会占据多个格子只能使用选择的方式进行填充！");
            //         return;
            //     }
            // }

            _isDragging = true;
            TrackCell(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging && _data.FillState == ElementFillState.Scroll)
            {
                TrackCell(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;
            _trackedCells.Clear();
        }

        private void TrackCell(PointerEventData eventData)
        {
            GameObject cell = FindCellAtPosition(eventData.position);
            if (cell != null)
            {
                CellInfo cellInfo = cell.GetComponent<CellInfo>();
                if (cellInfo == null)
                    return;
                if (_data.FillState == ElementFillState.Delete)
                {
                    var gridInfo = _levelData.FindCoordHoldGridInfo(cellInfo.x, cellInfo.y, realCoord: false);
                    if (gridInfo is { Count: > 0 })
                    {
                        CellInfo realCell = FindCellByCoord(gridInfo[0].StartCoord.x, gridInfo[0].StartCoord.y);
                        realCell.RemoveTopElement();
                    }
                }
                else
                {
                    if (!_trackedCells.Contains(cellInfo))
                    {
                        //如果有定制元素，再点击的话，就在这里打开编辑这个元素吧
                        if (eventData.button == PointerEventData.InputButton.Right && IsClickFillElement(cellInfo.x, cellInfo.y))
                        {
                            
                        }
                        else
                        {
                            int eleId = _data.CurFillElementId;
                            if (IsFillPosValidate(eleId, cellInfo.x, cellInfo.y))
                            {
                                bool isFillElement = MatchEditorUtils.IsFillElement(eleId);
                                if (isFillElement)
                                {
                                    cellInfo.AddFillElement(eleId, _editorWindow.CreateFillElement(eleId,new Vector2Int(cellInfo.x, cellInfo.y)), eleRoot);
                                }
                                else
                                {
                                    cellInfo.AddElement(eleId, eleRoot).Forget();
                                }

                                _trackedCells.Add(cellInfo);
                            }
                        }
                    }
                }
            }
        }

        private bool IsClickFillElement(int x, int y)
        {
            int eleId = _data.CurFillElementId;
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            var gridInfo = _levelData.FindCoordHoldGridInfo(x, y, true);
            if (gridInfo is { Count: > 0 })
            {
                int index = gridInfo.FindIndex(t => MatchEditorUtils.IsFillElement(t.ElementId));
                if (index >= 0)
                {
                    if (db[gridInfo[index].ElementId].elementType == ElementType.VerticalExpand &&
                        db[eleId].elementType == ElementType.TargetBlock)
                        return false;
                    
                    G.EventModule.DispatchEvent(MatchEditorConst.OnFirstAddFillElementToGrid,
                        EventThreeParam<int, int, int>.Create(gridInfo[index].ElementId, x, y));
                }
                return index >= 0;
            }
            return false;
        }

        private bool IsFillPosValidate(int eleId, int x, int y)
        {
            if (eleId == MatchEditorConst.RectangleElementId || eleId == MatchEditorConst.WhiteElementId)
                return true;
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[eleId];
            bool isPosValidate = true;
            if (config.direction == ElementDirection.Right)
            {
                for (int i = 0; i < config.holdGrid; i++)
                {
                    if (x + i >= _levelData.gridCol)
                    {
                        isPosValidate = false;
                    }
                }
            }
            else if (config.direction == ElementDirection.Down)
            {
                for (int i = 0; i < config.holdGrid; i++)
                {
                    if (y + i >= _levelData.gridRow)
                    {
                        isPosValidate = false;
                    }
                }
            }
            else if (config.direction == ElementDirection.None)
            {
                int count = 2;
                if((int)config.holdGrid == 9)
                    count = 3;
                int avCount = Mathf.Max(0, (int)config.holdGrid / count - 1);
                isPosValidate = x + avCount < _levelData.gridCol && y + avCount < _levelData.gridRow;
            }

            if (!isPosValidate)
            {
                Logger.Error("摆放位置非法!");
            }
            
            bool isEleValidate = true;
            bool isSameEle = false;
            var allGridInfo = _levelData.BuildElementHoldGridMap(true);
            foreach (var gridInfo in allGridInfo)
            {
                if (db[eleId].elementType != ElementType.TargetBlock && db[eleId].holdGrid >= 1 && db[gridInfo.ElementId].holdGrid >= 1)
                {
                    if(gridInfo.AllHoldGridPos.Contains(new Vector2Int(x,y)))
                        isEleValidate = false;
                }

                // if (gridInfo.ElementId == eleId)
                // {
                //     isSameEle = true;
                // }
            }

            // if (isSameEle)
            // {
            //     Logger.Error("相同元素不允许堆叠!");
            // }

            if (!isEleValidate)
            {
                Logger.Error("占据整格元素或相同元素不允许堆叠!");
            }

            return isPosValidate && isEleValidate && !isSameEle;
        }
        
        private GameObject FindCellAtPosition(Vector2 eventDataPosition)
        {
            foreach (var row in _cols)
            {
                foreach (Transform cellTran in row.transform)
                {
                    if (IsPointerOverCell(cellTran, eventDataPosition))
                    {
                        return cellTran.gameObject;
                    }
                }
            }

            return null;
        }

        private CellInfo FindCellByCoord(int x, int y)
        {
            foreach (var row in _cols)
            {
                foreach (Transform cellTran in row.transform)
                {
                    CellInfo info = cellTran.GetComponent<CellInfo>();
                    if (info != null && info.x == x && info.y == y)
                    {
                        return info;
                    }
                }
            }

            return null;
        }

        private bool IsPointerOverCell(Transform cell, Vector2 screenPos)
        {
            RectTransform rt = cell.GetComponent<RectTransform>();
            return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, G.UIModule.UICamera);
        }
    }
}
#endif