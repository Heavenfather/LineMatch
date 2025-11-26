#if UNITY_EDITOR
/*-------------------------------------
Author:DefaultAuthor
Time:2025/9/17 16:50:36
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchEditorEditDropFlag
	{
		private Button btn_bg;
		private Button btn_ok;
		private TMP_InputField input_xPos;
		private Button btn_addElement;
		private GameObject go_elemenCell;
		private GameObject go_elementRoot;

		public override void ScriptGenerate()
		{
			btn_bg = VariableArray.Get<Button>(0);
			btn_ok = VariableArray.Get<Button>(1);
			input_xPos = VariableArray.Get<TMP_InputField>(2);
			btn_addElement = VariableArray.Get<Button>(3);
			go_elemenCell = VariableArray.Get<RectTransform>(4).gameObject;
			go_elementRoot = VariableArray.Get<RectTransform>(5).gameObject;
		}
	}
}
#endif