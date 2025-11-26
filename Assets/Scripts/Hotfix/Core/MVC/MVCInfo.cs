using System;
using Cysharp.Threading.Tasks;
using HotfixCore.MemoryPool;
using HotfixCore.Module;

namespace HotfixCore.MVC
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MVCDefine : Attribute
    {
        /// <summary>
        /// 永不自动收回
        /// </summary>
        public bool NeverAutoCollect;

        /// <summary>
        /// 是否需要显示Loading界面
        /// </summary>
        public bool NeedShowLoading;
        
        /// <summary>
        /// 模块名称
        /// </summary>
        public string MVCName;

        /// <summary>
        /// 父MVC的模块
        /// 在加载MVC时会优先加载父MVC模块，加载完成后才加载次模块，存在依赖关系
        /// </summary>
        public string Parent;

        /// <summary>
        /// 模块资源所属的资源包
        /// </summary>
        public string PackageName;

        /// <summary>
        /// 指定的Model层
        /// </summary>
        public Type ModelType;

        public MVCDefine(string mvcName)
        {
            this.MVCName = mvcName;
        }

        public MVCDefine(string mvcName, bool neverAutoCollect)
        {
            this.MVCName = mvcName;
            this.NeverAutoCollect = neverAutoCollect;
        }

        public MVCDefine(string mvcName, string parent)
        {
            this.MVCName = mvcName;
            this.Parent = parent;
        }

        public MVCDefine(string mvcName, Type modelType)
        {
            this.MVCName = mvcName;
            this.ModelType = modelType;
            this.PackageName = "";
        }

        public MVCDefine(string mvcName, Type modelType,bool needShowLoading)
        {
            this.MVCName = mvcName;
            this.ModelType = modelType;
            this.PackageName = "";
            this.NeedShowLoading = needShowLoading;
        }
        
        public MVCDefine(string mvcName, Type modelType, string packageName)
        {
            this.MVCName = mvcName;
            this.ModelType = modelType;
            this.PackageName = packageName;
        }

        public MVCDefine(string mvcName, Type modelType, bool hideLast, bool neverAutoCollect, string packageName,
            string parent,bool needShowLoading)
        {
            this.MVCName = mvcName;
            this.ModelType = modelType;
            this.PackageName = packageName;
            this.Parent = parent;
            this.NeedShowLoading = needShowLoading;
            this.NeverAutoCollect = neverAutoCollect;
        }
    }

    public interface IMVC
    {
        UniTask Enter(object[] data);
    }

    public abstract class BaseModel : IMVC, IMemory
    {
        public object[] UserData { get; private set; }

        public UniTask Enter(params object[] data)
        {
            this.UserData = data;
            return OnInitialized();
        }

        public void Clear()
        {
            UserData = null;
            OnDestroy();
        }

        /// <summary>
        /// 等待数据初始化完成再进行界面操作
        /// </summary>
        /// <returns></returns>
        protected virtual UniTask OnInitialized()
        {
            return UniTask.CompletedTask;
        }

        protected virtual void OnDestroy()
        {
        }
    }

    public abstract class BaseController : IMVC, IMemory
    {
        protected object[] UserData { get; private set; }

        public abstract Type MainView { get; }

        public BaseModel Model;

        protected MVCModule Module { get; private set; }
        
        protected EventDispatcher Dispatcher { get; private set; }

        public UniTask Enter(params object[] data)
        {
            Dispatcher ??= MemoryPool.MemoryPool.Acquire<EventDispatcher>();
            UserData = data;
            ReActive(data);
            return OnInitialized();
        }

        public void Clear()
        {
            UserData = null;
            Model = null;
            Module = null;
            MemoryPool.MemoryPool.Release(Dispatcher);
            Dispatcher = null;
            OnDestroy();
        }

        public void BindModule(MVCModule module, BaseModel model)
        {
            Model = model;
            Module = module;
        }

        /// <summary>
        /// 模块重新激活时触发
        /// </summary>
        public void ReActive(object data = null)
        {
            OnReActive(data);
        }

        public void Sleep()
        {
            OnSleep();
        }

        protected virtual void OnReActive(object data = null)
        {
            
        }
        
        protected virtual void OnSleep()
        {
            
        }
        
        protected virtual UniTask OnInitialized()
        {
            return UniTask.CompletedTask;
        }

        protected virtual void OnDestroy()
        {
        }
    }
}