namespace GameCore.SDK
{
    public struct SDKCallbackCode
    {
        public int CallbackCode;

        public int ErrCode;
        
        public object CallBackData;
        
        public string ErrMsg;

        public string SDKType;

        public override string ToString()
        {
            return $"SDK CallbackCode: {CallbackCode}, Data: {CallBackData}, ErrCode: {ErrCode}, ErrMsg: {ErrMsg}, SDKType: {SDKType}";
        }
    }
}