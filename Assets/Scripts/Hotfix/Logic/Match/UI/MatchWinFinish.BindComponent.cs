/*-------------------------------------
Author:DefaultAuthor
Time:2025/9/22 17:24:25
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class MatchWinFinish
	{
		private GameObject go_finish;
		private SkeletonGraphic spine_ip;
		private Button btn_close;
		private GameObject go_top;
		private GameObject go_finishTitle;

		public override void ScriptGenerate()
		{
			go_finish = VariableArray.Get<RectTransform>(0).gameObject;
			spine_ip = VariableArray.Get<SkeletonGraphic>(1);
			btn_close = VariableArray.Get<Button>(2);
			go_top = VariableArray.Get<RectTransform>(3).gameObject;
			go_finishTitle = VariableArray.Get<RectTransform>(4).gameObject;
		}
	}
}
