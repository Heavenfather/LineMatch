#if UNITY_EDITOR
/*-------------------------------------
Author:DefaultAuthor
Time:2025/4/29 17:02:55
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class widget_elementCell
	{
		private Image img_icon;
		private GameObject go_select;
		private Button btn_cell;
		private TextMeshProUGUI text_id;

		public override void ScriptGenerate()
		{
			img_icon = VariableArray.Get<Image>(0);
			go_select = VariableArray.Get<RectTransform>(1).gameObject;
			btn_cell = VariableArray.Get<Button>(2);
			text_id = VariableArray.Get<TextMeshProUGUI>(3);
		}
	}
}
#endif