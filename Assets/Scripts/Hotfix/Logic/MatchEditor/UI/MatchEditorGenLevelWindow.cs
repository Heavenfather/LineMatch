#if UNITY_EDITOR
using System;
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;
using Random = UnityEngine.Random;

namespace HotfixLogic
{
	[Window(UILayer.UI,"uiprefab/matcheditor/matcheditorgenlevelwindow")]
	public partial class MatchEditorGenLevelWindow : UIWindow
	{
		private Action _okCallback;
		private LevelData _levelData;
		
		public override void OnCreate()
		{
			btn_bg.AddClick(CloseSelf);
			btn_ok.AddClick(() =>
			{
				_okCallback?.Invoke();
				CloseSelf();
			});
			
			input_height.AddEndEditListener(value =>
			{
				if (int.TryParse(value, out int result))
				{
					_levelData.gridRow = result;
				}
			});
			input_wid.AddEndEditListener((value =>
			{
				if (int.TryParse(value, out int result))
				{
					_levelData.gridCol = result;
				}
			}));
			
			btn_ranHeight.AddClick(() =>
			{
				int height = Random.Range(6, 11);
				_levelData.gridRow = height;
				input_height.text = _levelData.gridRow.ToString();
			});
			
			btn_ranWid.AddClick(() =>
			{
				int wid = Random.Range(6, 11);
				_levelData.gridCol = wid;
				input_wid.text = _levelData.gridCol.ToString();
			});
		}

		public void SetView(LevelData levelData, Action okCallback)
		{
			_levelData = levelData;
			_okCallback = okCallback;

			text_difficulty.text = $"难度:{levelData.difficulty}";
			text_level.text = $"关卡:{_levelData.id}";
			input_wid.SetTextWithoutNotify(levelData.gridCol.ToString());
			input_height.SetTextWithoutNotify(levelData.gridRow.ToString());
		}
	}
}

#endif
