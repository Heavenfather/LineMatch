using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HotfixCore.Module
{
    public abstract class UIBase : IUIBehaviour
    {
        /// <summary>
        /// UI类型。
        /// </summary>
        public enum UIType
        {
            /// <summary>
            /// 类型无。
            /// </summary>
            None,

            /// <summary>
            /// 类型Windows。
            /// </summary>
            Window,

            /// <summary>
            /// 类型Widget。
            /// </summary>
            Widget,
        }

        /// <summary>
        /// 所属UI父节点。
        /// </summary>
        protected UIBase parent = null;

        /// <summary>
        /// UI父节点。
        /// </summary>
        public UIBase Parent => parent;

        /// <summary>
        /// 自定义数据集。
        /// </summary>
        protected System.Object[] userDatas;

        /// <summary>
        /// 自定义数据。
        /// </summary>
        public System.Object UserData
        {
            get
            {
                if (userDatas != null && userDatas.Length >= 1)
                {
                    return userDatas[0];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 自定义数据集。
        /// </summary>
        public System.Object[] UserDatas => userDatas;


        /// <summary>
        /// 窗口的实例资源对象。
        /// </summary>
        public virtual GameObject gameObject { protected set; get; }

        /// <summary>
        /// 窗口位置组件。
        /// </summary>
        public virtual Transform transform { protected set; get; }

        /// <summary>
        /// 窗口矩阵位置组件。
        /// </summary>
        public virtual RectTransform rectTransform { protected set; get; }

        /// <summary>
        /// UI类型。
        /// </summary>
        public virtual UIType Type => UIType.None;


        /// <summary>
        /// 资源是否准备完毕。
        /// </summary>
        public bool IsPrepare { protected set; get; }

        /// <summary>
        /// UI子组件列表。
        /// </summary>
        public List<UIWidget> ListChild = new List<UIWidget>();

        /// <summary>
        /// 存在Update更新的UI子组件列表。
        /// </summary>
        protected List<UIWidget> ListUpdateChild = null;

        /// <summary>
        /// 是否持有Update行为。
        /// </summary>
        protected bool UpdateListValid = false;

        /// <summary>
        /// 是否需要Update
        /// </summary>
        protected bool HasOverrideUpdate = true;

        /// <summary>
        /// 计时器ID
        /// </summary>
        protected List<int> TimerIds = new List<int>(10);

        private VariableArray _variableArray;

        /// <summary>
        /// 控件绑定组件
        /// </summary>
        protected VariableArray VariableArray
        {
            get
            {
                if (_variableArray == null && this.gameObject != null)
                {
                    _variableArray = this.gameObject.GetComponent<VariableArray>();
                }

                return _variableArray;
            }
        }

        public virtual void ScriptGenerate()
        {
        }

        public virtual void AddListeners()
        {
        }

        /// <summary>
        /// UI创建成功
        /// </summary>
        public abstract void OnCreate();


        public virtual void OnRefresh()
        {
        }

        public virtual void OnUpdate()
        {
            HasOverrideUpdate = false;
        }

        public virtual void OnDestroy()
        {
        }

        internal void CallDestroy()
        {
            OnDestroy();
        }

        /// <summary>
        /// 当触发窗口的层级排序。
        /// </summary>
        protected virtual void OnSortDepth(int depth)
        {
        }

        /// <summary>
        /// 当因为全屏遮挡触或者窗口可见性触发窗口的显隐。
        /// </summary>
        protected virtual void OnSetVisible(bool visible)
        {
        }

        internal void SetUpdateDirty()
        {
            UpdateListValid = false;
            if (Parent != null)
            {
                Parent.SetUpdateDirty();
            }
        }

        #region Timer

        /// <summary>
        /// 添加计时器。
        /// </summary>
        /// <param name="timerHandler">计时器回调。</param>
        /// <param name="time">计时器间隔,以秒为单位。</param>
        /// <param name="isLoop">是否循环。</param>
        /// <param name="isUnscaled">是否不受时间缩放影响。</param>
        protected int AddTimer(TimerHandler timerHandler, float time, bool isLoop = false, bool isUnscaled = false)
        {
            int id = G.TimerModule.AddTimer(timerHandler, time, isLoop, isUnscaled);
            TimerIds.Add(id);
            return id;
        }

        /// <summary>
        /// 移除UI所有计时器
        /// </summary>
        protected void RemoveAllTimer()
        {
            if (TimerIds.Count > 0)
            {
                for (int i = 0; i < TimerIds.Count; i++)
                {
                    G.TimerModule.RemoveTimer(TimerIds[i]);
                }
                TimerIds.Clear();
            }
        }

        protected void RemoveTimer(int id)
        {
            if (TimerIds.Contains(id))
            {
                G.TimerModule.RemoveTimer(id);
                TimerIds.Remove(id);
            }
        }

        #endregion

        #region Event

        private EventDispatcher _dispatcher;

        protected EventDispatcher EventDispatcher
        {
            get
            {
                if (_dispatcher == null)
                    _dispatcher = MemoryPool.MemoryPool.Acquire<EventDispatcher>();
                return _dispatcher;
            }
        }

        /// <summary>
        /// 添加无参事件监听
        /// </summary>
        /// <param name="eventId">事件id</param>
        /// <param name="listener">监听函数</param>
        /// <param name="handle">持有者</param>
        protected void AddListeners(int eventId, Action listener, object handle = null)
        {
            if (handle == null)
                handle = this.gameObject;
            EventDispatcher.AddEventListener(eventId, listener, handle);
        }

        /// <summary>
        /// 添加带参事件监听
        /// </summary>
        /// <param name="eventId">事件id</param>
        /// <param name="listener">监听函数</param>
        /// <param name="handle">持有者</param>
        protected void AddListeners<T>(int eventId, Action<T> listener, object handle = null) where T : struct,IEventParameter
        {
            if (handle == null)
                handle = this.gameObject;
            EventDispatcher.AddEventListener<T>(eventId, listener, handle);
        }

        /// <summary>
        /// 移除所有监听
        /// </summary>
        protected void RemoveAllUIEvent()
        {
            if (_dispatcher != null)
            {
                MemoryPool.MemoryPool.Release(_dispatcher);
            }
        }

        #endregion

        #region UIWidget

        /// <summary>
        /// 通过实体创建Widget
        /// </summary>
        /// <param name="type">Widget类</param>
        /// <param name="root"></param>
        /// <param name="visible"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public UIWidget CreateWidget(Type type, GameObject root, bool visible = true)
        {
            if (type == null)
            {
                return null;
            }
            var widget = (UIWidget)Activator.CreateInstance(type);
            if (widget == null)
                throw new Exception("Create widget not valid");
            if (widget.Create(this, root, visible))
            {
                return widget;
            }

            return null;
        }

        /// <summary>
        /// 创建UIWidget通过父UI位置节点。
        /// <remarks>因为资源实例已经存在父物体所以不需要异步。</remarks>
        /// </summary>
        /// <param name="goPath">父UI位置节点。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public T CreateWidget<T>(string goPath, bool visible = true) where T : UIWidget, new()
        {
            var goRootTrans = FindChild(goPath);

            if (goRootTrans != null)
            {
                return CreateWidget<T>(goRootTrans.gameObject, visible);
            }

            return null;
        }


        /// <summary>
        /// 创建UIWidget通过父UI位置节点。
        /// <remarks>因为资源实例已经存在父物体所以不需要异步。</remarks>
        /// </summary>
        /// <param name="parentTrans"></param>
        /// <param name="goPath">父UI位置节点。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public T CreateWidget<T>(Transform parentTrans, string goPath, bool visible = true) where T : UIWidget, new()
        {
            var goRootTrans = FindChild(parentTrans, goPath);
            if (goRootTrans != null)
            {
                return CreateWidget<T>(goRootTrans.gameObject, visible);
            }

            return null;
        }

        /// <summary>
        /// 创建UIWidget通过游戏物体。
        /// <remarks>因为资源实例已经存在父物体所以不需要异步。</remarks>
        /// </summary>
        /// <param name="goRoot">游戏物体。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public T CreateWidget<T>(GameObject goRoot, bool visible = true) where T : UIWidget, new()
        {
            var widget = new T();
            if (widget.Create(this, goRoot, visible))
            {
                return widget;
            }

            return null;
        }

        /// <summary>
        /// 创建UIWidget通过资源定位地址。
        /// </summary>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="assetLocation">资源定位地址。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public async UniTask<T> CreateWidgetByPathAsync<T>(Transform parentTrans, string assetLocation,
            bool visible = true) where T : UIWidget, new()
        {
            GameObject goInst = await G.ResourceModule.LoadGameObjectAsync(assetLocation, parentTrans,
                gameObject.GetCancellationTokenOnDestroy());
            return CreateWidget<T>(goInst, visible);
        }
        
        /// <summary>
        /// 创建UIWidget通过资源定位地址。
        /// </summary>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="assetLocation">资源定位地址。</param>
        /// <param name="callback">加载成功回调。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public void CreateWidgetByPathAsync<T>(Transform parentTrans, string assetLocation,Action<T> callback,
            bool visible = true) where T : UIWidget, new()
        {
            G.ResourceModule.LoadGameObjectAsync(assetLocation, go =>
            {
                var widget = CreateWidget<T>(go, visible);
                callback?.Invoke(widget);
            }, parentTrans).Forget();
        }

        /// <summary>
        /// 根据prefab或者模版来创建新的 widget。
        /// </summary>
        /// <param name="goPrefab">资源创建副本。</param>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public T CreateWidgetByPrefab<T>(GameObject goPrefab, Transform parentTrans = null, bool visible = true)
            where T : UIWidget, new()
        {
            var widget = new T();
            if (!widget.CreateByPrefab(this, goPrefab, parentTrans, visible))
            {
                return null;
            }

            return widget;
        }

        /// <summary>
        /// 通过UI类型来创建widget。
        /// </summary>
        /// <param name="parentTrans">资源父节点。</param>
        /// <param name="visible">是否可见。</param>
        /// <typeparam name="T">UIWidget。</typeparam>
        /// <returns>UIWidget实例。</returns>
        public async UniTask<T> CreateWidgetByTypeAsync<T>(Transform parentTrans, bool visible = true)
            where T : UIWidget, new()
        {
            return await CreateWidgetByPathAsync<T>(parentTrans, typeof(T).Name, visible);
        }

        public Transform FindChild(string path)
        {
            return FindChildImp(rectTransform, path);
        }

        public Transform FindChild(Transform trans, string path)
        {
            return FindChildImp(trans, path);
        }

        public T FindChildComponent<T>(string path) where T : Component
        {
            return FindChildComponentImp<T>(rectTransform, path);
        }

        public T FindChildComponent<T>(Transform trans, string path) where T : Component
        {
            return FindChildComponentImp<T>(trans, path);
        }

        private static Transform FindChildImp(Transform transform, string path)
        {
            var findTrans = transform.Find(path);
            return findTrans != null ? findTrans : null;
        }

        private static T FindChildComponentImp<T>(Transform transform, string path) where T : Component
        {
            var findTrans = transform.Find(path);
            if (findTrans != null)
            {
                return findTrans.gameObject.GetComponent<T>();
            }

            return null;
        }

        #endregion
    }
}