/*-------------------------------------
Author:DefaultAuthor
Time:2025/10/11 14:31:23
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchBeginBoosterItem
	{
		private Image img_select;
		private Image img_icon;
		private GameObject go_lock;
		private GameObject go_select;
		private GameObject go_count;
		private Image img_countBg;
		private TextMeshProUGUI text_num;
		private GameObject go_buff;
		private Image img_buffBg;
		private TextMeshProUGUI text_buffTIme;
		private Button btn_select;
		private GameObject go_eff;

		public override void ScriptGenerate()
		{
			img_select = VariableArray.Get<Image>(0);
			img_icon = VariableArray.Get<Image>(1);
			go_lock = VariableArray.Get<RectTransform>(2).gameObject;
			go_select = VariableArray.Get<RectTransform>(3).gameObject;
			go_count = VariableArray.Get<RectTransform>(4).gameObject;
			img_countBg = VariableArray.Get<Image>(5);
			text_num = VariableArray.Get<TextMeshProUGUI>(6);
			go_buff = VariableArray.Get<RectTransform>(7).gameObject;
			img_buffBg = VariableArray.Get<Image>(8);
			text_buffTIme = VariableArray.Get<TextMeshProUGUI>(9);
			btn_select = VariableArray.Get<Button>(10);
			go_eff = VariableArray.Get<RectTransform>(11).gameObject;
		}
	}
}
