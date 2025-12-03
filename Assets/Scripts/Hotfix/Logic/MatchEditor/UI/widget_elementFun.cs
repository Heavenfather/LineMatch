#if UNITY_EDITOR
using System.Collections.Generic;
using GameConfig;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;
using UnityEngine.UI;

namespace HotfixLogic
{
    public partial class widget_elementFun : UIWidget
    {
        private List<Button> _operBtns = new List<Button>();
        private List<Button> _eleBtns = new List<Button>();
        private List<int> _curElementIds = new List<int>();
        private ElementMapDB _elementMapDB;
        private int _curOperation = 0;
        private int _curEleState = 0;
        private int _curSelectedElement = 0;
        
        private List<int> SpecialElements { get; } = new List<int> { 8, 9, 10, 11 , 12, 13, 14, 15, 16 };

        public override void OnCreate()
        {
            _elementMapDB = ConfigMemoryPool.Get<ElementMapDB>();

            _operBtns.Add(btn_scroll);
            _operBtns.Add(btn_select);
            _operBtns.Add(btn_del);

            _eleBtns.Add(btn_baseElement);
            _eleBtns.Add(btn_grid);
            _eleBtns.Add(btn_element);

            btn_scroll.AddClick(() =>
            {
                _curOperation = 0;
                UpdateOperBtnState();
                G.EventModule.DispatchEvent(MatchEditorConst.OnElementFillStateChanged,
                    EventOneParam<ElementFillState>.Create(ElementFillState.Scroll));
            });
            btn_select.AddClick(() =>
            {
                _curOperation = 1;
                UpdateOperBtnState();
                G.EventModule.DispatchEvent(MatchEditorConst.OnElementFillStateChanged,
                    EventOneParam<ElementFillState>.Create(ElementFillState.Selected));
            });

            btn_del.AddClick(() =>
            {
                _curOperation = 2;
                UpdateOperBtnState();
                G.EventModule.DispatchEvent(MatchEditorConst.OnElementFillStateChanged,
                    EventOneParam<ElementFillState>.Create(ElementFillState.Delete));
            });
            btn_baseElement.AddClick(() =>
            {
                _curEleState = 0;
                UpdateEleBtnState();
            });
            btn_grid.AddClick(() =>
            {
                _curEleState = 1;
                UpdateEleBtnState();
            });
            btn_element.AddClick(() =>
            {
                _curEleState = 2;
                UpdateEleBtnState();
            });

            scrollEx_element.SetDelegate(() => _curElementIds.Count);
            scrollEx_element.SetTempListRefreshFunc(this, typeof(widget_elementCell), (cell, view) =>
            {
                if (cell.Widget is widget_elementCell elementCell)
                {
                    elementCell.SetView(_curElementIds[cell.DataIndex],
                        _curSelectedElement == _curElementIds[cell.DataIndex], OnElementSelected);
                }
            });
            UpdateOperBtnState();
            UpdateEleBtnState();
        }

        private void UpdateOperBtnState()
        {
            for (int i = 0; i < _operBtns.Count; i++)
            {
                _operBtns[i].GetComponent<Image>().color = i == _curOperation ? Color.yellow : Color.white;
            }
        }

        private void UpdateEleBtnState()
        {
            for (int i = 0; i < _eleBtns.Count; i++)
            {
                _eleBtns[i].GetComponent<Image>().color = i == _curEleState ? Color.yellow : Color.white;
            }
            UpdateScroll();
        }

        private void UpdateScroll()
        {
            _curElementIds.Clear();
            if (_curEleState == 1)
            {
                //填充物
                _curElementIds.Add(MatchEditorConst.WhiteElementId); //空格
                _curElementIds.Add(MatchEditorConst.RectangleElementId);//格子
            }
            else
            {
                for (int i = 0; i < _elementMapDB.All.Length; i++)
                {
                    if (_curEleState == 0)
                    {
                        if (_elementMapDB.All[i].elementType == ElementType.Normal || IsFunctionElementId(_elementMapDB.All[i].Id))
                        {
                            _curElementIds.Add(_elementMapDB.All[i].Id);
                        }
                    }
                    else if (_curEleState == 2)
                    {
                        if (_elementMapDB.All[i].elementType != ElementType.Normal && !IsFunctionElementId(_elementMapDB.All[i].Id))
                        {
                            _curElementIds.Add(_elementMapDB.All[i].Id);
                        }
                    }
                }
            }

            scrollEx_element.Reload();
        }

        private bool IsFunctionElementId(int id)
        {
            return SpecialElements.Contains(id);
        }

        private void OnElementSelected(int id)
        {
            this._curSelectedElement = id;
            G.EventModule.DispatchEvent(MatchEditorConst.OnFillElementChanged,EventOneParam<int>.Create(id));
            scrollEx_element.RefreshData();
        }
    }
}
#endif