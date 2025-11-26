using UnityEngine;
using HotfixCore.Module;
using GameCore.Localization;
using GameConfig;
using Hotfix.Define;
using Cysharp.Threading.Tasks;
using HotfixLogic.Match;
using System.Collections.Generic;
using Hotfix.Utils;
using HotfixCore.Extensions;
using System;
using DG.Tweening;
using UnityEngine.UI;
using Hotfix.EventParameter;

namespace HotfixLogic
{
	[Window(UILayer.UI,"uiprefab/match/matchbeginpanel")]
	public partial class MatchBeginPanel : UIWindow
	{
		private MatchLevelType _matchType;

		private int _beginLVID;

		Tween _seq;

		float _touchTime = 0;

		public override void OnCreate()
		{
			btn_close.AddClick(CloseSelf);
			btn_begin.AddClick(BeginMatch);

			_matchType = MatchManager.Instance.CurrentMatchLevelType;
			_beginLVID = (int)UserDatas[0];

			go_tips.SetActive(false);
			
			SetLevel(_beginLVID).Forget();
			SetWinStreak();

			SetPopTransform(go_root.transform);

			ShowTips();

			SetLiveShow();

			FingerAwait();

			if (CommonUtil.IsWechatMiniGame()) {
				var rectTransform = go_property.GetComponent<RectTransform>();
				rectTransform.offsetMax = new Vector2(0, -50);
			}
		}

        public override void AddListeners()
        {
			AddListeners(GameEventDefine.OnUpdteGameItem, SetLiveShow);
			AddListeners<EventTwoParam<string, int>>(GameEventDefine.OnUpdateBuffTime, UpdateLiveBuff);	
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
			if (_seq != null) {
				_seq.Kill();
				_seq = null;
			}
        }

        public async UniTask SetLevel(int level)
        {
	        string lvStr = string.Format(LocalizationPool.Get("Match/BeginPanel/Title"), level);
	        text_levelNormal.text = lvStr;
	        text_levelHard.text = lvStr;
	        text_levelCrazy.text = lvStr;

	        var levelData = await LevelManager.Instance.GetLevel(_matchType, _beginLVID);
	        if (levelData == null)
	        {
		        CloseSelf();
		        return;
	        }

	        if (this.transform != null)
	        {
		        text_moveCount.text =
			        string.Format(LocalizationPool.Get("Match/BeginPanel/MoveStep"), levelData.stepLimit);
		        SetHardText(levelData.difficulty);
		        widget_target.InitTarget(levelData);
	        }
        }

		private void SetLiveShow() {
			bool hasLiveBuff = G.GameItemModule.CheckHasBuff("liveBuff");
			go_live.SetActive(!hasLiveBuff);
			img_liveBuff.gameObject.SetActive(hasLiveBuff);

			LayoutRebuilder.ForceRebuildLayoutImmediate(go_btnLayout.transform.GetComponent<RectTransform>());
		}

        private void SetHardText(int difficulty)
		{
			var matchDiff = MatchManager.Instance.GetMatchDifficulty(difficulty);
			go_normal.SetActive(matchDiff == MatchDifficulty.Normal);
			go_hard.SetActive(matchDiff == MatchDifficulty.Hard);
			go_crazy.SetActive(matchDiff == MatchDifficulty.Crazy);
		}

		private void BeginMatch() {
			if (!CommonUtil.CanBeginGame()) {
				CommonUtil.ShowCommonTips(LocalizationPool.Get("Common/LiveLack"));
				CommonUtil.ShowItemLackPanel("live", false);
				return;
			}

			if (Time.time - _touchTime < 3) return;
			_touchTime = Time.time;

			ReqBegin();
		}

		private void ReqBegin() {
			RecordWinStreakBooster();
			UseBegingBooster();

			G.HttpModule.ReportLevelGameBegin(_beginLVID, (result, code) =>
			{
				Debug.Log("ReportLevelGameBegin result:" + result + " code:" + code);
				if (code == 0) {
					if (img_liveBuff.gameObject.activeSelf) {
						BeginGame();
					} else {
						ShowBeginAnim();
					}
				} else {
					Debug.LogError("ReportLevelGameBegin failed, code:" + code);
				}
			});
		}

		private void ShowBeginAnim() {
			KillFingerTween();

			go_mask.SetActive(true);
			go_property.SetActive(true);

		}

		private void BeginGame() {
			G.EventModule.DispatchEvent(GameEventDefine.OnMatchBegin);

			MatchManager.Instance.SetMatchLevelID(_beginLVID);
			HotfixCore.MVC.MVCManager.Instance.ActiveModule(MVCEnum.Match.ToString(),true,null, _matchType).Forget();
			G.SceneModule.SetCurSceneType(SceneType.Match);

			G.PopModule.ClearPopWindow();
			CloseSelf();
		}


		private void UseBegingBooster() {
			var curSelectBoosterID = widget_booster.GetSelectBoosterID();

			// 请求后台，使用道具
			if (curSelectBoosterID.Count > 0) {
				List<ItemData> itemList = new List<ItemData>();
				foreach (var boosterID in curSelectBoosterID) {
					itemList.Add(new ItemData(boosterID, 1));
				}
				G.GameItemModule.UseItem(itemList, (cbList) => {
					List<string> useItemBoosterIDs = new List<string>();
					foreach (var id in cbList) {
						useItemBoosterIDs.Add(G.GameItemModule.GetItemName(id));
					}

					if (useItemBoosterIDs.Count > 0) {
						List<int> elementIDList = new List<int>();
						var db = ConfigMemoryPool.Get<ItemElementDictDB>();
						foreach (var boosterID in curSelectBoosterID)
						{
							elementIDList.Add(db.GetElementId(boosterID));
						}
						MatchManager.Instance.SetBeginUseElements(elementIDList);
					}
				});
			}
		}


		private void RecordWinStreakBooster() {
			if (!CheckOpenWinStreak() || MatchManager.Instance.GetWinStreakBox() <= 0) return;

			int winStreakCount = MatchManager.Instance.GetWinStreakBox();

			var rewardStr = ConfigMemoryPool.Get<streakRewardDB>()[winStreakCount].reward;
			List<int> rewardIDList = new List<int>();
			var db = ConfigMemoryPool.Get<ItemElementDictDB>();
			foreach (var item in rewardStr.Split('|')) {
				string itemName = item.Split('*')[0];
				int useCount = int.Parse(item.Split('*')[1]);
				for (int i = 0; i < useCount; i++) {
					rewardIDList.Add(db.GetElementId(itemName));
				}
			}

			if (rewardIDList.Count > 2) {
				rewardIDList.Shuffle();
			}

			MatchManager.Instance.SetWinStreakElements(rewardIDList);
		}

		private void SetWinStreak() {
			bool isShow = CheckOpenWinStreak();
			go_target.gameObject.SetActive(!isShow);			
			go_winstreak.gameObject.SetActive(isShow);
		}

		private bool CheckOpenWinStreak() {
			bool isOpenWinStreak = MatchManager.Instance.IsOpenWinStreak(_beginLVID);
			return isOpenWinStreak;
		}

		private void ShowTips() {
			bool isShow = MatchManager.Instance.IsShowBeginTips(_beginLVID);
			if (!isShow) return;

			go_tips.SetActive(true);

			var rankStr = LocalizationPool.Get("Match/BeginPanel/HardTips");
			rankStr = rankStr.Replace("\\n", "\n");

			text_tips.text = rankStr;

			widget_booster.ShowBoosterTips();
		}

		private void FingerAwait() {
			spine_touch.color = new Color(1, 1, 1, 0);
			spine_touch.gameObject.SetActive(true);

			var seq = DOTween.Sequence();
			seq.AppendInterval(3);
			seq.AppendCallback(() => {
				ShowFingerTouch();
				_seq = null;
			});

			_seq = seq;
		}

		private void ShowFingerTouch() {
			var seq = DOTween.Sequence();
			seq.Append(spine_touch.DOFade(1, 0.5f));
			seq.AppendCallback(() => {
				spine_touch.AnimationState.SetAnimation(0, "dianji", false);
			});
			seq.AppendInterval(1f);
			seq.Append(spine_touch.DOFade(0, 0.5f));
			seq.AppendInterval(1f);
			seq.AppendCallback(() => {
				spine_touch.gameObject.SetActive(true);
			});
			seq.SetLoops(-1);

			_seq = seq;
		}

		private void KillFingerTween() {
			if (_seq != null) {
				_seq.Kill();
				_seq = null;
			}
			spine_touch.gameObject.SetActive(false);
		}

		private void UpdateLiveBuff(EventTwoParam<string, int> param) {
			if (param.Arg1 == "liveBuff" && param.Arg2 <= 0 && img_liveBuff.gameObject.activeSelf) {
				SetLiveShow();
			}
		}
	}
}
