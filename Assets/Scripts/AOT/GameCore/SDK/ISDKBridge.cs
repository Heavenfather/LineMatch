using System;
using UnityEngine;

namespace GameCore.SDK
{
    public interface ISDKBridge
    {
        void OnSDKCallback(string callMethod, SDKReturn sdkReturn);
        void CallSDKNative(string callMethod, string jsonParam, string callbackName, Action<SDKReturn> callbackAction, object objData = null);
    }

    public abstract class MonoSDKBridge : MonoBehaviour, ISDKBridge
    {
        public abstract void OnSDKCallback(string callMethod, SDKReturn sdkReturn);

        public abstract void CallSDKNative(string callMethod, string jsonParam, string callbackName, Action<SDKReturn> callbackAction, object objData = null);
    }
}