using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GameConfig;
using GameCore.Localization;
using GameCore.SDK;
using GameCore.Settings;
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixLogic;
using HotfixLogic.Match;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.Module
{
    public class ShareInfo
    {
        public string shareId = "";
        public string title = "";
        public string imageUrl = "";
        public string query = "";
        public int delay = 3;
    }

    public class AdvModule : IModuleAwake, IModuleDestroy
    {
        private DateTime _setAdvTime;

        private Dictionary<int, AdvData> _advCount = new Dictionary<int, AdvData>();

        private int _curAdReviveCount = 0;
        private long _preAdReviveTime = 0;

        public void Awake(object parameter)
        {
        }

        public void Destroy()
        {
        }

        public int GetLastAdvCount(int advId) {
            if (_advCount.Count == 0) G.HttpModule.GetAdvCount();
            if (!_advCount.ContainsKey(advId)) return 0;

            var date = CommonUtil.GetNowDateTime();
            if (date.Day != _setAdvTime.Day) {
                G.HttpModule.GetAdvCount();
            }

            return _advCount[advId].daily_limit - _advCount[advId].get_reward_num;
        }

        public void SetAdvCount(List<AdvData> advDatas) {
            foreach (var advData in advDatas) {
                _advCount[advData.reward_id] = advData;
            }

            _setAdvTime = CommonUtil.GetNowDateTime();

            G.EventModule.DispatchEvent(GameEventDefine.OnUpdateAdvCount);
        }

        public void AddAdvCount(int advId, int count = 1) {
            if (!_advCount.ContainsKey(advId)) {
                G.HttpModule.GetAdvCount();
                return;
            }

            _advCount[advId].get_reward_num += count;


            G.EventModule.DispatchEvent(GameEventDefine.OnUpdateAdvCount);
        }

        public void PlayAdvVideo(int advId, Action<bool> callback = null,bool checkLimit = true, int level = 0) {
            if (checkLimit && GetLastAdvCount(advId) <= 0) {
                CommonUtil.ShowCommonTips(LocalizationPool.Get("Adv/LookLimit"));
                callback?.Invoke(false);
                return;
            }


            bool isPlayAdv = CommonUtil.IsWechatMiniGame() && PlayerPrefsUtil.GetInt("IsPlayAdv", 1) == 1;
            if (isPlayAdv) {
                SDKMgr.Instance.CallSDKMethod("showAd",$"{advId};{GiftIDDefine.AdvUnitId}", "", (returnData) =>
                {

                    var code = returnData.Code;
                    if (code.CallbackCode > 0)
                    {
                        Logger.Debug("showAd success");
                        G.HttpModule.ReportAdvState(advId, "pull", level);

                        callback?.Invoke(true);
                        
                        G.HttpModule.ReportAdvState(advId, "reward", level);
                        
                        TaskManager.Instance.TickTaskChanged();
                    }
                    else if (code.CallbackCode == 0)
                    {
                        Logger.Debug("showAd fail");
                        G.HttpModule.ReportAdvState(advId, "pull", level);

                        callback?.Invoke(false);
                    }
                    else
                    {
                        CommonUtil.ShowCommonTips(LocalizationPool.Get("Adv/LokkError"));
                        callback?.Invoke(false);
                    }

                    
                });
            } else {
                // G.HttpModule.ReportAdvState(advId, "pull", level);
                callback?.Invoke(true);
                // G.HttpModule.ReportAdvState(advId, "reward", level);
            }
        }

        public void PlayShare(Action<bool> callback = null, string query = "", string url = "") {
            if (CommonUtil.IsWechatMiniGame()) {
                ShareInfo shareInfo = new ShareInfo();
                if (query != "") shareInfo.query = query;
                if (url != "") shareInfo.imageUrl = url;

                string jsonArg = JsonUtility.ToJson(shareInfo);
                SDKMgr.Instance.CallSDKMethod("shareApp",jsonArg, "", returnData =>
                {
                    var code = returnData.Code;
                    if (code.CallbackCode > 0)
                    {
                        callback?.Invoke(true);
                        
                        G.HttpModule.ShareReport();
                    }
                    else
                    {
                        Debug.LogError("share fail：" + code.ErrMsg);
                        callback?.Invoke(false);
                    }
                });
            } else {
                callback?.Invoke(true);
            }
        }

        public int GetAdReviveCount() {
            if (_curAdReviveCount == 0) {
                _curAdReviveCount = PlayerPrefsUtil.GetInt("AdReviveCount", 0);
            }

            var dailyReviveCount = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("AdReviveCount");
            if (_curAdReviveCount >= dailyReviveCount) {
                var timeStr = PlayerPrefsUtil.GetString("AdReviveTime", "");
                
                if (timeStr == "") {
                    _preAdReviveTime = CommonUtil.GetNowTime();
                } else {
                    _preAdReviveTime = long.TryParse(timeStr, out var time) ? time : CommonUtil.GetNowTime();
                }

                var preDateTime = CommonUtil.UnixToLocalDateTime(_preAdReviveTime);
                var todayDateTime = CommonUtil.GetNowDateTime();
                var diffTime = todayDateTime - preDateTime;
                if (diffTime.TotalDays >= 1) {
                    _curAdReviveCount = 0;
                    PlayerPrefsUtil.SetInt("AdReviveCount", 0);
                }
            }
            

            return dailyReviveCount - _curAdReviveCount;
        }

        public void AddAdReviveCount() {
            _curAdReviveCount++;
            var timeStr = CommonUtil.GetNowTime().ToString();

            PlayerPrefsUtil.SetString("AdReviveTime", timeStr);
            PlayerPrefsUtil.GetInt("AdReviveCount", _curAdReviveCount);
        }
    }
}
