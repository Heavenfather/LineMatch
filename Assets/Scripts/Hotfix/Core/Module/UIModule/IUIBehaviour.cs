namespace HotfixCore.Module
{
    public interface IUIBehaviour
    {
        /// <summary>
        /// 自动生成代码绑定
        /// </summary>
        void ScriptGenerate();

        /// <summary>
        /// 添加UI事件监听
        /// </summary>
        void AddListeners();

        /// <summary>
        /// UI创建成功
        /// </summary>
        void OnCreate();

        /// <summary>
        /// 面板刷新
        /// </summary>
        void OnRefresh();

        /// <summary>
        /// 面板轮询
        /// </summary>
        void OnUpdate();

        /// <summary>
        /// 面板销毁
        /// </summary>
        void OnDestroy();
    }
}