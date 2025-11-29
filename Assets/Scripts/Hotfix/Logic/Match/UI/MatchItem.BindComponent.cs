/*-------------------------------------
Author:DefaultAuthor
Time:2025/11/6 14:53:51
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchItem
	{
		private CommonRewardItem widget_item;
		private Image img_numBg;
		private Button btn_add;
		private TextMeshProUGUI text_num;
		private Image img_buff;
		private TextMeshProUGUI text_buff;
		private GameObject go_lock;
		private Button btn_lock;

		public override void ScriptGenerate()
		{
			widget_item = base.CreateWidget<CommonRewardItem>(VariableArray.Get<RectTransform>(0).gameObject,VariableArray.Get<RectTransform>(0).gameObject.activeSelf);
			img_numBg = VariableArray.Get<Image>(1);
			btn_add = VariableArray.Get<Button>(2);
			text_num = VariableArray.Get<TextMeshProUGUI>(3);
			img_buff = VariableArray.Get<Image>(4);
			text_buff = VariableArray.Get<TextMeshProUGUI>(5);
			go_lock = VariableArray.Get<RectTransform>(6).gameObject;
			btn_lock = VariableArray.Get<Button>(7);
		}
	}
}
