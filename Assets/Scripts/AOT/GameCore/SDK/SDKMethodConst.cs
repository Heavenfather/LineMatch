namespace GameCore.SDK
{
    /// <summary>
    /// Unity调用SDK函数定义，与SDK层的onUnityCall函数对应
    /// </summary>
    public class SDKMethodConst
    {
        public const string Init = "init";
        public const string Login = "login";
        public const string Logout = "logout";
        public const string CheckLogin = "checkLogin";
        public const string LogEvent = "logEvent";
    }
}