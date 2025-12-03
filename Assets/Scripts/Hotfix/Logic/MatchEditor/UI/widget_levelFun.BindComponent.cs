#if UNITY_EDITOR

/*-------------------------------------
Author:DefaultAuthor
Time:2025/11/18 19:20:56
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class widget_levelFun
	{
		private TMP_InputField input_step;
		private TMP_InputField input_col;
		private TMP_InputField input_row;
		private TMP_Dropdown drop_difficulty;
		private Button btn_diffRandom;
		private TMP_Dropdown drop_matchType;

		public override void ScriptGenerate()
		{
			input_step = VariableArray.Get<TMP_InputField>(0);
			input_col = VariableArray.Get<TMP_InputField>(1);
			input_row = VariableArray.Get<TMP_InputField>(2);
			drop_difficulty = VariableArray.Get<TMP_Dropdown>(3);
			btn_diffRandom = VariableArray.Get<Button>(4);
			drop_matchType = VariableArray.Get<TMP_Dropdown>(5);
		}
	}
}

#endif