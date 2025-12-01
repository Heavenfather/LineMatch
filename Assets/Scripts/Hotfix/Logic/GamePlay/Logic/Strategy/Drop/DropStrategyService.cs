using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    public class DropStrategyService
    {
        private List<IDropStrategy> _strategies;
        private IDropStrategy _defaultStrategy; // 保底策略

        public DropStrategyService()
        {
            _strategies = new List<IDropStrategy>();

            // ----------- 初始化策略链 一定要注意添加顺序 -------------
            // 优先级高 越先添加会越早判断拿出棋子
            
            // 1.配置掉落目标
            _strategies.Add(new DropFlagStrategy());
            // 2.安全检查任务目标
            _strategies.Add(new TargetSafetyStrategy());

            // 动态难度
            // _strategies.Add(new DynamicDifficultyStrategy());

            // 最后保底：纯权重随机
            _defaultStrategy = new WeightedRandomDropStrategy();
        }

        public int CalculateDropId(int col, int row, GameStateContext context,
            List<DropAnalysisComponent> dropAnalysisComponents)
        {
            // 1. 遍历高优先级策略
            foreach (var strategy in _strategies)
            {
                int result = strategy.GetDropElementId(col, row, context, dropAnalysisComponents);
                if (result > 0)
                {
                    return result; // 策略命中，直接返回
                }
            }

            // 2. 只有当没有特殊策略命中时，使用保底策略
            return _defaultStrategy.GetDropElementId(col, row, context, dropAnalysisComponents);
        }
    }
}