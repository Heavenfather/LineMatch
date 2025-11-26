#if UNITY_EDITOR

/*-------------------------------------
Author:DefaultAuthor
Time:2025/4/29 15:32:23
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class widget_elementFun
	{
		private Button btn_scroll;
		private Button btn_select;
		private Button btn_del;
		private Button btn_baseElement;
		private Button btn_grid;
		private Button btn_element;
		private ScrollRectExtension scrollEx_element;

		public override void ScriptGenerate()
		{
			btn_scroll = VariableArray.Get<Button>(0);
			btn_select = VariableArray.Get<Button>(1);
			btn_del = VariableArray.Get<Button>(2);
			btn_baseElement = VariableArray.Get<Button>(3);
			btn_grid = VariableArray.Get<Button>(4);
			btn_element = VariableArray.Get<Button>(5);
			scrollEx_element = VariableArray.Get<ScrollRectExtension>(6);
		}
	}
}
#endif