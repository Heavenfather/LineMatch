using UnityEngine;
using HotfixCore.Module;
using GameConfig;
using HotfixLogic.Match;
using GameCore.Localization;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Hotfix.Define;
using DG.Tweening;
using Hotfix.Utils;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using Logger = GameCore.Log.Logger;
using HotfixCore.MVC;
using UnityEngine.UI;

namespace HotfixLogic
{
	[Window(UILayer.UI,"uiprefab/match/matchresultlose")]
	public partial class MatchResultLose : UIWindow
	{
		int _curStep = 1;
		int _maxStep = 2;

		bool _mustLose = false;

		bool _isOpenStreak = false;
		bool _hasWinStreak = false;

		int _continueStep = 0;

		List<ItemData> _continueItems = new List<ItemData>();
		// List<CommonRewardItem> _continueItemWidgets = new List<CommonRewardItem>();

		MatchBoosterSelect _boosters;

		Tween _tween;

		Dictionary<int, int> _targetData = new Dictionary<int, int>();

		int _reviveLostCount = 0;

		float _continueTouchTime = 0;

		Animator _animator;

		public override void OnCreate()
		{
			AudioUtil.PlayMatchLose();

			_animator = transform.GetComponent<Animator>();

			if (userDatas.Length > 0) {
				_mustLose = (bool)userDatas[0];
				_targetData = (Dictionary<int, int>)userDatas[1];
			}

			_reviveLostCount = MatchManager.Instance.LostCount + 1;
			if (_reviveLostCount > 5) _reviveLostCount = 5;

			// _continueItemWidgets.Add(widget_continueItem1);
			// _continueItemWidgets.Add(widget_continueItem2);

			btn_close.AddClick(OnClickCloseBtn);
			btn_begin.AddClick(OnClickBeginBtn);
			btn_continue.AddClick(OnClickContinueBtn);
			btn_free.AddClick(OnClickContinueBtn);
			btn_adv.AddClick(OnClickAdvBtn);

			if (CommonUtil.IsWechatMiniGame()) {
				var designResolution = G.UIModule.GetDesignResolution();
				if (Screen.width / Screen.height < designResolution.x / designResolution.y) {
					// var rectTransform = widg/et_property.transform.GetComponent<RectTransform>();
					// rectTransform.anchoredPosition = new Vector2(0, -80);
				}
			}

			InitWidget();
			InitRestartCoin();
			UpdateStep();

			SetLevel(MatchManager.Instance.CurLevelID).Forget();
		}

        public override void OnDestroy()
        {
            KillTween();
        }

        public override void AddListeners()
        {
            AddListeners(GameEventDefine.OnTrainMasterClose, ShowPop);
            AddListeners(GameEventDefine.OnMatchRestartComplete, OnMatchRestartComplete);
			AddListeners<EventTwoParam<string, int>>(GameEventDefine.OnUpdateBuffTime, UpdateLiveBuff);	

        }

        private void OnMatchRestartComplete()
        {
	        AddElements();
        }

        private void InitWidget() {
			InitStepLayout();

			_boosters = widget_booster;
			_boosters.SetBoosterUseBuff(false);

			_isOpenStreak = MatchManager.Instance.IsOpenWinStreak(MatchManager.Instance.CurLevelID);
			_hasWinStreak = _isOpenStreak && MatchManager.Instance.GetWinStreakBox() > 0;


			go_winStreak1.SetActive(_hasWinStreak);
			if (_hasWinStreak) CreateWidget<MatchWinStreak>(go_winStreak1);
			

			go_step.gameObject.SetActive(_maxStep != 1);

			var advAddStep = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("AdReviveStep");
			text_advStep.text = string.Format(LocalizationPool.Get("Match/Result/AdvRevive"), advAddStep);

			

			if (_mustLose) {
				_maxStep = 1;
			} else if (_hasWinStreak) {
				_maxStep = 3;
			} else {
				_maxStep = 2;
			}
		}

		private void InitStepLayout() {
			var config = ConfigMemoryPool.Get<reviveRewardDB>()[_reviveLostCount];

			if (config.addItem != "") {
				string[] res = config.addItem.Split('|');
				for (int i = 0; i < res.Length; i++) {
					string[] item = res[i].Split('*');
					_continueItems.Add(new ItemData(item[0], int.Parse(item[1])));
				}
			}

			img_add2.gameObject.SetActive(_continueItems.Count > 2);
			img_add3.gameObject.SetActive(_continueItems.Count > 2);
		}

		private void InitRestartCoin() {			
			var config = ConfigMemoryPool.Get<reviveRewardDB>()[_reviveLostCount];

			text_stepTips.text = string.Format(LocalizationPool.Get("Match/Result/AddStep"), config.addSteps);
			text_buyCoin.text = config.needCoin.ToString();

			_continueStep = config.addSteps;
		}

        public async UniTask SetLevel(int level)
		{
			var levelData = await LevelManager.Instance.GetLevel(level);
			if (levelData == null)
				return;

			SetHardText(levelData.difficulty);

			var widget = CreateWidget<MatchBeginTargetWidget>(go_target);
			widget.InitTarget(levelData, _targetData);
		}

		public void SetLevel(LevelData levelData)
		{
			SetHardText(levelData.difficulty);
		}

		private void SetHardText(int difficulty)
		{
			var matchDiff = MatchManager.Instance.GetMatchDifficulty(difficulty);
			go_normal.SetActive(matchDiff == MatchDifficulty.Normal);
			go_hard.SetActive(matchDiff == MatchDifficulty.Hard);
			go_crazy.SetActive(matchDiff == MatchDifficulty.Crazy);
		}

		private void UpdateStep() {
			if (_curStep == _maxStep) {
				G.EventModule.DispatchEvent(GameEventDefine.OnMatchResultFail);
			}

			ShowStep(_curStep < _maxStep);
			ShowRestart(_curStep == _maxStep);
			UpdateTitletext();

			if (_maxStep == 3 && _curStep == 2) {
				ShowAdvBtn();
			} else if (_curStep == _maxStep) {
				btn_adv.gameObject.SetActive(false);
				_boosters.SetBoosterUseBuff(true);
			}
		}

		private void ShowRestart(bool isShow) {
			btn_begin.gameObject.SetActive(isShow);
			go_streak.SetActive(isShow && _isOpenStreak);

			if (isShow && _isOpenStreak) {
				MatchManager.Instance.SetWinStreak(0);
				CreateWidget<MatchWinStreak>(go_winStreak2);
			}
			SetLiveShow();
		}

		private void SetLiveShow() {
			bool hasLiveBuff = G.GameItemModule.CheckHasBuff("liveBuff");
			go_live.SetActive(!hasLiveBuff);
			img_liveBuff.gameObject.SetActive(hasLiveBuff);

			LayoutRebuilder.ForceRebuildLayoutImmediate(btn_begin.transform.GetComponent<RectTransform>());
		}

		private void ShowAdvBtn() {
			if (G.AdvModule.GetAdReviveCount() <= 0 || MatchManager.Instance.HadAdvRevive) return;

			btn_adv.gameObject.SetActive(true);
			btn_adv.gameObject.transform.localScale = Vector3.zero;
			btn_adv.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
		}

		private void ShowStep(bool isShow)
		{
			btn_free.SetVisible(MatchManager.Instance.HasFreeTimes && isShow);
			btn_continue.gameObject.SetActive(isShow && !MatchManager.Instance.HasFreeTimes);
		}

		private void UpdateTitletext() {
			if (_curStep == _maxStep) {
				text_title.text = LocalizationPool.Get("Match/Result/LoseTitle2");
			} else {
				text_title.text = LocalizationPool.Get("Match/Result/LoseTitle1");
			}
		}

		private void OnClickCloseBtn() {
			if (_tween!= null) return;

			if (_curStep < _maxStep) {
				if (!MatchManager.Instance.HadAdvRevive && _maxStep == 2 && G.AdvModule.GetAdReviveCount() > 0 && 
						_reviveLostCount == 1 && !btn_adv.gameObject.activeSelf) {
					// 弹一次广告挽留
					CommonUtil.ShowCommonTips(LocalizationPool.Get("Match/Result/Retention"));
					ShowAdvBtn();
					return;
				}

				_curStep++;
				UpdateStep();
				AddStepAnim();
			} else {
				EventDispatcher.DispatchEvent(GameEventDefine.OnMatchCloseClick); 
			}
		}

		private void AddStepAnim() {
			var rectTransform = (RectTransform)go_layout.transform;
			var layoutSize = rectTransform.sizeDelta;
			var layoutPos = rectTransform.anchoredPosition;

			bool trainMasterFail = false;
			if (_curStep == _maxStep && G.TrainMasterModule.InTrainning()) {
				trainMasterFail = G.TrainMasterModule.GetFailWinStreakNum() > 0;
				if (trainMasterFail) {
					btn_close.interactable = false;
					btn_begin.interactable = false;
				}
			}

			_tween = rectTransform.DOAnchorPos(new Vector2(layoutPos.x - layoutSize.x, layoutPos.y), 0.3f).OnComplete(() => {
				_tween = null;

				if (trainMasterFail) {
					MVCManager.Instance.ActiveModule(MVCEnum.TrainMaster.ToString()).Forget();

					btn_close.interactable = true;
					btn_begin.interactable = true;
				}
			});
		}

		private void OnClickBeginBtn() {
            if (!CommonUtil.CanBeginGame())
            {
                CommonUtil.ShowCommonTips(LocalizationPool.Get("Common/LiveLack"));
                CommonUtil.ShowItemLackPanel("live", false);
                return;
            }
			
			ReqBeginMatch();
		}

		private void ShowBeginAnim() {
			go_mask.SetActive(true);

			var targetPos = img_live.transform.position;

			img_flyLive.gameObject.SetActive(true);
			go_eff.transform.position = targetPos;


		}

		private void ReqBeginMatch() {
			G.HttpModule.ReportLevelGameBegin(MatchManager.Instance.CurLevelID, (result, code) =>
			{
				Debug.Log("ReportLevelGameBegin result:" + result + " code:" + code);
				if (code == 0) {
					if (img_liveBuff.gameObject.activeSelf) {
						PlayAgain();
					} else {
						ShowBeginAnim();
					}
				} else {
					Debug.LogError("ReportLevelGameBegin failed, code:" + code);
				}
			});
		}

		private void PlayAgain() {
			G.GameItemModule.GameConsumLive();
			EventDispatcher.DispatchEvent(GameEventDefine.OnMatchRestart);
			PlayCloseAnim();
		}

		private void OnClickContinueBtn() {
			

			if (MatchManager.Instance.HasFreeTimes)
			{
				G.GameItemModule.UseItem((int)ItemDef.FreeRevive, 1, success => {
					if (success) {
						ContinueAddStep(_continueStep);
						AddElements();
					}
				}, false);
				return;
			}
			
			if (Time.time - _continueTouchTime < 3) return;
			_continueTouchTime = Time.time;


			var useCoin = int.Parse(text_buyCoin.text);
			G.GameItemModule.UseItem("coin", useCoin, success => {
				if (success) {
					MatchManager.Instance.AddLostCount();
					ContinueAddStep(_continueStep);
					AddElements();

					EventDispatcher.DispatchEvent(GameEventDefine.OnMatchAddStepUseCoin, EventOneParam<int>.Create(useCoin));
				}
			}, false);
			
		}

		private void AddElements(bool isRestart = false) {
			List<int> rewardIDList = new List<int>();

			var db = ConfigMemoryPool.Get<ItemElementDictDB>();
			if (isRestart) {
				foreach (var item in _continueItems) {
					for (int i = 0; i < item.Count; i++) {
						rewardIDList.Add(db.GetElementId(item.Name));
					}
				}
			}


			var curSelectBoosterID = _boosters.GetSelectBoosterID();
			List<ItemData> itemList = new List<ItemData>();
			foreach (var boosterID in curSelectBoosterID) {
				itemList.Add(new ItemData(boosterID, 1));
			}

			if (itemList.Count > 0) {
				G.GameItemModule.UseItem(itemList, (cbList) => {
					foreach (var boosterID in curSelectBoosterID) {
						rewardIDList.Add(db.GetElementId(boosterID));
					}
					if (rewardIDList.Count > 0) {
						var param = EventThreeParam<List<int>, List<int>, bool>.Create(rewardIDList, new List<int>(), true);
						G.EventModule.DispatchEvent(GameEventDefine.OnMatchUpdateSpecialElements,param);
					}
				});
			} else {
				if (rewardIDList.Count > 0) {
					var param = EventThreeParam<List<int>, List<int>, bool>.Create(rewardIDList, new List<int>(), true);
					G.EventModule.DispatchEvent(GameEventDefine.OnMatchUpdateSpecialElements,param);
				}
			}



		}

		private void KillTween() {
			if (_tween!= null) {
				_tween.Kill();
				_tween = null;
			}
		}

		private void OnClickAdvBtn() {
			G.AdvModule.PlayAdvVideo(GiftIDDefine.Adv_MatchLoseRevive, succ =>
			{
				if (succ)
				{
					G.AdvModule.AddAdReviveCount();

					MatchManager.Instance.SetHadAdvRevive(true);
					var addStep = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("AdReviveStep");
					ContinueAddStep(addStep, true);
				}
			}, false, MatchManager.Instance.CurLevelID);
		}

		private void ContinueAddStep(int step, bool isAdv = false) {
			PlayCloseAnim();
			EventDispatcher.DispatchEvent(GameEventDefine.OnMatchAddStep, EventTwoParam<int, bool>.Create(step, isAdv));
		}

		private void PlayCloseAnim() {
			_animator.PlayUIAnimation("ani_ef_wy_00008_end", (succ) => {
				CloseSelf();
			});
		}

		private void ShowPop() {
			go_root.transform.localScale = Vector3.zero;
            go_root.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack).OnComplete(OnPopComplete);
		}

		private void UpdateLiveBuff(EventTwoParam<string, int> param) {
			if (param.Arg1 == "liveBuff" && param.Arg2 <= 0 && img_liveBuff.gameObject.activeSelf) {
				SetLiveShow();
			}
		}
	}
}
