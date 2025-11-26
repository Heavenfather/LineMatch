using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixCore.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Logger = GameCore.Log.Logger;
using Object = UnityEngine.Object;

namespace HotfixCore.Module
{
    /// <summary>
    /// 窗口基类
    /// </summary>
    public abstract class UIWindow : UIBase
    {
        private System.Action<UIWindow,bool> _prepareCallback;

        private bool _isCreate = false;

        private bool _initVisible = true;
        
        private GameObject _panel;

        private Canvas _canvas;

        protected Canvas Canvas => _canvas;

        private Canvas[] _childCanvas;

        private GraphicRaycaster _raycaster;

        private TimeoutController _timeoutController = new TimeoutController();

        protected GraphicRaycaster GraphicRaycaster => _raycaster;

        private GraphicRaycaster[] _childRaycaster;

        public override UIType Type => UIType.Window;

        /// <summary>
        /// 窗口位置组件。
        /// </summary>
        public override Transform transform => _panel.transform;

        /// <summary>
        /// 窗口矩阵位置组件。
        /// </summary>
        public override RectTransform rectTransform => _panel.transform as RectTransform;

        /// <summary>
        /// 窗口的实例资源对象。
        /// </summary>
        public override GameObject gameObject => _panel;

        /// <summary>
        /// 窗口名称。
        /// </summary>
        public string WindowName { private set; get; }

        /// <summary>
        /// 窗口层级。
        /// </summary>
        public int WindowLayer { private set; get; }

        /// <summary>
        /// 资源定位地址。
        /// </summary>
        public string AssetName { private set; get; }

        /// <summary>
        /// 是否为全屏窗口。
        /// </summary>
        public virtual bool FullScreen { private set; get; } = false;

        /// <summary>
        /// 是内部资源无需AB加载。
        /// </summary>
        public bool FromResources { private set; get; }

        /// <summary>
        /// 隐藏窗口关闭时间。
        /// </summary>
        public int HideTimeToClose { get; set; }

        /// <summary>
        /// 弹框组件。
        /// </summary>
        private Transform _popTransform;

        /// <summary>
        /// 弹出方式
        /// </summary>
        protected GiftPopType _popType = GiftPopType.Touch;

        /// <summary>
        /// 窗口深度值。
        /// </summary>
        public int Depth
        {
            get
            {
                if (_canvas != null)
                {
                    return _canvas.sortingOrder;
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                if (_canvas != null)
                {
                    if (_canvas.sortingOrder == value)
                    {
                        return;
                    }

                    // 设置父类
                    _canvas.sortingOrder = value;

                    // 设置子类
                    int depth = value;
                    for (int i = 0; i < _childCanvas.Length; i++)
                    {
                        var canvas = _childCanvas[i];
                        if (canvas != _canvas)
                        {
                            depth += 5; //注意递增值
                            canvas.sortingOrder = depth;
                        }
                    }

                    // 虚函数
                    if (_isCreate)
                    {
                        OnSortDepth(value);
                    }
                }
            }
        }

        /// <summary>
        /// 窗口可见性
        /// </summary>
        public bool Visible
        {
            get
            {
                if (_canvas != null)
                {
                    return _canvas.gameObject.layer == UIModule.WINDOW_SHOW_LAYER;
                }
                else
                {
                    return false;
                }
            }

            set
            {
                if (_canvas != null)
                {
                    int setLayer = value ? UIModule.WINDOW_SHOW_LAYER : UIModule.WINDOW_HIDE_LAYER;
                    if (_canvas.gameObject.layer == setLayer)
                        return;

                    // 显示设置
                    _canvas.gameObject.layer = setLayer;
                    for (int i = 0; i < _childCanvas.Length; i++)
                    {
                        _childCanvas[i].gameObject.layer = setLayer;
                    }

                    // 交互设置
                    Interactable = value;

                    // 虚函数
                    if (_isCreate)
                    {
                        OnSetVisible(value);
                    }
                    gameObject.SetVisible(value); //Overlay模式需要隐藏节点
                }
            }
        }

        /// <summary>
        /// 窗口交互性
        /// </summary>
        private bool Interactable
        {
            get
            {
                if (_raycaster != null)
                {
                    return _raycaster.enabled;
                }
                else
                {
                    return false;
                }
            }

            set
            {
                if (_raycaster != null)
                {
                    _raycaster.enabled = value;
                    for (int i = 0; i < _childRaycaster.Length; i++)
                    {
                        _childRaycaster[i].enabled = value;
                    }
                }
            }
        }

        /// <summary>
        /// 是否加载完毕。
        /// </summary>
        internal bool IsLoadDone = false;

        /// <summary>
        /// UI是否销毁。
        /// </summary>
        internal bool IsDestroyed = false;

        /// <summary>
        /// UI是否隐藏标志位。
        /// </summary>
        public bool IsHide { internal set; get; } = false;

        public event Action<UIWindow> DestroyListener;

        public void Init(string name, int layer, bool fullScreen, string assetName, bool fromResources)
        {
            WindowName = name;
            WindowLayer = layer;
            FullScreen = fullScreen;
            AssetName = assetName;
            FromResources = fromResources;
        }

        internal void TryInvoke(System.Action<UIWindow,bool> prepareCallback,bool initVisible, System.Object[] userDatas)
        {
            base.userDatas = userDatas;
            this._initVisible = initVisible;
            if (IsPrepare)
            {
                prepareCallback?.Invoke(this,initVisible);
            }
            else
            {
                _prepareCallback = prepareCallback;
            }
        }

        internal async UniTaskVoid InternalLoad(string location, Action<UIWindow,bool> prepareCallback,
            System.Object[] userDatas, string customPackage = "",bool initVisible = true)
        {
            _prepareCallback = prepareCallback;
            this.userDatas = userDatas;
            this._initVisible = initVisible;
            if (!FromResources)
            {
                var uiInstance = await G.ResourceModule.LoadGameObjectAsync(location, UIModule.UIRoot,
                    _timeoutController.Timeout(TimeSpan.FromSeconds(ResourceModule.WAIT_MAX_SECONDS)), customPackage,
                    needShowWait: true);
                Handle_Completed(uiInstance);
            }
            else
            {
                var uiInstance = await Resources.LoadAsync<GameObject>(location).ToUniTask();
                GameObject panel = Object.Instantiate((GameObject)uiInstance, UIModule.UIRoot);
                Handle_Completed(panel);
            }
        }

        internal void InternalCreate()
        {
            if (_isCreate == false)
            {
                _isCreate = true;
                ScriptGenerate();
                AddListeners();
                OnCreate();
                OnPopAnim();
            }
        }

        internal void InternalRefresh()
        {
            OnRefresh();
        }

        internal bool InternalUpdate()
        {
            if (!IsPrepare || !Visible)
            {
                return false;
            }

            List<UIWidget> listNextUpdateChild = null;
            if (ListChild != null && ListChild.Count > 0)
            {
                listNextUpdateChild = ListUpdateChild;
                var updateListValid = UpdateListValid;
                List<UIWidget> listChild = null;
                if (!updateListValid)
                {
                    if (listNextUpdateChild == null)
                    {
                        listNextUpdateChild = new List<UIWidget>();
                        ListUpdateChild = listNextUpdateChild;
                    }
                    else
                    {
                        listNextUpdateChild.Clear();
                    }

                    listChild = ListChild;
                }
                else
                {
                    listChild = listNextUpdateChild;
                }

                for (int i = 0; i < listChild.Count; i++)
                {
                    var uiWidget = listChild[i];

                    if (uiWidget == null)
                    {
                        continue;
                    }

                    var needValid = uiWidget.InternalUpdate();

                    if (!updateListValid && needValid)
                    {
                        listNextUpdateChild.Add(uiWidget);
                    }
                }

                if (!updateListValid)
                {
                    UpdateListValid = true;
                }
            }

            bool needUpdate = false;
            if (listNextUpdateChild == null || listNextUpdateChild.Count <= 0)
            {
                HasOverrideUpdate = true;
                OnUpdate();
                needUpdate = HasOverrideUpdate;
            }
            else
            {
                OnUpdate();
                needUpdate = true;
            }

            return needUpdate;
        }

        internal void InternalDestroy()
        {
            _isCreate = false;

            RemoveAllUIEvent();
            RemoveAllTimer();

            for (int i = 0; i < ListChild.Count; i++)
            {
                var uiChild = ListChild[i];
                uiChild.CallDestroy();
                uiChild.OnDestroyWidget();
            }

            // 注销回调函数
            _prepareCallback = null;

            DestroyListener?.Invoke(this);
            OnDestroy();

            // 销毁面板对象
            if (_panel != null)
            {
                Object.Destroy(_panel);
                _panel = null;
            }

            IsDestroyed = true;
        }

        /// <summary>
        /// 处理资源加载完成回调。
        /// </summary>
        /// <param name="panel">面板资源实例。</param>
        private void Handle_Completed(GameObject panel)
        {
            if (panel == null)
            {
                Logger.Error($"Loca Panel {this.WindowName} is null");
                return;
            }

            IsLoadDone = true;

            if (IsDestroyed)
            {
                Object.Destroy(panel);
                return;
            }

            panel.name = GetType().Name;
            _panel = panel;
            _panel.transform.localPosition = Vector3.zero;

            // 获取组件
            _canvas = _panel.GetOrAddComponent<Canvas>();
            if (_canvas == null)
            {
                throw new Exception($"Not found {nameof(Canvas)} in panel {WindowName}");
            }

            _canvas.overrideSorting = true;
            _canvas.sortingOrder = 0;
            _canvas.sortingLayerName = "Default";

            // 获取组件
            _raycaster = _panel.GetOrAddComponent<GraphicRaycaster>();
            _childCanvas = _panel.GetComponentsInChildren<Canvas>(true);
            _childRaycaster = _panel.GetComponentsInChildren<GraphicRaycaster>(true);

            // 通知UI管理器
            IsPrepare = true;
            _prepareCallback?.Invoke(this, _initVisible);
        }

        public virtual void HideSelf()
        {
            G.UIModule.HideUI(this.GetType());
        }

        public virtual void CloseSelf()
        {
            G.UIModule.CloseUI(this.GetType());
        }

        protected void SetPopTransform(Transform popTransform)
        {
            _popTransform = popTransform;
        }

        private void OnPopAnim() {
            if (_popTransform == null)
            {
                G.EventModule.DispatchEvent(GameEventDefine.OnWindowOpen,EventOneParam<string>.Create(WindowName));
                return;
            }

            AudioUtil.PlayPopWindow();
            _popTransform.localScale = Vector3.zero;
            _popTransform.DOScale(1, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                OnPopComplete();
                G.EventModule.DispatchEvent(GameEventDefine.OnWindowOpen,EventOneParam<string>.Create(WindowName));
            });
        }

        protected virtual void OnPopComplete() {}

        public override void OnDestroy()
        {
            G.PopModule.CheckPopClose(WindowName);
        }

        public virtual void SetPopType(GiftPopType popType)
        {
            _popType = popType;
        }
    }
}