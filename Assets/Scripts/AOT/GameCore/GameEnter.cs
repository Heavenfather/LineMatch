using System.Collections;
using GameCore.Log;
using GameCore.Logic;
using GameCore.Process;
using GameCore.Resource;
using GameCore.SDK;
using GameCore.Settings;
using GameCore.Utils;
using UnityEngine;
using Logger = GameCore.Log.Logger;

public class GameEnter : MonoBehaviour
{
    [SerializeField] private int _frameRate = 60;

    [SerializeField] private float _gameSpeed = 1f;

    [SerializeField] private bool _runInBackground = true;

    [SerializeField] private bool _neverSleep = true;

    [SerializeField] private GameObject _debugConsole;

    void Awake()
    {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
            WeChatWASM.WX.GetSystemInfo(new WeChatWASM.GetSystemInfoOption()
            {
                success = info =>
                {
                    Logger.Info($"当前设备为：{info.brand} 系统版本为：{info.system}");
                }
            });
            WeChatWASM.WX.OnMemoryWarning((result) =>
            {
                //收到微信的内存警告 调用一次GC
                Logger.Info("微信的内存警告,强制GC");
                OnLowMemory();
                WeChatWASM.WX.TriggerGC();
            });
#else
        Application.lowMemory += OnLowMemory;
#endif
        //游戏启动上报
        SDKMgr.Instance.CallSDKMethod("loadingBeginLog", null,"loadingBeginLog",(result)=>{});
        HotUpdateManager.Instance.SetGameLoadingWatch(false);
        DontDestroyOnLoad(this);
        Application.targetFrameRate = _frameRate;
        Time.timeScale = _gameSpeed;
        Application.runInBackground = _runInBackground;
        Screen.sleepTimeout = _neverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;

        TimeUtils.StartFrame();
    }

    private void OnLowMemory()
    {
        if (ResourceModuleDriver.Instance.OnLowMemory != null)
            ResourceModuleDriver.Instance.OnLowMemory();
    }

    void Start()
    {
        Logger.SetLogHelper(new DefaultLogHelper(), ELogLevel.Debug,GameSettings.Instance.ProjectSetting.AppMode == EAppMode.Debug);
        if (_debugConsole)
        {
            _debugConsole.SetActive(GameSettings.Instance.IsShowDebugConsole());
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
        var childs = _debugConsole.GetComponentsInChildren<TMPro.TMP_InputField>(true);
        foreach (var child in childs)
        {
            if (child.GetComponent<WXInputFieldTmpAdapter>() == null)
            {
                child.gameObject.AddComponent<WXInputFieldTmpAdapter>();
            }
        }

        foreach (var child in this.GetComponentsInChildren<UnityEngine.UI.InputField>())
        {
            if (child.GetComponent<WXInputFieldAdapter>() == null)
            {
                child.gameObject.AddComponent<WXInputFieldAdapter>();
            }
        }
#elif UNITY_WEBGL && DOUYINMINIGAME && !UNITY_EDITOR
        var childs = _debugConsole.GetComponentsInChildren<TMPro.TMP_InputField>(true);
        foreach (var child in childs)
        {
            if (child.GetComponent<TTInputFieldTmpAdapter>() == null)
            {
                child.gameObject.AddComponent<TTInputFieldTmpAdapter>();
            }
        }

        foreach (var child in this.GetComponentsInChildren<UnityEngine.UI.InputField>())
        {
            if (child.GetComponent<TTInputFieldAdapter>() == null)
            {
                child.gameObject.AddComponent<TTInputFieldAdapter>();
            }
        }
#endif
        }

        StartCoroutine(Launch());
    }

    void Update()
    {
        TimeUtils.StartFrame();
        ProcessManager.Instance.OnUpdate();
    }

    void FixedUpdate()
    {
        TimeUtils.StartFrame();
    }

    void LateUpdate()
    {
        TimeUtils.StartFrame();
    }

    IEnumerator Launch()
    {
        yield return new WaitForEndOfFrame();
        ProcessManager.Instance.StartUp();
    }
}