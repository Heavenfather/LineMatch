namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 蔓延水组件
    /// 标记该元素是蔓延水，可以向相邻格子传播
    /// </summary>
    public struct SpreadWaterComponent : IEcsAutoReset<SpreadWaterComponent>
    {
        /// <summary>
        /// 水的配置ID
        /// </summary>
        public int WaterConfigId;

        /// <summary>
        /// 是否已经初始化视觉效果
        /// </summary>
        public bool IsVisualInitialized;

        public bool IsInitHideIcon;

        public void AutoReset(ref SpreadWaterComponent com)
        {
            com.WaterConfigId = 0;
            com.IsVisualInitialized = false;
        }
    }
}
