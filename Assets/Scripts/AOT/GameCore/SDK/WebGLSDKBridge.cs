#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using GameCore.LitJson;
using GameCore.Logic;
using GameCore.Settings;
using JingYouSdk;
using UnityEngine;
using WeChatWASM;
using Logger = GameCore.Log.Logger;

namespace GameCore.SDK
{
    [System.Serializable]
    public class OpenDataMessage
    {
        public string type;
        public int score;
    }

    public class WebGLSDKBridge : MonoSDKBridge
    {
        const string app_id = "1756792903";
        const string app_key = "G1MQto3IzikwBGex1HwMHp6nMMbXnFc4";
        const string channel_id = "1003";

        private bool _isInit = false;

        WXOpenDataContext _openDataContext = null;

        GCenterEnv _centerEnv = GCenterEnv.PROD;

        private Dictionary<string, Action<SDKReturn>> _callbackMap =
            new Dictionary<string, Action<SDKReturn>>(20);

        void Awake()
        {
            Logger.Debug("WebGLSDKBridge Awake");

            GlobalListener globalListener = new GlobalListener();
            globalListener.OnResult = (code, data, error) =>
            {
                Logger.Info(string.Format("接收回调>>>code:{0}, data:{1}, error:{2}", code, data, error.ToString()));
                string callbackMethod = "";
                SDKCallbackCode callbackCode = default;
                switch (code)
                {
                    case GlobalCode.CODE_OF_INIT_SUCCESS:
                        callbackMethod = "init";
                        _isInit = true;
                        break;
                    case GlobalCode.CODE_OF_INIT_FAILURE:
                        callbackMethod = "init";
                        break;
                    case GlobalCode.CODE_OF_LOGIN_SUCCESS:
                        data = JsonMapper.ToJson((JingYouSdk.UserInfo)data);
                        callbackMethod = "login";
                        break;
                    case GlobalCode.CODE_OF_LOGIN_FAILURE:
                        callbackMethod = "login";
                        break;
                    case GlobalCode.CODE_OF_LOGOUT_SUCCESS:
                        callbackMethod = "logout";
                        break;
                    case GlobalCode.CODE_OF_LOGOUT_FAILURE:
                        callbackMethod = "logout";
                        break;
                    case GlobalCode.CODE_OF_PAY_SUCCESS:
                        data = JsonMapper.ToJson((PayResult)data);
                        callbackMethod = "pay";
                        break;
                    case GlobalCode.CODE_OF_PAY_FAILURE:
                        data = JsonMapper.ToJson((PayResult)data);
                        callbackMethod = "pay";
                        break;
                    case GlobalCode.CODE_OF_PAY_CANCEL:
                        data = JsonMapper.ToJson((PayResult)data);
                        callbackMethod = "pay";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(code), code, null);
                };

                callbackCode = new SDKCallbackCode
                {
                    CallBackData = data,
                    CallbackCode = (int)code,
                    ErrMsg = error.GetErrMsg(),
                    ErrCode = error.GetErrCode()
                };

                if (!string.IsNullOrEmpty(callbackMethod))
                {
                    OnSDKCallback(callbackMethod, callbackCode);
                }
            };

            // 设置全局监听
            JYSDK.Instance.SetGlobalListener(globalListener);
        }

        public void OnSDKCallback(string callMethod, SDKCallbackCode callbackCode) {
            OnSDKCallback(callMethod, new SDKReturn("", callbackCode, ""));
        }

        public override void OnSDKCallback(string callMethod, SDKReturn sdkReturn)
        {
            Logger.Info($"OnSDKCallback callMethod = {callMethod} sdkReturn = {sdkReturn}");
            if (_callbackMap.TryGetValue(callMethod, out var callback))
            {
                sdkReturn.Code.SDKType = "JYSDK";
                callback.Invoke(sdkReturn);
                _callbackMap.Remove(callMethod);
            }
        }

        public override void CallSDKNative(string callMethod, string jsonParam, string callbackName, Action<SDKReturn> callbackAction, object objData)
        {
            // if(_callbackMap.ContainsKey(callMethod))
            //     return;
            if (callbackAction != null) {
                _callbackMap.TryAdd(callMethod, callbackAction);
            }
            switch (callMethod)
            {
                case "init":
                    Logger.Debug("CallSDKNative init  _isInit = " + _isInit);
                    if (_isInit)
                    {
                        callbackAction?.Invoke(null);
                        _callbackMap.Remove(callMethod);
                        return;
                    }
                    // 初始化
                    InitConfig config = new InitConfig();
                    config.client_ver = Application.version; // 游戏客户端版本
                    config.app_id = app_id; // 京游平台AppId
                    config.app_key = app_key; // 京游平台AppKey
                    config.channel_id = channel_id; // 京游平台ChannelId
                    Logger.Debug("CallSDKNative 初始化SDK");
                    JYSDK.Instance.Init(config);
                    break;
                case "login":
                    JYSDK.Instance.Login();
                    break;
                case "shareApp":
                    ShareInfo shareInfo = JsonMapper.ToObject<ShareInfo>(jsonParam);
                    ShareAppMessage(shareInfo);
                    break;
                case "onShareApp":
                    ShareInfo onShareInfo = JsonMapper.ToObject<ShareInfo>(jsonParam);
                    OnShareAppMessage(onShareInfo);
                    break;
                case "preAd":
                    JYSDK.Instance.PreLoadRewardVideo(jsonParam);
                    break;
                case "showAd":
                    string[] args = jsonParam.Split(';');
                    ShowRewardVideo(args[0], args[1]);
                    break;
                case "pay":
                    var jsonObj = JsonMapper.ToObject(jsonParam);
                    PayInfo payInfo = new PayInfo()
                    {
                        price=(double)jsonObj["price"],
                        order_no=(string)jsonObj["order_no"],
                        product_id=(string)jsonObj["product_id"],
                        product_name=(string)jsonObj["product_name"],
                        extra=(string)jsonObj["extra"],
                        notify_url=(string)jsonObj["notify_url"]
                    };
                    Logger.Debug($"PayInfo:{payInfo}");
                    Pay(payInfo);
                    break;
                case "getUserInfo":
                    GetUserInfo(jsonParam);
                    break;
                case "updateFriendRank":
                    UpdateFriendRank(jsonParam, objData);
                    break;
                case "setUserRecord":
                    SetUserRecord(jsonParam);
                    break;
                case "hideFriendsRank":
                    HideOpenDataContext();
                    break;
                case "updateInviteFriend":
                    UpdateInviteFriend(jsonParam, objData);
                    break;
                case "hideOpenDataContext":
                    HideOpenDataContext();
                    break;
                case "loadingBeginLog":
                    LoadingBeginLog();
                    break;
                case "loadingEndLog":
                    LoadingEndLog();
                    break;
                case "enterGameLog":
                    EnterGameLog(jsonParam);
                    break;
                case "setClipboard":
                    SetClipboard(jsonParam);
                    break;
                case "getLaunchParams":
                    GetLaunchParams();
                    break;
                case "getEnterParams":
                    GetEnterParams();
                    break;
                case "logEvent":
                    SendLogEvent(jsonParam);
                    break;
                case "getNotice":
                    GetNotice();
                    break;
                case "openDebug":
                    OpenDebug();
                    break;
                case "setEvirnment":
                    SetEvirnment(jsonParam);
                    break;
                case "stageStartLog":
                    JYSDK.Instance.SendStageStartLog(jsonParam);
                    break;
                case "stageEndLog":
                    OnStageEndLog(jsonParam);
                    break;
                case "quickGame":
                    QuickGame();
                    break;
                case "playWXAudio":
                    PlayWXAudio(jsonParam);
                    break;
                case "preloadWXAudio":
                    PreloadWXAudio(jsonParam);
                    break;
            }
        }

        private void OnStageEndLog(string jsonParam)
        {
            string[] args = jsonParam.Split("-");
            JYSDK.Instance.SendStageEndLog(args[0], bool.Parse(args[1]));
        }
        
        private void ShareAppMessage(ShareInfo shareInfo)
        {
            ShareListener listener = new ShareListener();
            listener.OnSuccess = () =>
            {
                OnSDKCallback("shareApp",new SDKCallbackCode() {CallbackCode = 1});
            };
            listener.OnFailure = (error) =>
            {
                SDKCallbackCode callbackCode = new SDKCallbackCode()
                {
                    CallbackCode = -1,
                    ErrMsg = error.GetErrMsg(),
                    ErrCode = error.GetErrCode(),
                };
                OnSDKCallback("shareApp",callbackCode);
            };
            JYSDK.Instance.ShareAppMessage(shareInfo, listener);
        }

        private void OnShareAppMessage(ShareInfo shareInfo)
        {
            ShareListener listener = new ShareListener();
            listener.OnSuccess = () =>
            {
                OnSDKCallback("onShareApp",new SDKCallbackCode() {CallbackCode = 1});
            };
            listener.OnFailure = (error) =>
            {
                SDKCallbackCode callbackCode = new SDKCallbackCode()
                {
                    CallbackCode = 1,
                    ErrMsg = error.GetErrMsg(),
                    ErrCode = error.GetErrCode(),
                };
                OnSDKCallback("onShareApp",callbackCode);
            };
            JYSDK.Instance.OnShareAppMessage(shareInfo, listener);
        }

        private void ShowRewardVideo(string sceneId, string adUnitId)
        {
            RewardVideoListener listener = new RewardVideoListener();
            listener.OnReward = () =>
            {
                OnSDKCallback("showAd", new SDKCallbackCode(){CallbackCode = 1});
            };
            listener.OnClose = () =>
            {
                OnSDKCallback("showAd", new SDKCallbackCode(){CallbackCode = 0});
            };
            listener.OnError = (error) =>
            {
                var code = new SDKCallbackCode
                {
                    CallbackCode = -1,
                    ErrMsg = error.GetErrMsg(),
                    ErrCode = error.GetErrCode()
                    
                };
                OnSDKCallback("showAd", code);
            };
            JYSDK.Instance.ShowRewardVideo(sceneId, adUnitId, listener);
        }

        private void Pay(PayInfo payInfo)
        {
            Logger.Debug($"Pay info: {payInfo}");
            JYSDK.Instance.Pay(payInfo);
        }

        private void GetUserInfo(string touchArea)
        {
            UserInfoListener listener = new UserInfoListener();
            listener.OnSuccess = (data) =>
            {
                SDKCallbackCode code = new SDKCallbackCode();
                code.CallbackCode = 1;
                code.CallBackData = data;
                OnSDKCallback("getUserInfo", code);
            };
            listener.OnFailure = (error) =>
            {
                SDKCallbackCode code = new SDKCallbackCode();
                code.CallbackCode = -1;
                code.ErrMsg = error.GetErrMsg();
                code.ErrCode = error.GetErrCode();
                OnSDKCallback("getUserInfo", code);
            };
            listener.OnAuthorize = () =>
            {
                if (touchArea == "")
                {
                    return null;
                } else
                {
                    Dictionary<string, object> areaData = JsonMapper.ToObject<Dictionary<string, object>>(touchArea);
                    return areaData;
                }
            };
            JYSDK.Instance.GetUserInfo(listener);
        }

        private void UpdateFriendRank(string callData, object objData)
        {
            if (_openDataContext == null) {
                _openDataContext = WX.GetOpenDataContext(new OpenDataContextOption
                {
                    sharedCanvasMode = CanvasType.ScreenCanvas
                });
            }

            Texture texture = (Texture)objData;
            Dictionary<string, int> areaData = JsonMapper.ToObject<Dictionary<string, int>>(callData);
            
            Logger.Info("UpdateFriendRank areaData.x : " + areaData["x"] + " areaData.y : " + areaData["y"] + " areaData.width : " + areaData["width"] + " areaData.height : " + areaData["height"]);
            WX.ShowOpenData(texture, areaData["x"], areaData["y"], areaData["width"], areaData["height"]);

            OpenDataMessage msgData = new OpenDataMessage();
            msgData.type = "showFriendsRank";
            string msg = JsonUtility.ToJson(msgData);
            _openDataContext.PostMessage(msg);
        }

        private void UpdateInviteFriend(string callData, object objData)
        {
            if (_openDataContext == null) {
                _openDataContext = WX.GetOpenDataContext(new OpenDataContextOption
                {
                    sharedCanvasMode = CanvasType.ScreenCanvas
                });
            }

            Texture texture = (Texture)objData;
            Dictionary<string, string> areaData = JsonMapper.ToObject<Dictionary<string, string>>(callData);

            int x = int.Parse(areaData["x"]);
            int y = int.Parse(areaData["y"]);
            int width = int.Parse(areaData["width"]);
            int height = int.Parse(areaData["height"]);
            
            Logger.Debug("UpdateInviteFriend areaData.x : " + areaData["x"] + " areaData.y : " + areaData["y"] + " areaData.width : " + areaData["width"] + " areaData.height : " + areaData["height"]);
            WX.ShowOpenData(texture, x, y, width, height);

            Logger.Debug("UpdateInviteFriend areaData.inviteKey : " + areaData["inviteKey"]);

            WX.SetMessageToFriendQuery(new SetMessageToFriendQueryOption() {
                query = areaData["inviteKey"],
            });

            OpenDataMessage msgData = new OpenDataMessage();
            msgData.type = "inviteUnregisterFriends";

            string msg = JsonUtility.ToJson(msgData);
            _openDataContext.PostMessage(msg);
        }

        private void SetUserRecord(string callData)
        {
            if (_openDataContext == null) {
                _openDataContext = WX.GetOpenDataContext(new OpenDataContextOption
                {
                    sharedCanvasMode = CanvasType.ScreenCanvas
                });
            }

            Logger.Debug("SetUserRecord callData : " + callData);

            OpenDataMessage msgData = new OpenDataMessage();
            msgData.type = "setUserRecord";
            msgData.score = int.Parse(callData);

            string msg = JsonUtility.ToJson(msgData);
            _openDataContext.PostMessage(msg);
        }

        private void HideOpenDataContext()
        {
            WX.HideOpenData();
        }

        private void LoadingBeginLog()
        {
            Logger.Debug("LoadingBeginLog ");
            JYSDK.Instance.SendLoadingStartLog();
        }

        private void LoadingEndLog()
        {
            Logger.Debug("LoadingEndLog ");
            JYSDK.Instance.SendLoadingEndLog();
        }

        private void EnterGameLog(string callData)
        {
            RoleInfo roleInfo = JsonMapper.ToObject<RoleInfo>(callData);
            Logger.Debug("EnterGameLog roleInfo : " + roleInfo.role_id + " " + roleInfo.role_name);
            JYSDK.Instance.SendEnterGameLog(roleInfo);
        }

        private void SetClipboard(string str) {
            Logger.Debug("WX SetClipboard str : " + str);

            WX.SetClipboardData(new SetClipboardDataOption() {
                data = str,
                success = (res) => {
                    Logger.Debug("SetClipboard success");
                },
                fail = (res) => {
                    Logger.Debug("SetClipboard fail");
                }
            });
        }

        private void SendLogEvent(string jsonParam)
        {
            SDKEventParam param = JsonMapper.ToObject<SDKEventParam>(jsonParam);
            if (param != null)
            {
                GCenterCallInfo info = new GCenterCallInfo();
                info.gcevent = GCenterEvent.TRACK_LOG_REPORT;
                info.data = new Dictionary<string, object>()
                {
                    { "event", param.Key },
                    { "properties", param.Params }
                };
                info.listener = new GameCenterCallListener();
                info.listener.OnFailure = (fail) =>
                {
                    OnSDKCallback("logEvent", new SDKCallbackCode() { CallbackCode = -1 ,ErrMsg = fail.GetErrMsg()});
                };
                info.listener.OnSuccess = (success) =>
                {
                    OnSDKCallback("logEvent", new SDKCallbackCode() { CallbackCode = 1 });
                };
                info.env = GameSettings.Instance.ProjectSetting.AppMode == EAppMode.Debug ? GCenterEnv.DEV : GCenterEnv.PROD;
                Logger.Info($"Client Send Event Log {jsonParam} env {info.env}");
                
                JYSDK.Instance.GameCenterCall(info);
            }
        }

        private void GetLaunchParams() {

            var launchParams = WX.GetLaunchOptionsSync();

            var callbackCode = new SDKCallbackCode();
            callbackCode.CallbackCode = 1;

            string paramStr = "";
            if (launchParams.query != null && launchParams.query.Count > 0) {
                paramStr = JsonMapper.ToJson(launchParams.query);
            }


            var returnData = new SDKReturn("", callbackCode, paramStr);
            OnSDKCallback("getLaunchParams",returnData);
        }

        private void GetEnterParams() {

            var launchParams = WX.GetEnterOptionsSync();

            var callbackCode = new SDKCallbackCode();
            callbackCode.CallbackCode = 1;

            string paramStr = "";
            if (launchParams.query != null && launchParams.query.Count > 0) {
                paramStr = JsonMapper.ToJson(launchParams.query);
            }


            var returnData = new SDKReturn("", callbackCode, paramStr);
            OnSDKCallback("getEnterParams",returnData);
        }

        private void GetNotice() {
            GCenterCallInfo callInfo = new GCenterCallInfo();
            callInfo.gcevent = GCenterEvent.NOTICE_LIST;
            callInfo.listener = new GameCenterCallListener();
            callInfo.env = _centerEnv;
            Debug.Log("WebGLSDKBridge GetNotice");
            callInfo.listener.OnSuccess = (Dictionary<string, object> data) =>
            {
                // 获取游戏公告列表成功
                try
                {
                    List<Dictionary<string, object>> list = (List<Dictionary<string, object>>)data["list"];


                    var callbackCode = new SDKCallbackCode();
                    callbackCode.CallbackCode = 1;

                    var paramStr = JsonMapper.ToJson(list);
                    Logger.Debug("WebGLSDKBridge GetNotice paramStr : " + paramStr);

                    var returnData = new SDKReturn("", callbackCode, paramStr);
                    OnSDKCallback("getNotice",returnData);
                } catch (Exception e) { 
                    Logger.Error("GetNotice error : " + e.Message);
                    OnSDKCallback("getNotice",new SDKCallbackCode() {CallbackCode = -1});
                }
            };
            callInfo.listener.OnFailure = (GlobalError error) =>
            {
                // 获取游戏公告列表失败
                Logger.Error("获取游戏公告列表失败");
                OnSDKCallback("getNotice",new SDKCallbackCode() {CallbackCode = -1});
            };
            JYSDK.Instance.GameCenterCall(callInfo);
        }

        private void OpenDebug() {
            JYSDK.Instance.LogDebug(true);
        }

        private void SetEvirnment(string paramStr) {
            bool isDev = bool.Parse(paramStr);
            _centerEnv = isDev ? GCenterEnv.DEV : GCenterEnv.PROD;
        }

        private void QuickGame() {
            WX.ExitMiniProgram(new ExitMiniProgramOption() {
                success = (res) => {
                    Logger.Debug("QuickGame success");
                },
                fail = (res) => {
                    Logger.Debug("QuickGame fail");
                }
            });
        }

        private void PreloadWXAudio(string path) {
            Logger.Debug("PreloadWXAudio path = " + path);

            var audioContext = WX.CreateInnerAudioContext();
            audioContext.src = path;
        }

        private void PlayWXAudio(string path) {
            Logger.Debug("PlayWXAudio path = " + path);

            var audioContext = WX.CreateInnerAudioContext();
            audioContext.src = path;
            audioContext.Play();
        }
    }
}
#endif