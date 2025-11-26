/*-------------------------------------
Author:DefaultAuthor
Time:2025/8/7 17:16:38
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchBeginTargetWidget
	{
		private Image img_icon1;
		private TextMeshProUGUI text_count1;
		private Image img_finish1;
		private Image img_error1;
		private Image img_icon2;
		private TextMeshProUGUI text_count2;
		private Image img_finish2;
		private Image img_error2;
		private Image img_icon3;
		private TextMeshProUGUI text_count3;
		private Image img_finish3;
		private Image img_error3;
		private Image img_icon4;
		private TextMeshProUGUI text_count4;
		private Image img_finish4;
		private Image img_error4;

		public override void ScriptGenerate()
		{
			img_icon1 = VariableArray.Get<Image>(0);
			text_count1 = VariableArray.Get<TextMeshProUGUI>(1);
			img_finish1 = VariableArray.Get<Image>(2);
			img_error1 = VariableArray.Get<Image>(3);
			img_icon2 = VariableArray.Get<Image>(4);
			text_count2 = VariableArray.Get<TextMeshProUGUI>(5);
			img_finish2 = VariableArray.Get<Image>(6);
			img_error2 = VariableArray.Get<Image>(7);
			img_icon3 = VariableArray.Get<Image>(8);
			text_count3 = VariableArray.Get<TextMeshProUGUI>(9);
			img_finish3 = VariableArray.Get<Image>(10);
			img_error3 = VariableArray.Get<Image>(11);
			img_icon4 = VariableArray.Get<Image>(12);
			text_count4 = VariableArray.Get<TextMeshProUGUI>(13);
			img_finish4 = VariableArray.Get<Image>(14);
			img_error4 = VariableArray.Get<Image>(15);
		}
	}
}
