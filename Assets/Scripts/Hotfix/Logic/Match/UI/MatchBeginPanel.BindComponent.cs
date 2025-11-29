/*-------------------------------------
Author:DefaultAuthor
Time:2025/11/24 14:51:41
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchBeginPanel
	{
		private GameObject go_property;
		private CommonWidgetProperty widget_property;
		private GameObject go_root;
		private GameObject go_normal;
		private TextMeshProUGUI text_levelNormal;
		private GameObject go_hard;
		private SkeletonGraphic spine_fireHard;
		private TextMeshProUGUI text_levelHard;
		private GameObject go_crazy;
		private SkeletonGraphic spine_fireCray;
		private TextMeshProUGUI text_levelCrazy;
		private Button btn_close;
		private GameObject go_target;
		private MatchBeginTargetWidget widget_target;
		private TextMeshProUGUI text_moveCount;
		private GameObject go_winstreak;
		private MatchWinStreak widget_winStreak;
		private GameObject go_booster;
		private MatchBoosterSelect widget_booster;
		private Button btn_begin;
		private GameObject go_btnLayout;
		private Image img_liveBuff;
		private GameObject go_live;
		private Image img_live;
		private GameObject go_tips;
		private TextMeshProUGUI text_tips;
		private SkeletonGraphic spine_touch;
		private GameObject go_mask;
		private Image img_flyLive;
		private GameObject go_eff;

		public override void ScriptGenerate()
		{
			go_property = VariableArray.Get<RectTransform>(0).gameObject;
			widget_property = base.CreateWidget<CommonWidgetProperty>(VariableArray.Get<RectTransform>(1).gameObject,VariableArray.Get<RectTransform>(1).gameObject.activeSelf);
			go_root = VariableArray.Get<RectTransform>(2).gameObject;
			go_normal = VariableArray.Get<RectTransform>(3).gameObject;
			text_levelNormal = VariableArray.Get<TextMeshProUGUI>(4);
			go_hard = VariableArray.Get<RectTransform>(5).gameObject;
			spine_fireHard = VariableArray.Get<SkeletonGraphic>(6);
			text_levelHard = VariableArray.Get<TextMeshProUGUI>(7);
			go_crazy = VariableArray.Get<RectTransform>(8).gameObject;
			spine_fireCray = VariableArray.Get<SkeletonGraphic>(9);
			text_levelCrazy = VariableArray.Get<TextMeshProUGUI>(10);
			btn_close = VariableArray.Get<Button>(11);
			go_target = VariableArray.Get<RectTransform>(12).gameObject;
			widget_target = base.CreateWidget<MatchBeginTargetWidget>(VariableArray.Get<RectTransform>(13).gameObject,VariableArray.Get<RectTransform>(13).gameObject.activeSelf);
			text_moveCount = VariableArray.Get<TextMeshProUGUI>(14);
			go_winstreak = VariableArray.Get<RectTransform>(15).gameObject;
			widget_winStreak = base.CreateWidget<MatchWinStreak>(VariableArray.Get<RectTransform>(16).gameObject,VariableArray.Get<RectTransform>(16).gameObject.activeSelf);
			go_booster = VariableArray.Get<RectTransform>(17).gameObject;
			widget_booster = base.CreateWidget<MatchBoosterSelect>(VariableArray.Get<RectTransform>(18).gameObject,VariableArray.Get<RectTransform>(18).gameObject.activeSelf);
			btn_begin = VariableArray.Get<Button>(19);
			go_btnLayout = VariableArray.Get<RectTransform>(20).gameObject;
			img_liveBuff = VariableArray.Get<Image>(21);
			go_live = VariableArray.Get<RectTransform>(22).gameObject;
			img_live = VariableArray.Get<Image>(23);
			go_tips = VariableArray.Get<RectTransform>(24).gameObject;
			text_tips = VariableArray.Get<TextMeshProUGUI>(25);
			spine_touch = VariableArray.Get<SkeletonGraphic>(26);
			go_mask = VariableArray.Get<RectTransform>(27).gameObject;
			img_flyLive = VariableArray.Get<Image>(28);
			go_eff = VariableArray.Get<RectTransform>(29).gameObject;
		}
	}
}
