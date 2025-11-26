using Cysharp.Threading.Tasks;
using GameCore.SDK;

namespace GameCore.Process
{
    /// <summary>
    /// 初始化SDK
    /// </summary>
    public class ProcessSDKInit : IProcess
    {
        public void Init()
        {
        }

        public void Enter()
        {
            SDKMgr.Instance.CallSDKMethod(SDKMethodConst.Init, "", SDKMethodConst.Init, (code) =>
            {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
                WeChatWASM.WX.GetSystemInfo(new WeChatWASM.GetSystemInfoOption()
                {
                    success = info =>
                    {
                        SDKMgr.Instance.SetDeviceInfo(info.platform, info.brand, info.model, info.language,
                            info.screenHeight, info.screenWidth, info.pixelRatio);
                        WeChatWASM.WX.GetDeviceBenchmarkInfo(new WeChatWASM.GetDeviceBenchmarkInfoOption()
                        {
                            success =
                                benchInfo => { SDKMgr.Instance.SetDeviceBenchmark(benchInfo.benchmarkLevel); }
                        });
                    }
                });
                
                var appBase = WeChatWASM.WX.GetAppBaseInfo();
                SDKMgr.Instance.SetAppIsEnableDebug(appBase.enableDebug);
#endif

                ProcessManager.Instance.Enter<ProcessAppUpdate>();
            });
        }

        public void Leave()
        {
        }

        public void Update()
        {
        }
    }
}