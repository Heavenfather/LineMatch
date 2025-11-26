#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#elif UNITY_WEBGL && DOUYINMINIGAME && !UNITY_EDITOR
using TTSDK;
#endif

namespace GameCore.Utils
{
    public static class PlayerPrefsUtil
    {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
    public static string GetString(string key, string defaultValue = "")
    {
        return WX.StorageGetStringSync(key, defaultValue);
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        return WX.StorageGetIntSync(key, defaultValue);
    }

    public static float GetFloat(string key, float defaultValue = 0.0f)
    {
        return WX.StorageGetFloatSync(key, defaultValue);
    }

    public static void SetInt(string key, int value)
    {
        WX.StorageSetIntSync(key, value);
    }

    public static void SetFloat(string key, float value)
    {
        WX.StorageSetFloatSync(key, value);
    }

    public static void SetString(string key, string value)
    {
        WX.StorageSetStringSync(key, value);
    }

     public static void DeleteKey(string key)
    {
        WX.StorageDeleteKeySync(key);
    }

    public static void DeleteAll()
    {
        WX.StorageDeleteAllSync();
    }

    public static bool HasKey(string key)
    {
        return WX.StorageHasKeySync(key);
    }
    
    public static void Save()
    {

    }
#elif UNITY_WEBGL && DOUYINMINIGAME && !UNITY_EDITOR
    //抖音小游戏构建将使用WebGL方案，所以在这里需要重写PlayerPrefs
    
    public static void DeleteKey(string key)
    {
        TT.PlayerPrefs.DeleteKey(key);
    }

    public static void Save()
    {
        TT.PlayerPrefs.Save();
    }
    
    public static void DeleteAll()
    {
        TT.PlayerPrefs.DeleteAll();
    }

    public static bool HasKey(string key)
    {
        return TT.PlayerPrefs.HasKey(key);
    }
    
    public static string GetString(string key, string defaultValue = "")
    {
        return TT.PlayerPrefs.GetString(key, defaultValue);
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        return TT.PlayerPrefs.GetInt(key, defaultValue);
    }

    public static float GetFloat(string key, float defaultValue = 0.0f)
    {
        return TT.PlayerPrefs.GetFloat(key, defaultValue);
    }

    public static void SetInt(string key, int value)
    {
        TT.PlayerPrefs.SetInt(key, value);
    }

    public static void SetFloat(string key, float value)
    {
        TT.PlayerPrefs.SetFloat(key, value);
    }

    public static void SetString(string key, string value)
    {
        TT.PlayerPrefs.SetString(key, value);
    }
#else
        public static void DeleteKey(string key)
        {
            UnityEngine.PlayerPrefs.DeleteKey(key);
        }

        public static void Save()
        {
            UnityEngine.PlayerPrefs.Save();
        }

        public static void DeleteAll()
        {
            UnityEngine.PlayerPrefs.DeleteAll();
        }

        public static bool HasKey(string key)
        {
            return UnityEngine.PlayerPrefs.HasKey(key);
        }

        public static string GetString(string key, string defaultValue = "")
        {
            return UnityEngine.PlayerPrefs.GetString(key, defaultValue);
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            return UnityEngine.PlayerPrefs.GetInt(key, defaultValue);
        }

        public static float GetFloat(string key, float defaultValue = 0.0f)
        {
            return UnityEngine.PlayerPrefs.GetFloat(key, defaultValue);
        }

        public static void SetInt(string key, int value)
        {
            UnityEngine.PlayerPrefs.SetInt(key, value);
        }

        public static void SetFloat(string key, float value)
        {
            UnityEngine.PlayerPrefs.SetFloat(key, value);
        }

        public static void SetString(string key, string value)
        {
            UnityEngine.PlayerPrefs.SetString(key, value);
        }
#endif
    }
}