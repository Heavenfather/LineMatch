using System.Collections.Generic;
using Hotfix.Tools.Random;

namespace Hotfix.Logic.GamePlay
{
    public class TargetSafetyStrategy : IDropStrategy
    {
        private const int SAFETY_SPAWN_RATE = 5000;

        public int GetDropElementId(int col, int row, GameStateContext context,
            List<DropAnalysisComponent> dropAnalysisComponents)
        {
            var matchState = context.MatchStateContext;
            List<int> banList = null;
            if (dropAnalysisComponents != null && dropAnalysisComponents.Count > 0)
            {
                banList = new List<int>(dropAnalysisComponents.Count);
                foreach (var component in dropAnalysisComponents)
                {
                    if(!banList.Contains(component.BanDropElementId))
                        banList.Add(component.BanDropElementId);
                }
            }

            // 1. 遍历当前所有还有缺口的目标
            foreach (var kvp in matchState.GlobalDropQuotas)
            {
                int targetId = kvp.Key;
                int remainingNeed = kvp.Value;

                if (remainingNeed <= 0) continue; // 这个目标已经掉够了，不需要兜底

                // 2. 检查黑名单 (BanList)
                if (banList != null && banList.Contains(targetId)) continue;

                // 3. 检查当前列是否是"合法产出列"
                if (matchState.TargetValidColumns.TryGetValue(targetId, out var validCols))
                {
                    if (validCols.Contains(col))
                    {
                        // 4. 触发概率
                        if (RandomTools.RandomRange(0, 10000) < SAFETY_SPAWN_RATE) // 给个固定的随机策略，为了不要一直都是掉落同一个目标看起来那么呆
                        {
                            return targetId;
                        }
                    }
                }
                else
                {
                    // 极端情况：棋盘上初始没有这个目标，且 DropFlag 也没配。
                    // 这种情况下 validCols 为空。这里也不掉，强迫策划去配 DropFlag。
                }
            }

            return 0; // 没触发兜底，交给保底随机
        }
    }
}