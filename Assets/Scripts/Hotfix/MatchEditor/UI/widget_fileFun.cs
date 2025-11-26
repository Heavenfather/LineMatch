#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;
using TMPro;
using UnityEngine.UI;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic
{
    public partial class widget_fileFun : UIWidget
    {
        private class ElementCellObj
        {
            public GameObject GameObject;
            public int ID;
            public int Index;
            public float Rate;
            public bool IsDrop;

            private TMP_InputField _inputId;
            private TMP_InputField _inputRate;
            private Button _btnDel;
            private float _newRate;

            public void Init()
            {
                _inputId = GameObject.transform.Find("inputId").GetComponent<TMP_InputField>();
                _inputId.AddEndEditListener(value =>
                {
                    if (int.TryParse(value, out ID))
                        DispatchEvent(IsDrop
                            ? MatchEditorConst.OnDropElementValueChanged
                            : MatchEditorConst.OnInitElementValueChanged);
                });
                _inputRate = GameObject.transform.Find("inputRate").GetComponent<TMP_InputField>();
                _inputRate.AddEndEditListener(value =>
                {
                    if (float.TryParse(value, out _newRate))
                    {
                        DispatchEvent(IsDrop
                            ? MatchEditorConst.OnDropElementValueChanged
                            : MatchEditorConst.OnInitElementValueChanged);
                    }
                    else
                    {
                        Logger.Error("请输入正确格式的概率!");
                    }
                });
                _btnDel = GameObject.transform.Find("del").GetComponent<Button>();
                _btnDel.AddClick(() =>
                {
                    G.EventModule.DispatchEvent(
                        IsDrop ? MatchEditorConst.OnDeleteDropElement : MatchEditorConst.OnDeleteInitElement,
                        EventOneParam<int>.Create(this.Index));
                });
            }

            public void UpdateValue()
            {
                _newRate = Rate / 100.0f;
                _inputId.SetTextWithoutNotify(ID.ToString());
                _inputRate.SetTextWithoutNotify($"{_newRate}");
            }

            public void SetVisible(bool visible)
            {
                this.GameObject.SetVisible(visible);
            }

            private void DispatchEvent(int eventId)
            {
                Rate = _newRate * 100;
                G.EventModule.DispatchEvent(eventId,
                    EventThreeParam<int, int, int>.Create(this.Index, this.ID, (int)Rate));
            }
        }

        private LevelData _levelData;
        private List<ElementCellObj> _baseElements = new List<ElementCellObj>();
        private List<ElementCellObj> _dropElements = new List<ElementCellObj>();

        public override void OnCreate()
        {
#if UNITY_EDITOR
            btn_import.AddClick(() =>
            {
                string folder = UnityEditor.EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
                if (string.IsNullOrEmpty(folder))
                    return;
                UnityEditor.EditorPrefs.SetString(MatchEditorConst.LevelPathKey, folder);
                SetLevelPath();
            });
            btn_import.SetVisible(true);
            input_levelPath.SetVisible(false);
#else
            btn_import.SetVisible(false);
            input_levelPath.SetVisible(true);
            input_levelPath.AddEndEditListener(value =>
            {
                if (string.IsNullOrEmpty(value))
                    return;
                if (!Directory.Exists(value))
                    return;
                PlayerPrefs.SetString(MatchEditorConst.LevelPathKey, value);
                SetLevelPath();
            });
#endif
            btn_export.AddClick(() => { Logger.Debug("导出目录?"); });

            //上一关
            btn_preLevel.AddClick(() => { G.EventModule.DispatchEvent(MatchEditorConst.OnClickPreviousLevel); });

            //下一关
            btn_nextLevel.AddClick(() => { G.EventModule.DispatchEvent(MatchEditorConst.OnClickNextLevel); });

            //保存
            btn_save.AddClick(() => { G.EventModule.DispatchEvent(MatchEditorConst.OnClickSave); });

            //保存并新建
            btn_saveAndNew.AddClick(() => { G.EventModule.DispatchEvent(MatchEditorConst.OnClickSaveAndNew); });

            input_levelId.AddEndEditListener(value =>
            {
                if (int.TryParse(value, out int level))
                {
                    G.EventModule.DispatchEvent(MatchEditorConst.OnEditLevelChanged, EventOneParam<int>.Create(level));
                }
            });

            input_cosLevel.AddEndEditListener(value =>
            {
                if (int.TryParse(value, out int cosLevel) && int.TryParse(input_levelId.text, out int levelId))
                {
                    G.EventModule.DispatchEvent(MatchEditorConst.OnCosLevelChanged,
                        EventTwoParam<int, int>.Create(levelId, cosLevel));
                }
            });

            btn_addElement.AddClick(() => { G.EventModule.DispatchEvent(MatchEditorConst.OnAddInitElement); });

            btn_dropElement.AddClick(() => { G.EventModule.DispatchEvent(MatchEditorConst.OnAddDropElement); });

            SetLevelPath();
        }

        public void UpdateView(LevelData levelData)
        {
            _levelData = levelData;
            input_levelId.SetTextWithoutNotify(levelData.id.ToString());
            input_cosLevel.SetTextWithoutNotify(levelData.referenceId.ToString());
            UpdateLevelElement();
        }

        public void RefreshFileName(int level)
        {
            text_jsonName.text = $"关卡Json名称:Level{level}.json";
        }

        private void UpdateLevelElement()
        {
            int[] baseElements = _levelData.initColor;
            for (int i = 0; i < _baseElements.Count; i++)
            {
                _baseElements[i].SetVisible(false);
            }

            for (int i = 0; i < baseElements.Length; i++)
            {
                if (i >= _baseElements.Count)
                {
                    GameObject go = GameObject.Instantiate(go_elementCell, parent: go_baseElementRoot.transform);
                    ElementCellObj cell = new ElementCellObj();
                    cell.GameObject = go;
                    cell.IsDrop = false;
                    cell.Init();
                    _baseElements.Add(cell);
                }

                _baseElements[i].Index = i;
                _baseElements[i].ID = baseElements[i];
                _baseElements[i].Rate = i < _levelData.initColorRate.Length ? _levelData.initColorRate[i] : 0;
                _baseElements[i].UpdateValue();
                _baseElements[i].SetVisible(true);
            }

            int[] dropElements = _levelData.dropColor;
            for (int i = 0; i < _dropElements.Count; i++)
            {
                _dropElements[i].SetVisible(false);
            }

            for (int i = 0; i < dropElements.Length; i++)
            {
                if (i >= _dropElements.Count)
                {
                    GameObject go = GameObject.Instantiate(go_elementCell, parent: go_dropElementRoot.transform);
                    ElementCellObj cell = new ElementCellObj();
                    cell.GameObject = go;
                    cell.IsDrop = true;
                    cell.Init();
                    _dropElements.Add(cell);
                }

                _dropElements[i].Index = i;
                _dropElements[i].ID = dropElements[i];
                _dropElements[i].Rate = i < _levelData.dropColorRate.Length ? _levelData.dropColorRate[i] : 0;
                _dropElements[i].UpdateValue();
                _dropElements[i].SetVisible(true);
            }
        }

        private void SetLevelPath()
        {
#if UNITY_EDITOR
            string editLevelPath = UnityEditor.EditorPrefs.GetString(MatchEditorConst.LevelPathKey,
                $"{Application.dataPath}/ArtLoad/Match/Level");
#else
            string editLevelPath =
                PlayerPrefs.GetString(MatchEditorConst.LevelPathKey, $"{Application.dataPath}/MatchLevel");
#endif
            text_filePath.text = $"项目路径:{editLevelPath}";
            text_lvPath.text = $"关卡Json路径:{editLevelPath}";
            G.EventModule.DispatchEvent(MatchEditorConst.OnLevelFileFolderChanged);
        }
    }
}
#endif