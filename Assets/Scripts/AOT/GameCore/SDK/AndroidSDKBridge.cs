using System;
using System.Collections.Generic;
using GameCore.LitJson;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace GameCore.SDK
{
    public class AndroidSDKBridge : MonoSDKBridge
    {
        private Dictionary<string, Action<SDKReturn>> _callbackMap = new Dictionary<string, Action<SDKReturn>>(50);

        public override void OnSDKCallback(string callMethod, SDKReturn sdkReturn)
        {
        }

        public override void CallSDKNative(string callMethod, string jsonParam, string callbackName,
            Action<SDKReturn> callbackAction, object objData = null)
        {
        }
    }
}