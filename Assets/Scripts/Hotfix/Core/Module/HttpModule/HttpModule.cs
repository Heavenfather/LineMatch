using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using GameConfig;
using GameCore.LitJson;
using GameCore.Localization;
using GameCore.Logic;
using GameCore.SDK;
using GameCore.Settings;
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixCore.MVC;
using HotfixLogic;
using HotfixLogic.Match;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.Module
{
    public class HttpModule : IModuleAwake, IModuleDestroy
    {
        //hack 上传地址
        // private string ADDRESS = "https://dev-slow-gallery-api.game.jingyougz.com";
        private string ADDRESS
        {
            get
            {
                return GameSettings.Instance.ProjectSetting.GameAddress;
            }
        }
        private const string payNotify = "/api/v1/notify/callback";
        private const string SIGN_KEY = "02a6ee58-ccd8-4e14-8fae-644975b500af";
        private string Access_Token = "";
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private string Open_ID = "";
        private string Pf_Open_ID = "";
        private bool _isDevEvironment = true;

        public void Awake(object parameter)
        {
            Open_ID = PlayerPrefsUtil.GetString("open_id", "");
            if (Open_ID == "") {
                Open_ID = GenerateRandomString(10);
                PlayerPrefsUtil.SetString("open_id", Open_ID);
            }
            Pf_Open_ID = Open_ID;

            _isDevEvironment = GameSettings.Instance.ProjectSetting.AppMode == EAppMode.Debug;

            SDKMgr.Instance.CallSDKMethod("setEvirnment", _isDevEvironment.ToString(), "", null);
        }

        public void Destroy()
        {
        }

        public void UpdateData(string open_id, string pf_open_id, string access_token) {
            UpdateOpenID(open_id);
            UpdatePFOpenID(pf_open_id);
            UpdateAccessToken(access_token);

            Logger.Debug("UpdateData open_id = " + open_id + " pf_open_id = " + pf_open_id + " access_token = " + access_token);
        }

        public void UpdateOpenID(string openID) {
            Open_ID = openID;
        }

        public void UpdatePFOpenID(string pf_openID) {
            Pf_Open_ID = pf_openID;
        }

        public string GetPayNotifyUrl() {
            return ADDRESS + payNotify;
        }

        private void UpdateAccessToken(string token) {
            Logger.Debug("UpdateAccessToken token = " + token);
            if (token!= "") {
                Access_Token = token;
            }
        }

        /// <summary>
        /// 获得要Get或Post的地址
        /// </summary>
        /// <param name="interfaceName">接口名字，详见const定义</param>
        /// <param name="dataJson">除固定参数外的参数</param>
        /// <returns></returns>
        public string GetUrl(string interfaceName, Dictionary<string, object> dataJson = null)
        {
            string baseUrl = GetBaseUrl();

            string param = "";

            if (dataJson != null && dataJson.Count > 0) {
                var keys = new List<string>(dataJson.Keys);
                keys.Sort(); // 手动排序
                foreach (var key in keys)
                {
                    param += key;
                    param += '=';
                    param += dataJson[key];
                    param += '&';
                }
                param = param.Remove(param.Length - 1);
            }


            //获取url
            string url = ADDRESS + interfaceName + '?' + baseUrl;
            if (param != "") {
                url += "&" + param;
            }

            return url;
        }

        public string GetBaseUrl() {
            Dictionary<string, object> dataJson = new Dictionary<string, object>();

            string openID = Open_ID;

            // 配置app_id
            dataJson.Add("app_id", GameSettings.Instance.AppSetting.AppID);
            // 配置channel_id
            dataJson.Add("channel_id", GameSettings.Instance.AppSetting.ChannelID);
            // 配置version
            dataJson.Add("version", Application.version);
            // 配置trace_id
            dataJson.Add("trace_id", GenerateRandomString(32));
            // 配置open_id
            dataJson.Add("open_id", openID);
            // 配置时间戳
            dataJson.Add("t", (int)CommonUtil.GetNowTime());

            string param = "";
            var keys = new List<string>(dataJson.Keys);
            keys.Sort(); // 手动排序
            foreach (var key in keys)
            {
                param += key;
                param += '=';
                param += dataJson[key];
                param += '&';
            }
            param = param.Remove(param.Length - 1);

            param += "&sign=" + GetMd5(param + SIGN_KEY);


            return param;
        }
        
        /// <summary>
        /// 获取Md5
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string GetMd5(string input)
        {
            // 创建 MD5CryptoServiceProvider 对象的新实例。
            MD5 md5Hasher = MD5.Create();
            
            // 将输入字符串转换为字节数组并计算哈希。
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            
            // 创建一个新的 StringBuilder 来收集字节并创建一个字符串。
            StringBuilder sBuilder = new StringBuilder();
            
            // 循环遍历散列数据的每个字节，并将每个字节格式化为十六进制字符串。
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            
            // 返回十六进制字符串。
            return sBuilder.ToString();
        }

        private void CheckEnterParam() {
            SDKMgr.Instance.CallSDKMethod("getLaunchParams", "", "", (returnData) => {
                var param = returnData.Param;
                Logger.Debug("CheckGetShareCard param = " + param);

                if (string.IsNullOrEmpty(param) || param == "") return;
                
                var dict = JsonMapper.ToObject<Dictionary<string, string>>(param);
                if (dict == null) return;

                if (dict.ContainsKey("shareCode")) {
                    Logger.Debug("shareCode = " + dict["shareCode"]);
                    ReceiveShareCard(dict["shareCode"]);
                }

                if (dict.ContainsKey("recommend_code")) {
                    Logger.Debug("recommend_code = " + dict["recommend_code"]);
                    UpdateUserInfo("recommend_code", dict["recommend_code"]);
                }
            });
        }

        private List<ItemData> ExchangeAddItemDatas(List<ServerItem> rewardDatas) {
            List<ItemData> itemDatas = new List<ItemData>(rewardDatas.Count);
            foreach (var item in rewardDatas) {
                var data = new ItemData(item.item_id, item.reward_num);
                
                if (item.item_expend_time > 0) {
                    data.Time = item.item_expend_time;
                }
                itemDatas.Add(data);
            }
            return itemDatas;
        }

        private List<ItemData> ExchangeSetItemDatas(List<ServerItem> serverItems) {
            List<ItemData> itemDatas = new List<ItemData>();
            foreach (var item in serverItems) {
                var data = new ItemData(item.item_id, item.item_num);
                if (item.item_expend_time > 0) {
                    data.Time = item.item_expend_time;
                } else if (item.item_expire_time > 0) {
                    data.Time = item.item_expire_time;
                }
                itemDatas.Add(data);
            }
            return itemDatas;
        }

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="length"></param>
        private string GenerateRandomString(int length)
        {
            
            char[] randomChars = new char[length];
            System.Random random = new System.Random();

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(chars.Length);
                randomChars[i] = chars[index];
            }

            return new string(randomChars);
        }

        private void SendRequest(string interfaceName, Dictionary<string, object> data, Action<string, int> callback = null, bool isPost = true) {
            string url = GetUrl(interfaceName, data);
            Logger.Debug("PostWebRequest url = " + url);

            if (isPost) {
                PostWebRequest(url, data, callback).Forget();
            } else {
                GetWebRequest(url, callback).Forget();
            }
        }

        private async UniTask PostWebRequest(string url, Dictionary<string, object> postData, Action<string, int> callback = null)
        {
            Dictionary<string, string> headDict = null;

            if (Access_Token != "") {
                headDict = new Dictionary<string, string>();
                headDict.Add("Authorization", "Bearer " + Access_Token);
            }
            
            // string postDataStr = JsonConvert.SerializeObject(postData);
            string result = await HttpUtil.Post(url, null, headDict);
            if (string.IsNullOrEmpty(result))
            {
                callback?.Invoke("", -1);
                return;
            }

            JsonData root;
            try
            {
                root = JsonMapper.ToObject(result);
            } catch (Exception e) {
                Logger.Error("PostWebRequest result = " + result);
                Logger.Error("PostWebRequest json parse error = " + e.Message);
                return;
            }

            if (root == null) return;
            int resultCode = (int)root["code"];
            Logger.Debug("GetWebRequest result = " + result);

            
            if (resultCode == 0) {
                JsonData dataToken = root["data"];
                string dataStr = string.Empty;
                if (dataToken != null)
                {
                    dataStr =  dataToken.ToJson();
                }

                CommonUtil.UpdateServerTime((long)root["time"]);

                // 调用回调函数并传递结果
                callback?.Invoke(dataStr, resultCode);
            } else if (resultCode == 3000) {
                // 顶号了
                CommonUtil.ShowMultiLogin();
            } else {
                Logger.Info("resultCode = " + resultCode);
                Logger.Info("msg = " + root["msg"]);
                // if (resultCode == 1006) {
                //     CommonUtil.ShowCommonTips((string)root["msg"]);
                // }
                callback?.Invoke(root["msg"].ToString(), resultCode);
            }
        }


        private async UniTask GetWebRequest(string url, Action<string, int> callback = null)
        {

            Dictionary<string, string> headDict = null;
            if (Access_Token != "") {
                headDict = new Dictionary<string, string>();
                headDict.Add("Authorization", "Bearer " + Access_Token);
            }
            string result = await HttpUtil.Get(url, headDict);

            JsonData root = JsonMapper.ToObject(result);
            int resultCode = (int)root["code"];
            

            if (resultCode == 0) {
                JsonData dataToken = root["data"];
                string dataStr = dataToken.ToJson();
            
                CommonUtil.UpdateServerTime((long)root["time"]);
                // 调用回调函数并传递结果
                callback?.Invoke(dataStr, resultCode);
            } else {
                Logger.Error("resultCode = " + resultCode);
                Logger.Error("msg = " + root["msg"]);
                callback?.Invoke(root["msg"].ToString(), resultCode);
            }
        }

        public void GetServerTime() {   

            Logger.Debug("GetServerTime  ");
            SendRequest("/api/v1/server/time", null, (string result, int code) =>
            {
                if (code != 0) {
                    Logger.Error("GetServerTime fail");
                    return;
                }
                Logger.Debug("GetServerTime success");
            }, false);
        }

        public void PlayerLogin()
        {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("pf_openid", Pf_Open_ID);

            SendRequest("/api/v1/login/simplify", infoDic, (string result, int code) =>
            {
                if (code != 0) {
                    Logger.Info("PlayerLogin fail");
                    return;
                }
                Logger.Info($"PlayerLogin result:{result},code:{code}");

                LoginResponseData loginData = JsonMapper.ToObject<LoginResponseData>(result);
                RoleServerInfo roleServerInfo = new RoleServerInfo()
                {
                    server_id = "1",
                    server_name = "test",
                    role_id = loginData.user.user_id.ToString(),
                    role_name = $"{loginData.user.nickname}_{loginData.user.user_id}",
                    role_level = 1,
                    vip_level = 1,
                };
                string roleInfoJson = JsonMapper.ToJson(roleServerInfo);
                SDKMgr.Instance.CallSDKMethod("enterGameLog", roleInfoJson, "", cal => { });
                
                UpdateAccessToken(loginData.access_token);

                G.UserInfoModule.SetUserId(loginData.user.user_id);
                G.UserInfoModule.SetCreateTime(loginData.user.create_time);
                G.UserInfoModule.SetNickname(loginData.user.nickname);
                G.UserInfoModule.SetAvatar(loginData.user.avatar);
                G.UserInfoModule.SetAvatarFrame(loginData.user.avatar_frame);
                G.UserInfoModule.SetFirstPassCount(loginData.user.once_pass_times);
                G.UserInfoModule.SetMaxRank(loginData.user.max_ranking);
                G.UserInfoModule.SetMaxWinStreak(loginData.user.max_win_streak);
                G.UserInfoModule.SetUsingMedal(loginData.user.medal);
                G.UserInfoModule.SetNameColorID(loginData.user.nickname_color);
                G.UserInfoModule.SetInviteCode(loginData.user.invite_code);

                CheckEnterParam();

                G.RedDotModule.SetRedDotCount(RedDotDefine.SignDaily,loginData.daily_sign_state == 0 ? 1 : -1);

                // 发送心跳
                G.TimerModule.AddTimer(ReportHeartBeat, 60f, true);

                QueryBaseData();

                G.EventModule.DispatchEvent(GameEventDefine.OnLoginFinish);
            });
        }

        private void QueryBaseData() {
            // AB分组
            QueryABConfig();
            //开关状态
            QuerySwitchData();
            //道具列表
            UpdateItemData();
            //请求关卡数据
            QueryStageData();
            //目标任务数据
            GetTargetTaskQuery();
            //请求寻宝数据
            QueryPuzzleInfo();
            GetCollectCardData();
        }

        public void ReqGetGuideReward(string rewardId, Action<string, int> callback = null)
        {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("reward_id", rewardId);
            infoDic.Add("scene_id", 0);
            
            SendRequest("/api/v1/activity_reward/finish/item_guide", infoDic, (result, code) =>
            {
                if (code != 0) {
                    Logger.Error($"Get Guide reward {rewardId} fail");
                    return;
                }
                
                SignGetRewardData rewardData = JsonMapper.ToObject<SignGetRewardData>(result);
                List<ItemData> itemDatas = ExchangeAddItemDatas(rewardData.rewards);
                G.GameItemModule.AddItemCount(itemDatas);
                callback?.Invoke(result, code);
            });
        }

        public void QueryPuzzleInfo() {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            
            SendRequest("/api/v1/puzzle/puzzle_info", infoDic, (result, code) =>
            {
                if (code != 0) {
                    Logger.Error("QueryPuzzleInfo fail");
                    return;
                }
                
                ServerPuzzleData data = JsonMapper.ToObject<ServerPuzzleData>(result);

            });
        }

        // 寻宝进度上传
        public void UploadPuzzleData(int map_id, string data, Action<string, int> callback = null)
        {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("map_id", map_id);
            infoDic.Add("data", data);

            SendRequest("/api/v1/puzzle/update_user_data", infoDic, (string result, int code) =>
            {
                if (code != 0) {
                    Logger.Error("UploadPuzzleData fail");
                    return;
                }
                Logger.Info($"UploadPuzzleData result:{result},code:{code}");
                TaskManager.Instance.TickTaskChanged();
                callback?.Invoke(result, code);
            });
        }

        public void ShareReport()
        {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/share/report", infoDic, (msg, code) =>
            {
                if (code != 0)
                {
                    TaskManager.Instance.TickTaskChanged();
                }
            });
        }
        
        public void UseItem(ItemData items, Action<string, int> callback = null, int scene_id = 0) {
            List<ServerItem> serverItems = new List<ServerItem>();
            serverItems.Add(new ServerItem() {
                item_id = items.Id,
                item_num = items.Count
            });
            UseItem(serverItems, callback, scene_id);
        }

        public void UseItem(List<ItemData> items, Action<string, int> callback = null, int scene_id = 0) {
            List<ServerItem> serverItems = new List<ServerItem>();
            foreach (var item in items) {
                serverItems.Add(new ServerItem() {
                    item_id = item.Id,
                    item_num = item.Count
                });
            }
            UseItem(serverItems, callback, scene_id);
        }

        public void UseItem(List<ServerItem> serverItems, Action<string, int> callback = null, int scene_id = 0) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("scene_id", scene_id);

            string itemJson = JsonMapper.ToJson(serverItems);
            infoDic.Add("item", itemJson);
            SendRequest("/api/v1/item/consume", infoDic, (msg, code) =>
            {
                // Logger.Info($"Use Item msg : {msg} , code : {code}");
                if (code == 0)
                {
                    ItemListData listData = JsonMapper.ToObject<ItemListData>(msg);

                    var itemDatas = ExchangeSetItemDatas(listData.items);
                    G.GameItemModule.SetItemCount(itemDatas);
                }
                callback?.Invoke(msg, code);
            });
        }

        // GM加道具  action:add_item  param:item_id,change_num
        public void GMRequest(string param, string action = "add_item", Action<bool> callback = null)
        {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("action", action);
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("param", param);
            SendRequest("/api/v1/gm", infoDic, (string result, int code) =>
            {
                if (code != 0) {
                    Logger.Debug("GMRequest fail");
                    CommonUtil.ShowCommonTips("GM请求失败");
                    return;
                }

                Logger.Info($"GMRequest result:{result},code:{code}");
                ServerItem serverItem = JsonMapper.ToObject<ServerItem>(result);
                G.GameItemModule.AddItemCount(serverItem.item_id, serverItem.change_num);

                callback?.Invoke(true);

                // var name = ConfigMemoryPool.Get<ItemEnumDB>()[serverItem.item_id].alias;
                // CommonUtil.ShowCommonTips($"添加成功\n道具ID:{name}，数量:{serverItem.item_num}");
            });
        }

        // 游戏开始上报
        public void ReportLevelGameBegin(int stage_id, Action<string, int> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("stage_id", stage_id);
            SendRequest("/api/v1/stage/start", infoDic, (string result, int code) =>
            {
                if (code != 0) {
                    Logger.Debug("ReportLevelGameBegin fail");
                    if (code == 1006) {
                        Logger.Debug(LocalizationPool.Get("Common/LiveLack"));
                        UpdateItemData();
                    }
                } else {
                    StageData stageData = JsonMapper.ToObject<StageData>(result);
                    if (stageData.stage_id == MatchManager.Instance.MaxLevel)
                        LevelManager.Instance.SetCurrentStagePlayCount(stageData.setting.stage_play_cnt);
                    else
                        LevelManager.Instance.SetCurrentStagePlayCount(1); //重复玩的关卡，直接记录为1

                    LevelManager.Instance.ResetLevelDifficultyValue(stageData.setting.stage_val_detail.behavior_val, stageData.setting.stage_val_detail.group_val);

                    G.GameItemModule.GameConsumLive();
                    SDKMgr.Instance.CallSDKMethod("stageStartLog",$"{stageData.stage_id}","stageStartLog",cal=>{});

                    MatchManager.Instance.SetBeginTipsData(stage_id);
                }

                callback?.Invoke(result, code);
            });
        }

        // 游戏结束上报
        public void ReportLevelGameEnd(int stage_id, bool is_success, int use_step, int conf_step, int use_time, int stage_diff, int stage_behavior_val, 
                                        int stage_group_val, int targetNum = 0, int score = 0, int ad_revive = 0, int coin_revive = 0, 
                                        int revive_coin = 0, string use_items = "", string coin_exchange = "", List<ItemData> rewardItems = null,string item_generate_and_use = "") {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("stage_id", stage_id);
            infoDic.Add("is_success", is_success ? 1 : 0);
            infoDic.Add("use_step", use_step); 
            infoDic.Add("conf_step", conf_step); 
            infoDic.Add("use_time", use_time);
            infoDic.Add("objective_num", targetNum); 
            infoDic.Add("use_items", use_items); 
            infoDic.Add("score", score);
            infoDic.Add("ad_revive", ad_revive);
            infoDic.Add("coin_revive", coin_revive);
            infoDic.Add("coin_exchange", coin_exchange);
            infoDic.Add("revive_coin", revive_coin);
            infoDic.Add("stage_diff", stage_diff);
            infoDic.Add("stage_behavior_val", stage_behavior_val);
            infoDic.Add("stage_group_val", stage_group_val);
            infoDic.Add("item_generate_and_use", item_generate_and_use);

            int stage_type = LevelManager.Instance.IsCoinLevel ? 2 : 0;
            infoDic.Add("stage_type", stage_type);
            

            List<ServerItem> rewardServerItems = new List<ServerItem>();
            if (rewardItems != null)
            {
                foreach (var item in rewardItems)
                {
                    rewardServerItems.Add(new ServerItem()
                    {
                        item_id = item.Id,
                        item_num = item.Count
                    });
                }
            }

            var stage_reward = JsonMapper.ToJson(rewardServerItems);
            infoDic.Add("stage_reward", stage_reward);

            SendRequest("/api/v1/stage/end", infoDic, (string result, int code) =>
            {
                if (code != 0) {
                    Logger.Debug("ReportLevelGameEnd fail");
                    return;
                }
                Logger.Debug($"ReportLevelGameEnd result:{result},code:{code}");

                var addCoin = 0;
                var addStar = 0;

                StageEndData stageEndData = JsonMapper.ToObject<StageEndData>(result);

                Logger.Debug("ReportLevelGameEnd is_once_pass = " + stageEndData.is_once_pass);
                bool isFirstWin = stage_id == MatchManager.Instance.MaxLevel;

                if (is_success) {
                    foreach (var item in rewardItems) {
                        if (item.Name == "star") {
                            var curStar = MatchManager.Instance.GetLevelStar(stage_id);
                            if (item.Count > curStar) {
                                MatchManager.Instance.SetLevelState(stage_id, item.Count);
                                G.GameItemModule.AddItemCount(item.Name, item.Count - curStar);

                                addStar = item.Count - curStar;
                            }
                        } else {
                            if (item.Name == "coin") {
                                addCoin = item.Count;
                            }
                            G.GameItemModule.AddItemCount(item.Name, item.Count);
                        }
                    }

                    var param = EventTwoParam<int, int>.Create(addCoin, addStar);
                    G.EventModule.DispatchEvent(GameEventDefine.OnMatchResultAddCoinAndStar, param);
                    
                    TaskManager.Instance.TickTaskChanged();
                } else {
                    MatchManager.Instance.AddMaxLvLostCount(stage_id);
                }

                MatchManager.Instance.SetMatchLevelID(stageEndData.stage_id);
                LevelManager.Instance.ResetLevelDifficultyValue(stageEndData.setting.stage_val_detail.behavior_val, stageEndData.setting.stage_val_detail.group_val);
                if (stageEndData.stage_id == MatchManager.Instance.MaxLevel)
                {
                    LevelManager.Instance.SetIsCoinLevelState(stageEndData.is_coin_stage);
                }

                if (isFirstWin) {
                    if (is_success) {
                        MatchManager.Instance.AddWinStreak();
                        G.TrainMasterModule.AddWinStreak();

                        if (stageEndData.is_once_pass == 1) {
                            G.UserInfoModule.AddFirstPassCount();
                        }
                    } else {
                        MatchManager.Instance.SetWinStreak(0);
                        G.TrainMasterModule.ResetWinStreak();
                    }
                }
                SDKMgr.Instance.CallSDKMethod("stageEndLog",$"{stage_id}-{is_success}","stageEndLog",callback=>{});

                G.TargetTaskModule.SetTargetTask(stageEndData.objective);
                G.EventModule.DispatchEvent(GameEventDefine.OnTargetAddCount, EventOneParam<int>.Create(targetNum));
            });
        }

        public void GameEndCoinMulti(int coin, int multiple, Action<bool> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("coin", coin);
            infoDic.Add("multiple", multiple);
            SendRequest("/api/v1/stage/additional_coin", infoDic, (string result, int code) =>
            {
                if (code != 0) {
                    Logger.Debug("GameEndCoinMulti fail");
                    return;
                } else {
                    float mul = (float)multiple / 10;
                    int addCoin = (int)(coin * mul) - coin;
                    G.GameItemModule.AddItemCount("coin", addCoin);
                }
                callback?.Invoke(code == 0);
            });
        }

        public void GetShopData(Action<bool> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/shop/product_list", infoDic, (string result, int code) =>
            {
                Logger.Debug($"GetShopData result:{result},code:{code}");
                if (code != 0) {
                    Logger.Debug("GetShopData fail");
                    return;
                } else {
                    ServerShopData shopData = JsonMapper.ToObject<ServerShopData>(result);
                    G.UserInfoModule.SetRegisterTime(shopData.register_time);

                    G.IAPModule.SetMoonthlyAmount(shopData.pay_money / 100);
                    G.IAPModule.SetBuyItemData(shopData.product_list);
                }
                callback?.Invoke(code == 0);
            });
        }

        // 创建订单
        public void CreateOrder(int product_id, int scene_id = 0, int scene_gift = 0, int scene_pop = 0) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("product_id", product_id);
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("scene_id", scene_id);
            infoDic.Add("scene_gift", scene_gift);
            infoDic.Add("scene_pop", scene_pop);

            // Logger.Debug("http 创建账单");
            SendRequest("/api/v1/order/create", infoDic, (string result, int code) =>
            {
                if (code == 0) {
                    Logger.Debug("CreateOrder success");
                    OrderData orderData = JsonMapper.ToObject<OrderData>(result);

                    Logger.Debug("是否微信小游戏 " + CommonUtil.IsWechatMiniGame());

                    if (CommonUtil.IsWechatMiniGame()) {
                        G.IAPModule.PayOrder(product_id, orderData.order_no, scene_id);
                    } else {
                        var dict = new Dictionary<string, object>();
                        dict.Add("order_no", orderData.order_no);
                        dict.Add("scene_id", scene_id);
                        dict.Add("shop_id", product_id);
                        dict.Add("shop_time", CommonUtil.GetNowTime());

                        G.IAPModule.RecordOrder(orderData.order_no, JsonMapper.ToJson(dict));
                        OrderDeliver(orderData.order_no);
                    }
                    } else {
                        Logger.Debug("创建订单 失败");
                    }
            });
        }

        // 通知服务器订单支付成功
        public void OrderDeliver(string order_no) {
            Logger.Debug("http 通知发货");

            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("order_no", order_no);
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/order/deliver", infoDic, (string result, int code) =>
            {
                if (code == 0) {
                    Logger.Debug("OrderDeliver success");
                    // OrderStateData orderStateData = JsonConvert.DeserializeObject<OrderStateData>(result);
                    // Logger.Debug("OrderStateQuery success order_state = " + orderStateData.order_state);
                    // if (orderStateData.order_state == 1) {
                    //     Logger.Debug("支付订单创建");
                    // } else if (orderStateData.order_state == 2) {
                    //     Logger.Debug("支付成功");
                    // }

                    G.IAPModule.ShowShopReward(order_no);

                    G.IAPModule.BuyProductFinish(order_no);
                } else {
                    Logger.Debug("订单查询 失败");
                }
            });
        }

        // 订单状态查询
        public void OrderStateQuery(string order_no, int scene_id = 0) {
            Logger.Debug("http 查询账单状态");

            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("order_no", order_no);
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/order/query", infoDic, (string result, int code) =>
            {
                if (code == 0) {
                    Logger.Debug("OrderStateQuery success  = " + result);
                    OrderStateData orderStateData = JsonMapper.ToObject<OrderStateData>(result);
                    
                    if (orderStateData.order_state == 1) {
                        Logger.Debug("支付订单创建");
                    } else if (orderStateData.order_state == 2) {
                        Logger.Debug("支付成功");
                    }

                    if (orderStateData.order_state == 2) {
                        G.IAPModule.ShowShopReward(order_no);
                        G.IAPModule.BuyProductFinish(order_no);
                    }
                } else {
                    Logger.Debug("订单查询 失败");
                }
            });
        }

        // 获取奖励
        public void ShopExchange(int exchange_id, Action<bool> callback = null, int scene_id = 0) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("exchange_id", exchange_id);
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("scene_id", scene_id);
            SendRequest("/api/v1/coin_shop/exchange", infoDic, (string result, int code) =>
            {
                if (code != 0) {
                    Logger.Debug("ShopExchange fail");
                    return;
                }
                Logger.Debug("ShopExchange success result = " + result);
                ShopExchangeData shopExchangeData = JsonMapper.ToObject<ShopExchangeData>(result);

                List<ItemData> itemDatas = ExchangeAddItemDatas(shopExchangeData.rewards);

                G.GameItemModule.AddItemCount(itemDatas);
                CommonUtil.ShowRewardWindow(itemDatas, LocalizationPool.Get("Reward/Tips/CoinExchange"));

                callback?.Invoke(code == 0);

                G.EventModule.DispatchEvent(GameEventDefine.OnExchangeCoinShopSucc, EventOneParam<int>.Create(exchange_id));
            });
        }

        // 获取奖励
        public void GetTargetTaskReward(string reward_id, bool is_double = false, int scene_id = 0, Action<List<ItemData>> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("reward_id", reward_id);
            infoDic.Add("is_double", is_double ? 1 : 0);
            infoDic.Add("scene_id", scene_id);
            SendRequest("/api/v1/activity_reward/finish/objective_reward", infoDic, (string result, int code) =>
            {
                if (code != 0) {
                    Logger.Debug("GetTargetTaskReward fail");
                    callback?.Invoke(null);
                    return;
                }
                Logger.Debug("GetTargetTaskReward success result = " + result);

                G.TargetTaskModule.GetTargetTaskRewardFinish();

                ActivityReward targetTaskReward = JsonMapper.ToObject<ActivityReward>(result);
                List<ItemData> itemDatas = ExchangeAddItemDatas(targetTaskReward.rewards);
                for (int i = itemDatas.Count - 1; i >= 0; i--) {
                    if (itemDatas[i].Id == 0) {
                        itemDatas.RemoveAt(i);
                        Logger.Error("返回的奖励ID = 0");
                    }
                }


                G.GameItemModule.AddItemCount(itemDatas);
                
                callback?.Invoke(itemDatas);
            });
        }

        // 获取奖励
        public void GetTargetTaskQuery() {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/activity_reward/query/objective_reward", infoDic, (string result, int code) =>
            {
                if (code != 0) {
                    Logger.Debug("GetTargetTaskQuery fail");
                    return;
                }
                Logger.Debug("GetTargetTaskQuery success result = " + result);

                TargetTaskQueryData targetTaskQueryData = JsonMapper.ToObject<TargetTaskQueryData>(result);
                G.TargetTaskModule.SetTargetTask(targetTaskQueryData.objective);

                G.EventModule.DispatchEvent(GameEventDefine.OnTargetTaskUpdate);
            });
        }

        public void GetADReward(int reward_id, int scene_id = 0, bool is_double = false, Action<bool> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("reward_id", reward_id);
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("is_double", is_double);
            infoDic.Add("scene_id", scene_id);

            SendRequest("/api/v1/ad_reward/finish", infoDic, (string result, int code) => {
                if (code == 0) {
                    var rewardData = JsonMapper.ToObject<RewardData>(result);
                    List<ItemData> itemDatas = ExchangeAddItemDatas(rewardData.rewards);

                    G.GameItemModule.AddItemCount(itemDatas);
                    CommonUtil.ShowRewardWindow(itemDatas, LocalizationPool.Get("Reward/Tips/Adv"));

                    G.AdvModule.AddAdvCount(reward_id);
                    G.EventModule.DispatchEvent(GameEventDefine.OnGetAdRewardSucc, EventOneParam<int>.Create(reward_id));
                }

                callback?.Invoke(code == 0);
            });
        }

        public void GetAdvCount() {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/ad_reward/query", infoDic, (string result, int code) =>
            {
                if (code == 0) {
                    Logger.Debug("GetAdvCount success result = " + result);
                    var advList = JsonMapper.ToObject<List<AdvData>>(result);
                    G.AdvModule.SetAdvCount(advList);

                    if (G.PopModule.CheckDailyIsPop("ShopRedDot")) {
                        G.RedDotModule.SetRedDotCount(RedDotDefine.Shop, G.AdvModule.GetLastAdvCount(1));
                    }
                } else {
                    Logger.Debug("GetAdvCount fail");
                }
            });
        }

        #region 排行榜相关
        public void GetRankOfWorld(Action<bool> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);

            SendRequest("/api/v1/rank/world", infoDic, (string result, int code) =>
            {
                if (code == 0) {
                    Logger.Debug($"GetRankOfWorld result:{result},code:{code}");
                    WorldRankingData worldRankingData = JsonMapper.ToObject<WorldRankingData>(result);

                    G.RankModule.SetWorldRankData(worldRankingData.self_rank, worldRankingData.ranks);
                } else {
                    Logger.Debug("GetRankOfWorld fail");
                }

                callback?.Invoke(code == 0);
            });
        }

        public void GetRankOfWinStreak(Action<bool> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/rank/win_streak", infoDic, (string result, int code) =>
            {
                if (code == 0) {
                    Logger.Debug($"GetRankOfWinStreak result:{result},code:{code}");
                    WinStreakRankingData data = JsonMapper.ToObject<WinStreakRankingData>(result);

                    G.RankModule.SetWinStreakRankData(data.self_rank, data.ranks, data.end_tim);
                    G.RankModule.SetLastSreakState(data.reward_state, data.last_week_rank);
                } else {
                    Logger.Debug("GetRankOfWinStreak fail");
                }

                callback?.Invoke(code == 0);
            });
        }

        public void GetRankOfWinStreakReward(Action<bool> callback = null, int scene_id = 0) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("scene_id", scene_id);
            SendRequest("/api/v1/activity_reward/finish/win_streak_rank", infoDic, (string result, int code) =>
            {
                if (code == 0) {
                    Logger.Debug($"GetRankOfWinStreakReward result:{result},code:{code}");
                    ActivityReward rankReward = JsonMapper.ToObject<ActivityReward>(result);
                    List<ItemData> itemDatas = ExchangeAddItemDatas(rankReward.rewards);

                    G.GameItemModule.AddItemCount(itemDatas);
                    CommonUtil.ShowRewardWindow(itemDatas, LocalizationPool.Get("Reward/Tips/Ranking"));

                    G.RankModule.SetWinStreakRewardState(2);
                } else {
                    Logger.Debug("GetRankOfWinStreakReward fail");
                }
                callback?.Invoke(code == 0);
            });
        }
        #endregion


        #region 集卡相关
        public void GetCardData(Action<bool> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/card/list", infoDic, (string result, int code) =>
            {
                if (code == 0) {
                    Logger.Debug($"GetMyCardData result:{result},code:{code}");

                    var data = JsonMapper.ToObject<CardResponseData>(result);
                    G.CardModule.SetThemeID(data.theme_id, data.end_time);

                    G.CardModule.SetTotalRewardID(data.reward_id);
                    G.CardModule.SetTotalRewardState(data.collect_card_total);
                    if (data.collect_card_total == 1) {
                        G.RedDotModule.AddRedDotCount(RedDotDefine.CardTotalReward);
                    }

                    foreach (var rewardData in data.star.reward_list) {
                        G.CardModule.SetStarRewardState(rewardData.reward_id, rewardData.reward_state);
                        if (rewardData.reward_state == 1) {
                            G.RedDotModule.AddRedDotCount(RedDotDefine.CardStar);
                        }
                    }

                    foreach (var rewardData in data.pack) {
                        G.CardModule.SetPackRewardState(rewardData.reward_id, rewardData.reward_state);
                        if (rewardData.reward_state == 1) {
                            G.RedDotModule.AddRedDotCount(RedDotDefine.CardPackReward);
                        }
                    }

                    foreach (var card in data.cards) {
                        // 设置卡片数量
                        var curCardCount = G.CardModule.GetCardCount(card.card_id);
                        G.CardModule.AddCardData(card.card_id, card.card_num - curCardCount, false);
                    }

                    G.CardModule.SetStartCount(data.star.star_num);
                    G.CardModule.AddShareCount(data.share_limit);
                } else {
                    Logger.Debug("GetMyCardData fail");
                }

                callback?.Invoke(code == 0);
            });
        }

        public void GetCollectCardData() {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/user/get_user_card_collect", infoDic, (string result, int code) =>
            {
                if (code == 0) {
                    Logger.Debug($"GetCollectCardData result:{result},code:{code}");
                    var data = JsonMapper.ToObject<CollectCardData>(result);
                    G.CardModule.SetCollectCardList(data.card_collect);
                }
            });
        }

        public void UseCardPack(ItemData items, Action<List<CardData>> callback = null, int scene_id = 0) {
            List<ServerItem> serverItems = new List<ServerItem>();
            serverItems.Add(new ServerItem() {
                item_id = items.Id,
                item_num = items.Count
            });
            UseCardPack(serverItems, callback);
        }

        public void UseCardPack(List<ItemData> itemData, Action<List<CardData>> callback = null, int scene_id = 0) {
            List<ServerItem> serverItems = new List<ServerItem>();
            foreach (var item in itemData) {
                serverItems.Add(new ServerItem() {
                    item_id = item.Id,
                    item_num = item.Count
                });
            }
            UseCardPack(serverItems, callback);
        }

        public void UseCardPack(List<ServerItem> serverItems, Action<List<CardData>> callback = null, int scene_id = 0) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("scene_id", scene_id);

            var itemJson = JsonMapper.ToJson(serverItems);
            infoDic.Add("item", itemJson);
            SendRequest("/api/v1/card/consume_card_pack", infoDic, (string result, int code) =>
            {
                List<CardData> cardDatas = new List<CardData>();
                if (code == 0) {
                    Logger.Debug($"UseCardPack result:{result},code:{code}");
                    foreach (var item in serverItems) {
                        G.GameItemModule.AddItemCount(item.item_id, -item.item_num);
                    }
                    var data = JsonMapper.ToObject<UsePackData>(result);
                    if (serverItems.Count == 1 && serverItems[0].item_num == 1) {
                        for (int i = 0; i < data.rewards.Count; i++) {
                            var cardData = data.rewards[i];
                            if (cardData.card_num == 1) {
                                cardDatas.Add(cardData);
                            } else {
                                // 卡牌数量大于1
                                for (int j = 0; j < cardData.card_num; j++) {
                                    CardData newCardData = new CardData();
                                    newCardData.card_id = cardData.card_id;
                                    newCardData.card_num = 1;
                                    newCardData.card_star = cardData.card_star / cardData.card_num;
                                    cardDatas.Add(newCardData);
                                }
                            }
                        }   

                        var random = new System.Random();
                        cardDatas = cardDatas.OrderBy(n => random.Next()).ToList();
                    } else {
                        cardDatas = data.rewards;
                    }
                } else {
                    Logger.Debug("UseCardPack fail");
                }
                callback?.Invoke(cardDatas);

                foreach (var card in cardDatas) {
                    G.CardModule.AddCardData(card.card_id, card.card_num);
                }
                G.CardModule.UpdateAllRewardState();
            });
        }

        public void GetPackReward(int reward_id, int scene_id = 0, Action<bool> callback = null) {
            GetCardReward(reward_id, "/api/v1/activity_reward/finish/collect_card", (success) => {
                if (success) {
                    G.CardModule.SetPackRewardState(reward_id, 2);
                    G.EventModule.DispatchEvent(GameEventDefine.OnCardPackState);

                    G.RedDotModule.AddRedDotCount(RedDotDefine.CardPackReward, -1);
                }
                callback?.Invoke(success);
            }, scene_id);
        }

        public void GetCardThemeReward(int reward_id, int scene_id = 0, Action<bool> callback = null) {
            GetCardReward(reward_id, "/api/v1/activity_reward/finish/collect_card_total", (success) => {
                if (success) {
                    G.CardModule.SetTotalRewardState(2);
                    G.EventModule.DispatchEvent(GameEventDefine.OnCardTotalState);

                    G.RedDotModule.AddRedDotCount(RedDotDefine.CardTotalReward, -1);
                }
                callback?.Invoke(success);
            }, scene_id, boxType:RewardBoxType.Red);
        }

        public void GetCardStarReward(int reward_id, int scene_id = 0) {
            GetCardReward(reward_id, "/api/v1/activity_reward/finish/collect_card_star", (success) => {
                if (success) {
                    G.CardModule.SetStarRewardState(reward_id, 2);
                    G.EventModule.DispatchEvent(GameEventDefine.OnCardStarState);

                    G.RedDotModule.AddRedDotCount(RedDotDefine.CardStar, -1);
                }
            }, scene_id);
        }

        public void GetCardReward(int reward_id, string postStr, Action<bool> callback = null, int scene_id = 0, RewardBoxType boxType = RewardBoxType.None) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("reward_id", reward_id);
            infoDic.Add("scene_id", scene_id);
            SendRequest(postStr, infoDic, (string result, int code) =>
            {
                if (code == 0) {
                    Logger.Debug($"GetPackReward result:{result},code:{code}");
                    var data = JsonMapper.ToObject<CardRewardData>(result);

                    List<ItemData> itemDatas = ExchangeAddItemDatas(data.rewards);
                    G.GameItemModule.AddItemCount(itemDatas);
                    CommonUtil.ShowRewardWindow(itemDatas, LocalizationPool.Get("Reward/Tips/CardPack"), boxType: boxType);
                } else {
                    Logger.Debug("GetPackReward fail");
                }

                callback.Invoke(code == 0);
            });
        }

        public void ShareCard(int card_id, Action<string> callback = null, int scene_id = 0) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("card_id", card_id);
            infoDic.Add("scene_id", scene_id);

            string share_code = "";
            SendRequest("/api/v1/card/share", infoDic, (string result, int code) =>
            {
                if (code == 0) {
                    Logger.Debug($"ShareCard result:{result},code:{code}");
                    var data = JsonMapper.ToObject<ShareCardData>(result);
                    share_code = data.share_code;
                } else {
                    Logger.Debug("ShareCard fail");
                }

                callback?.Invoke(share_code);
            });
        }

        public void ReceiveShareCard(string share_code, Action<bool> callback = null, int scene_id = 0) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("code", share_code);
            infoDic.Add("scene_id", scene_id);
            SendRequest("/api/v1/card/receive", infoDic, (string result, int code) =>
            {
                if (code == 0) {
                    Logger.Debug($"GetShareCard result:{result},code:{code}");
                    var data = JsonMapper.ToObject<ReceiveCardData>(result);
                    G.CardModule.AddCardData(data.reward.card_id, data.reward.card_num);
                    G.CardModule.UpdateAllRewardState();

                } else {
                    Logger.Debug("GetShareCard fail");
                }

                callback?.Invoke(code == 0);
            });
        }
        #endregion

        #region 签到

        public void ReqSignInfo(Action<string, int> callback)
        {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/activity_reward/query/daily_reward", infoDic, (string result, int code) =>
            {
                Logger.Debug($"ReqSignInfo result:{result},code:{code}");
                callback?.Invoke(result, code);
            });
        }

        public void ReqSignReward(int doubleTag,int rewardId, Action<List<ItemData>> callback,int sceneId = 0)
        {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("scene_id", sceneId);
            infoDic.Add("is_double", doubleTag);
            infoDic.Add("reward_id", rewardId);
            
            SendRequest("/api/v1/activity_reward/finish/daily_reward", infoDic, (string result, int code) =>
            {
                List<ItemData> itemDatas = null;
                if (code == 0)
                {
                    SignGetRewardData rewardData = JsonMapper.ToObject<SignGetRewardData>(result);
                    itemDatas = ExchangeAddItemDatas(rewardData.rewards);
                    G.GameItemModule.AddItemCount(itemDatas);

                    G.RedDotModule.SetRedDotCount(RedDotDefine.SignDaily, 0);
                }
                
                callback?.Invoke(itemDatas);
            });
        }

        public void ReqSignAccumulativeReward(int rewardId, Action<List<ItemData>> callback, int sceneId = 0)
        {
            Dictionary<string,object> infoDic = new Dictionary<string, object>();
            infoDic.Add("reward_id", rewardId);
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("scene_id", sceneId);
            infoDic.Add("is_double", 0);
            SendRequest("/api/v1/activity_reward/finish/daily_total_reward", infoDic, (string result, int code) =>
            {
                List<ItemData> itemDatas = null;
                if (code == 0)
                {
                    SignGetRewardData rewardData = JsonMapper.ToObject<SignGetRewardData>(result);
                    itemDatas = ExchangeAddItemDatas(rewardData.rewards);
                    G.GameItemModule.AddItemCount(itemDatas);

                    G.RedDotModule.AddRedDotCount(RedDotDefine.SignDaily, -1);
                }
                callback?.Invoke(itemDatas);
            });
        }


        #endregion

        public async UniTask ReqChangedServerTime(DateTime timeSpan, Action<bool> callback = null)
        {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("time", timeSpan.ToString("yyyy-MM-dd HH:mm:ss"));
            string param = "";
            var keys = new List<string>(infoDic.Keys);
            foreach (var key in keys)
            {
                param += key;
                param += '=';
                param += infoDic[key];
                param += '&';
            }
            param = param.Remove(param.Length - 1);
            string url = $"{ADDRESS}/api/v1/test/update_time?{param}";
            await HttpUtil.Post(url, "");
            callback?.Invoke(true);
        }
        
        public void UpdateUserInfo<T>(string key, T val, Action<bool> callback = null, int scene_id = 0) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add(key, val);
            infoDic.Add("scene_id", scene_id);
            SendRequest("/api/v1/user/update_user_info", infoDic, (string result, int code) =>
            {
                Logger.Debug($"UpdateUserInfo result:{result},code:{code}");
                callback?.Invoke(code == 0);
            });
        }

        private void QueryStageData() {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/stage/stage_info", infoDic, (string result, int code) =>
            {
                Logger.Debug($"QueryStageData result:{result},code:{code}");
                if (code == 0) {
                    var data = JsonMapper.ToObject<ServerStageData>(result);


                    MatchManager.Instance.SetWinStreak(data.stage.win_streak);
                    MatchManager.Instance.SetMatchLevelID(data.stage.stage_id);
                    MatchManager.Instance.SetMaxWinStreak(data.stage.win_streak_rank_score);
                    MatchManager.Instance.InitBeginTipsData();

                    LevelManager.Instance.ResetLevelDifficultyValue(data.stage.setting.stage_val_detail.behavior_val, data.stage.setting.stage_val_detail.group_val);
                    LevelManager.Instance.SetCurrentStagePlayCount(data.stage.setting.stage_play_cnt);
                    LevelManager.Instance.SetIsCoinLevelState(data.stage.is_coin_stage);

                    // 获取历史关卡数据
                    MatchManager.Instance.ReqLevelState().Forget();
                }
            });
        }

        public void GetLevelData(int star_stage, int end_stage) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("star_stage", star_stage);
            infoDic.Add("end_stage", end_stage);
            Logger.Info($"Req Get Level data");
            SendRequest("/api/v1/stage/list", infoDic, (string result, int code) =>
            {
                Logger.Info("GetLevelData finish");
                Logger.Debug($"GetLevelData result:{result},code:{code}");
                if (code == 0) {
                    var data = JsonMapper.ToObject<ServerLiveData>(result);

                    Logger.Info("GetLevelData finish1");
                    var levelItemList = new List<LevelItem>();
                    for(int i = star_stage; i < end_stage; i++) {
                        LevelItem levelItem;
                        levelItem = data.list.Find(x => x.stage_id == i);
                        if (levelItem == null) {
                            levelItem = new LevelItem();
                            levelItem.stage_id = i;
                            levelItem.star_num = 1;
                        }

                        levelItemList.Add(levelItem);
                    }
                    MatchManager.Instance.SetLevelState(levelItemList);

                    G.EventModule.DispatchEvent(GameEventDefine.OnMatchGetLevelDetailFinish);
                }
            });
        }

        #region 月卡
        public void ReqMonthCardInfo()
        {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/activity_reward/query/month_card_reward", infoDic, (string result, int code) =>
            {
                Logger.Debug($"ReqMonthCardInfo result:{result},code:{code}");
                if (code == 0) {
                    var data = JsonMapper.ToObject<ServerMonthCardData[]>(result);
                    G.MonthCardModule.UpdateMonthCardList(data.ToList());
                }
            });
        }
        public void GetMonthCardReward(int reward_id, int scene_id = 0) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("reward_id", reward_id);
            infoDic.Add("scene_id", scene_id);
            SendRequest("/api/v1/activity_reward/finish/month_card_reward", infoDic, (string result, int code) =>
            {
                Logger.Debug($"GetMonthCardReward result:{result},code:{code}");
                if (code == 0) {
                    var rewardData = JsonMapper.ToObject<ServerMonthCardReward>(result);
                    List<ItemData> itemDatas = ExchangeAddItemDatas(rewardData.rewards);
                    G.GameItemModule.AddItemCount(itemDatas);
                    CommonUtil.ShowRewardWindow(itemDatas, LocalizationPool.Get("Reward/Tips/MonthCard"));
                    G.MonthCardModule.UpdateMonthCardState(reward_id, 2);
                }
            });
        }
        #endregion


        public void QueryTrainMaster() {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/activity_reward/query/master_trial", infoDic, (string result, int code) =>
            {
                Logger.Debug($"QueryTrainMaster result:{result},code:{code}");
                if (code == 0) {
                    var trainMasterData = JsonMapper.ToObject<ServerTrainMaster>(result);
                    G.TrainMasterModule.SetTrainMasterInfo(trainMasterData.master_trial);
                }
            });
        }

        public void BeginTranMaster(int coin_reward, Action<bool> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("coin_reward", coin_reward);
            SendRequest("/api/v1/activity_reward/start/master_trial", infoDic, (string result, int code) =>
            {
                Logger.Debug($"BeginTranMaster result:{result},code:{code}");
                if (code == 0) {
                    var trainMasterData = JsonMapper.ToObject<ServerTrainMaster>(result);
                    G.TrainMasterModule.SetTrainMasterInfo(trainMasterData.master_trial);
                }

                callback?.Invoke(code == 0);
            });
        }

        public void GetTrainMasterReward(Action<bool> callback = null , int scene_id = 0) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("scene_id", scene_id);
            SendRequest("/api/v1/activity_reward/finish/master_trial", infoDic, (string result, int code) =>
            {
                Logger.Debug($"GetTrainMasterReward result:{result},code:{code}");
                if (code == 0) {
                    G.TrainMasterModule.FinishTrainning();
                }

                callback?.Invoke(code == 0);
            });
        }

        private void ReportHeartBeat() {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/user/heartbeat", infoDic, (string result, int code) =>
            {
            });
        }

        public void ReportAdvState(int scene_id, string step, int stage_id = 0) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("scene_id", scene_id);
            infoDic.Add("step", step);
            infoDic.Add("stage_id", stage_id);
            SendRequest("/api/v1/log/ad", infoDic, (string result, int code) =>
            {

            });
        }


        #region 邮件
        public void GetEmailList() {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("limit", 200);
            SendRequest("/api/v1/email/list", infoDic, (string result, int code) =>
            {
                Logger.Debug($"GetMailList result:{result},code:{code}");
                if (code == 0) {
                    var emailData = JsonMapper.ToObject<ServerEmailData>(result);
                    G.EmailModule.SetEmailList(emailData.list);
                }
            });
        }

        public void ReadEmail(int email_id, Action<bool> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("email_id", email_id);
            SendRequest("/api/v1/email/read", infoDic, (string result, int code) =>
            {
                Logger.Debug($"ReadEmail result:{result},code:{code}");
                if (code == 0) {
                    G.EmailModule.UpdateEmailIsRead(email_id);
                }
                callback?.Invoke(code == 0);
            });
        }

        public void GetEmailReward(string email_id, Action<bool> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("email_id", email_id);
            SendRequest("/api/v1/email/take", infoDic, (string result, int code) =>
            {
                Logger.Debug($"GetEmailReward result:{result},code:{code}");
                if (code == 0) {
                    var rewardData = JsonMapper.ToObject<ServerEmailReward>(result);
                    var rewardList = new List<ServerItem>();
                    foreach (var item in rewardData.rewards) {
                        rewardList.Add(item.Value);
                    }

                    List<ItemData> itemDatas = ExchangeAddItemDatas(rewardList);
                    G.GameItemModule.AddItemCount(itemDatas);
                    CommonUtil.ShowRewardWindow(itemDatas, LocalizationPool.Get("Reward/Tips/Email"));

                    var strArr = email_id.Split('|');
                    for (int i = 0; i < strArr.Length; i++) {
                        if (int.TryParse(strArr[i], out int id)) {
                            G.EmailModule.UpdateEmailRewardState(id);
                            G.EventModule.DispatchEvent(GameEventDefine.OnEamilGetReward, EventOneParam<int>.Create(id));
                        }
                    }
                }
                callback?.Invoke(code == 0);
            });
        }

        public void DeleteEmail(string email_id, Action<bool> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("email_id", email_id);
            SendRequest("/api/v1/email/delete", infoDic, (string result, int code) =>
            {
                Logger.Debug($"DeleteEmail result:{result},code:{code}");
                callback.Invoke(code == 0);
            });
        }
        #endregion

        #region 任务

        public void ReqTaskList(Action<ServerTaskData> callback)
        {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("scene_id", 0);
            SendRequest("/api/v1/activity_reward/query/task_reward", infoDic, (string result, int code) =>
            {
                Logger.Debug(result);
                if(code != 0)
                {
                    callback?.Invoke(null);
                    return;
                }
                var taskData = GameCore.LitJson.JsonMapper.ToObject<ServerTaskData>(result);
                callback?.Invoke(taskData);
            });
        }

        public void ReqTaskActiveList(Action<ServerActiveTaskData> callback)
        {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("scene_id", 0);
            SendRequest("/api/v1/activity_reward/query/engagement_reward", infoDic, (string result, int code) =>
            {
                Logger.Debug(result);
                if(code != 0)
                {
                    callback?.Invoke(null);
                    return;
                }

                var taskData = GameCore.LitJson.JsonMapper.ToObject<ServerActiveTaskData>(result);
                callback?.Invoke(taskData);
            });
        }

        public void ReqGetTaskReward(int taskId, Action<bool> callback = null)
        {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("reward_id", taskId.ToString());
            infoDic.Add("scene_id", 0);
            SendRequest("/api/v1/activity_reward/finish/task_reward", infoDic, (string result, int code) =>
            {
                Logger.Debug(result);
                if(code != 0)
                {
                    callback?.Invoke(false);
                    return;
                }

                var jsonData = JsonMapper.ToObject(result);
                var rewards = JsonMapper.ToObject<List<ServerItem>>(jsonData["rewards"].ToJson());
                var itemDatas = ExchangeAddItemDatas(rewards);
                G.GameItemModule.AddItemCount(itemDatas);
                CommonUtil.ShowRewardWindow(itemDatas);
                callback?.Invoke(true);
            });
        }

        public void ReqGetTaskActiveReward(int rewardId, Action<bool> callback = null)
        {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("reward_id", rewardId);
            infoDic.Add("scene_id", 0);
            
            SendRequest("/api/v1/activity_reward/finish/engagement_reward", infoDic, (string result, int code) =>
            {
                Logger.Debug(result);
                if(code != 0)
                {
                    callback?.Invoke(false);
                    return;
                }

                var jsonData = JsonMapper.ToObject(result);
                var rewards = JsonMapper.ToObject<List<ServerItem>>(jsonData["rewards"].ToJson());
                var itemDatas = ExchangeAddItemDatas(rewards);
                G.GameItemModule.AddItemCount(itemDatas);
                CommonUtil.ShowRewardWindow(itemDatas);
                callback?.Invoke(true);
            });
        }
        
        #endregion

        public void QueryInviteReward(Action<ServerInviteData> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/activity_reward/query/invite_reward", infoDic, (string result, int code) =>
            {
                Logger.Debug($"QueryInviteReward result:{result},code:{code}");
                ServerInviteData data = null;
                if (code == 0) {
                    data = JsonMapper.ToObject<ServerInviteData>(result);
                }
                callback?.Invoke(data);
            });
        }

        public void GetnviteReward(string reward_id, Action<bool> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("reward_id", reward_id);
            infoDic.Add("scene_id", 0);
            SendRequest("/api/v1/activity_reward/finish/invite_reward", infoDic, (string result, int code) =>
            {
                Logger.Debug($"GetnviteReward result:{result},code:{code}");
                callback?.Invoke(code == 0);
            });
        }

        public void PlayerLogout() {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/user/logout", infoDic, (string result, int code) =>
            {
                Logger.Debug($"PlayerLogout result:{result},code:{code}");
            });
        }

        public void QueryTreasure() {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/activity_reward/query/treasure_reward", infoDic, (string result, int code) =>
            {
                Logger.Debug($"QueryTreasure result:{result},code:{code}");
                if (code == 0) {
                    var data = JsonMapper.ToObject<ServerTreasureData>(result);
                }
            });
        }

        public void GetTreasureReward(int reward_id, Action<bool> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("reward_id", reward_id);
            infoDic.Add("scene_id", 0);
            SendRequest("/api/v1/activity_reward/finish/treasure_reward", infoDic, (string result, int code) =>
            {
                Logger.Debug($"GetTreasureReward result:{result},code:{code}");
                callback?.Invoke(code == 0);
            });
        }

        private void UpdateItemData() {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/item/list", infoDic, (string result, int code) =>
            {
                var data = JsonMapper.ToObject<ServerItemDatas>(result);
                // 道具数据
                var itemDatas = ExchangeSetItemDatas(data.items);
                G.GameItemModule.SetItemCount(itemDatas);
            });
        }


        public void ReportFeedBack(int score, string type, string message, Action<bool> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("score", score);
            infoDic.Add("type", type);
            infoDic.Add("message", message);
            SendRequest("/api/v1/feedback", infoDic, (string result, int code) =>
            {
                Logger.Debug($"ReportFeedBack result:{result},code:{code}");
                callback?.Invoke(code == 0);
            });
        }

        public void QueryQuestion() {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/question/query", infoDic, (string result, int code) =>
            {
                Logger.Debug($"QueryQuestion result:{result},code:{code}");
                if (code == 0) {
                    var data = JsonMapper.ToObject<ServerQuestData>(result);


                    List<int> questIds = new List<int>();
                    foreach (var item in data.question) {
                        questIds.Add(item.id);
                    }

                    List<ItemData> itemDatas = ExchangeAddItemDatas(data.rewards);

                }
            });
        }

        public void ReportQuestion(string answer, int use_time, Action<bool> callback = null) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("answer", answer);
            infoDic.Add("use_time", use_time);
            SendRequest("/api/v1/question/answer", infoDic, (string result, int code) =>
            {
                Logger.Debug($"ReportQuestion result:{result},code:{code}");
                if (code == 0) {
                    var data = JsonMapper.ToObject<ServerItemDatas>(result);

                    // 道具数据
                    var itemDatas = ExchangeAddItemDatas(data.items);
                    if (itemDatas.Count > 0) {
                        G.GameItemModule.AddItemCount(itemDatas);
                        CommonUtil.ShowRewardWindow(itemDatas, LocalizationPool.Get("Reward/Tips/QuestReward"));
                    }
                }

                callback?.Invoke(code == 0);
            });
        }

        public void ReportQuestLog(string step, int use_time, int exit_question_id) {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            infoDic.Add("step", step);
            infoDic.Add("use_time", use_time);
            infoDic.Add("exit_question_id", exit_question_id);
            SendRequest("/api/v1/log/question", infoDic, (string result, int code) =>
            {
                Logger.Debug($"ReportQuestLog result:{result},code:{code}");
            });
        }

        public void QueryABConfig() {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/user/get_user_ab_config", infoDic, (string result, int code) =>
            {
                Logger.Debug($"QueryABConfig result:{result},code:{code}");
                if (code == 0) {
                    var data = JsonMapper.ToObject<ServerABConfigData>(result);
                    G.SwitchModule.InitABConfig(data.ab_config);
                }
                MatchManager.Instance.SetLevelType(G.SwitchModule.GetMatchABLevelType());
            });
        }

        public void QuerySwitchData() {
            Dictionary<string, object> infoDic = new Dictionary<string, object>();
            infoDic.Add("user_id", G.UserInfoModule.UserId);
            SendRequest("/api/v1/config/func_switch", infoDic, (string result, int code) =>
            {
                Logger.Debug($"QuerySwitchData result:{result},code:{code}");
                if (code == 0) {
                    var data = JsonMapper.ToObject<ServerSwitchData>(result);
                    G.SwitchModule.InitSwitchData(data.func_switch);
                }
            });
        }
    }
}
