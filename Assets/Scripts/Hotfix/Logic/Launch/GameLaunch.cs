using Cysharp.Threading.Tasks;
using GameCore.LitJson;
using GameCore.Localization;
using GameCore.SDK;
using GameCore.Settings;
using GameCore.Singleton;
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.Utils;
using HotfixCore.Module;
using HotfixLogic.Match;
using IngameDebugConsole;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic
{
    public partial class GameLaunch : LazySingleton<GameLaunch>
    {
        public async UniTask Start()
        {
            await Preload();
            if (GameSettings.Instance.IsShowDebugConsole())
                DebugLogManager.Instance.BindLogCopyAction(OnLogCopyClick);
        }

        private void OnLogCopyClick(string log)
        {
            CommonUtil.SetClipboardText(log);
        }

        /// <summary>
        /// 预加载资源
        /// </summary>
        private async UniTask Preload()
        {
            Logger.Info("GameLaunch Preload Start");

            //多语言文本准备
            await PreLocalization();
            await LevelManager.Instance.PrepareLevelDifficulty();

            Logger.Info("Localization and level done");
            G.AudioModule.Initialize(GameSettings.Instance.AudioSetting.audioGroupConfigs);
            G.AudioModule.MusicVolume = PlayerPrefsUtil.GetInt("Music", 1);
            G.AudioModule.SoundVolume = PlayerPrefsUtil.GetInt("Effect", 1);

            Logger.Info("GameLaunch Preload 进入主场景");
            await HotfixCore.MVC.MVCManager.Instance.ActiveModule(MVCEnum.Main.ToString());

            if (CommonUtil.IsWechatMiniGame()) {
                JYSDKLogin();
                // SDKMgr.Instance.CallSDKMethod("openDebug","", "", null);
            } else {
                Logger.Debug("GameLaunch Preload 登录");
                HttpLogin();
            }
        }

        private async UniTask PreLocalization()
        {
            string language = PlayerPrefsUtil.GetString(GamePrefsKey.LANGUAGELOCALKEY, string.Empty);
            if (string.IsNullOrEmpty(language))
                language = GameSettings.Instance.ProjectSetting.Language;
            string rawFile = $"Localization/RawLocalization_{language}".ToLower();
            string customFile = $"Localization/CustomLocalization_{language}".ToLower();
            await LocalizationPool.ReadI18N(new string[] { rawFile, customFile },
                GameSettings.Instance.ProjectSetting.DefaultPackageName);
        }

        private void JYSDKLogin() {
            Logger.Info("GameLaunch Preload JYSDK登录");

            SDKMgr.Instance.CallSDKMethod("login","", "login", returnData =>
            {
                var code = returnData.Code;
                if (code.CallbackCode == 2000)
                {
                    var json = JsonMapper.ToObject((string)code.CallBackData);
                    string open_id = (string)json["open_id"];
                    string pf_openid = (string)json["pf_openid"];
                    string access_token = (string)json["access_token"];
                    string avatar = (string)json["avatar"];
                    string nickname = (string)json["nickname"];

                    G.HttpModule.UpdateData(open_id, pf_openid, access_token);
                    Logger.Info("SDK登录成功，转Http登录");
                    HttpLogin();
                    
                    // SDK登录成功，预加载广告
                    SDKMgr.Instance.CallSDKMethod("preAd", GiftIDDefine.AdvUnitId, "", c => {});
                        
                    // 没有用户登录信息是，授权获取用户信息
                    if(nickname == "" && avatar == "") {
                        // Logger.Debug("SDK登录成功，没有用户登录信息，开始授权获取用户信息");
                        // JYSDKGetUserInfo();
                    } else {
                        G.UserInfoModule.SetChannelAvatar(avatar);
                        G.UserInfoModule.SetChannelNickName(nickname);
                    }
                }
                else
                {
                    Logger.Debug("SDK登录失败");
                }
            });
        }

        private void HttpLogin() {
            G.HttpModule.PlayerLogin();
        }
    }
}