#if UNITY_EDITOR
/*-------------------------------------
Author:DefaultAuthor
Time:2025/9/17 17:47:56
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchEditorWindow
	{
		private GameObject go_eleRoot;
		private widget_fileFun widget_fileFun;
		private widget_operFun widget_operFun;
		private widget_levelFun widget_levelFun;
		private widget_targetFun widget_targetFun;
		private widget_elementFun widget_elementFun;
		private GameObject go_fillElement;
		private GameObject go_wait;
		private TextMeshProUGUI text_handleFiles;

		public override void ScriptGenerate()
		{
			go_eleRoot = VariableArray.Get<RectTransform>(0).gameObject;
			widget_fileFun = base.CreateWidget<widget_fileFun>(VariableArray.Get<RectTransform>(1).gameObject,VariableArray.Get<RectTransform>(1).gameObject.activeSelf);
			widget_operFun = base.CreateWidget<widget_operFun>(VariableArray.Get<RectTransform>(2).gameObject,VariableArray.Get<RectTransform>(2).gameObject.activeSelf);
			widget_levelFun = base.CreateWidget<widget_levelFun>(VariableArray.Get<RectTransform>(3).gameObject,VariableArray.Get<RectTransform>(3).gameObject.activeSelf);
			widget_targetFun = base.CreateWidget<widget_targetFun>(VariableArray.Get<RectTransform>(4).gameObject,VariableArray.Get<RectTransform>(4).gameObject.activeSelf);
			widget_elementFun = base.CreateWidget<widget_elementFun>(VariableArray.Get<RectTransform>(5).gameObject,VariableArray.Get<RectTransform>(5).gameObject.activeSelf);
			go_fillElement = VariableArray.Get<RectTransform>(6).gameObject;
			go_wait = VariableArray.Get<RectTransform>(7).gameObject;
			text_handleFiles = VariableArray.Get<TextMeshProUGUI>(8);
		}
	}
}
#endif