using Cysharp.Threading.Tasks;
using GameCore.Localization;
using GameCore.Logic;
using GameCore.Settings;
using GameCore.Utils;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace GameCore.Process
{
    /// <summary>
    /// 正式启动游戏流程
    /// </summary>
    public class ProcessLaunch : IProcess
    {
        public void Init()
        {
        }

        public void Enter()
        {
            Logger.Info("Launch Game!!!");
            //读取包体配置
            EnterGame().Forget();
        }

        public void Leave()
        {
        }

        public void Update()
        {
        }

        private async UniTask EnterGame()
        {
            if (GameSettings.Instance.ProjectSetting.AppMode == EAppMode.Debug &&
                Application.platform == RuntimePlatform.WindowsPlayer)
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            //流程正式启动 设置音量相关
            InitSoundSetting();
            //构建包体内的多语言
            await LocalizationPool.LanguageFactory(async addEntry =>
            {
                string configPath = $"Config/NativeLocalization_{GameSettings.Instance.ProjectSetting.Language}";
                var request = Resources.LoadAsync<NativeLocalization>(configPath);
                await request.ToUniTask(timing: PlayerLoopTiming.Update);
                if (request.asset == null)
                    Logger.Fatal($"Failed to load {configPath}");
                var config = request.asset as NativeLocalization;
                if (config != null)
                {
                    for (int i = 0; i < config.languageEntries.Length; i++)
                    {
                        addEntry(config.languageEntries[i].Key, config.languageEntries[i].Value);
                    }
                }
            }, true);
            string resVersion = PlayerPrefsUtil.GetString("GAME_RES_VERSION", "0");
            HotUpdateManager.Instance.PackageVersion = resVersion;
            HotUpdateManager.Instance.UpdateLoadingVersion();
            HotUpdateManager.Instance.UpdateProgress(EUpdateState.Launch);
            //直接进入SDK初始化流程
            ProcessManager.Instance.Enter<ProcessSDKInit>();
        }

        private void InitSoundSetting()
        {
        }

    }
}