#if UNITY_EDITOR
/*-------------------------------------
Author:DefaultAuthor
Time:2025/4/28 21:23:20
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchEditorSelectElement
	{
		private ScrollRectExtension scrollEx_elements;
		private Button btn_ok;

		public override void ScriptGenerate()
		{
			scrollEx_elements = VariableArray.Get<ScrollRectExtension>(0);
			btn_ok = VariableArray.Get<Button>(1);
		}
	}
}
#endif