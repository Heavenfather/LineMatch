using UnityEngine;
using HotfixCore.Module;
using GameConfig;
using Cysharp.Threading.Tasks;
using Hotfix.Utils;
using GameCore.Localization;
using HotfixLogic.Match;
using Hotfix.EventParameter;
using Hotfix.Define;
using HotfixCore.Extensions;
using UnityEngine.UI;

namespace HotfixLogic
{
	public partial class MatchBeginBoosterItem : UIWidget
	{
		int _id;
		string _boosterID;
		bool _isSelect;
		bool _isLock;
		bool _canUseBuff;

		public override void OnCreate()
		{
			_isSelect = false;
			btn_select.AddClick(OnSelect);
		}

        public override void AddListeners()
        {
			AddListeners(GameEventDefine.OnUpdteGameItem, OnUpdteGameItem);
            AddListeners<EventTwoParam<string, int>>(GameEventDefine.OnUpdateBuffTime, UpdateBuffTime);			
        }

        public void SetBoosterID(string boosterID, bool canUseBuff = true)
		{
			_canUseBuff = canUseBuff;
			_boosterID = boosterID;
			_id = ConfigMemoryPool.Get<ItemEnumDB>()[boosterID].Id;

			var lockLevel = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("MBeginBoosterUnlock");
			if (MatchManager.Instance.MaxLevel < lockLevel) {
				SetLockState(true);
			} else {
				bool hasBuff = G.GameItemModule.CheckHasBuffByItemName(_boosterID);
				if (hasBuff) {
					_isSelect = true;

					var expireTime = G.GameItemModule.GetBuffExpireTime(_boosterID + "Buff");
					UpdateTimeText(expireTime - (int)CommonUtil.GetNowTime());
				}

				SetSelectState(_isSelect);


			}

			SetLockState(MatchManager.Instance.MaxLevel < lockLevel);
			UpdateItemCount();
			SetSprite();
			ShowItemCount();
		}

		private void OnSelect()
		{
			if (_isLock) {
				var lockLevel = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("MBeginBoosterUnlock");
				var str = string.Format(LocalizationPool.Get("CommonTips/LockUseMatchBooster"), lockLevel);
				CommonUtil.ShowCommonTips(str);
				return;
			}

			var itemCount = G.GameItemModule.GetItemCount(_boosterID);
			if (itemCount <= 0 && !_canUseBuff || itemCount <= 0 && !G.GameItemModule.CheckHasBuffByItemName(_boosterID)) {
				CommonUtil.ShowItemLackPanel(_boosterID, true);
				return;
			}

			_isSelect = !_isSelect;
			SetSelectState(_isSelect);
		}

		private void SetSelectState(bool isSelect)
		{
			if (_isLock) return;

			_isSelect = isSelect;

			img_select.gameObject.SetActive(_isSelect);			
			go_select.gameObject.SetActive(_isSelect);

			
		}

		private void ShowItemCount() {
			if (_isLock) return;

			bool hasBuff = G.GameItemModule.CheckHasBuffByItemName(_boosterID);
			go_count.gameObject.SetActive(!hasBuff || !_canUseBuff);
			go_buff.gameObject.SetActive(hasBuff && _canUseBuff);
		}

		private void SetLockState(bool isLock) {
			_isLock = isLock;

			go_lock.gameObject.SetActive(isLock);

			if (isLock) {
				go_count.gameObject.SetActive(false);
				go_select.gameObject.SetActive(false);
				img_select.gameObject.SetActive(false);
				go_buff.gameObject.SetActive(false);
			}
		}

		private void SetSprite() {
			G.GameItemModule.SetItemSprite(_boosterID, sprite => {
				img_icon.sprite = sprite;
			});
		}

		public bool IsUse() {
			return _isSelect;
		}

		public string GetBoosterID() {
			return _boosterID;
		}

		public int GetID() {
			return _id;
		}

		public void UpdateBuffTime(EventTwoParam<string, int> param) {
			if (!_canUseBuff) return;

			var key = param.Arg1;
			if (!key.Contains(_boosterID)) return;

			int leftTime = param.Arg2;
			UpdateTimeText(leftTime);
		}

		public void UpdateTimeText(int leftTime) {
			if (leftTime > 0) {
				int minute = leftTime % 3600 / 60;
				int second = leftTime % 60;
				text_buffTIme.text = $"{minute:00}:{second:00}";
			} else {
				go_buff.SetActive(false);
				go_count.SetActive(true);
			}
		}

		private void UpdateItemCount() {
			text_num.text = G.GameItemModule.GetItemCount(_boosterID).ToString();
			LayoutRebuilder.ForceRebuildLayoutImmediate(text_num.rectTransform);
			var size = text_num.rectTransform.sizeDelta;
			if (size.x < 35) {
				img_countBg.rectTransform.sizeDelta = new Vector2(63, 40);
			} else {
				img_countBg.rectTransform.sizeDelta = new Vector2(size.x + 20, 40);
			}
		}

		private void OnUpdteGameItem() {
			UpdateItemCount();
		}

		public void SetUseBuff(bool bl) {
			_canUseBuff = bl;

			if (bl) {
				bool hasBuff = G.GameItemModule.CheckHasBuffByItemName(_boosterID);
				if (hasBuff) _isSelect = true;
			} else {
				_isSelect = false;
			}

			SetSelectState(_isSelect);
			ShowItemCount();
		}

		public void SetBtnEffectShow(float delay) {
			var timerID = 0;
			timerID = AddTimer(() => {
				go_eff.SetActive(true);
				RemoveTimer(timerID);
			}, delay);
		}
	}
}
