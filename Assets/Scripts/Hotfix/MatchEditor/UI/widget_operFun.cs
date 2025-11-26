#if UNITY_EDITOR
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;

namespace HotfixLogic
{
	public partial class widget_operFun : UIWidget
	{
		public override void OnCreate()
		{
			btn_clearAll.AddClick(() =>
			{
				G.EventModule.DispatchEvent(MatchEditorConst.OnClearAllElement);
			});
			
			btn_preStep.AddClick(() =>
			{
				G.EventModule.DispatchEvent(MatchEditorConst.OnPreStepClick);
			});
			
			btn_nextStep.AddClick(() =>
			{
				G.EventModule.DispatchEvent(MatchEditorConst.OnNextStepClick);
			});
			
			btn_levelPreview.AddClick(() =>
			{
				G.EventModule.DispatchEvent(MatchEditorConst.OnLevelPreviewClick);
			});
		}
	}
}
#endif