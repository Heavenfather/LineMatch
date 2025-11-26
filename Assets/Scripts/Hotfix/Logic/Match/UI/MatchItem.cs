using System;
using GameConfig;
using GameCore.Localization;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixCore.Extensions;
using HotfixCore.Module;
using HotfixLogic.Match;
using UnityEngine;
using UnityEngine.UI;

namespace HotfixLogic
{
    public partial class MatchItem : UIWidget
    {
        private int _itemId;
        public int ItemId => _itemId;

        private string _itemName;

        private int _useCount;   // 标记是否使用过buff
        private bool _isInUsing;

        private bool _isUnlock;

        private int _unlockLv;

        public override void OnCreate()
        {
            _isInUsing = false;
            btn_add.AddClick(OnRewardItemClick);
            btn_lock.AddClick(OnLockClick);
        }

        public override void AddListeners()
        {
            AddListeners(GameEventDefine.OnUpdteGameItem, RefreshNum);	
            AddListeners<EventTwoParam<string, int>>(GameEventDefine.OnUpdateBuffTime, UpdateBuffTime);			
        }

        public void Init(int itemId)
        {
            _itemId = itemId;
            this.gameObject.name = $"match_item_{itemId}";
            _itemName = G.GameItemModule.GetItemName(itemId);

            int itemNum = G.GameItemModule.GetItemCount(_itemId);

            var maxLv = MatchManager.Instance.MaxLevel;
            _unlockLv = ConfigMemoryPool.Get<ConstConfigDB>().GetMatchBoosterUnlockLv(_itemName);

            _isUnlock = _unlockLv <= maxLv;
            go_lock.SetActive(!_isUnlock);

            RefreshNum();
        }

        public void SetBuffVisible(bool visible)
        {
            img_buff.SetVisible(visible && _isUnlock);
        }

        public void SetNumBgVisible(bool visible)
        {
            img_numBg.SetVisible(visible && _isUnlock);
        }

        public void RefreshNum()
        {
            int itemNum = G.GameItemModule.GetItemCount(_itemId);

            if (GuideManager.Instance.IsGuiding())
            {
                int guideId = GuideManager.Instance.CurrentGuideId;
                GuideConfig guideConfig = GuideManager.Instance.FindGuideData(guideId);
                int.TryParse(guideConfig.guideParameters, out int itemId);
                if (this._itemId == itemId)
                {
                    btn_add.SetVisible(false);
                    text_num.SetVisible(true);
                    text_num.text = LocalizationPool.Get("Common/Free");
                }
                else
                {
                    btn_add.SetVisible(itemNum <= 0);
                    text_num.SetVisible(itemNum > 0);
                    text_num.text = itemNum.ToString();
                }
            }
            else
            {
                btn_add.SetVisible(itemNum <= 0);
                text_num.SetVisible(itemNum > 0);
                text_num.text = itemNum.ToString();
            }

            if (_useCount > 0) {
                SetBuffVisible(false);
                SetNumBgVisible(true);
            } else {
                bool hasBuff = G.GameItemModule.CheckHasBuffByItemName(_itemName);
                SetBuffVisible(hasBuff);
                SetNumBgVisible(!hasBuff);
            }
        }

        public void SetGrayOrLight(int itemId)
        {
            if (itemId < 0)
            {
                this.gameObject.SetGray(false);
                btn_add.interactable = true;
                _isInUsing = false;
            }
            else
            {
                btn_add.interactable = false;
                if (itemId != _itemId)
                {
                    this.gameObject.SetGray(true);
                    _isInUsing = false;
                }
                else
                {
                    this.gameObject.SetGray(false);
                    _isInUsing = true;
                }

            }
        }

        public void UpdateUseCount() {
            _useCount++;
        }

        public void ResetUseCount() {
            _useCount = 0;
        }

        private void OnRewardItemClick()
        {
            if (!CanUseItem())
            {
                CommonUtil.ShowItemLackPanel(_itemName);
                return;
            }

            if (_isInUsing)
            {
                //取消使用
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchCancelItem);
                _isInUsing = false;
                return;
            }

            //新手引导
            // if (GuideManager.Instance.IsGuiding())
            // {
            //     GuideManager.Instance.FinishCurrentGuide();
            // }

            _isInUsing = true;
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchUseItem,
                EventOneParam<int>.Create(_itemId));

        }

        private void UpdateBuffTime(EventTwoParam<string, int> param) {
			var key = param.Arg1;
			if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(_itemName) || !key.Contains(_itemName)) return;

			int leftTime = param.Arg2;
			if (leftTime > 0) {
				int minute = leftTime / 60;
				int second = leftTime % 60;
				text_buff.text = $"{minute:00}:{second:00}";
			} else {
				SetNumBgVisible(true);
                SetBuffVisible(false);
			}
		}

        private bool CanUseItem() {
            if (_useCount <= 0 && (G.GameItemModule.CheckHasBuffByItemName(_itemName) || GuideManager.Instance.IsGuiding())) {
                return true;
            }

            int itemNum = G.GameItemModule.GetItemCount(_itemId);
            return itemNum > 0;
        }

        public GameObject GetIconWidgetObj()
        {
            return null;
        }

        private void OnLockClick() {
            var tipsStr = string.Format(LocalizationPool.Get("Match/Booster/UnlockItemTips"), _unlockLv);
            CommonUtil.ShowCommonTips(tipsStr);
        }
    }
}