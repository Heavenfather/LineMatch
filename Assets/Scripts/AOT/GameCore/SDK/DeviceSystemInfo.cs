using UnityEngine;

namespace GameCore.SDK
{
    public class DeviceSystemInfo
    {
        /// <summary>
        /// 设备平台
        /// - 'ios': iOS微信（包含 iPhone、iPad）;
        /// - 'android': Android微信;
        /// - 'ohos': HarmonyOS微信;
        /// - 'windows': Windows微信;
        /// - 'mac': macOS微信;
        /// - 'devtools': 微信开发者工具;
        /// </summary>
        public string Platform { get; }

        /// <summary>
        /// 设备品牌
        /// </summary>
        public string Brand { get; }

        /// <summary>
        /// 设备型号
        /// </summary>
        public string Model { get; }

        /// <summary>
        /// 设备设置语言
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// 屏幕高度
        /// </summary>
        public double ScreenHeight { get; }

        /// <summary>
        /// 屏幕宽度
        /// </summary>
        public double ScreenWidth { get; }

        /// <summary>
        /// 设备像素比
        /// </summary>
        public double PixelRatio { get; }

        /// <summary>
        /// 设备性能等级
        /// </summary>
        public double DeviceBenchmarkLevel { get; private set; }

        public bool IsEnableDebug { get; private set; } = false;

        public DeviceSystemInfo()
        {
            Platform = "PC";
            Brand = "Windows";
            Model = "Windows";
            Language = "zh-CN";
            ScreenHeight = Screen.height;
            ScreenWidth = Screen.width;
            PixelRatio = Screen.dpi / 96;
            DeviceBenchmarkLevel = 100;
            Debug.Log("设备信息：" + Platform + " " + Model + " " + Language + " " + ScreenHeight + " " + ScreenWidth + " " +
                      PixelRatio);
        }

        public DeviceSystemInfo(string platform, string brand, string model, string language, double screenHeight,
            double screenWidth,
            double pixelRatio)
        {
            Platform = platform;
            Model = model;
            Brand = brand;
            Language = language;
            ScreenHeight = screenHeight;
            ScreenWidth = screenWidth;
            PixelRatio = pixelRatio;
            Debug.Log("设备信息：" + platform + " " + model + " " + language + " " + screenHeight + " " + screenWidth + " " +
                      pixelRatio);
        }

        public void SetDeviceBenchmarkLevel(double level)
        {
            DeviceBenchmarkLevel = level;
        }

        public void SetIsEnableDebug(bool isEnableDebug)
        {
            IsEnableDebug = isEnableDebug;
        }
    }
}