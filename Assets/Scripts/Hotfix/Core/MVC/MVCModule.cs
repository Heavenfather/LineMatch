using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Log;
using Hotfix.Define;
using HotfixCore.Module;
using HotfixCore.Utils;
using HotfixLogic;

namespace HotfixCore.MVC
{
    public class MVCModule
    {
        /// <summary>
        /// 模块当前状态
        /// </summary>
        public enum ModuleState
        {
            /// <summary>
            /// 未加载
            /// </summary>
            Unloaded,

            /// <summary>
            /// 已加载但是未激活
            /// </summary>
            Sleeping,

            /// <summary>
            /// 激活中
            /// </summary>
            Activating,

            /// <summary>
            /// 正在激活
            /// </summary>
            Active
        }

        /// <summary>
        /// 模块当前状态
        /// </summary>
        public ModuleState State { get; private set; } = ModuleState.Unloaded;

        /// <summary>
        /// 模块最后的激活时间 MVC内部根据LRU规则动态淘汰
        /// </summary>
        public DateTime LastActiveTime { get; private set; }

        /// <summary>
        /// 模块定义
        /// </summary>
        public MVCDefine ModuleDefine { get; private set; }

        /// <summary>
        /// 模块数据层
        /// </summary>
        protected BaseModel Model { get; private set; }

        /// <summary>
        /// 模块显示的主界面
        /// </summary>
        protected UIWindow MainView { get; private set; }

        /// <summary>
        /// 模块控制层
        /// </summary>
        protected BaseController Controller { get; private set; }

        private bool _isNavigation;
        private UIWindow _currentActiveWindow;
        private readonly List<UIWindow> _uiStack = new List<UIWindow>(5); //存放模块打开的窗口，如一些弹窗，在跳转时需要关闭

        public MVCModule(MVCDefine mvcDefine)
        {
            ModuleDefine = mvcDefine;
        }

        /// <summary>
        /// 激活当前模块
        /// </summary>
        /// <param name="initData"></param>
        public async UniTask Activate(params object[] initData)
        {
            if (!string.IsNullOrEmpty(ModuleDefine.Parent))
            {
                var parentDefine = MVCConfig.GetMVCDefine(ModuleDefine.Parent);
                if (parentDefine != null)
                {
                    var parentModule = new MVCModule(parentDefine);
                    await parentModule.Activate(initData);
                    return;
                }
            }

            if (State == ModuleState.Active)
                return;

            LastActiveTime = DateTime.Now;

            G.UIModule.SetWaitingVisible(true);
            string lockReason = ModuleDefine.MVCName;
            G.UIModule.ScreenLock(lockReason,true);
            if (ModuleDefine.NeedShowLoading)
            {
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            }
            await ActiveMainWindow(initData);
            // GuideManager.Instance.ForceCloseGuide();
            G.UIModule.ScreenLock(lockReason, false);
        }

        /// <summary>
        /// 使当前模块休眠
        /// </summary>
        public async UniTask Sleep()
        {
            if (State != ModuleState.Active)
                return;

            State = ModuleState.Sleeping;
            Controller.Sleep();
            //隐藏所有的UI
            for (int i = _uiStack.Count - 1; i >= 0; i--)
            {
                _uiStack[i].HideSelf();
            }

            MainView?.HideSelf();
            await UniTask.Yield();
        }

        public void UnloadByExMainViewClose()
        {
            if (State == ModuleState.Unloaded)
                return;
            State = ModuleState.Unloaded;
            //销毁所有的UI
            for (int i = _uiStack.Count - 1; i >= 0; i--)
            {
                _uiStack[i].CloseSelf();
            }

            _uiStack.Clear();
            //内存池归还
            if (Model != null)
            {
                MemoryPool.MemoryPool.Release(Model);
                Model = null;
            }
            if (Controller != null)
            {
                MemoryPool.MemoryPool.Release(Controller);
                Controller = null;
            }
            
            G.ResourceModule.UnloadAllUnusedAssets().Forget();
        }
        
        /// <summary>
        /// 关闭当前模块
        /// </summary>
        public void Unload()
        {
            if (State == ModuleState.Unloaded)
                return;
            State = ModuleState.Unloaded;
            //销毁所有的UI
            for (int i = _uiStack.Count - 1; i >= 0; i--)
            {
                _uiStack[i].CloseSelf();
            }

            _uiStack.Clear();
            MainView?.CloseSelf();
            //内存池归还
            if (Model != null)
            {
                MemoryPool.MemoryPool.Release(Model);
                Model = null;
            }
            if (Controller != null)
            {
                MemoryPool.MemoryPool.Release(Controller);
                Controller = null;
            }
            
            G.ResourceModule.UnloadAllUnusedAssets().Forget();
        }

        /// <summary>
        /// 打开模块中的弹出窗口
        /// </summary>
        /// <param name="hideLast">是否隐藏上一个子窗口</param>
        /// <param name="userDatas"></param>
        /// <typeparam name="T"></typeparam>
        public async UniTask<T> OpenChildWindow<T>(bool hideLast = false, params System.Object[] userDatas) where T : UIWindow
        {
            if (_isNavigation) return null;
            _isNavigation = true;

            try
            {
                if (hideLast)
                {
                    if (_uiStack.Count > 0)
                    {
                        for (int i = 0; i < _uiStack.Count; i++)
                        {
                            if(_uiStack[i].WindowName == _currentActiveWindow.WindowName)
                                _uiStack[i].HideSelf();
                        }
                    }
                    else
                    {
                        //没有子窗口就表示隐藏的主界面
                        MainView?.HideSelf();
                    }
                }
                
                //由UI模块内部判断是否存在窗口
                var childWindow = await G.UIModule.ShowUIAsyncAwait<T>(ModuleDefine.PackageName, userDatas);
                _currentActiveWindow = childWindow;
                childWindow.DestroyListener += OnSubWindowClose;
                bool hasChildWindow = false;
                for (int i = 0; i < _uiStack.Count; i++)
                {
                    if (_uiStack[i].WindowName == childWindow.WindowName)
                    {
                        hasChildWindow = true;
                        break;
                    }
                }

                if (!hasChildWindow)
                    _uiStack.Add(childWindow);
                return (T)childWindow;
            }
            finally
            {
                _isNavigation = false;
                
            }
        }

        /// <summary>
        /// 获取当前模块打开的窗口
        /// </summary>
        /// <param name="uiType"></param>
        /// <returns></returns>
        public UIWindow GetActiveWindow(Type uiType)
        {
            string windowName = uiType.FullName;
            if(MainView.WindowName == windowName)
                return MainView;
            for (int i = 0; i < _uiStack.Count; i++)
            {
                if (_uiStack[i].WindowName == windowName)
                {
                    return _uiStack[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 关闭当前打开的子窗口
        /// </summary>
        public void CloseCurrentWindow()
        {
            if (_uiStack.Count > 0 && _currentActiveWindow != null)
            {
                for (int i = _uiStack.Count - 1; i >= 0; i--)
                {
                    if (_uiStack[i].WindowName == _currentActiveWindow.WindowName)
                    {
                        _uiStack[i].CloseSelf();
                        break;
                    }
                }
            }
        }
        
        private async UniTask ActiveMainWindow(params object[] initData)
        {
            if (State == ModuleState.Unloaded)
            {
                State = ModuleState.Activating;
                var controllerType = AssemblyUtil.GetType($"HotfixLogic.{ModuleDefine.MVCName}Controller");
                if (controllerType == null)
                {
                    throw new Exception($"{ModuleDefine.MVCName} controller could not be found");
                }
                Controller = (BaseController)MemoryPool.MemoryPool.Acquire(controllerType);
                
                //进入模块
                if (ModuleDefine.ModelType != null)
                {
                    Model = (BaseModel)MemoryPool.MemoryPool.Acquire(ModuleDefine.ModelType);
                    await Model.Enter(initData);
                }

                if (State == ModuleState.Unloaded)
                {
                    throw new Exception($"{ModuleDefine.MVCName} module unloaded");
                }
                
                MainView = await G.UIModule.ShowUIAsyncAwait(Controller.MainView, ModuleDefine.PackageName,false, initData);
                if (State == ModuleState.Unloaded)
                {
                    MainView?.CloseSelf();
                    throw new Exception($"{ModuleDefine.MVCName} module unloaded");
                }

                MainView.DestroyListener += OnMainViewDestroy;
                _currentActiveWindow = MainView;

                State = ModuleState.Active;
                
                Controller.BindModule(this, Model);
                await Controller.Enter(initData);
                
                if (State == ModuleState.Unloaded)
                {
                    throw new Exception($"{ModuleDefine.MVCName} module unloaded");
                }

                GuideManager.Instance.CheckAndExecuteGuide(ModuleDefine.MVCName, Model);
                
                MainView.Visible = true;
                G.UIModule.SetWaitingVisible(false);
            }
            else if (State == ModuleState.Sleeping)
            {
                //休眠中的模块重新激活
                Controller.ReActive(initData);
                State = ModuleState.Active;
                MainView.Visible = true;
                GuideManager.Instance.CheckAndExecuteGuide(ModuleDefine.MVCName, Model);
            }
        }

        private void OnMainViewDestroy(UIWindow window)
        {
            //意外情况导致外部关闭了UI
            if (State != ModuleState.Unloaded)
            {
                UnloadByExMainViewClose();
            }
        }

        private void OnSubWindowClose(UIWindow window)
        {
            int idx = -1;

            for (int i = _uiStack.Count - 1; i >= 0; i--)
            {
                if (_uiStack[i].WindowName == window.WindowName)
                {
                    idx = i;
                    break;
                }
            }

            if (idx != -1)
                _uiStack.RemoveAt(idx);
        }
    }
}