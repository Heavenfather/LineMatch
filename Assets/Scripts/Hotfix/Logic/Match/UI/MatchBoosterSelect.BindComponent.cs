/*-------------------------------------
Author:DefaultAuthor
Time:2025/7/31 14:06:57
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchBoosterSelect
	{
		private GameObject go_root;
		private MatchBeginBoosterItem widget_boosterItem1;
		private MatchBeginBoosterItem widget_boosterItem2;
		private MatchBeginBoosterItem widget_boosterItem3;

		public override void ScriptGenerate()
		{
			go_root = VariableArray.Get<RectTransform>(0).gameObject;
			widget_boosterItem1 = base.CreateWidget<MatchBeginBoosterItem>(VariableArray.Get<RectTransform>(1).gameObject,VariableArray.Get<RectTransform>(1).gameObject.activeSelf);
			widget_boosterItem2 = base.CreateWidget<MatchBeginBoosterItem>(VariableArray.Get<RectTransform>(2).gameObject,VariableArray.Get<RectTransform>(2).gameObject.activeSelf);
			widget_boosterItem3 = base.CreateWidget<MatchBeginBoosterItem>(VariableArray.Get<RectTransform>(3).gameObject,VariableArray.Get<RectTransform>(3).gameObject.activeSelf);
		}
	}
}
