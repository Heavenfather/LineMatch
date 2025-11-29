/*-------------------------------------
Author:DefaultAuthor
Time:2025/11/21 18:34:28
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchResultLose
	{
		private GameObject go_root;
		private GameObject go_normal;
		private GameObject go_hard;
		private GameObject go_crazy;
		private TextMeshProUGUI text_title;
		private GameObject go_layout;
		private GameObject go_step;
		private TextMeshProUGUI text_stepTips;
		private Image img_add1;
		private Image img_add2;
		private Image img_add3;
		private CommonRewardItem widget_continueItem1;
		private CommonRewardItem widget_continueItem2;
		private GameObject go_winStreak1;
		private GameObject go_target;
		private Button btn_close;
		private GameObject go_booster;
		private MatchBoosterSelect widget_booster;
		private GameObject go_streak;
		private GameObject go_winStreak2;
		private Button btn_begin;
		private GameObject go_live;
		private Image img_live;
		private Image img_liveBuff;
		private TextMeshProUGUI text_begin;
		private Button btn_continue;
		private TextMeshProUGUI text_buyCoin;
		private Button btn_free;
		private GameObject go_property;
		private CommonWidgetProperty widget_property;
		private Button btn_adv;
		private TextMeshProUGUI text_advStep;
		private GameObject go_gift;
		private Image img_coin;
		private TextMeshProUGUI text_coin;
		private GameObject go_mask;
		private Image img_flyLive;
		private GameObject go_eff;

		public override void ScriptGenerate()
		{
			go_root = VariableArray.Get<RectTransform>(0).gameObject;
			go_normal = VariableArray.Get<RectTransform>(1).gameObject;
			go_hard = VariableArray.Get<RectTransform>(2).gameObject;
			go_crazy = VariableArray.Get<RectTransform>(3).gameObject;
			text_title = VariableArray.Get<TextMeshProUGUI>(4);
			go_layout = VariableArray.Get<RectTransform>(5).gameObject;
			go_step = VariableArray.Get<RectTransform>(6).gameObject;
			text_stepTips = VariableArray.Get<TextMeshProUGUI>(7);
			img_add1 = VariableArray.Get<Image>(8);
			img_add2 = VariableArray.Get<Image>(9);
			img_add3 = VariableArray.Get<Image>(10);
			widget_continueItem1 = base.CreateWidget<CommonRewardItem>(VariableArray.Get<RectTransform>(11).gameObject,VariableArray.Get<RectTransform>(11).gameObject.activeSelf);
			widget_continueItem2 = base.CreateWidget<CommonRewardItem>(VariableArray.Get<RectTransform>(12).gameObject,VariableArray.Get<RectTransform>(12).gameObject.activeSelf);
			go_winStreak1 = VariableArray.Get<RectTransform>(13).gameObject;
			go_target = VariableArray.Get<RectTransform>(14).gameObject;
			btn_close = VariableArray.Get<Button>(15);
			go_booster = VariableArray.Get<RectTransform>(16).gameObject;
			widget_booster = base.CreateWidget<MatchBoosterSelect>(VariableArray.Get<RectTransform>(17).gameObject,VariableArray.Get<RectTransform>(17).gameObject.activeSelf);
			go_streak = VariableArray.Get<RectTransform>(18).gameObject;
			go_winStreak2 = VariableArray.Get<RectTransform>(19).gameObject;
			btn_begin = VariableArray.Get<Button>(20);
			go_live = VariableArray.Get<RectTransform>(21).gameObject;
			img_live = VariableArray.Get<Image>(22);
			img_liveBuff = VariableArray.Get<Image>(23);
			text_begin = VariableArray.Get<TextMeshProUGUI>(24);
			btn_continue = VariableArray.Get<Button>(25);
			text_buyCoin = VariableArray.Get<TextMeshProUGUI>(26);
			btn_free = VariableArray.Get<Button>(27);
			go_property = VariableArray.Get<RectTransform>(28).gameObject;
			widget_property = base.CreateWidget<CommonWidgetProperty>(VariableArray.Get<RectTransform>(29).gameObject,VariableArray.Get<RectTransform>(29).gameObject.activeSelf);
			btn_adv = VariableArray.Get<Button>(30);
			text_advStep = VariableArray.Get<TextMeshProUGUI>(31);
			go_gift = VariableArray.Get<RectTransform>(32).gameObject;
			img_coin = VariableArray.Get<Image>(33);
			text_coin = VariableArray.Get<TextMeshProUGUI>(34);
			go_mask = VariableArray.Get<RectTransform>(35).gameObject;
			img_flyLive = VariableArray.Get<Image>(36);
			go_eff = VariableArray.Get<RectTransform>(37).gameObject;
		}
	}
}
