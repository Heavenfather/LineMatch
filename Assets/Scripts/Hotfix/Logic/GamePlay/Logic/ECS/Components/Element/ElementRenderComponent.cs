
namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 元素渲染组件
    /// </summary>
    public struct ElementRenderComponent
    {
        /// <summary>
        /// 元素渲染预制体键值
        /// </summary>
        public string PrefabKey;
        
        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible;
        
        /// <summary>
        /// 元素渲染视图引用
        /// </summary>
        public ElementView ViewInstance;

        /// <summary>
        /// 标记是否刚创建
        /// </summary>
        public bool IsDirty;

        /// <summary>
        /// 是否被选中
        /// </summary>
        public bool IsSelected;
        
        public bool WasSelected;    // 上一帧状态 (用于变化检测)
    }
}