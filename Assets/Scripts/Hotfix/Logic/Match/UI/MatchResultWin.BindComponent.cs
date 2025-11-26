/*-------------------------------------
Author:DefaultAuthor
Time:2025/11/19 18:01:37
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchResultWin
	{
		private GameObject go_win;
		private SkeletonGraphic spine_bg;
		private Button btn_close;
		private TextMeshProUGUI text_close;
		private GameObject go_winStreak;
		private GameObject go_matchWinStreak;
		private GameObject go_coin;
		private Button btn_adv;
		private TextMeshProUGUI text_maxTIps;
		private TextMeshProUGUI text_advCoin;
		private GameObject go_jinbiEff;
		private GameObject go_multiple;
		private Image img_multiple;
		private TextMeshProUGUI text_coin;
		private TextMeshProUGUI text_multi1;
		private TextMeshProUGUI text_multi5;
		private TextMeshProUGUI text_multi2;
		private TextMeshProUGUI text_multi3;
		private TextMeshProUGUI text_multi4;
		private Button btn_continue2;
		private Button btn_continue;
		private SkeletonGraphic spine_touch;
		private GameObject go_continue;
		private SkeletonGraphic spine_continue;
		private Button btn_continue1;
		private GameObject go_top;
		private GameObject go_start;
		private TextMeshProUGUI text_title;
		private TextMeshProUGUI text_score;
		private Image img_starBg1;
		private Image img_star1;
		private GameObject go_effStar1;
		private Image img_starBg2;
		private Image img_star2;
		private GameObject go_effStar2;
		private Image img_starBg3;
		private Image img_star3;
		private GameObject go_effStar3;

		public override void ScriptGenerate()
		{
			go_win = VariableArray.Get<RectTransform>(0).gameObject;
			spine_bg = VariableArray.Get<SkeletonGraphic>(1);
			btn_close = VariableArray.Get<Button>(2);
			text_close = VariableArray.Get<TextMeshProUGUI>(3);
			go_winStreak = VariableArray.Get<RectTransform>(4).gameObject;
			go_matchWinStreak = VariableArray.Get<RectTransform>(5).gameObject;
			go_coin = VariableArray.Get<RectTransform>(6).gameObject;
			btn_adv = VariableArray.Get<Button>(7);
			text_maxTIps = VariableArray.Get<TextMeshProUGUI>(8);
			text_advCoin = VariableArray.Get<TextMeshProUGUI>(9);
			go_jinbiEff = VariableArray.Get<RectTransform>(10).gameObject;
			go_multiple = VariableArray.Get<RectTransform>(11).gameObject;
			img_multiple = VariableArray.Get<Image>(12);
			text_coin = VariableArray.Get<TextMeshProUGUI>(13);
			text_multi1 = VariableArray.Get<TextMeshProUGUI>(14);
			text_multi5 = VariableArray.Get<TextMeshProUGUI>(15);
			text_multi2 = VariableArray.Get<TextMeshProUGUI>(16);
			text_multi3 = VariableArray.Get<TextMeshProUGUI>(17);
			text_multi4 = VariableArray.Get<TextMeshProUGUI>(18);
			btn_continue2 = VariableArray.Get<Button>(19);
			btn_continue = VariableArray.Get<Button>(20);
			spine_touch = VariableArray.Get<SkeletonGraphic>(21);
			go_continue = VariableArray.Get<RectTransform>(22).gameObject;
			spine_continue = VariableArray.Get<SkeletonGraphic>(23);
			btn_continue1 = VariableArray.Get<Button>(24);
			go_top = VariableArray.Get<RectTransform>(25).gameObject;
			go_start = VariableArray.Get<RectTransform>(26).gameObject;
			text_title = VariableArray.Get<TextMeshProUGUI>(27);
			text_score = VariableArray.Get<TextMeshProUGUI>(28);
			img_starBg1 = VariableArray.Get<Image>(29);
			img_star1 = VariableArray.Get<Image>(30);
			go_effStar1 = VariableArray.Get<RectTransform>(31).gameObject;
			img_starBg2 = VariableArray.Get<Image>(32);
			img_star2 = VariableArray.Get<Image>(33);
			go_effStar2 = VariableArray.Get<RectTransform>(34).gameObject;
			img_starBg3 = VariableArray.Get<Image>(35);
			img_star3 = VariableArray.Get<Image>(36);
			go_effStar3 = VariableArray.Get<RectTransform>(37).gameObject;
		}
	}
}
