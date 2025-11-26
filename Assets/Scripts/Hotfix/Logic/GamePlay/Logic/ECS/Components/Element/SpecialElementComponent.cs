using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    public struct SpecialElementComponent
    {
        /// <summary>
        /// 特殊元素配置Id
        /// </summary>
        public int ConfigId;
        
        /// <summary>
        /// 元素类型
        /// </summary>
        public ElementType ElementType;
        
        /// <summary>
        /// 特殊元素被选中时排序优先级
        /// </summary>
        public int Priority;

        public int Entity;
    }
}