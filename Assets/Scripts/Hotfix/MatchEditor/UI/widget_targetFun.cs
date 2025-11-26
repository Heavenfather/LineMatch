#if UNITY_EDITOR
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;

namespace HotfixLogic
{
	public partial class widget_targetFun : UIWidget
	{
		private LevelData _levelData;
		
		public override void OnCreate()
		{
			input_score.AddEndEditListener(value =>
			{
				if (int.TryParse(value, out int score))
				{
					G.EventModule.DispatchEvent(MatchEditorConst.OnFullScoreChanged,EventOneParam<int>.Create(score));
				}
			});
			btn_addTarget.AddClick(() =>
			{
				G.EventModule.DispatchEvent(MatchEditorConst.OnAddTargetElement);
			});
			scrollEx_target.SetDelegate(GetTargetCount);
			scrollEx_target.SetRefreshFunc(this,typeof(widget_targetCell),view =>
			{
				if (view.Widget is widget_targetCell cell)
				{
					TargetElement target = _levelData.target[view.DataIndex];
					cell.SetView(target,view.DataIndex);
				}
			});
		}

		private int GetTargetCount()
		{
			return _levelData.target.Length;
		}

		public void UpdateView(LevelData levelData)
		{
			_levelData = levelData;
			input_score.SetTextWithoutNotify(levelData.fullScore.ToString());
			scrollEx_target.Reload();
		}
	}
}
#endif