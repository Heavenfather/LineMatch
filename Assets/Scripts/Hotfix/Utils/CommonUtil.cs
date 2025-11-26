using System;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using GameConfig;
using GameCore.LitJson;
using GameCore.Localization;
using GameCore.Logic;
using GameCore.SDK;
using GameCore.Settings;
using GameCore.Utils;
using Hotfix.Define;
using HotfixCore.Module;
using HotfixLogic;
using TMPro;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Utils
{
    public static class CommonUtil
    {
        private static long offsetTime = 0;

        public static void ShowCommonPrompt(string message, Action confirmCallback = null, Action closeCallback = null,
            string title = "", string btnStr = "")
        {
            if (string.IsNullOrEmpty(title) || title == "")
            {
                title = LocalizationPool.Get("Common/Tips1");
            }

        }

        public static void ShowCommonTips(string message)
        {
            
        }

        public static void ShowCommonTipsFromPool(string key) {
            var message = LocalizationPool.Get(key);
            ShowCommonTips(message);
        }

        public static void ShowRewardWindow(List<ItemData> itemList, string tipsStr = "", Action callBack = null, SceneType excludeScene = SceneType.None, 
                                                bool isSign = false, RewardBoxType boxType = RewardBoxType.None)
        {
            if (itemList.Count <= 0) return;

            if (excludeScene != SceneType.None)
            {
                var currentType = G.SceneModule.CurSceneType;
                if(currentType != excludeScene)
                    return;
            }

        }

        public static void ShowCardReward(List<CardData> itemList)
        {
            string rewardStr = LocalizationPool.Get("Card/GetCard");
            var cardDB = ConfigMemoryPool.Get<CardDB>();
            foreach (var cardData in itemList)
            {
                if (rewardStr != LocalizationPool.Get("Card/GetCard"))
                {
                    rewardStr += ",";
                }

                rewardStr += cardDB[cardData.card_id].cardName_cn + "x" + cardData.card_num;
            }

            ShowCommonPrompt(rewardStr);
        }

        public static List<ItemData> GetItemDatasByStr(string itemStr)
        {
            var itemArr = itemStr.Split('|');
            var itemList = new List<ItemData>();
            var itemDB = ConfigMemoryPool.Get<ItemEnumDB>();
            foreach (var str in itemArr)
            {
                var id = str.Split('*')[0];
                var count = str.Split('*')[1];
                itemList.Add(new ItemData(itemDB[id].Id, int.Parse(count)));
            }

            return itemList;
        }

        public static List<ItemData> GetItemDataByStrList(List<string> itemStrList)
        {
            var itemList = new List<ItemData>();
            for (int i = 0; i < itemStrList.Count; i++)
            {
                itemList.Add(GetItemDataByStr(itemStrList[i]));
            }
            return itemList;
        }
        
        public static ItemData GetItemDataByStr(string itemStr)
        {
            var itemDB = ConfigMemoryPool.Get<ItemEnumDB>();
            string[] itemArr = itemStr.Split('*');
            if(itemArr.Length < 2)
                return null;
            return new ItemData(itemDB[itemArr[0]].Id, int.Parse(itemArr[1]));
        }

        public static void ShowItemLackPanel(string itemName, bool showTips = true, Action exchangeCallback = null, bool checkExchange = true)
        {
            if (showTips) ShowCommonTips(LocalizationPool.Get("Common/ItemLack"));

            // 检测是否有足够的金币购买 或者 是否能看广告获取
            if (exchangeCallback != null || checkExchange)
            {
                var config = ConfigMemoryPool.Get<CoinShopItemsDB>().All;
                List<CoinShopItems> coinShopItems = new List<CoinShopItems>();
                foreach (var item in config)
                {
                    if (item.reward.Contains(itemName))
                    {
                        coinShopItems.Add(item);
                    }
                }
            }

            // 体力和金币需要走档位礼包
            if (itemName == "live") {
                ShowGradeGift("live");
                return;
            }


            // 没有足够的条件获取，则弹出礼包
            var shopDB = ConfigMemoryPool.Get<ShopItemsDB>().All;
            var itemList = new List<ShopItems>();

            var fineKey = itemName + "*";
            foreach (var item in shopDB)
            {
                if (item.giftType == 3 && item.reward.Contains(fineKey))
                {
                    itemList.Add(item);
                }
            }

        }

        public static long GetNowTime()
        {
            return offsetTime + DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public static DateTime GetNowDateTime()
        {
            return UnixToLocalDateTime(GetNowTime());
        }

        public static string GetTimeOut(int leftTime, bool showHour = true)
        {
            int hour = leftTime / 3600;
            if (hour >= 24)
            {
                int day = hour / 24;
                hour %= 24;
                string formatStr = LocalizationPool.Get("Common/DayHour");
                return string.Format(formatStr, day, hour);
            }
            else
            {
                int minute = (leftTime % 3600) / 60;
                int second = leftTime % 60;
                if (hour > 0) {
                    return $"{hour:D2}:{minute:D2}:{second:D2}";
                } else {
                    if (showHour) {
                        return $"00:{minute:D2}:{second:D2}";
                    } else {
                        return $"{minute:D2}:{second:D2}";
                    }
                }
            }
        }

        public static string GetTimeOut(long leftTime, bool showHour = true)
        {
            long hour = leftTime / 3600;
            if (hour >= 24)
            {
                long day = hour / 24;
                hour %= 24;
                string formatStr = LocalizationPool.Get("Common/DayHour");
                return string.Format(formatStr, day, hour);
            }
            else
            {
                long minute = (leftTime % 3600) / 60;
                long second = leftTime % 60;
                if (hour > 0) {
                    return $"{hour:D2}:{minute:D2}:{second:D2}";
                } else {
                    if (showHour) {
                        return $"00:{minute:D2}:{second:D2}";
                    } else {
                        return $"{minute:D2}:{second:D2}";
                    }
                }
            }
        }

        public static string GetAgoTime(DateTime agoTime) {
            TimeSpan timeDifference = GetNowDateTime() - agoTime;
            long leftTime = (long)timeDifference.TotalSeconds;
            var agoDay = 999;

            if (leftTime < 86400) {
                return LocalizationPool.Get("Common/Today");
            } else if (leftTime < 86400 * agoDay) {
                int day = (int)(leftTime / 86400);
                return string.Format(LocalizationPool.Get("Common/DayAgo"), day);
            } else {
                return string.Format(LocalizationPool.Get("Common/DayAgo"), agoDay);
            }
        }

        public static void UpdateServerTime(long serverTime)
        {
            offsetTime = serverTime - DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public static string FormatMaxNum(int num, int maxNum = 99)
        {
            if (num <= maxNum)
            {
                return num.ToString();
            }
            else
            {
                return $"{maxNum}+";
            }
        }

        /// <summary>
        /// 将Unix时间戳转换为本地DateTime
        /// </summary>
        public static DateTime UnixToLocalDateTime(long unixTimestamp)
        {
            // 创建UTC时间
            DateTime utcDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(unixTimestamp);

            // 转换为本地时区
            return utcDateTime.ToLocalTime();
        }

        /// <summary>
        /// 本地DateTime转换为Unix时间戳
        /// </summary>
        /// <returns></returns>
        public static long LocalDateTimeToUnix(DateTime dateTime)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(dateTime.ToUniversalTime() - epoch).TotalSeconds;
        }

        /// <summary>
        /// 设备震动
        /// </summary>
        /// <param name="intensity"></param>
        public static void DeviceVibration(int intensity, float duration = 0)
        {
            var vibrateValue = PlayerPrefsUtil.GetInt("Vibrate", 1);
            if (vibrateValue == 0) return;
            if (duration == 0) {
                SDKMgr.Instance.OnDeviceVibration(intensity * vibrateValue);
            } else {
                DOVirtual.DelayedCall(duration, () => {
                    SDKMgr.Instance.OnDeviceVibration(intensity * vibrateValue);
                });
            }
        }

        /// <summary>
        /// 客户端上报埋点
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="param"></param>
        public static void LogEvent(string logId, Dictionary<string, object> param)
        {
            SDKEventParam eventParam = new SDKEventParam();
            eventParam.Key = logId;
            eventParam.Params = param;
            string jsonParam = JsonMapper.ToJson(eventParam);
            SDKMgr.Instance.CallSDKMethod("logEvent", jsonParam, "logEvent", returnData =>
            {
                if (returnData.Code.CallbackCode > 0)
                    Logger.Info($"Log Event {logId} success");
                else
                {
                    Logger.Info($"Log Event {logId} fail.Message:{returnData.Code.ErrMsg}");
                }
            });
        }
        
        /// <summary>
        /// 当前包是否微信小游戏
        /// </summary>
        /// <returns></returns>
        public static bool IsWechatMiniGame()
        {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
            return true;
#endif
            return false;
        }

        /// <summary>
        /// 获取设备平台
        /// </summary>
        /// <returns></returns>
        public static string GetDevicePlatform()
        {
            return SDKMgr.Instance.GetDeviceSystemInfo().Model;
        }

        public static void SetClipboardText(string str)
        {
            // 将文本复制到剪切板
            if (IsWechatMiniGame())
            {
                SDKMgr.Instance.CallSDKMethod("setClipboard", str, "", null);
            }
            else
            {
                GUIUtility.systemCopyBuffer = str;
            }

            ShowCommonTips(LocalizationPool.Get("Common/CopySuccess"));
        }

        public static bool CanBeginGame()
        {
            var liveConsumCount = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("LiveConsum");
            if (!G.GameItemModule.CheckHasBuff("liveBuff") && G.GameItemModule.GetItemCount("live") < liveConsumCount)
            {
                return false;
            }

            return true;
        }

        public static string TruncateString(string inputStr, int maxLength)
        {
            if (string.IsNullOrEmpty(inputStr)) return inputStr;

            int currentLength = 0;
            StringBuilder truncated = new StringBuilder();

            foreach (char c in inputStr)
            {
                // 如果字符是ASCII字符（通常是英文字符），则长度加2
                // 否则，长度加1（通常是中文字符）
                if (c >= 33 && c <= 126) // ASCII字符范围
                {
                    if (currentLength + 1 > maxLength)
                    {
                        truncated.Append("...");
                        break;
                    }
                    truncated.Append(c);
                    currentLength += 1;
                }
                else
                {
                    if (currentLength + 2 > maxLength)
                    {
                        truncated.Append("...");
                        break;
                    }
                    truncated.Append(c);
                    currentLength += 2;
                }
            }

            return truncated.ToString();
        }

        public static void ShowLackCoinPanel(int lackNum) {
            ShowCommonTips(LocalizationPool.Get("CommonTips/LockGoals"));
            
            var giftList = ConfigMemoryPool.Get<GradeGiftDB>().GetGradeGiftsByType(GradeGiftType.ResultCoin);
            var itemList = new List<ShopItems>();

            for (int i = 0; i < giftList.Count; i++)
            {
                var shopId = giftList[i].shopID;
                var shopDB = ConfigMemoryPool.Get<ShopItemsDB>();
                var item = shopDB[shopId];
                var itemDatas = GetItemDatasByStr(item.reward);
                foreach (var itemData in itemDatas) {
                    if (itemData.Name == "coin" && itemData.Count >= lackNum) {
                        itemList.Add(item);
                        break;
                    }
                }

                if (itemList.Count >= 2) {
                    break;
                }
            }

            if (itemList.Count == 0) return;
            if (itemList.Count == 1) {
                itemList.Add(itemList[0]);
            }

        }

        public static void ShowGradeGift(string itemName) {
            var shopDB = ConfigMemoryPool.Get<ShopItemsDB>();
            GradeGiftData data = null;
            if (itemName == "live") {
                data = G.IAPModule.GetGradeGiftData(GradeGiftType.LiveLack);
            } else if (itemName == "coin") {
                data = G.IAPModule.GetGradeGiftData(GradeGiftType.ResultCoin);
            } else {
                return;
            }

            var itemList = new List<ShopItems>();
            foreach (var shopID in data.shopID)
            {
                itemList.Add(shopDB[shopID]);
            }

            itemList.Sort((item1, item2) => item1.price.CompareTo(item2.price));
        }


        public static Sequence TextShine(TextMeshProUGUI text) {
            var seq = DOTween.Sequence();
			seq.Append(text.DOFade(1f, 0.5f));
			seq.AppendInterval(1.5f);
			seq.Append(text.DOFade(0f, 0.5f));
			seq.AppendInterval(0.5f);
			seq.SetLoops(-1);

            return seq;
        }

        public static Sequence BeatObj(GameObject obj) {
            var seq = DOTween.Sequence();
            seq.Append(obj.transform.DOScale(0.8f, 0.1f).SetEase(Ease.Linear));
            seq.Append(obj.transform.DOScale(1f, 0.1f).SetEase(Ease.OutBack));
            return seq;
        }

        public static string GetShopCoinIconPath(int coinNum) {
            var path = "uisprites/shop/shop_icon_";
            var iconNum = "0001";
            if (coinNum < 2000) {
                iconNum = "0002";
            } else if (coinNum < 5000) {
                iconNum = "0003";
            } else if (coinNum < 10000) {
                iconNum = "0004";
            } else if (coinNum < 50000) {
                iconNum = "0005";
            } else {
                iconNum = "0006";
            }

            return path + iconNum;
        }

        public static string GetShopBoxIconPath(int coinNum) {
            var path = "uisprites/shop/shop_icon_";
            var boxNum = "0007";
            if (coinNum < 5000) {
                boxNum = "0007";
            } else if (coinNum < 10000) {
                boxNum = "0008";
            } else if (coinNum < 50000) {
                boxNum = "0009";
            } else if (coinNum < 100000) {
                boxNum = "0010";
            } else {
                boxNum = "0011";
            }
            return path + boxNum;
        }

        public static string GetHeadName(string avatarId) {
            if (string.IsNullOrEmpty(avatarId)) return "Head01";
            if (avatarId.Contains("http")) return avatarId;

            var headID = int.TryParse(avatarId, out int id)? id : 1450;
            var itemDB = ConfigMemoryPool.Get<ItemEnumDB>();
            var headName = itemDB[headID].name;
            if (headName.Contains("Head")) return headName;

            return "Head01";
        }

        public static string GetHeadFrameName(string avatarFrame) {
            if (string.IsNullOrEmpty(avatarFrame)) return "Frame01";

            var headFrameID = int.TryParse(avatarFrame, out int id)? id : 1401;
            var itemDB = ConfigMemoryPool.Get<ItemEnumDB>();
            var headName = itemDB[headFrameID].name;
            if (headName.Contains("Frame")) return headName;

            return "Frame01";
        }

        /// <summary>
        /// 获取设备等级
        /// </summary>
        /// <returns></returns>
        public static DeviceLevel GetDeviceLevel()
        {
            var deviceInfo = SDKMgr.Instance.GetDeviceSystemInfo();
            string platform = deviceInfo.Platform;
            var benchmark = deviceInfo.DeviceBenchmarkLevel;
            //参数由微信小游戏提供:https://developers.weixin.qq.com/minigame/dev/guide/performance/perf-benchmarkLevel.html
            //抖音小游戏，需要重新获取和评级
            if (platform == "ios")
            {
                if(benchmark >= 36)
                    return DeviceLevel.High;
                if(benchmark >= 30 && benchmark < 36)
                    return DeviceLevel.Middle;
                return DeviceLevel.Low;
            }
            if (platform == "android" || platform == "ohos")
            {
                if(benchmark >= 30)
                    return DeviceLevel.High;
                if(benchmark >= 23 && benchmark < 30)
                    return DeviceLevel.Middle;
                return DeviceLevel.Low;
            }
            return DeviceLevel.High;
        }

        public static string GetCoinFormat(int coinNum) {
            if (coinNum < 10000) {
                return coinNum.ToString();
            } else {
                if (coinNum / 10000f < 100) {
                    return (coinNum / 10000f).ToString("0.00") + LocalizationPool.Get("Common/Wan");
                } else {
                    return (coinNum / 10000f).ToString("0.0") + LocalizationPool.Get("Common/Wan");
                }
            }
        }

        public static void ShowGiftUI<T>(params System.Object[] userDatas) where T : UIWindow
        {
            if (!G.SwitchModule.IsOpenPlay()) return;
            G.UIModule.ShowUIAsync<T>("", userDatas);
        }

        // 顶号处理
        public static void ShowMultiLogin() {
            ShowCommonPrompt(LocalizationPool.Get("Common/LoginMulti"), () => {
                if (IsWechatMiniGame()) {
                    SDKMgr.Instance.CallSDKMethod("quickGame", "", "", null);
                } else {
                    #if UNITY_STANDALONE
						Application.Quit();
                    #endif
                }
            });
        }
    }
}