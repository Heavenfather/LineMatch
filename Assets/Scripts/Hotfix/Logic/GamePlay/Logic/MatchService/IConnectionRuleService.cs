using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 棋子是否能相连的规则服务
    /// </summary>
    public interface IConnectionRuleService
    {
        /// <summary>
        /// 判断两个实体是否可以连接
        /// </summary>
        bool CanConnect(EcsWorld world, int fromEntity, int toEntity, int matchConfigId, IMatchService matchService, List<int> currentSelectedEntities);

        /// <summary>
        /// 判断是否可以作为起手元素
        /// </summary>
        bool IsSelectable(EcsWorld world, int entity);
    }
}