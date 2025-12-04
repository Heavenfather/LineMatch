using System;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 收集道具飞行完成回调组件
    /// 用于存储飞行完成后需要执行的回调
    /// </summary>
    public struct CollectItemCallbackComponent
    {
        /// <summary>
        /// 回调委托
        /// </summary>
        public Action Callback;

        /// <summary>
        /// 道具元素ID
        /// </summary>
        public int CallbackEntity;
    }
}
