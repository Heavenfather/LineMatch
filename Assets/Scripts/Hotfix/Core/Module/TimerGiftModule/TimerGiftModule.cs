using System.Collections.Generic;
using GameConfig;
using GameCore.LitJson;
using GameCore.Localization;
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.Utils;

namespace HotfixCore.Module
{
    public class TimerGiftData
    {
        public TimerGiftType GiftType;
        public long EndTime;
        public List<int> ShopIds;
    }


    public class TimerGiftModule : IModuleAwake, IModuleDestroy
    {
        private List<TimerGiftData> _timerGiftDatas;

        public void Awake(object parameter)
        {
            _timerGiftDatas = new List<TimerGiftData>();
            InitTimerGift();
        }

        public void Destroy()
        {
        }

        private void InitTimerGift() {
            var dataStr = PlayerPrefsUtil.GetString(TimerGiftType.LimitTimeGift.ToString(), "");
            if (dataStr != "") {
                var data = JsonMapper.ToObject<TimerGiftData>(dataStr);
                if (data != null) {
                    _timerGiftDatas.Add(data);
                }
            }
        }


        #region 结算限时礼包
        public void StartLimitTimeGift() {
            TimerGiftData data = _timerGiftDatas.Find(x => x.GiftType == TimerGiftType.LimitTimeGift);
            if (data != null) {
                var dataDate = CommonUtil.UnixToLocalDateTime(data.EndTime);
                var nowDate = CommonUtil.GetNowDateTime();

                // 如果不是同一天，数据重置
                if (dataDate.Day != nowDate.Day) {
                    data = null;
                }
            }

            if (data == null) {
                data = new TimerGiftData {
                    GiftType = TimerGiftType.LimitTimeGift,
                    EndTime = CommonUtil.GetNowTime() + 1800,
                    ShopIds = new List<int> { 41 }
                };

                var str = JsonMapper.ToJson(data);
                PlayerPrefsUtil.SetString(TimerGiftType.LimitTimeGift.ToString(), str);
            }

            UpdateTimerGift(data);
            G.EventModule.DispatchEvent(GameEventDefine.OnUpdateLimitGiftTime);
        }

        public void UpdateTimerGift(TimerGiftData timerGiftData) {
            var data = _timerGiftDatas.Find(x => x.GiftType == timerGiftData.GiftType);
            if (data == null) {
                _timerGiftDatas.Add(timerGiftData);
            } else {
                data.EndTime = timerGiftData.EndTime;
                data.ShopIds = timerGiftData.ShopIds;
            }
        }

        public bool CheckGiftInTime(TimerGiftType giftType) {
            var data = _timerGiftDatas.Find(x => x.GiftType == giftType);
            if (data == null) {
                return false;
            }
            var now = CommonUtil.GetNowTime();
            return now <= data.EndTime;
        }

        public TimerGiftData GetTimerGiftData(TimerGiftType giftType) {
            return _timerGiftDatas.Find(x => x.GiftType == giftType);
        }

        public bool HasTadayLimitTimeGift() {
            var data = _timerGiftDatas.Find(x => x.GiftType == TimerGiftType.LimitTimeGift);
            if (data == null) return false;
            
            if (data.EndTime >= CommonUtil.GetNowTime()) return true;

            var dateEnd = CommonUtil.UnixToLocalDateTime(data.EndTime);
            var dateNow = CommonUtil.GetNowDateTime();
            return dateEnd.Year == dateNow.Year && dateEnd.Month == dateNow.Month && dateEnd.Day == dateNow.Day;
        }
        #endregion


        public bool CanBuyGiftOneYuan() {
            var giftEndTime = G.UserInfoModule.RegisterTime + 3600 * 24 * ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("OneYuanDay");
			var buyData = G.IAPModule.GetBuyItemData(ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("OneYuanShopID"));
			if (CommonUtil.GetNowTime() < giftEndTime && (buyData == null || buyData.buy_count < buyData.limit_num)) {
                return true;
            } 

            return false;
        }

        public bool CanBuyFirstBuyGift() {
            var idConfig = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigStrVal("ShouChongShopID");
			string[] idStrArr;

            if (!G.SwitchModule.IsInitAbGroup()) return false;

            var groupVal = G.SwitchModule.GetABStringValue("first_gift_group").ToLower();

			if (groupVal != "b") {
                idStrArr = idConfig.Split('|')[0].Split('_');
            } else {
				idStrArr = idConfig.Split('|')[1].Split('_');
			}
			
            var shopID = int.Parse(idStrArr[idStrArr.Length - 1]);
            var buyData = G.IAPModule.GetBuyItemData(shopID);
			if (buyData == null || buyData.buy_count < buyData.limit_num) {
                return true;
            } 

            return false;
        }

        public bool CheckIsMostFirstBuyGift(int shopID) {
            var idConfig = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigStrVal("ShouChongShopID");
			string[] idStrArr;

            var abVal = G.SwitchModule.GetABStringValue("first_gift_group").ToLower();

			if (abVal != "b") {
                idStrArr = idConfig.Split('|')[0].Split('_');
            } else {
				idStrArr = idConfig.Split('|')[1].Split('_');
			}

            return shopID == int.Parse(idStrArr[idStrArr.Length - 1]);
        }

        public void ShopBuyFirstBuyGiftAllReward() {
            var idConfig = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigStrVal("ShouChongShopID");
			string[] idStrArr;

            var abVal = G.SwitchModule.GetABStringValue("first_gift_group").ToLower();
			if (abVal != "b") {
                idStrArr = idConfig.Split('|')[0].Split('_');
            } else {
				idStrArr = idConfig.Split('|')[1].Split('_');
			}

            var itemDatas = new List<ItemData>();
            foreach (var id in idStrArr) {
                var shopID = int.Parse(id);
                var buyData = G.IAPModule.GetBuyItemData(shopID);
                if (buyData == null || buyData.buy_count < buyData.limit_num) {
                    var shopItem = ConfigMemoryPool.Get<ShopItemsDB>()[shopID];
                    var data = CommonUtil.GetItemDatasByStr(shopItem.reward);
                    itemDatas.AddRange(data);
                } 
            }
            CommonUtil.ShowRewardWindow(itemDatas, LocalizationPool.Get("Reward/Tips/FirstBugGift"));
            G.GameItemModule.AddItemCount(itemDatas);

        }
    }
}
