using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 掉落生成策略接口
    /// </summary>
    public interface IDropStrategy
    {
        /// <summary>
        /// 尝试计算掉落元素的配置ID
        /// </summary>
        /// <param name="col">列索引</param>
        /// <param name="row">行索引</param>
        /// <param name="context">游戏上下文</param>
        /// <param name="dropAnalysisComponents">掉落分析组件列表，用于传递给不同策略本次输入的信息</param>
        /// <returns>返回 > 0 表示策略生效并决定了ID；返回 0 表示跳过，交给下一个策略</returns>
        int GetDropElementId(int col, int row, GameStateContext context,List<DropAnalysisComponent> dropAnalysisComponents);
    }
}