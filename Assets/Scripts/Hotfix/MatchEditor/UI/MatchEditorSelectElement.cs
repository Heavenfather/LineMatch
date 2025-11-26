#if UNITY_EDITOR
using System.Collections.Generic;
using GameConfig;
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HotfixLogic
{
	[Window(UILayer.Top,"uiprefab/matcheditor/matcheditorselectelement")]
	public partial class MatchEditorSelectElement : UIWindow
	{
		private ElementMapDB _configDB;
		private int _curSelect;
		private UnityAction<int> _callback;
		
		public override void OnCreate()
		{
			_configDB = ConfigMemoryPool.Get<ElementMapDB>();
			Button btnBg = this.transform.Find("bg").GetComponent<Button>();
			btnBg.AddClick(HideSelf);
			btn_ok.AddClick(() =>
			{
				if (this._curSelect <= 0)
				{
					return;
				}
				_callback?.Invoke(this._curSelect);
				HideSelf();
			});
			scrollEx_elements.SetDelegate(GetElementCount);
			scrollEx_elements.SetTempListRefreshFunc(this,typeof(widget_elementCell), (cell, parent) =>
			{
				if (cell.Widget is widget_elementCell widget)
				{
					widget.SetView(_configDB.All[cell.DataIndex].Id,_configDB.All[cell.DataIndex].Id == _curSelect,OnCellClick);
				}
			});
		}

		public override void OnRefresh()
		{
			TargetElement cur = (TargetElement)UserData;
			_curSelect = cur.targetId;
			scrollEx_elements.Reload();
		}

		public void SetModifyCallback(UnityAction<int> callback)
		{
			this._callback = callback;
		}
		
		private int GetElementCount()
		{
			return _configDB.Count;
		}
		
		private void OnCellClick(int id)
		{
			this._curSelect = id;
			scrollEx_elements.RefreshData();
		}

	}
}
#endif