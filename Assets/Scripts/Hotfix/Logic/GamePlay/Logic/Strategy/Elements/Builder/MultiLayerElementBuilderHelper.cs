using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 多层元素构建辅助类
    /// 提供通用的多层元素初始化方法
    /// 
    /// 使用方式：
    /// 在各种障碍物的Builder中调用AddMultiLayerComponent方法
    /// </summary>
    public static class MultiLayerElementBuilderHelper
    {
        /// <summary>
        /// 为实体添加MultiLayerComponent并初始化
        /// </summary>
        /// <param name="context">游戏状态上下文</param>
        /// <param name="entity">实体ID</param>
        /// <param name="config">元素配置</param>
        public static void AddMultiLayerComponent(GameStateContext context, int entity, in ElementMap config)
        {
            var multiLayerPool = context.World.GetPool<MultiLayerComponent>();
            ref var multiLayer = ref multiLayerPool.Add(entity);

            // 初始化剩余层数
            multiLayer.RemainingLayers = CalculateTotalLayers(config.Id, context);
            multiLayer.IsEliminate = false;
            multiLayer.IsWillTransform = false;
        }

        /// <summary>
        /// 计算元素的总层数
        /// 通过递归查找所有可能的转换形态来确定
        /// </summary>
        private static int CalculateTotalLayers(int configId, GameStateContext context)
        {
            int layers = 1; // 当前层
            int currentConfigId = configId;
            int transitionCount = 0;
            const int MAX_TRANSITIONS = 10;

            var transitionRule = MatchBoot.Container.Resolve<IElementTransitionRuleService>();
            var matchService = context.ServiceFactory.GetService(context.CurrentMatchType);

            // 递归查找所有下一层
            while (transitionCount < MAX_TRANSITIONS)
            {
                transitionCount++;

                if (!transitionRule.TryTransitionToNextElement(currentConfigId, matchService, out var nextConfigId))
                {
                    // 没有下一层了
                    break;
                }

                layers++;
                currentConfigId = nextConfigId;
            }

            return layers;
        }
    }
}
