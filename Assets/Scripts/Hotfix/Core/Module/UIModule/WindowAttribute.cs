using System;

namespace HotfixCore.Module
{
    /// <summary>
    /// UI层级枚举。
    /// </summary>
    public enum UILayer : int
    {
        /// <summary>
        /// 底部UI 如主界面
        /// </summary>
        Bottom = 0,
        /// <summary>
        /// 全屏UI
        /// </summary>
        UI = 1,
        /// <summary>
        /// 弹出UI
        /// </summary>
        Top = 2,
        /// <summary>
        /// 提示类UI 跑马灯等
        /// </summary>
        Tips = 3,
        /// <summary>
        /// 引导层
        /// </summary>
        Guide = 4,
        /// <summary>
        /// 系统级别UI，层级最高
        /// </summary>
        System = 5,
    }


    [AttributeUsage(AttributeTargets.Class)]
    public class WindowAttribute : Attribute
    {
        /// <summary>
        /// 窗口层级
        /// </summary>
        public readonly int WindowLayer;

        /// <summary>
        /// 资源定位地址。
        /// </summary>
        public readonly string Location;

        /// <summary>
        /// 全屏窗口标记。
        /// </summary>
        public readonly bool FullScreen;

        /// <summary>
        /// 是内部资源无需AB加载。
        /// </summary>
        public readonly bool FromResources;

        public WindowAttribute(int windowLayer, string location = "", bool fullScreen = false)
        {
            WindowLayer = windowLayer;
            Location = location;
            FullScreen = fullScreen;
        }

        public WindowAttribute(UILayer windowLayer, string location = "", bool fullScreen = false)
        {
            WindowLayer = (int)windowLayer;
            Location = location;
            FullScreen = fullScreen;
        }

        public WindowAttribute(UILayer windowLayer, bool fromResources, bool fullScreen = false)
        {
            WindowLayer = (int)windowLayer;
            FromResources = fromResources;
            FullScreen = fullScreen;
        }

        public WindowAttribute(UILayer windowLayer, bool fromResources, string location, bool fullScreen = false)
        {
            WindowLayer = (int)windowLayer;
            FromResources = fromResources;
            Location = location;
            FullScreen = fullScreen;
        }
    }
}