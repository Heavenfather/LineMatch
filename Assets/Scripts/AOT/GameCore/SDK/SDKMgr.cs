using System;
using GameCore.Singleton;
using UnityEngine;

namespace GameCore.SDK
{
    public class SDKMgr : LazySingleton<SDKMgr>
    {
        private GameObject _entity;
        private ISDKBridge _sdkBridge;
        private DeviceSystemInfo _deviceSystemInfo;

        protected override void OnInitialized()
        {
            MakeEntity();
        }

        private void MakeEntity()
        {
            _entity = new GameObject("[SDKBridge]");
            GameObject.DontDestroyOnLoad(_entity);
#if UNITY_ANDROID && !UNITY_EDITOR
            _sdkBridge = _entity.AddComponent<AndroidSDKBridge>();
#elif UNITY_WEBGL && !UNITY_EDITOR
            _sdkBridge = _entity.AddComponent<WebGLSDKBridge>();
#else
            _sdkBridge = _entity.AddComponent<WinSDKBridge>();
#endif
        }

        /// <summary>
        /// 设置设备信息
        /// </summary>
        public void SetDeviceInfo(string platform, string brand, string model, string language, double screenHeight,
            double screenWidth,
            double pixelRatio)
        {
            _deviceSystemInfo = new DeviceSystemInfo(platform, brand, model, language, screenHeight, screenWidth,
                pixelRatio);
        }

        /// <summary>
        /// 设置设备等级
        /// </summary>
        /// <param name="level"></param>
        public void SetDeviceBenchmark(double level)
        {
            if (_deviceSystemInfo != null)
            {
                _deviceSystemInfo.SetDeviceBenchmarkLevel(level);
            }
        }

        public void SetAppIsEnableDebug(bool isEnableDebug)
        {
            _deviceSystemInfo?.SetIsEnableDebug(isEnableDebug);
        }

        /// <summary>
        /// 获取设备信息
        /// </summary>
        /// <returns></returns>
        public DeviceSystemInfo GetDeviceSystemInfo()
        {
            return _deviceSystemInfo ??= new DeviceSystemInfo();
        }

        /// <summary>
        /// 调用SDK接口
        /// </summary>
        /// <param name="callMethod">调用SDK的方法名称</param>
        /// <param name="jsonParam">传到SDK的json格式参数</param>
        /// <param name="callbackName">SDK回调回来的监听名称</param>
        /// <param name="callback">SDK回调监听方法</param>
        public void CallSDKMethod(string callMethod, string jsonParam, string callbackName, Action<SDKReturn> callback, object objData = null)
        {
            if (_sdkBridge == null) {
                Debug.LogError("SDKBridge is null");
                return;
            }

            if (!string.IsNullOrEmpty(jsonParam))
            {
                Debug.Log("CallSDKMethod: " + callMethod + " " + jsonParam);
            }
            _sdkBridge.CallSDKNative(callMethod, jsonParam, callbackName, callback, objData);
        }

        /// <summary>
        /// 设备震动
        /// </summary>
        /// <param name="intensity">震动强度[0-3]</param>
        public void OnDeviceVibration(int intensity)
        {
            if (intensity <= 0)
                return;
            //目前只处理微信小游戏上面的效果
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
            string wxIntensity = "light";
            if (intensity == 2)
                wxIntensity = "medium";
            if (intensity >= 3)
                wxIntensity = "heavy";
            WeChatWASM.WX.VibrateShort(new WeChatWASM.VibrateShortOption()
            {
                type = wxIntensity
            });
#elif UNITY_WEBGL && DOUYINMINIGAME && !UNITY_EDITOR
            //抖音小游戏没有震动强度的概念
            TTSDK.TT.Vibrate(new long[1] { 400 });
#endif
        }
    }
}