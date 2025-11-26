using System;

namespace GameCore.SDK
{
    [Serializable]
    public class SDKReturn
    {
        public string CallbackName;
        public SDKCallbackCode Code;
        public string Param;

        internal SDKReturn(string callbackName, SDKCallbackCode code, string param)
        {
            CallbackName = callbackName;
            Code = code;
            Param = param;
        }
        
        public override string ToString()
        {
            return $"CallbackName:{CallbackName} Code:{Code} Param:{Param}";
        }
    }
}