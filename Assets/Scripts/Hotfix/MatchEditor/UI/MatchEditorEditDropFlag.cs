#if UNITY_EDITOR
using System.Collections.Generic;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;

namespace HotfixLogic
{
	[Window(UILayer.UI,"uiprefab/matcheditor/matcheditoreditdropflag")]
	public partial class MatchEditorEditDropFlag : UIWindow
	{
		private DropFlag _dropFlag;
		private List<DropFlagElement> _configElements;
		private List<EditDropFlagCell> _flagCells = new List<EditDropFlagCell>();
		
		public override void OnCreate()
		{
			btn_addElement.AddClick(OnAddElementClick);
			btn_bg.AddClick(CloseSelf);
			btn_ok.AddClick(OnOkClick);
			
			_dropFlag = (DropFlag)UserData;
			_configElements = new List<DropFlagElement>(_dropFlag.dropElements);
		}

		private void OnAddElementClick()
		{
			DropFlagElement element = new DropFlagElement()
			{
				elementId = 0,
				dropRate = 0,
				dropLimitMax = 0
			};
			_configElements.Add(element);
			UpdateCells();
		}

		public override void OnRefresh()
		{
			input_xPos.SetTextWithoutNotify(_dropFlag.dropX.ToString());
			UpdateCells();
		}

		private void OnOkClick()
		{
			G.EventModule.DispatchEvent(MatchEditorConst.OnEditDropFlagOk,
				EventTwoParam<int, List<DropFlagElement>>.Create(_dropFlag.dropX, _configElements));
			CloseSelf();
		}
		
		private void UpdateCells()
		{
			for (int i = 0; i < _flagCells.Count; i++)
			{
				_flagCells[i].Visible = false;
			}

			for (int i = 0; i < _configElements.Count; i++)
			{
				if (i >= _flagCells.Count)
				{
					EditDropFlagCell cell = CreateWidgetByPrefab<EditDropFlagCell>(go_elemenCell, go_elementRoot.transform);
					_flagCells.Add(cell);
				}

				_flagCells[i].SetData(i, _configElements[i],OnDeleteElementClick,OnElementChanged);
			}

		}
		
		private void OnDeleteElementClick(int index)
		{
			if(index < 0 || index >= _configElements.Count)
				return;
			_configElements.RemoveAt(index);
			UpdateCells();
		}
		
		private void OnElementChanged(DropElementFlagConfig obj)
		{
			int index = obj.Index;
			if (index < 0 || index >= _configElements.Count)
				return;
			_configElements[index].elementId = obj.ElementId;
			_configElements[index].dropRate = obj.Rate;
			_configElements[index].dropLimitMax = obj.Limit;
		}
	}
}
#endif