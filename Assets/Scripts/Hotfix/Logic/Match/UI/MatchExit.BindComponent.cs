/*-------------------------------------
Author:DefaultAuthor
Time:2025/9/24 12:08:57
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchExit
	{
		private GameObject go_root;
		private SkeletonGraphic spine_ip;
		private Button btn_close;
		private GameObject go_mask;
		private GameObject go_layout;
		private GameObject go_tips1;
		private GameObject go_tips2;
		private Image img_live;
		private Image img_livebuff;
		private Image img_winstreak;
		private Image img_mastertrain;
		private TextMeshProUGUI text_title;
		private Button btn_confirm;
		private TextMeshProUGUI text_btn;
		private TextMeshProUGUI text_desc;

		public override void ScriptGenerate()
		{
			go_root = VariableArray.Get<RectTransform>(0).gameObject;
			spine_ip = VariableArray.Get<SkeletonGraphic>(1);
			btn_close = VariableArray.Get<Button>(2);
			go_mask = VariableArray.Get<RectTransform>(3).gameObject;
			go_layout = VariableArray.Get<RectTransform>(4).gameObject;
			go_tips1 = VariableArray.Get<RectTransform>(5).gameObject;
			go_tips2 = VariableArray.Get<RectTransform>(6).gameObject;
			img_live = VariableArray.Get<Image>(7);
			img_livebuff = VariableArray.Get<Image>(8);
			img_winstreak = VariableArray.Get<Image>(9);
			img_mastertrain = VariableArray.Get<Image>(10);
			text_title = VariableArray.Get<TextMeshProUGUI>(11);
			btn_confirm = VariableArray.Get<Button>(12);
			text_btn = VariableArray.Get<TextMeshProUGUI>(13);
			text_desc = VariableArray.Get<TextMeshProUGUI>(14);
		}
	}
}
