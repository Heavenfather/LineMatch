/*-------------------------------------
Author:DefaultAuthor
Time:2025/8/22 11:02:30
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchGMChangeBoard
	{
		private GameObject go_root;
		private Button btn_close;
		private TMP_InputField input_bgColor;
		private TMP_InputField input_green;
		private TMP_InputField input_blue;
		private TMP_InputField input_yellow;
		private TMP_InputField input_red;
		private TMP_InputField input_purple;
		private TMP_InputField input_orange;
		private TMP_InputField input_cycan;
		private Button btn_ok;
		private Button btn_reset;

		public override void ScriptGenerate()
		{
			go_root = VariableArray.Get<RectTransform>(0).gameObject;
			btn_close = VariableArray.Get<Button>(1);
			input_bgColor = VariableArray.Get<TMP_InputField>(2);
			input_green = VariableArray.Get<TMP_InputField>(3);
			input_blue = VariableArray.Get<TMP_InputField>(4);
			input_yellow = VariableArray.Get<TMP_InputField>(5);
			input_red = VariableArray.Get<TMP_InputField>(6);
			input_purple = VariableArray.Get<TMP_InputField>(7);
			input_orange = VariableArray.Get<TMP_InputField>(8);
			input_cycan = VariableArray.Get<TMP_InputField>(9);
			btn_ok = VariableArray.Get<Button>(10);
			btn_reset = VariableArray.Get<Button>(11);
		}
	}
}
