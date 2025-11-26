using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Logic;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Logger = GameCore.Log.Logger;
using Object = UnityEngine.Object;

namespace HotfixCore.Module
{
    public class UIModule : IModuleAwake, IModuleUpdate, IModuleDestroy
    {
        private static Transform _instanceRoot = null;

        /// <summary>
        /// UI根节点。
        /// </summary>
        public static Transform UIRoot => _instanceRoot;

        private Canvas _canvas;
        public Canvas Canvas => _canvas;

        private AssetLoadingWait _assetLoadingWait;
        private Camera _gameMainCamera;
        private Camera _uiCamera = null;
        public Camera UICamera => _uiCamera;
        private Camera _currentCamera;
        public Camera CurrentCamera => _currentCamera;
        private readonly List<UIWindow> _uiStack = new List<UIWindow>(100);
        private int _waitingTimerId;
        private ScreenLocker _screenLocker;

        public const int LAYER_DEEP = 2000;
        public const int WINDOW_DEEP = 100;
        public const int WINDOW_HIDE_LAYER = 2; // Ignore Raycast
        public const int WINDOW_SHOW_LAYER = 5; // UI

        private Vector2Int _canvasResolution = new Vector2Int(750, 1334);

        public void Awake(object parameter)
        {
            _gameMainCamera = GameObject.Find("GameMainCamera").GetComponent<Camera>();
            Logger.Info($"UI Set Main Camera : type:{_gameMainCamera.cameraType} cameraStack:{_gameMainCamera.GetUniversalAdditionalCameraData().cameraStack.Count}");
            var uiRoot = GameObject.Find("UIRoot");
            if (uiRoot != null)
            {
                _canvas = uiRoot.GetComponentInChildren<Canvas>();
                _instanceRoot = _canvas.transform;
                var lockGo = _instanceRoot.transform.Find("ScreenLocker")?.gameObject;
                if (lockGo != null)
                {
                    Logger.Info("AddLockComponent");
                    _screenLocker = lockGo.AddComponent<ScreenLocker>();
                }
                _uiCamera = uiRoot.GetComponentInChildren<Camera>();
                SetCanvasResolution(_canvasResolution.x, _canvasResolution.y);
            }
            else
            {
                Logger.Fatal("UIRoot not found !");
                return;
            }

            Object.DontDestroyOnLoad(_instanceRoot.parent != null ? _instanceRoot.parent : _instanceRoot);
            _instanceRoot.gameObject.layer = LayerMask.NameToLayer("UI");

            GameObject assetLoadingWait = GameObject.Find("UIRoot/UICanvas/AssetLoadingWait");
            if (assetLoadingWait != null)
            {
                // _assetLoadingWait = assetLoadingWait.GetComponent<AssetLoadingWait>();
            }
            else
            {
                Logger.Warning("Loading asset cant found");
            }
        }

        public void Tick(float elapseSeconds, float realElapseSeconds)
        {
            if (_uiStack == null)
            {
                return;
            }

            int count = _uiStack.Count;
            for (int i = 0; i < _uiStack.Count; i++)
            {
                if (_uiStack.Count != count)
                {
                    break;
                }

                var window = _uiStack[i];
                window.InternalUpdate();
            }
        }

        public void Destroy()
        {
            CloseAll();
            if (_instanceRoot != null && _instanceRoot.parent != null)
            {
                Object.Destroy(_instanceRoot.parent.gameObject);
            }
        }


        /// <summary>
        /// 获取所有层级下顶部的窗口名称。
        /// </summary>
        public string GetTopWindow()
        {
            if (_uiStack.Count == 0)
            {
                return string.Empty;
            }

            UIWindow topWindow = _uiStack[^1];
            return topWindow.WindowName;
        }

        /// <summary>
        /// 获取指定层级下顶部的窗口名称。
        /// </summary>
        public string GetTopWindow(int layer)
        {
            UIWindow lastOne = null;
            for (int i = 0; i < _uiStack.Count; i++)
            {
                if (_uiStack[i].WindowLayer == layer)
                    lastOne = _uiStack[i];
            }

            if (lastOne == null)
                return string.Empty;

            return lastOne.WindowName;
        }

        /// <summary>
        /// 是否有任意窗口正在加载。
        /// </summary>
        public bool IsAnyLoading()
        {
            for (int i = 0; i < _uiStack.Count; i++)
            {
                var window = _uiStack[i];
                if (window.IsLoadDone == false)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 查询窗口是否存在。
        /// </summary>
        /// <typeparam name="T">界面类型。</typeparam>
        /// <returns>是否存在。</returns>
        public bool HasWindow<T>()
        {
            return HasWindow(typeof(T));
        }

        /// <summary>
        /// 查询窗口是否存在。
        /// </summary>
        /// <param name="type">界面类型。</param>
        /// <returns>是否存在。</returns>
        public bool HasWindow(Type type)
        {
            return IsContains(type.FullName);
        }

        /// <summary>
        /// 异步打开窗口。
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="userDatas">用户自定义数据。</param>
        /// <returns>打开窗口操作句柄。</returns>
        public void ShowUIAsync<T>(string packageName = "", params System.Object[] userDatas) where T : UIWindow
        {
            ShowUIImp(typeof(T), packageName,true, userDatas);
        }

        /// <summary>
        /// 异步打开窗口。
        /// </summary>
        /// <param name="type">界面类型。</param>
        /// <param name="packageName"></param>
        /// <param name="userDatas">用户自定义数据。</param>
        /// <returns>打开窗口操作句柄。</returns>
        public void ShowUIAsync(Type type, string packageName = "", params System.Object[] userDatas)
        {
            ShowUIImp(type, packageName,true, userDatas);
        }

        /// <summary>
        /// 异步打开窗口。
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="userDatas">用户自定义数据。</param>
        /// <returns>打开窗口操作句柄。</returns>
        public async UniTask<UIWindow> ShowUIAsyncAwait<T>(string packageName = "", params System.Object[] userDatas)
            where T : UIWindow
        {
            return await ShowUIAwaitImp(typeof(T), packageName,true, userDatas);
        }

        /// <summary>
        /// 异步打开窗口。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="packageName"></param>
        /// <param name="initVisible">首帧是否显示</param>
        /// <param name="userDatas">用户自定义数据。</param>
        /// <returns>打开窗口操作句柄。</returns>
        public async UniTask<UIWindow> ShowUIAsyncAwait(Type type, string packageName = "",bool initVisible = true,
            params System.Object[] userDatas)
        {
            return await ShowUIAwaitImp(type, packageName, initVisible, userDatas);
        }

        private void ShowUIImp(Type type, string packageName = "",bool initVisible = true, params System.Object[] userDatas)
        {
            string windowName = type.FullName;

            // 如果窗口已经存在
            if (IsContains(windowName))
            {
                UIWindow window = GetWindow(windowName);
                Pop(window); //弹出窗口
                Push(window); //重新压入
                window.TryInvoke(OnWindowPrepare,initVisible, userDatas);
            }
            else
            {
                UIWindow window = CreateInstance(type);
                Push(window); //首次压入
                window.InternalLoad(window.AssetName, OnWindowPrepare, userDatas, packageName,initVisible).Forget();
            }
        }

        private async UniTask<UIWindow> ShowUIAwaitImp(Type type, string packageName = "",bool initVisible = true,
            params System.Object[] userDatas)
        {
            string windowName = type.FullName;

            // 如果窗口已经存在
            if (IsContains(windowName))
            {
                UIWindow window = GetWindow(windowName);
                Pop(window); //弹出窗口
                Push(window); //重新压入
                window.TryInvoke(OnWindowPrepare,initVisible, userDatas);
                return window;
            }
            else
            {
                UIWindow window = CreateInstance(type);
                Push(window); //首次压入
                window.InternalLoad(window.AssetName, OnWindowPrepare, userDatas, packageName,initVisible).Forget();
                float time = 0f;
                while (!window.IsLoadDone)
                {
                    time += Time.deltaTime;
                    if (time > 60f)
                    {
                        break;
                    }

                    await UniTask.Yield();
                }

                return window;
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public void CloseUI<T>() where T : UIWindow
        {
            CloseUI(typeof(T));
        }

        public void CloseUI(Type type)
        {
            string windowName = type.FullName;
            UIWindow window = GetWindow(windowName);
            if (window == null)
                return;

            window.InternalDestroy();
            Pop(window);
            OnSortWindowDepth(window.WindowLayer);
            OnSetWindowVisible();
            G.EventModule.DispatchEvent(GameEventDefine.OnWindowClose,EventOneParam<string>.Create(window.WindowName));
        }

        public void HideUI<T>() where T : UIWindow
        {
            HideUI(typeof(T));
        }

        public void HideUI(Type type)
        {
            string windowName = type.FullName;
            UIWindow window = GetWindow(windowName);
            if (window == null)
            {
                return;
            }

            window.Visible = false;
            window.IsHide = true;

            if (window.FullScreen)
            {
                OnSetWindowVisible();
            }
        }

        /// <summary>
        /// 关闭所有窗口。
        /// </summary>
        public void CloseAll()
        {
            for (int i = 0; i < _uiStack.Count; i++)
            {
                UIWindow window = _uiStack[i];
                window.InternalDestroy();
            }

            _uiStack.Clear();
        }

        /// <summary>
        /// 关闭所有窗口除了。
        /// </summary>
        public void CloseAllWithOut(UIWindow withOut)
        {
            for (int i = _uiStack.Count - 1; i >= 0; i--)
            {
                UIWindow window = _uiStack[i];
                if (window == withOut)
                {
                    continue;
                }

                window.InternalDestroy();
                _uiStack.RemoveAt(i);
            }
        }

        /// <summary>
        /// 关闭所有窗口除了。
        /// </summary>
        public void CloseAllWithOut<T>() where T : UIWindow
        {
            for (int i = _uiStack.Count - 1; i >= 0; i--)
            {
                UIWindow window = _uiStack[i];
                if (window.GetType() == typeof(T))
                {
                    continue;
                }

                window.InternalDestroy();
                _uiStack.RemoveAt(i);
            }
        }

        public void CloseAllWithOut(List<string> windowNames)
        {
            for (int i = _uiStack.Count - 1; i >= 0; i--)
            {
                UIWindow window = _uiStack[i];
                if (windowNames.Contains(window.WindowName))
                {
                    continue;
                }

                window.InternalDestroy();
                _uiStack.RemoveAt(i);
            }
        }

        /// <summary>
        /// 设置资源加载中界面是否可见
        /// </summary>
        public void SetWaitingVisible(bool visible, float timeout = 30)
        {
            if (_assetLoadingWait != null)
            {
                _assetLoadingWait.SetVisible(visible);
                if (visible)
                {
                    _assetLoadingWait.transform.SetAsLastSibling();
                }
                if (visible && timeout > 0)
                {
                    if (_waitingTimerId > 0)
                    {
                        G.TimerModule.RemoveTimer(_waitingTimerId);
                    }
                    _waitingTimerId = G.TimerModule.AddTimer(() =>
                    {
                        G.TimerModule.RemoveTimer(_waitingTimerId);
                    }, timeout);
                }
            }
        }

        /// <summary>
        /// 设置画布设计分辨率
        /// </summary>
        public void SetCanvasResolution(int width, int height,float matchWidthOrHeight = 0)
        {
            if (_canvas != null)
            {
                var canvasScaler = _canvas.GetComponent<CanvasScaler>();
                canvasScaler.referenceResolution = new Vector2(width, height);
                canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
            }
        }

        private void OnWindowPrepare(UIWindow window,bool initVisible)
        {
            OnSortWindowDepth(window.WindowLayer);
            window.InternalCreate();
            window.InternalRefresh();
            window.IsHide = false;
            OnSetWindowVisible(initVisible);
        }

        private void OnSortWindowDepth(int layer)
        {
            int depth = layer * LAYER_DEEP;
            for (int i = 0; i < _uiStack.Count; i++)
            {
                if (_uiStack[i].WindowLayer == layer)
                {
                    _uiStack[i].Depth = depth;
                    depth += WINDOW_DEEP;
                }
            }
        }

        private void OnSetWindowVisible(bool initVisible = true)
        {
            bool isHideNext = false;
            for (int i = _uiStack.Count - 1; i >= 0; i--)
            {
                UIWindow window = _uiStack[i];
                if (isHideNext == false)
                {
                    if (window.IsHide)
                    {
                        continue;
                    }

                    window.Visible = true;
                    if (window.IsPrepare && window.FullScreen)
                    {
                        isHideNext = true;
                    }
                }
                else
                {
                    window.Visible = false;
                }
            }
        }

        private UIWindow CreateInstance(Type type)
        {
            UIWindow window = Activator.CreateInstance(type) as UIWindow;
            WindowAttribute attribute = Attribute.GetCustomAttribute(type, typeof(WindowAttribute)) as WindowAttribute;

            if (window == null)
                throw new Exception($"Window {type.FullName} create instance failed.");

            if (attribute != null)
            {
                string assetName = string.IsNullOrEmpty(attribute.Location) ? type.Name : attribute.Location;
                window.Init(type.FullName, attribute.WindowLayer, attribute.FullScreen, assetName,
                    attribute.FromResources);
            }
            else
            {
                window.Init(type.FullName, (int)UILayer.UI, fullScreen: window.FullScreen, assetName: type.Name,
                    fromResources: false);
            }

            return window;
        }

        /// <summary>
        /// 异步获取窗口。
        /// </summary>
        /// <returns>打开窗口操作句柄。</returns>
        public async UniTask<T> GetUIAsyncAwait<T>() where T : UIWindow
        {
            string windowName = typeof(T).FullName;
            var window = GetWindow(windowName);
            if (window == null)
            {
                return null;
            }

            var ret = window as T;

            if (ret == null)
            {
                return null;
            }

            if (ret.IsLoadDone)
            {
                return ret;
            }

            float time = 0f;
            while (!ret.IsLoadDone)
            {
                time += Time.deltaTime;
                if (time > 60f)
                {
                    break;
                }

                await UniTask.Yield();
            }

            return ret;
        }

        /// <summary>
        /// 异步获取窗口。
        /// </summary>
        /// <param name="callback">回调。</param>
        /// <returns>打开窗口操作句柄。</returns>
        public void GetUIAsync<T>(Action<T> callback) where T : UIWindow
        {
            string windowName = typeof(T).FullName;
            var window = GetWindow(windowName);
            if (window == null)
            {
                return;
            }

            var ret = window as T;

            if (ret == null)
            {
                return;
            }

            GetUIAsyncImp(callback).Forget();

            async UniTaskVoid GetUIAsyncImp(Action<T> ctx)
            {
                float time = 0f;
                while (!ret.IsLoadDone)
                {
                    time += Time.deltaTime;
                    if (time > 60f)
                    {
                        break;
                    }

                    await UniTask.Yield();
                }

                ctx?.Invoke(ret);
            }
        }

        /// <summary>
        /// 屏幕锁
        /// </summary>
        /// <param name="reason">锁住屏幕的原因</param>
        /// <param name="visible">加锁/解锁</param>
        /// <param name="lockTime">超时自动解锁</param>
        public void ScreenLock(string reason,bool visible,float lockTime = 3.0f)
        {
            if (_screenLocker != null)
            {
                _screenLocker.ScreenLock(reason, visible, lockTime);
            }
        }

        public void SetSceneCamera(Camera camera)
        {
            if (camera == null)
            {
                _gameMainCamera.SetVisible(true);
                _currentCamera = _gameMainCamera;
                return;
            }
            _gameMainCamera.SetVisible(false);
            _currentCamera = camera;
            AddUICameraOverlay(camera);
        }

        public void AddUICameraOverlay(Camera camera)
        {
            var newCamData = camera.GetUniversalAdditionalCameraData();
            newCamData.cameraStack.Insert(0, _uiCamera);
            Camera overUICam = GameObject.Find("GameEnter/TopOverUICamera")?.GetComponent<Camera>();
            if (overUICam != null)
            {
                newCamData.cameraStack.Insert(1, overUICam);
            }
        }

        public UIWindow GetWindow(string windowName)
        {
            for (int i = 0; i < _uiStack.Count; i++)
            {
                UIWindow window = _uiStack[i];
                if (window.WindowName == windowName)
                {
                    return window;
                }
            }

            return null;
        }

        private bool IsContains(string windowName)
        {
            for (int i = 0; i < _uiStack.Count; i++)
            {
                UIWindow window = _uiStack[i];
                if (window.WindowName == windowName)
                {
                    return true;
                }
            }

            return false;
        }

        private void Push(UIWindow window)
        {
            // 如果已经存在
            if (IsContains(window.WindowName))
                throw new System.Exception($"Window {window.WindowName} is exist.");

            // 获取插入到所属层级的位置
            int insertIndex = -1;
            for (int i = 0; i < _uiStack.Count; i++)
            {
                if (window.WindowLayer == _uiStack[i].WindowLayer)
                {
                    insertIndex = i + 1;
                }
            }

            // 如果没有所属层级，找到相邻层级
            if (insertIndex == -1)
            {
                for (int i = 0; i < _uiStack.Count; i++)
                {
                    if (window.WindowLayer > _uiStack[i].WindowLayer)
                    {
                        insertIndex = i + 1;
                    }
                }
            }

            // 如果是空栈或没有找到插入位置
            if (insertIndex == -1)
            {
                insertIndex = 0;
            }

            // 最后插入到堆栈
            _uiStack.Insert(insertIndex, window);
        }

        private void Pop(UIWindow window)
        {
            // 从堆栈里移除
            _uiStack.Remove(window);
        }

        public Vector2 GetDesignResolution() {
            if (_canvas) {
                return _canvas.GetComponent<CanvasScaler>().referenceResolution;
            }
            return _canvasResolution;
        }

        /// <summary>
        /// 获取UI组件的世界坐标
        /// </summary>
        /// <param name="uiTransform"></param>
        /// <returns></returns>
        public Vector3 GetCompScreenPosition(RectTransform uiTransform)
        {
            Vector3 worldPosition =
                _uiCamera.WorldToScreenPoint(uiTransform.position);
            return worldPosition;
        }

        /// <summary>
        /// 屏幕坐标转UI世界坐标
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector3 GetScreenPosToWorldPos(Vector3 pos)
        {
            return _uiCamera.ScreenToWorldPoint(pos);
        }

        public Vector3 GetUIScreenPos(Vector3 pos) {
            return RectTransformUtility.WorldToScreenPoint(G.UIModule.UICamera, pos);
        }
    }
}