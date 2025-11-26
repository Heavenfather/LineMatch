#if UNITY_EDITOR
/*-------------------------------------
Author:DefaultAuthor
Time:2025/7/18 19:22:50
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchEditorEditFillBlock
	{
		private Button btn_ok;
		private Button btn_cancel;
		private TextMeshProUGUI text_Height;
		private TMP_InputField input_height;
		private TMP_InputField input_wid;
		private TMP_InputField input_target;
		private TMP_InputField input_targetNum;

		public override void ScriptGenerate()
		{
			btn_ok = VariableArray.Get<Button>(0);
			btn_cancel = VariableArray.Get<Button>(1);
			text_Height = VariableArray.Get<TextMeshProUGUI>(2);
			input_height = VariableArray.Get<TMP_InputField>(3);
			input_wid = VariableArray.Get<TMP_InputField>(4);
			input_target = VariableArray.Get<TMP_InputField>(5);
			input_targetNum = VariableArray.Get<TMP_InputField>(6);
		}
	}
}
#endif