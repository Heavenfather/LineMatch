/*-------------------------------------
Author:DefaultAuthor
Time:2025/12/1 18:23:04
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchMainWindow
	{
		private GameObject go_root;
		private GameObject go_top;
		private Image img_starProgress;
		private GameObject go_effProgress;
		private GameObject go_effProgressFrame;
		private GameObject go_star3;
		private GameObject go_star2;
		private GameObject go_star1;
		private TextMeshProUGUI text_score;
		private TextMeshProUGUI text_left;
		private GameObject go_text;
		private TextMeshProUGUI text_step;
		private GameObject go_stepEff;
		private GameObject go_targetRoot;
		private GameObject go_targetCell;
		private GameObject go_coinCell;
		private GameObject go_ipMask;
		private SkeletonGraphic spine_success;
		private SkeletonGraphic spine_fail;
		private SkeletonGraphic spine_hello;
		private SkeletonGraphic spine_idle;
		private SkeletonGraphic spine_triple;
		private SkeletonGraphic spine_warm;
		private GameObject go_bottom;
		private Button btn_settings;
		private TextMeshProUGUI text_lv;
		private Button btn_gmColor;
		private GameObject go_items;
		private MatchItem widget_matchItem1;
		private MatchItem widget_matchItem2;
		private MatchItem widget_matchItem3;
		private MatchItem widget_matchItem4;
		private MatchItem widget_matchItem5;
		private TextMeshProUGUI text_popTips;
		private GameObject go_elementRoot;
		private Image img_elementIcon;
		private TextMeshProUGUI text_topTips;
		private TextMeshProUGUI text_bottomTips;
		private GameObject go_bottomTip;
		private TextMeshProUGUI text_centerTips;
		private Button btn_guideLevelFinish;
		private GameObject go_beginTarget;
		private CanvasGroup canvasGroup_beginTarget;
		private GameObject go_targetLayout;
		private GameObject go_flyTarget;
		private GameObject go_line;
		private GameObject go_hLine;
		private Image img_line1;
		private Image img_line2;
		private GameObject go_vLine1;
		private Image img_line3;
		private Image img_line4;
		private GameObject go_vLine2;
		private Image img_line5;
		private Image img_line6;

		public override void ScriptGenerate()
		{
			go_root = VariableArray.Get<RectTransform>(0).gameObject;
			go_top = VariableArray.Get<RectTransform>(1).gameObject;
			img_starProgress = VariableArray.Get<Image>(2);
			go_effProgress = VariableArray.Get<RectTransform>(3).gameObject;
			go_effProgressFrame = VariableArray.Get<RectTransform>(4).gameObject;
			go_star3 = VariableArray.Get<RectTransform>(5).gameObject;
			go_star2 = VariableArray.Get<RectTransform>(6).gameObject;
			go_star1 = VariableArray.Get<RectTransform>(7).gameObject;
			text_score = VariableArray.Get<TextMeshProUGUI>(8);
			text_left = VariableArray.Get<TextMeshProUGUI>(9);
			go_text = VariableArray.Get<RectTransform>(10).gameObject;
			text_step = VariableArray.Get<TextMeshProUGUI>(11);
			go_stepEff = VariableArray.Get<RectTransform>(12).gameObject;
			go_targetRoot = VariableArray.Get<RectTransform>(13).gameObject;
			go_targetCell = VariableArray.Get<RectTransform>(14).gameObject;
			go_coinCell = VariableArray.Get<RectTransform>(15).gameObject;
			go_ipMask = VariableArray.Get<RectTransform>(16).gameObject;
			spine_success = VariableArray.Get<SkeletonGraphic>(17);
			spine_fail = VariableArray.Get<SkeletonGraphic>(18);
			spine_hello = VariableArray.Get<SkeletonGraphic>(19);
			spine_idle = VariableArray.Get<SkeletonGraphic>(20);
			spine_triple = VariableArray.Get<SkeletonGraphic>(21);
			spine_warm = VariableArray.Get<SkeletonGraphic>(22);
			go_bottom = VariableArray.Get<RectTransform>(23).gameObject;
			btn_settings = VariableArray.Get<Button>(24);
			text_lv = VariableArray.Get<TextMeshProUGUI>(25);
			btn_gmColor = VariableArray.Get<Button>(26);
			go_items = VariableArray.Get<RectTransform>(27).gameObject;
			widget_matchItem1 = base.CreateWidget<MatchItem>(VariableArray.Get<RectTransform>(28).gameObject,VariableArray.Get<RectTransform>(28).gameObject.activeSelf);
			widget_matchItem2 = base.CreateWidget<MatchItem>(VariableArray.Get<RectTransform>(29).gameObject,VariableArray.Get<RectTransform>(29).gameObject.activeSelf);
			widget_matchItem3 = base.CreateWidget<MatchItem>(VariableArray.Get<RectTransform>(30).gameObject,VariableArray.Get<RectTransform>(30).gameObject.activeSelf);
			widget_matchItem4 = base.CreateWidget<MatchItem>(VariableArray.Get<RectTransform>(31).gameObject,VariableArray.Get<RectTransform>(31).gameObject.activeSelf);
			widget_matchItem5 = base.CreateWidget<MatchItem>(VariableArray.Get<RectTransform>(32).gameObject,VariableArray.Get<RectTransform>(32).gameObject.activeSelf);
			text_popTips = VariableArray.Get<TextMeshProUGUI>(33);
			go_elementRoot = VariableArray.Get<RectTransform>(34).gameObject;
			img_elementIcon = VariableArray.Get<Image>(35);
			text_topTips = VariableArray.Get<TextMeshProUGUI>(36);
			text_bottomTips = VariableArray.Get<TextMeshProUGUI>(37);
			go_bottomTip = VariableArray.Get<RectTransform>(38).gameObject;
			text_centerTips = VariableArray.Get<TextMeshProUGUI>(39);
			btn_guideLevelFinish = VariableArray.Get<Button>(40);
			go_beginTarget = VariableArray.Get<RectTransform>(41).gameObject;
			canvasGroup_beginTarget = VariableArray.Get<CanvasGroup>(42);
			go_targetLayout = VariableArray.Get<RectTransform>(43).gameObject;
			go_flyTarget = VariableArray.Get<RectTransform>(44).gameObject;
			go_line = VariableArray.Get<RectTransform>(45).gameObject;
			go_hLine = VariableArray.Get<RectTransform>(46).gameObject;
			img_line1 = VariableArray.Get<Image>(47);
			img_line2 = VariableArray.Get<Image>(48);
			go_vLine1 = VariableArray.Get<RectTransform>(49).gameObject;
			img_line3 = VariableArray.Get<Image>(50);
			img_line4 = VariableArray.Get<Image>(51);
			go_vLine2 = VariableArray.Get<RectTransform>(52).gameObject;
			img_line5 = VariableArray.Get<Image>(53);
			img_line6 = VariableArray.Get<Image>(54);
		}
	}
}
