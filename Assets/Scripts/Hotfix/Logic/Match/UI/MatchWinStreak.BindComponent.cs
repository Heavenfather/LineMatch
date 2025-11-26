/*-------------------------------------
Author:DefaultAuthor
Time:2025/9/12 14:18:31
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchWinStreak
	{
		private GameObject go_root;
		private Button btn_winStreakDesc;
		private SlicedFilledImage fillImg_winStreakPro;
		private TextMeshProUGUI text_winStreakPro;
		private TextMeshProUGUI text_title;
		private Image img_arrow;
		private Button btn_descClose;

		public override void ScriptGenerate()
		{
			go_root = VariableArray.Get<RectTransform>(0).gameObject;
			btn_winStreakDesc = VariableArray.Get<Button>(1);
			fillImg_winStreakPro = VariableArray.Get<SlicedFilledImage>(2);
			text_winStreakPro = VariableArray.Get<TextMeshProUGUI>(3);
			text_title = VariableArray.Get<TextMeshProUGUI>(4);
			img_arrow = VariableArray.Get<Image>(5);
			btn_descClose = VariableArray.Get<Button>(6);
		}
	}
}
