/*-------------------------------------
Author:DefaultAuthor
Time:2025/10/9 17:29:56
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class GuideMainWindow
	{
		private GameObject go_root;
		private GameObject go_mask;
		private Image img_mask;
		private GameObject go_animator;
		private RectTransform tf_finger;
		private GameObject go_contentRoot;
		private Image img_ipIcon;
		private TextMeshProUGUI text_content;
		private Image img_itemIcon;
		private Button btn_touch;

		public override void ScriptGenerate()
		{
			go_root = VariableArray.Get<RectTransform>(0).gameObject;
			go_mask = VariableArray.Get<RectTransform>(1).gameObject;
			img_mask = VariableArray.Get<Image>(2);
			go_animator = VariableArray.Get<RectTransform>(3).gameObject;
			tf_finger = VariableArray.Get<RectTransform>(4);
			go_contentRoot = VariableArray.Get<RectTransform>(5).gameObject;
			img_ipIcon = VariableArray.Get<Image>(6);
			text_content = VariableArray.Get<TextMeshProUGUI>(7);
			img_itemIcon = VariableArray.Get<Image>(8);
			btn_touch = VariableArray.Get<Button>(9);
		}
	}
}
