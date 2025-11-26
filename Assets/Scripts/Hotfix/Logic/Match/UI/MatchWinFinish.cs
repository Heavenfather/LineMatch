using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using Hotfix.Utils;
using Spine.Unity;
using Spine;
using System;
using AudioType = GameCore.Settings.AudioType;

namespace HotfixLogic
{
	[Window(UILayer.UI,"uiprefab/match/matchwinfinish")]
	public partial class MatchWinFinish : UIWindow
	{
		Action _callback;

		public override void OnCreate()
		{
			AudioUtil.PlayMatchWin();
			AudioUtil.PlaySound("audio/match/match_finish");

			btn_close.AddClick(OnClickClose);

			spine_ip.AnimationState.Complete += OnSpineAnimComplete;

			_callback = userDatas[0] as Action;
		}

		private void OnClickClose() {
			_callback?.Invoke();
			CloseSelf();
		}

		private void OnSpineAnimComplete(TrackEntry trackEntry) {
			OnClickClose();
		}

		private void StopAudio() {
			G.AudioModule.Stop(AudioType.Sound, true);
		}
	}

}
