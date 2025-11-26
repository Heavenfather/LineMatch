#if UNITY_EDITOR
/*-------------------------------------
Author:DefaultAuthor
Time:2025/4/27 10:52:50
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class widget_operFun
	{
		private Button btn_preStep;
		private Button btn_nextStep;
		private Button btn_clearAll;
		private Button btn_resume;
		private Button btn_levelPreview;
		private Button btn_jsonPreview;

		public override void ScriptGenerate()
		{
			btn_preStep = VariableArray.Get<Button>(0);
			btn_nextStep = VariableArray.Get<Button>(1);
			btn_clearAll = VariableArray.Get<Button>(2);
			btn_resume = VariableArray.Get<Button>(3);
			btn_levelPreview = VariableArray.Get<Button>(4);
			btn_jsonPreview = VariableArray.Get<Button>(5);
		}
	}
}
#endif