using UnityEngine;
using HotfixCore.Module;
using HotfixCore.Extensions;
using HotfixLogic.Match;
using GameConfig;
using UnityEngine.UI;
using GameCore.Localization;
using Hotfix.Define;
using Hotfix.Utils;
using System;
using DG.Tweening;

namespace HotfixLogic
{
	[Window(UILayer.UI,"uiprefab/match/matchexit")]
	public partial class MatchExit : UIWindow
	{
		private bool _hasLiveBuff;
		private int _maxStep = 1;
		private int _curStep = 1;
		private bool _isExit = false;
		Tween _tween;

		public override void OnCreate()
		{
			SetPopTransform(go_root.transform);

			_isExit = (bool)userDatas[0];

			btn_close.AddClick(CloseSelf);
			btn_confirm.AddClick(UpdateStep);

			img_live.gameObject.SetActive(false);
			img_livebuff.gameObject.SetActive(false);
			img_winstreak.gameObject.SetActive(false);
			img_mastertrain.gameObject.SetActive(false);

			_hasLiveBuff = G.GameItemModule.CheckHasBuff("liveBuff");

			InitPanel();
			
		}

        public override void OnDestroy()
        {
            base.OnDestroy();
			if (_tween!= null) {
				_tween.Kill();
				_tween = null;
			}
        }

        private void InitPanel() {
			bool isPlayMaxLv = MatchManager.Instance.CurLevelID == MatchManager.Instance.MaxLevel;

			bool isOpenStreak = MatchManager.Instance.IsOpenWinStreak(MatchManager.Instance.CurLevelID);
			bool hasWinStreak = isOpenStreak && MatchManager.Instance.GetWinStreakBox() > 0;

			var trainMasterWinStreak = G.TrainMasterModule.GetWinStreakNum();
			bool hasMasterTrain = trainMasterWinStreak > 0 && trainMasterWinStreak < 7 && isPlayMaxLv;


			GameObject liveObj = _hasLiveBuff ? img_livebuff.gameObject : img_live.gameObject;
			GameObject winstreakObj = hasWinStreak ? img_winstreak.gameObject : null;
			GameObject mastertrainObj = hasMasterTrain ? img_mastertrain.gameObject : null;


			InitText(_hasLiveBuff, hasWinStreak, hasMasterTrain);


			if (_isExit) {
				liveObj.transform.SetParent(go_tips1.transform);
				liveObj.SetActive(true);

				if (hasWinStreak || hasMasterTrain) {
					if (hasWinStreak) {
						img_winstreak.transform.SetParent(go_tips2.transform);
						img_winstreak.gameObject.SetActive(true);
					}
					
					if (hasMasterTrain) {
						img_mastertrain.transform.SetParent(go_tips2.transform);
						img_mastertrain.gameObject.SetActive(true);
					}
					_maxStep = 2;
				}
			} else {
				if (!hasWinStreak && !hasMasterTrain) {
					liveObj.transform.SetParent(go_tips1.transform);
					liveObj.SetActive(true);
				} else {
					if (liveObj == img_live.gameObject) {
						liveObj.transform.SetParent(go_tips1.transform);
						liveObj.SetActive(true);
					}

					if (hasWinStreak) {
						winstreakObj.transform.SetParent(go_tips1.transform);
						img_winstreak.gameObject.SetActive(true);
					}
					if (hasMasterTrain) {
						mastertrainObj.transform.SetParent(go_tips1.transform);
						img_mastertrain.gameObject.SetActive(true);
					}
				}

				_maxStep = 1;
			}
		}

		private void InitText(bool isLiveBuff, bool hasWinStreak, bool hasMasterTrain) {
			var titleStr = _isExit ? LocalizationPool.Get("Match/Exit/ExitTitle") : LocalizationPool.Get("Common/Restart") + "?";
			text_title.text = titleStr;

			if (_isExit) {
				text_desc.text = isLiveBuff ? LocalizationPool.Get("Match/Exit/Tips2") : LocalizationPool.Get("Match/Exit/Tips1");
			} else {
				if (hasWinStreak || hasMasterTrain) {
					text_desc.text = LocalizationPool.Get("Match/Exit/Tips4");
				} else {
					text_desc.text = isLiveBuff ? LocalizationPool.Get("Match/Exit/Tips3") : LocalizationPool.Get("Match/Exit/Tips1");
				}
			}
			text_btn.text = _isExit ? LocalizationPool.Get("Common/Exit") : LocalizationPool.Get("Common/Confirm");
		}

		private void UpdateStep() {
			if (_curStep == _maxStep) {
				if (_isExit) {
					if (MatchManager.Instance.MaxLevel == MatchManager.Instance.CurLevelID) {
						MatchManager.Instance.SetWinStreak(0);
					}
					G.EventModule.DispatchEvent(GameEventDefine.OnMatchMidwayClose); 
					CloseSelf();
				} else {
					RestartGame();
				}
				return;
			}

			_curStep++;
			AddStepAnim();
		}

		private void AddStepAnim() {
			var rectTransform = (RectTransform)go_layout.transform;
			var layoutSize = rectTransform.sizeDelta;
			var layoutPos = rectTransform.anchoredPosition;

			_tween = rectTransform.DOAnchorPos(new Vector2(layoutPos.x - layoutSize.x, layoutPos.y), 0.3f).OnComplete(() => {
				_tween = null;
				text_desc.text = LocalizationPool.Get("Match/Exit/Tips4");;
			});
		}

		private void RestartGame() {
            if (!CommonUtil.CanBeginGame())
            {
                CommonUtil.ShowCommonTips(LocalizationPool.Get("Common/LiveLack"));
                CommonUtil.ShowItemLackPanel("live", false);
                return;
            }
			ReqStartGame();
		}

		private void ReqStartGame() {
			G.EventModule.DispatchEvent(GameEventDefine.OnMatchResultFail);
			G.HttpModule.ReportLevelGameBegin(MatchManager.Instance.CurLevelID, (result, code) =>
			{
				Debug.Log("ReportLevelGameBegin result:" + result + " code:" + code);
				if (code == 0) {
					G.GameItemModule.GameConsumLive();
					G.EventModule.DispatchEvent(GameEventDefine.OnMatchRestart);
					CloseSelf();
				} else {
					Debug.LogError("ReportLevelGameBegin failed, code:" + code);
				}
			});
		}
	}
}
