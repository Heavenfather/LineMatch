using System;
using System.Collections.Generic;
using GameCore.LitJson;
using Logger = GameCore.Log.Logger;

namespace GameCore.SDK
{
    public class WinSDKBridge : MonoSDKBridge
    {
        const string app_id = "1745916541";
        const string app_key = "G1MQto3IzikwBGex1HwMHp6nMMbXnFc4";
        const string channel_id = "1003";

        private bool _isInit = false;

        private Dictionary<string, Action<SDKReturn>> _callbackMap =
            new Dictionary<string, Action<SDKReturn>>(20);

        private void Awake()
        {
        }

        public void OnSDKCallback(string callMethod, SDKCallbackCode callbackCode) {
            OnSDKCallback(callMethod, new SDKReturn("", callbackCode, ""));
        }

        public override void OnSDKCallback(string callMethod, SDKReturn sdkReturn)
        {
            if (_callbackMap.TryGetValue(callMethod, out var callback))
            {
                sdkReturn.Code.SDKType = "JYSDK";
                callback.Invoke(sdkReturn);
                _callbackMap.Remove(callMethod);
            }
        }

        public override void CallSDKNative(string callMethod, string jsonParam, string callbackName, Action<SDKReturn> callbackAction, object objData = null)
        {
            callbackAction?.Invoke(new SDKReturn(callbackName, default, jsonParam));
        }
    }
}