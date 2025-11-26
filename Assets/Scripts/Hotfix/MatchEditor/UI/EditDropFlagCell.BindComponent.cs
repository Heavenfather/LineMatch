#if UNITY_EDITOR
/*-------------------------------------
Author:DefaultAuthor
Time:2025/9/17 14:53:25
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class EditDropFlagCell
	{
		private TMP_InputField input_id;
		private TMP_InputField input_rate;
		private TMP_InputField input_limit;
		private Button btn_del;

		public override void ScriptGenerate()
		{
			input_id = VariableArray.Get<TMP_InputField>(0);
			input_rate = VariableArray.Get<TMP_InputField>(1);
			input_limit = VariableArray.Get<TMP_InputField>(2);
			btn_del = VariableArray.Get<Button>(3);
		}
	}
}
#endif