#if UNITY_EDITOR

/*-------------------------------------
Author:DefaultAuthor
Time:2025/5/15 14:30:34
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class widget_targetCell
	{
		private Image img_icon;
		private TMP_InputField input_num;
		private Button btn_del;

		public override void ScriptGenerate()
		{
			img_icon = VariableArray.Get<Image>(0);
			input_num = VariableArray.Get<TMP_InputField>(1);
			btn_del = VariableArray.Get<Button>(2);
		}
	}
}
#endif