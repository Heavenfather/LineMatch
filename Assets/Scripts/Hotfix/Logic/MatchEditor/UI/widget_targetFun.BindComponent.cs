#if UNITY_EDITOR

/*-------------------------------------
Author:DefaultAuthor
Time:2025/4/28 20:41:18
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class widget_targetFun
	{
		private TMP_InputField input_score;
		private Button btn_addTarget;
		private ScrollRectExtension scrollEx_target;

		public override void ScriptGenerate()
		{
			input_score = VariableArray.Get<TMP_InputField>(0);
			btn_addTarget = VariableArray.Get<Button>(1);
			scrollEx_target = VariableArray.Get<ScrollRectExtension>(2);
		}
	}
}
#endif