using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using GameCore.LitJson;
using GameCore.Localization;
using GameCore.SDK;
using GameCore.Singleton;
using UnityEngine;
using UnityEngine.Events;
using Logger = GameCore.Log.Logger;

namespace GameCore.Logic
{
    public class UpdateStateDesc
    {
        /// <summary>
        /// 显示的多语言文本Key
        /// </summary>
        public string LocalKey;

        /// <summary>
        /// 期望进度
        /// </summary>
        public float ExpectProgress;
    }

    public class HotUpdateManager : MonoBehaviour
    {
        private static HotUpdateManager _instance;
        public static HotUpdateManager Instance => _instance;
        
        [SerializeField] private GameObject _hotUpdateLoadingBg;
        [SerializeField] private HotUpdateLoading _hotUpdateLoading;

        [SerializeField] private HotUpdateVersion _versionPanel;
        // [SerializeField] private HotUpdateConfirmUI _hotUpdateConfirmUI;

        private static Dictionary<EUpdateState, UpdateStateDesc> _mapDesc =
            new Dictionary<EUpdateState, UpdateStateDesc>
            {
                { EUpdateState.Launch, new UpdateStateDesc() { LocalKey = "Initializing", ExpectProgress = 1.0f } },
                {
                    EUpdateState.RetryState,
                    new UpdateStateDesc() { LocalKey = "HotUpdateRetryState", ExpectProgress = 0.0f }
                },
                {
                    EUpdateState.CheckRemoteVersion,
                    new UpdateStateDesc() { LocalKey = "CheckRemoteVersion", ExpectProgress = 0.65f }
                },
                {
                    EUpdateState.CheckRemoteVersionSuccess,
                    new UpdateStateDesc() { LocalKey = "CheckRemoteVersionSuccess", ExpectProgress = 0.85f }
                },
                {
                    EUpdateState.CheckRemoteVersionFail,
                    new UpdateStateDesc() { LocalKey = "CheckRemoteVersionFail", ExpectProgress = 0.85f }
                },
                {
                    EUpdateState.CheckAppUpdate,
                    new UpdateStateDesc() { LocalKey = "CheckAppUpdate", ExpectProgress = 1.0f }
                },
                {
                    EUpdateState.InitializePackage,
                    new UpdateStateDesc() { LocalKey = "InitializePackage", ExpectProgress = 0.65f }
                },
                {
                    EUpdateState.UpdateResVersion,
                    new UpdateStateDesc() { LocalKey = "UpdateResVersion", ExpectProgress = 0.75f }
                },
                {
                    EUpdateState.UpdateResManifest,
                    new UpdateStateDesc() { LocalKey = "UpdateResManifest", ExpectProgress = 0.85f }
                },
                {
                    EUpdateState.UpdatePreloading,
                    new UpdateStateDesc() { LocalKey = "UpdatePreloading", ExpectProgress = 0.95f }
                },
                { EUpdateState.DownloadRes, new UpdateStateDesc() { LocalKey = "DownloadRes", ExpectProgress = 0f } },
                { EUpdateState.DownloadDoneClearCache, new UpdateStateDesc() { LocalKey = "DownloadDoneClearCache", ExpectProgress = 0.85f } },
                {
                    EUpdateState.LoadAssembly,
                    new UpdateStateDesc() { LocalKey = "LoadAssembly", ExpectProgress = 0.97f }
                },
                {
                    EUpdateState.DoneSuccess, new UpdateStateDesc() { LocalKey = "DoneSuccess", ExpectProgress = 0.99f }
                },
            };

        private Stopwatch _gameLoadingWatch;

        private void Awake()
        {
            _instance = this;
        }

        /// <summary>
        /// 当前资源版本
        /// </summary>
        public string PackageVersion { get; set; }
        
        /// <summary>
        /// 更新目标版本
        /// </summary>
        public string TargetVersion { get; set; }

        /// <summary>
        /// 更新UI显示版本号
        /// </summary>
        public void UpdateLoadingVersion()
        {
            if (!this._hotUpdateLoading.gameObject.activeSelf)
                this._hotUpdateLoading.gameObject.SetActive(true);

            _hotUpdateLoading.UpdateVersion();
        }

        /// <summary>
        /// 更新进度
        /// </summary>
        /// <param name="state">设置当前进度</param>
        /// <param name="updatePercent">外部传入的进度值，不传直接使用配置的预期值</param>
        public void UpdateProgress(EUpdateState state, float updatePercent = -1)
        {
            if (!this._hotUpdateLoading.gameObject.activeSelf)
                this._hotUpdateLoading.gameObject.SetActive(true);
            _mapDesc.TryGetValue(state, out UpdateStateDesc desc);
            _hotUpdateLoading.SetProgress(state, desc, updatePercent);
        }

        /// <summary>
        /// 显示重试弹窗
        /// </summary>
        /// <param name="content"></param>
        /// <param name="callback"></param>
        public void ShowRetryInfo(string content, Action callback)
        {
            ShowConfirm(async () =>
            {
                UpdateProgress(EUpdateState.RetryState);
                await UniTask.WaitForSeconds(1.0f);
                callback();
            }, content, LocalizationPool.Get("UpdateRetry"));
        }

        /// <summary>
        /// 显示确认弹窗
        /// </summary>
        /// <param name="confirmAction"></param>
        /// <param name="content"></param>
        /// <param name="confirmText"></param>
        /// <param name="hideClose"></param>
        public void ShowConfirm(UnityAction confirmAction, string content, string confirmText, bool hideClose = true)
        {
            // if (!this._hotUpdateConfirmUI.gameObject.activeSelf)
            //     this._hotUpdateConfirmUI.gameObject.SetActive(true);
            // this._hotUpdateConfirmUI.SetView(confirmAction, content, confirmText, hideClose);
        }

        public void SetGameLoadingWatch(bool isStop)
        {
            if(_gameLoadingWatch == null)
                _gameLoadingWatch = new Stopwatch();
            if (isStop)
            {
                _gameLoadingWatch.Stop();
                Logger.Info($"游戏逻辑加载完成，耗时：{_gameLoadingWatch.Elapsed.TotalSeconds:F2} s");
                //上报耗时事件
                SDKEventParam param = new SDKEventParam();
                param.Key = "custom_gameloading_time";
                Dictionary<string, object> paramDict = new Dictionary<string, object>();
                paramDict.Add("total_ms_time", _gameLoadingWatch.Elapsed.TotalSeconds.ToString("F2"));
                param.Params = paramDict;
                string jsonParam = JsonMapper.ToJson(param);
                SDKMgr.Instance.CallSDKMethod("logEvent", jsonParam, "logEvent", result => { });
            }
            else
            {
                _gameLoadingWatch.Start();
            }
        }

        public void ShowVersionPanel(Action callback)
        {
            if (_versionPanel != null)
            {
                _versionPanel.gameObject.SetActive(true);
                _versionPanel.SetEnterCallback(callback);
            }
        }

        public void Hide()
        {
            if (_hotUpdateLoading != null)
            {
                Destroy(_hotUpdateLoading.gameObject);
                _hotUpdateLoading = null;
            }

            // if (_hotUpdateConfirmUI != null)
            // {
            //     Destroy(_hotUpdateConfirmUI.gameObject);
            //     _hotUpdateConfirmUI = null;
            // }
            if (_versionPanel != null)
            {
                Destroy(_versionPanel.gameObject);
                _versionPanel = null;
            }

            if (_hotUpdateLoadingBg != null)
            {
                Destroy(_hotUpdateLoadingBg.gameObject);
                _hotUpdateLoadingBg = null;
            }
            //游戏逻辑加载完成上报
            SDKMgr.Instance.CallSDKMethod("loadingEndLog", null,"loadingEndLog", result =>
            {
                
            });
            SetGameLoadingWatch(true);
        }
    }
}