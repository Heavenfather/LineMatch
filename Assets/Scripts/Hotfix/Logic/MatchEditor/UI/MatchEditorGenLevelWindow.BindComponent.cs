#if UNITY_EDITOR
/*-------------------------------------
Author:DefaultAuthor
Time:2025/5/8 10:59:52
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchEditorGenLevelWindow
	{
		private Button btn_bg;
		private Button btn_ok;
		private Button btn_ranHeight;
		private Button btn_ranWid;
		private TMP_InputField input_height;
		private TMP_InputField input_wid;
		private TextMeshProUGUI text_difficulty;
		private TextMeshProUGUI text_level;

		public override void ScriptGenerate()
		{
			btn_bg = VariableArray.Get<Button>(0);
			btn_ok = VariableArray.Get<Button>(1);
			btn_ranHeight = VariableArray.Get<Button>(2);
			btn_ranWid = VariableArray.Get<Button>(3);
			input_height = VariableArray.Get<TMP_InputField>(4);
			input_wid = VariableArray.Get<TMP_InputField>(5);
			text_difficulty = VariableArray.Get<TextMeshProUGUI>(6);
			text_level = VariableArray.Get<TextMeshProUGUI>(7);
		}
	}
}
#endif