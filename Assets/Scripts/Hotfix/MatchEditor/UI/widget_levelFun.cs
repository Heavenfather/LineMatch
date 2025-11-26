#if UNITY_EDITOR
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;

namespace HotfixLogic
{
	public partial class widget_levelFun : UIWidget
	{
		public override void OnCreate()
		{
			input_step.AddEndEditListener((value) =>
			{
				if (int.TryParse(value, out int step))
				{
					G.EventModule.DispatchEvent(MatchEditorConst.OnLevelStepChanged, EventOneParam<int>.Create(step));
				}
			});
			
			drop_difficulty.AddValueChangedListener((value) =>
			{
				G.EventModule.DispatchEvent(MatchEditorConst.OnDifficultyChanged,EventOneParam<int>.Create(value + 1));
			});
			
			drop_matchType.AddValueChangedListener((value) =>
			{
				G.EventModule.DispatchEvent(MatchEditorConst.OnMatchTypeChanged,EventOneParam<int>.Create(value));
			});
			int matchType = PlayerPrefsUtil.GetInt(MatchEditorConst.EditorMatchGameType, 0);
			drop_matchType.value = matchType;
			
			input_row.AddEndEditListener((value) =>
			{
				OnGridSizeChanged();
			});
			
			input_col.AddEndEditListener((value) =>
			{
				OnGridSizeChanged();
			});
			
			btn_diffRandom.AddClick(() =>
			{
				G.EventModule.DispatchEvent(MatchEditorConst.OnRandomDifficulty);
			});
		}

		public void UpdateView(LevelData levelData)
		{
			drop_difficulty.SetValueWithoutNotify(levelData.difficulty - 1);
			input_step.SetTextWithoutNotify(levelData.stepLimit.ToString());
			input_col.SetTextWithoutNotify(levelData.gridCol.ToString());
			input_row.SetTextWithoutNotify(levelData.gridRow.ToString());
		}

		private void OnGridSizeChanged()
		{
			if (int.TryParse(input_col.text, out int col) && int.TryParse(input_row.text, out int row))
			{
				G.EventModule.DispatchEvent(MatchEditorConst.OnLevelGridCountChanged, EventTwoParam<int, int>.Create(col, row));
			}
		}
	}
}
#endif