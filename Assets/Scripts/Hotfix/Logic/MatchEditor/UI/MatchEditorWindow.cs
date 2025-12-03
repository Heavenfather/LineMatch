#if UNITY_EDITOR
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;

namespace HotfixLogic
{
    [Window(UILayer.UI, "uiprefab/matcheditor/matcheditorwindow")]
    public partial class MatchEditorWindow : UIWindow
    {
        private MatchEditorGridBoard _gridBoard;
        private LevelData _levelData;

        public override void OnCreate()
        {
            _gridBoard = this.gameObject.transform.Find("gridBoard").GetComponent<MatchEditorGridBoard>();
        }

        public widget_fillElement CreateFillElement(int elementId,Vector2Int coord)
        {
            widget_fillElement widget = CreateWidgetByPrefab<widget_fillElement>(go_fillElement);
            widget.SetData(elementId,coord);
            
            var gridInfos = _levelData.FindCoordHoldGridInfo(coord.x, coord.y, true);
            if (gridInfos is { Count: > 0 })
            {
                for (int i = 0; i < gridInfos.Count; i++)
                {
                    if (gridInfos[i].ElementId == elementId)
                    {
                        var info = gridInfos[i];
                        widget.RefreshIconSize(info.TargetElementId, info.TargetElementNum, info.ElementWidth,
                            info.ElementHeight);
                        break;
                    }
                }
            }
            else
            {
                widget.RefreshIconSize(0, 0, 1, 1);
            }
            return widget;
        }

        public void UpdateLevelFun(LevelData levelData, bool updateBoard)
        {
            _levelData = levelData;
            widget_fileFun.UpdateView(levelData);
            widget_levelFun.UpdateView(levelData);
            widget_targetFun.UpdateView(levelData);
            if (updateBoard)
                RefreshGridBoard(levelData);
        }

        public void SetGridBoardData(MatchEditorData data)
        {
            _gridBoard.Init(this, data);
        }

        public void RefreshFileName(int level)
        {
            widget_fileFun.RefreshFileName(level);
        }

        public void UpdateHandleFileProgress(int progress, int total)
        {
            if (progress >= total)
            {
                go_wait.SetVisible(false);
            }
            else
            {
                go_wait.SetVisible(true);
                text_handleFiles.text = $"等待处理文件中...({progress}/{total})";
            }
        }

        public void RefreshFillElement(Vector2Int coord,FillElementData fillElementData)
        {
            _gridBoard.UpdateFillElement(coord, fillElementData);
        }

        public void DelTopElement(Vector2Int coord)
        {
            _gridBoard.DelElement(coord);
        }
        
        private void RefreshGridBoard(LevelData levelData)
        {
            _gridBoard.UpdateGrid(levelData);
        }
    }
}
#endif