#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using EditorMatch.Scripts;
using GameConfig;
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Command;
using HotfixCore.Module;
using HotfixCore.MVC;
using HotfixCore.Utils;
using HotfixLogic.Match;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic
{
    [MVCDefine("MatchEditor", typeof(MatchEditorData))]
    public class MatchEditorController : BaseController
    {
        private MatchEditorData _data => (MatchEditorData)Model;

        private MatchEditorWindow _window;

        private CommandExecutor _commandExecutor;

        private bool _isCoinFolder;
        private bool _isGuideFolder;

        public override Type MainView { get; } = typeof(MatchEditorWindow);

        protected override async UniTask OnInitialized()
        {
            CommandManager.Instance.GetOrAddCommandExecutor("MatchEditorLevel", out _commandExecutor);

            Dispatcher.AddEventListener(MatchEditorConst.OnLevelFileFolderChanged, OnLevelFileFolderChanged, this);
            Dispatcher.AddEventListener(MatchEditorConst.OnClickPreviousLevel, OnClickPreviousLevel, this);
            Dispatcher.AddEventListener(MatchEditorConst.OnClickNextLevel, OnClickNextLevel, this);
            Dispatcher.AddEventListener(MatchEditorConst.OnClickSave, OnClickSave, this);
            Dispatcher.AddEventListener(MatchEditorConst.OnClickSaveAndNew, OnClickSaveAndNew, this);
            Dispatcher.AddEventListener(MatchEditorConst.OnAddTargetElement, OnAddTargetElement, this);
            Dispatcher.AddEventListener(MatchEditorConst.OnAddInitElement, OnAddInitElement, this);
            Dispatcher.AddEventListener(MatchEditorConst.OnAddDropElement, OnAddDropElement, this);
            Dispatcher.AddEventListener(MatchEditorConst.OnClearAllElement, OnClearAllElement, this);
            Dispatcher.AddEventListener(MatchEditorConst.OnPreStepClick, OnPreStepClick, this);
            Dispatcher.AddEventListener(MatchEditorConst.OnNextStepClick, OnNextStepClick, this);
            Dispatcher.AddEventListener(MatchEditorConst.OnLevelPreviewClick, OnLevelPreviewClick, this);
            Dispatcher.AddEventListener(MatchEditorConst.OnRandomDifficulty, OnRandomDifficulty, this);
            Dispatcher.AddEventListener<EventOneParam<ElementFillState>>(MatchEditorConst.OnElementFillStateChanged,
                OnElementFillStateChanged,
                this);
            Dispatcher.AddEventListener<EventOneParam<int>>(MatchEditorConst.OnDifficultyChanged, OnDifficultyChanged,
                this);
            Dispatcher.AddEventListener<EventOneParam<int>>(MatchEditorConst.OnMatchTypeChanged, OnMatchTypeChanged,
                this);
            Dispatcher.AddEventListener<EventOneParam<int>>(MatchEditorConst.OnDeleteInitElement, OnDeleteInitElement,
                this);
            Dispatcher.AddEventListener<EventOneParam<int>>(MatchEditorConst.OnDeleteDropElement, OnDeleteDropElement,
                this);
            Dispatcher.AddEventListener<EventOneParam<int>>(MatchEditorConst.OnFillElementChanged, OnFillElementChanged,
                this);
            Dispatcher.AddEventListener<EventOneParam<int>>(MatchEditorConst.OnFullScoreChanged, OnFullScoreChanged,
                this);
            Dispatcher.AddEventListener<EventOneParam<int>>(MatchEditorConst.OnLevelStepChanged, OnLevelStepChanged,
                this);
            Dispatcher.AddEventListener<EventOneParam<int>>(MatchEditorConst.OnEditLevelChanged, OnEditLevelChanged,
                this);
            Dispatcher.AddEventListener<EventOneParam<int>>(MatchEditorConst.OnOpenEditDropFlag, OnOpenEditDropFlag,
                this);
            Dispatcher.AddEventListener<EventTwoParam<int, int>>(MatchEditorConst.OnCosLevelChanged,
                OnCosLevelIdChanged, this);
            Dispatcher.AddEventListener<EventTwoParam<int, int>>(MatchEditorConst.OnClearGridElement,
                OnClearGridElement, this);
            Dispatcher.AddEventListener<EventTwoParam<int, int>>(MatchEditorConst.OnGridNotWhite,
                OnGridNotWhite, this);
            Dispatcher.AddEventListener<EventThreeParam<int, int, int>>(MatchEditorConst.OnFirstAddFillElementToGrid,
                OnFirstAddFillElementToGrid, this);
            Dispatcher.AddEventListener<EventTwoParam<int, int>>(MatchEditorConst.OnFillElementToGrid,
                OnFillElementToGrid, this);
            Dispatcher.AddEventListener<EventTwoParam<int, int>>(MatchEditorConst.OnLevelGridCountChanged,
                OnLevelGridSizeChanged, this);
            Dispatcher.AddEventListener<EventTwoParam<int, int>>(MatchEditorConst.OnDelTopElement,
                OnDelTopElement, this);
            Dispatcher.AddEventListener<EventTwoParam<int, TargetElement>>(MatchEditorConst.OnTargetElementChanged,
                OnTargetElementChanged, this);
            Dispatcher.AddEventListener<EventTwoParam<int, TargetElement>>(MatchEditorConst.OnDelTargetElement,
                OnDelTargetElement, this);
            Dispatcher.AddEventListener<EventTwoParam<int, TargetElement>>(MatchEditorConst.OnTargetElementIconClick,
                OnTargetElementIconClick, this);
            Dispatcher.AddEventListener<EventTwoParam<int, List<DropFlagElement>>>(MatchEditorConst.OnEditDropFlagOk,
                OnEditDropFlagOk, this);
            Dispatcher.AddEventListener<EventThreeParam<int, int, int>>(MatchEditorConst.OnInitElementValueChanged,
                OnInitElementValueChanged, this);
            Dispatcher.AddEventListener<EventThreeParam<int, int, int>>(MatchEditorConst.OnDropElementValueChanged,
                OnDropElementValueChanged, this);
            Dispatcher.AddEventListener<EventOneParam<FillElementData>>(MatchEditorConst.OnEditFillBlockOkClick,
                OnEditFillBlockOkClick, this);


            // G.SceneModule.LoadScene("MatchLevelEditor", LoadSceneMode.Additive);

            await UniTask.CompletedTask;
            _window = Module.GetActiveWindow(typeof(MatchEditorWindow)) as MatchEditorWindow;
            OnLevelFileFolderChanged();
        }

        private void OnMatchTypeChanged(EventOneParam<int> obj)
        {
            MatchManager.Instance.SetMatchGameType((MatchGameType)obj.Arg);
            PlayerPrefsUtil.SetInt(MatchEditorConst.EditorMatchGameType, (int)obj.Arg);
        }

        private void OnOpenEditDropFlag(EventOneParam<int> obj)
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                int dropX = obj.Arg;
                DropFlag dropFlag = null;
                if (level.dropFlags != null)
                {
                    int index = level.dropFlags.FindIndex(x => x.dropX == dropX);
                    if (index >= 0)
                    {
                        dropFlag = level.dropFlags[index];
                    }
                }

                if (dropFlag == null)
                {
                    dropFlag = new DropFlag() { dropX = dropX, dropElements = new List<DropFlagElement>() };
                }

                Module.OpenChildWindow<MatchEditorEditDropFlag>(false, dropFlag).Forget();
            }
        }

        private void OnEditDropFlagOk(EventTwoParam<int, List<DropFlagElement>> obj)
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                int xIndex = obj.Arg1;
                List<DropFlagElement> dropFlags = obj.Arg2;
                ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
                if (dropFlags != null)
                {
                    for (int i = 0; i < dropFlags.Count; i++)
                    {
                        if (!db.IsContain(dropFlags[i].elementId))
                        {
                            Logger.Error($"配置元素 [{dropFlags[i].elementId}] 不存在");
                            return;
                        }
                    }
                }
                bool canAddToDrop = dropFlags != null && dropFlags.Count > 0;

                if (level.dropFlags != null)
                {
                    int index = level.dropFlags.FindIndex(x => x.dropX == xIndex);
                    if (index >= 0)
                    {
                        if (level.dropFlags[index].dropElements != null)
                        {
                            level.dropFlags[index].dropElements.Clear();
                            if (canAddToDrop)
                            {
                                for (int i = 0; i < dropFlags.Count; i++)
                                {
                                    level.dropFlags[index].dropElements.Add(dropFlags[i]);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (canAddToDrop)
                        {
                            level.dropFlags.Add(new DropFlag
                            {
                                dropX = xIndex,
                                dropElements = new List<DropFlagElement>(dropFlags)
                            });
                        }
                    }
                }
                else
                {
                    if (canAddToDrop)
                    {
                        level.dropFlags = new List<DropFlag>();
                        level.dropFlags.Add(new DropFlag
                        {
                            dropX = xIndex,
                            dropElements = new List<DropFlagElement>(dropFlags)
                        });
                    }
                }

                if (level.dropFlags != null)
                {
                    List<int> delIndexes = new List<int>();
                    for (int i = 0; i < level.dropFlags.Count; i++)
                    {
                        if (level.dropFlags[i].dropElements == null || level.dropFlags[i].dropElements.Count <= 0)
                        {
                            delIndexes.Add(i);
                        }
                    }

                    if (delIndexes.Count > 0)
                    {
                        for (int i = 0; i < delIndexes.Count; i++)
                        {
                            level.dropFlags.RemoveAt(delIndexes[i]);
                        }
                    }
                }
            }
        }

        private void OnFirstAddFillElementToGrid(EventThreeParam<int, int, int> obj)
        {
            int eleId = obj.Arg1;
            int x = obj.Arg2;
            int y = obj.Arg3;
            TryGetLocalLevel(_data.CurrentLevelId, out var level);
            Module.OpenChildWindow<MatchEditorEditFillBlock>(false, eleId, x, y, level).Forget();
        }

        private void OnEditFillBlockOkClick(EventOneParam<FillElementData> obj)
        {
            var data = obj.Arg;
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                var infos = level.FindCoordHoldGridInfo(data.X, data.Y, realCoord: false);
                if (infos != null)
                {
                    int infoIndex = infos.FindIndex(x => x.ElementId == data.ElementId);
                    if (infoIndex >= 0)
                    {
                        data.X = infos[infoIndex].StartCoord.x;
                        data.Y = infos[infoIndex].StartCoord.y;
                    }
                }

                bool isValid = true;
                ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
                ref readonly ElementMap config = ref db[data.ElementId];
                if ((config.direction == ElementDirection.None || config.direction == ElementDirection.Right) &&
                    data.X + (data.Width - 1) > level.gridCol)
                {
                    isValid = false;
                    Logger.Error("宽度超出棋盘边界");
                }

                if (config.direction == ElementDirection.Left && data.X - (data.Width - 1) < 0)
                {
                    isValid = false;
                    Logger.Error("宽度超出棋盘边界");
                }

                if ((config.direction == ElementDirection.None || config.direction == ElementDirection.Down) &&
                    data.Y + (data.Height - 1) > level.gridRow)
                {
                    isValid = false;
                    Logger.Error("高度超出棋盘边界");
                }

                if (config.direction == ElementDirection.Up && data.Y - (data.Height - 1) < 0)
                {
                    isValid = false;
                    Logger.Error("高度超出棋盘边界");
                }

                GridElement element = null;

                int index = level.grid[data.X][data.Y].elements.FindIndex(e => data.ElementId == e.id);
                if (index >= 0)
                {
                    if(!isValid)
                        return;
                    element = level.grid[data.X][data.Y].elements[index];
                }
                else
                {
                    if (!isValid)
                    {
                        _window.DelTopElement(new Vector2Int(data.X, data.Y));
                        return;
                    }
                    
                    element = new GridElement();
                }
                _window.RefreshFillElement(new Vector2Int(data.X, data.Y), data);

                element.id = data.ElementId;
                element.targetElementId = data.TargetId;
                element.targetElementCount = data.TargetNum;
                element.elementWid = data.Width;
                element.elementHeight = data.Height;
                var coords = MatchEditorUtils.GetFillElementCoords(data.X, data.Y, data.Width, data.Height,config.direction);
                int[] holdsX = new int[coords.Count];
                int[] holdsY = new int[coords.Count];
                for (int i = 0; i < coords.Count; i++)
                {
                    holdsX[i] = coords[i].x;
                    holdsY[i] = coords[i].y;
                }

                element.holdCoordsX = holdsX;
                element.holdCoordsY = holdsY;
                if (index < 0)
                    level.grid[data.X][data.Y].elements.Add(element);
            }
        }

        protected override void OnReActive(object data = null)
        {
            G.UIModule.SetCanvasResolution(1080, 1920, 1);
        }

        private void OnLevelPreviewClick()
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                if (level.Validate())
                {
                    LevelManager.Instance.SetCurrentLevelData(level);
                    MatchManager.Instance.SetMaxLevelID(level.id);
                    level.BuildElementHoldGridMap(true);
                    MVCManager.Instance.ActiveModule(MVCEnum.Match.ToString(), true,null, level).Forget();
                    OnClickSave();
                }
            }
        }

        private void OnNextStepClick()
        {
            _commandExecutor.Redo();
        }

        private void OnPreStepClick()
        {
            _commandExecutor.Undo();
        }

        private void OnClearAllElement()
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                for (int x = 0; x < level.gridCol; x++)
                {
                    for (int y = 0; y < level.gridRow; y++)
                    {
                        level.grid[x][y].elements = new List<GridElement>();
                    }
                }

                LoadLevelData(level, true);
                AddLevelCommand(command, level);
            }
        }

        private void OnDelTopElement(EventTwoParam<int, int> obj)
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                int x = obj.Arg1;
                int y = obj.Arg2;
                if (level.grid[x][y].elements == null || level.grid[x][y].elements.Count <= 0)
                    return;
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);

                level.grid[x][y].elements[^1] = null;

                level.grid[x][y].elements = level.grid[x][y].elements.Where(t => t != null).ToList();
                level.BuildElementHoldGridMap(true);

                AddLevelCommand(command, level);
            }
        }

        private void OnFillElementToGrid(EventTwoParam<int, int> obj)
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                int eleId = _data.CurFillElementId;
                //起始点
                int x = obj.Arg1;
                int y = obj.Arg2;
                ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
                ref readonly ElementMap config = ref db[eleId];
                if (config.holdGrid <= 1)
                {
                    level.grid[x][y].elements.Add(new GridElement()
                        { id = eleId, elementWid = 1, elementHeight = 1, holdCoordsX = new int[1] { x }, holdCoordsY = new int[1] { y } });
                }
                else
                {
                    int wid = 0;
                    int height = 0;
                    if (config.direction == ElementDirection.Right)
                    {
                        wid = (int)config.holdGrid;
                        height = 1;
                    }
                    else if (config.direction == ElementDirection.Down)
                    {
                        wid = 1;
                        height = (int)config.holdGrid;
                    }
                    else if (config.direction == ElementDirection.None)
                    {
                        if ((int)config.holdGrid == 4)
                        {
                            wid = 2;
                            height = 2;
                        }
                        else if ((int)config.holdGrid == 9)
                        {
                            wid = 3;
                            height = 3;
                        }
                    }

                    var coords = MatchEditorUtils.GetFillElementCoords(x, y, wid, height,config.direction);
                    int[] holdsX = new int[coords.Count];
                    int[] holdsY = new int[coords.Count];
                    for (int i = 0; i < coords.Count; i++)
                    {
                        holdsX[i] = coords[i].x;
                        holdsY[i] = coords[i].y;
                    }

                    level.grid[x][y].elements.Add(new GridElement()
                    {
                        id = eleId,
                        elementHeight = height,
                        elementWid = wid,
                        targetElementId = eleId,
                        targetElementCount = config.eliminateCount,
                        holdCoordsX = holdsX,
                        holdCoordsY = holdsY
                    });
                }

                AddLevelCommand(command, level);
            }
        }

        private void OnGridNotWhite(EventTwoParam<int, int> obj)
        {
            int x = obj.Arg1;
            int y = obj.Arg2;

            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                level.grid[x][y].isWhite = false;
                AddLevelCommand(command, level);
            }
        }

        private void OnClearGridElement(EventTwoParam<int, int> obj)
        {
            int x = obj.Arg1;
            int y = obj.Arg2;

            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                level.grid[x][y].isWhite = true;
                level.grid[x][y].elements = new List<GridElement>();
                AddLevelCommand(command, level);
            }
        }

        private void OnDeleteDropElement(EventOneParam<int> obj)
        {
            int index = obj.Arg;
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                level.dropColor[index] = 0;
                level.dropColorRate[index] = 0;
                level.dropColor = level.dropColor.Where(x => x > 0).ToArray();
                level.dropColorRate = level.dropColorRate.Where(x => x > 0).ToArray();
                LoadLevelData(level, false);
                AddLevelCommand(command, level);
            }
        }

        private void OnDeleteInitElement(EventOneParam<int> obj)
        {
            int index = obj.Arg;
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                level.initColor[index] = 0;
                if (level.initColorRate.Length != level.initColor.Length)
                    Array.Resize(ref level.initColorRate, level.initColor.Length);
                level.initColorRate[index] = 0;
                level.initColor = level.initColor.Where(x => x > 0).ToArray();
                level.initColorRate = level.initColorRate.Where(x => x > 0).ToArray();
                LoadLevelData(level, false);
                AddLevelCommand(command, level);
            }
        }

        private void OnDropElementValueChanged(EventThreeParam<int, int, int> obj)
        {
            int index = obj.Arg1;
            int id = obj.Arg2;
            int rate = obj.Arg3;
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                level.dropColor[index] = id;
                if (level.dropColor.Length != level.dropColorRate.Length)
                    Array.Resize(ref level.dropColorRate, level.dropColor.Length);
                level.dropColorRate[index] = rate;

                AddLevelCommand(command, level);
            }
        }

        private void OnInitElementValueChanged(EventThreeParam<int, int, int> obj)
        {
            int index = obj.Arg1;
            int id = obj.Arg2;
            int rate = obj.Arg3;
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                level.initColor[index] = id;
                if (level.initColor.Length != level.initColorRate.Length)
                    Array.Resize(ref level.initColorRate, level.initColor.Length);
                level.initColorRate[index] = rate;

                AddLevelCommand(command, level);
            }
        }

        private void OnAddDropElement()
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                int oriId = level.dropColor[^1];
                Array.Resize(ref level.dropColor, level.dropColor.Length + 1);
                level.dropColor[^1] = oriId + 1;
                Array.Resize(ref level.dropColorRate, level.dropColorRate.Length + 1);
                level.dropColorRate[^1] = 5000;
                LoadLevelData(level, false);
                AddLevelCommand(command, level);
            }
        }

        private void OnAddInitElement()
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                int oriId = level.initColor[^1];
                Array.Resize(ref level.initColor, level.initColor.Length + 1);
                level.initColor[^1] = oriId + 1;
                Array.Resize(ref level.initColorRate, level.initColorRate.Length + 1);
                level.initColorRate[^1] = 0;
                LoadLevelData(level, false);
                AddLevelCommand(command, level);
            }
        }

        private void OnFillElementChanged(EventOneParam<int> obj)
        {
            int elementId = obj.Arg;
            _data.CurFillElementId = elementId;
        }

        private void OnElementFillStateChanged(EventOneParam<ElementFillState> obj)
        {
            var state = obj.Arg;
            _data.FillState = state;
        }

        private void OnAddTargetElement()
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                int targetId = 1;
                var targetInfo = FindGridTargetNum(level);
                foreach (var kp in targetInfo)
                {
                    targetId = kp.Key;
                    bool haveTarget = false;
                    for (int i = 0; i < level.target.Length; i++)
                    {
                        if (level.target[i].targetId == targetId)
                        {
                            haveTarget = true;
                            break;
                        }
                    }

                    if (!haveTarget)
                    {
                        break;
                    }
                }

                Array.Resize(ref level.target, level.target.Length + 1);
                level.target[^1] = new TargetElement()
                    { targetId = targetId, targetNum = targetInfo.ContainsKey(targetId) ? targetInfo[targetId] : 5 };
                LoadLevelData(level, false);
                AddLevelCommand(command, level);
            }
        }

        private void OnTargetElementIconClick(EventTwoParam<int, TargetElement> obj)
        {
            UniTask.Create(async () =>
            {
                var window = await Module.OpenChildWindow<MatchEditorSelectElement>(false, userDatas: obj.Arg2);
                window.SetModifyCallback(result =>
                {
                    if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
                    {
                        LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                        int index = obj.Arg1;
                        if (index < 0 || index >= level.target.Length)
                            return;
                        var targetInfo = FindGridTargetNum(level);
                        level.target[index].targetId = result;
                        level.target[index].targetNum = targetInfo.ContainsKey(result) ? targetInfo[result] : 0;

                        LoadLevelData(level, false);
                        AddLevelCommand(command, level);
                    }
                });
            }).Forget();
        }

        private Dictionary<int, int> FindGridTargetNum(LevelData level)
        {
            Dictionary<int, int> gridTargetNum = new Dictionary<int, int>();
            for (int x = 0; x < level.gridCol; x++)
            {
                for (int y = 0; y < level.gridRow; y++)
                {
                    var elements = level.grid[x][y].elements;
                    for (int i = 0; i < elements.Count; i++)
                    {
                        gridTargetNum.TryAdd(elements[i].id, 0);
                        gridTargetNum[elements[i].id]++;
                    }
                }
            }

            return gridTargetNum;
        }

        private void OnDelTargetElement(EventTwoParam<int, TargetElement> obj)
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                int index = obj.Arg1;
                if (index < 0 || index >= level.target.Length)
                    return;
                level.target[obj.Arg1] = null;
                level.target = level.target.Where(x => x != null).ToArray();
                LoadLevelData(level, false);
                AddLevelCommand(command, level);
            }
        }

        private void OnTargetElementChanged(EventTwoParam<int, TargetElement> obj)
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                int index = obj.Arg1;
                if (index < 0 || index >= level.target.Length)
                    return;
                level.target[index] = obj.Arg2;
                AddLevelCommand(command, level);
            }
        }

        private void OnFullScoreChanged(EventOneParam<int> obj)
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                level.fullScore = obj.Arg;
                AddLevelCommand(command, level);
            }
        }

        private void OnLevelGridSizeChanged(EventTwoParam<int, int> obj)
        {
            int col = obj.Arg1;
            int row = obj.Arg2;
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                level.gridCol = col;
                level.gridRow = row;
                MatchEditorUtils.ResizeLevelGrid(ref level, row, col);
                LoadLevelData(level, true);
                AddLevelCommand(command, level);
            }
        }

        private void OnDifficultyChanged(EventOneParam<int> obj)
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                level.difficulty = obj.Arg;
                AddLevelCommand(command, level);
            }
        }

        private void OnRandomDifficulty()
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                UniTask.Create(async () =>
                {
                    var window = await Module.OpenChildWindow<MatchEditorGenLevelWindow>();
                    window.SetView(level, () =>
                    {
                        LevelGenerator.RefLevelGrid(ref level);
                        LoadLevelData(level, true);
                    });
                }).Forget();
            }
        }

        private void OnLevelStepChanged(EventOneParam<int> obj)
        {
            if (TryGetLocalLevel(_data.CurrentLevelId, out var level))
            {
                LevelDataChangeCommand command = new LevelDataChangeCommand(level, this);
                level.stepLimit = obj.Arg;
                AddLevelCommand(command, level);
            }
        }

        private void OnClickSaveAndNew()
        {
            OnClickSave();
            OnClickNextLevel();
        }

        private void OnClickSave()
        {
            int levelId = _data.CurrentLevelId;
            if (TryGetLocalLevel(levelId, out LevelData levelData))
            {
                string filePath = FormatLevelFilePath(levelId);
                MatchEditorUtils.SaveLevelData(filePath, levelData);
                _commandExecutor.ClearHistory();
            }
        }

        private void OnCosLevelIdChanged(EventTwoParam<int, int> obj)
        {
            int levelId = obj.Arg1;
            int cosLevelId = obj.Arg2;
            if (TryGetLocalLevel(levelId, out LevelData levelData))
            {
                //本地有关卡id，直接修改数据
                levelData.referenceId = cosLevelId;
                if (TryGetLocalLevel(cosLevelId, out LevelData cosLevelData))
                {
                    cosLevelData.DeepClone(levelData);
                    LoadLevelData(levelData, true);
                }
            }
            else
            {
                //本地没有关卡 当有变更时，直接拷贝参考关卡数据过来
                LevelData newLevelData = LevelGenerator.GenerateLevelData(levelId);
                newLevelData.referenceId = cosLevelId;
                _data.AllLevelData.Add(newLevelData);
                if (TryGetLocalLevel(cosLevelId, out LevelData cosLevelData))
                {
                    cosLevelData.DeepClone(newLevelData);
                    LoadLevelData(newLevelData, true);
                }
            }
        }

        private void OnEditLevelChanged(EventOneParam<int> obj)
        {
            int levelId = obj.Arg;
            _data.CurrentLevelId = levelId;
            //先看本地有没有指定关卡
            if (TryGetLocalLevel(levelId, out LevelData levelData))
            {
                LoadLevelData(levelData, true);
            }
            else
            {
                //没有执行的关卡，可能是想新建关卡
                LevelData newLevelData = LevelGenerator.GenerateLevelData(levelId);
                _data.AllLevelData.Add(newLevelData);
                LoadLevelData(newLevelData, true);
            }

            _commandExecutor.ClearHistory();
            _window.RefreshFileName(_data.CurrentLevelId);
        }

        private void OnClickNextLevel()
        {
            ++_data.CurrentLevelId;
            //先看本地有没有指定关卡
            if (TryGetLocalLevel(_data.CurrentLevelId, out LevelData levelData))
            {
                LoadLevelData(levelData, true);
            }
            else
            {
                LevelData newLevelData = LevelGenerator.GenerateLevelData(_data.CurrentLevelId);
                _data.AllLevelData.Add(newLevelData);
                LoadLevelData(newLevelData, true);
            }

            _commandExecutor.ClearHistory();
            _window.RefreshFileName(_data.CurrentLevelId);
        }

        private void OnClickPreviousLevel()
        {
            --_data.CurrentLevelId;
            if (TryGetLocalLevel(_data.CurrentLevelId, out LevelData levelData))
            {
                LoadLevelData(levelData, true);
                _commandExecutor.ClearHistory();
                _window.RefreshFileName(_data.CurrentLevelId);
                return;
            }

            Logger.Error($"无法找到上一关:{_data.CurrentLevelId}");
            //恢复
            ++_data.CurrentLevelId;
        }

        private async void OnLevelFileFolderChanged()
        {
            _commandExecutor.ClearHistory();
            _data.AllLevelData.Clear();
            if (!Directory.Exists(_data.LevelPath))
            {
                Directory.CreateDirectory(_data.LevelPath);
            }

            string path = _data.LevelPath.ToLower();
            _isCoinFolder = path.Contains("coin");
            _isGuideFolder = path.Contains("guide");
            string[] files = Directory.GetFiles(_data.LevelPath, "*.json", SearchOption.AllDirectories);
            await LoadLevelDataAsync(files);

            ShowDefaultLevel();
        }

        private async UniTask LoadLevelDataAsync(string[] files)
        {
            int framesPerBatch = 30;  // 每帧处理批次
            int filesPerFrame = 100;  // 每帧处理文件数
            int totalFiles = files.Length;
            int processedFiles = 0;
            _window.UpdateHandleFileProgress(processedFiles, totalFiles);
            //按批次处理，避免单帧加载过高
            for (int batchStart = 0; batchStart < totalFiles; batchStart+=filesPerFrame)
            {
                int batchSize = Mathf.Min(filesPerFrame, totalFiles - batchStart);
                var batchTasks = new UniTask<LevelData>[batchSize];
            
                // 并行处理当前批次
                for (int i = 0; i < batchSize; i++)
                {
                    int fileIndex = batchStart + i;
                    batchTasks[i] = ParseLevelDataAsync(files[fileIndex]);
                }
            
                // 等待当前批次完成
                var results = await UniTask.WhenAll(batchTasks);
            
                foreach (var levelData in results)
                {
                    if (levelData != null)
                        _data.AllLevelData.Add(levelData);
                }
            
                processedFiles += batchSize;
                // 每处理完一批等待几帧，避免卡顿
                if (batchStart + batchSize < totalFiles)
                {
                    for (int i = 0; i < framesPerBatch; i++)
                    {
                        await UniTask.Yield(); // 等待一帧
                    }   
                }

                _window.UpdateHandleFileProgress(processedFiles, totalFiles);
            }
        }

        private async UniTask<LevelData> ParseLevelDataAsync(string filePath)
        {
            try
            {
                string jsonStr = await File.ReadAllTextAsync(filePath);
                return JsonMapper.ToObject<LevelData>(jsonStr);
            }
            catch (Exception e)
            {
                Debug.LogError($"解析文件失败: {filePath}\nError: {e.Message}");
                return null;
            }
        }

        private void ShowDefaultLevel()
        {
            _window.SetGridBoardData(_data);

            int lastEditorLevel = MatchEditorUtils.GetOrSetLastEditLevel(-1);
            if (TryGetLocalLevel(lastEditorLevel, out LevelData levelData))
            {
                _data.CurrentLevelId = lastEditorLevel;
                LoadLevelData(levelData, true);
            }

            _window.RefreshFileName(_data.CurrentLevelId);
        }

        private bool TryGetLocalLevel(int levelId, out LevelData levelData)
        {
            for (int i = 0; i < _data.AllLevelData.Count; i++)
            {
                if (_data.AllLevelData[i].id == levelId)
                {
                    levelData = _data.AllLevelData[i];
                    return true;
                }
            }

            levelData = null;
            return false;
        }

        public void LoadLevelData(LevelData data, bool updateBoard)
        {
            _window.UpdateLevelFun(data, updateBoard);
            MatchEditorUtils.GetOrSetLastEditLevel(data.id);
        }

        private string FormatLevelFilePath(int levelId)
        {
            if(_isCoinFolder)
                return $"{_data.LevelPath}/Level{levelId}_coin.json";
            if(_isGuideFolder)
                return $"{_data.LevelPath}/Level{levelId}_guide.json";
            return $"{_data.LevelPath}/Level{levelId}.json";
        }

        private void AddLevelCommand(LevelDataChangeCommand command, LevelData newData)
        {
            command.SetNewLevelData(newData);
            _commandExecutor.ExecuteCommand(command);
        }
    }
}
#endif