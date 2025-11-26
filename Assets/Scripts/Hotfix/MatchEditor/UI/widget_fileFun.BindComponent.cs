#if UNITY_EDITOR

/*-------------------------------------
Author:DefaultAuthor
Time:2025/8/11 11:14:36
--------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace HotfixLogic
{
	public partial class widget_fileFun
	{
		private Button btn_import;
		private TMP_InputField input_levelPath;
		private Button btn_export;
		private TextMeshProUGUI text_filePath;
		private TextMeshProUGUI text_lvPath;
		private TextMeshProUGUI text_jsonName;
		private Button btn_save;
		private Button btn_saveAndNew;
		private TMP_InputField input_levelId;
		private TMP_InputField input_cosLevel;
		private Button btn_preLevel;
		private Button btn_nextLevel;
		private Button btn_addElement;
		private GameObject go_elementCell;
		private GameObject go_baseElementRoot;
		private Button btn_dropElement;
		private GameObject go_dropElementRoot;

		public override void ScriptGenerate()
		{
			btn_import = VariableArray.Get<Button>(0);
			input_levelPath = VariableArray.Get<TMP_InputField>(1);
			btn_export = VariableArray.Get<Button>(2);
			text_filePath = VariableArray.Get<TextMeshProUGUI>(3);
			text_lvPath = VariableArray.Get<TextMeshProUGUI>(4);
			text_jsonName = VariableArray.Get<TextMeshProUGUI>(5);
			btn_save = VariableArray.Get<Button>(6);
			btn_saveAndNew = VariableArray.Get<Button>(7);
			input_levelId = VariableArray.Get<TMP_InputField>(8);
			input_cosLevel = VariableArray.Get<TMP_InputField>(9);
			btn_preLevel = VariableArray.Get<Button>(10);
			btn_nextLevel = VariableArray.Get<Button>(11);
			btn_addElement = VariableArray.Get<Button>(12);
			go_elementCell = VariableArray.Get<RectTransform>(13).gameObject;
			go_baseElementRoot = VariableArray.Get<RectTransform>(14).gameObject;
			btn_dropElement = VariableArray.Get<Button>(15);
			go_dropElementRoot = VariableArray.Get<RectTransform>(16).gameObject;
		}
	}
}

#endif