using System.Collections.Generic;
using GameConfig;
using GameCore.Log;
using Hotfix.Tools.Random;
using HotfixLogic.Match;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 配置掉落策略：处理 LevelData 中的 DropFlag 配置
    /// 用于生成目标棋子、障碍物等指定列的特殊掉落
    /// </summary>
    public class DropFlagStrategy : IDropStrategy
    {
        public int GetDropElementId(int col, int row, GameStateContext context,
            List<DropAnalysisComponent> dropAnalysisComponents)
        {
            // 1. 获取关卡掉落配置
            LevelData levelData = context.CurrentLevel;
            if (levelData == null || levelData.dropFlags == null || levelData.dropFlags.Count == 0)
            {
                return 0; // 没有配置，跳过
            }

            // 2. 查找当前列是否有特殊配置
            DropFlag targetFlag = null;
            for (int i = 0; i < levelData.dropFlags.Count; i++)
            {
                if (levelData.dropFlags[i].dropX == col)
                {
                    targetFlag = levelData.dropFlags[i];
                    break;
                }
            }

            if (targetFlag == null || targetFlag.dropElements == null) return 0;
            
            // 3. 遍历该列配置的所有特殊掉落物
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            foreach (var dropItem in targetFlag.dropElements)
            {
                int configId = dropItem.elementId;
                if (!db[configId].isMovable)
                {
                    Logger.Error($"{levelData.id} 关卡,第 {col} 列，掉落物 {configId} 为不可移动类型，不允许配置在掉落里");
                    continue;
                }
                
                // 如果配置了上限 (dropLimitMax > 0)，且已达到上限，则跳过
                if (dropItem.dropLimitMax > 0)
                {
                    int currentCount = context.MatchStateContext.GetDropCount(configId);
                    if (currentCount >= dropItem.dropLimitMax)
                    {
                        continue;
                    }
                }
                
                if (context.MatchStateContext.GlobalDropQuotas.TryGetValue(configId, out int quota))
                {
                    if (quota <= 0) continue;
                }
                else
                {
                    bool quotaExhausted = false;
                    foreach (var kvp in context.MatchStateContext.GlobalDropQuotas)
                    {
                        int targetId = kvp.Key;
                        int left = kvp.Value;
                        
                        if (left <= 0 && MatchElementUtil.IsContributingToTarget(targetId, configId))
                        {
                            quotaExhausted = true;
                            break;
                        }
                    }
                    if (quotaExhausted) continue;
                }

                // dropRate 是万分比
                if (RandomTools.RandomRange(0, 10000) < dropItem.dropRate)
                {
                    return configId;
                }
            }

            return 0;
        }
    }
}