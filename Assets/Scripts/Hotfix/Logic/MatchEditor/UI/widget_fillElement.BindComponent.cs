#if UNITY_EDITOR

/*-------------------------------------
Author:DefaultAuthor
Time:2025/6/18 18:22:45
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class widget_fillElement
	{
		private Image img_icon;
		private TextMeshProUGUI text_count;

		public override void ScriptGenerate()
		{
			img_icon = VariableArray.Get<Image>(0);
			text_count = VariableArray.Get<TextMeshProUGUI>(1);
		}
	}
}

#endif