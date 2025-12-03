using System.Collections.Generic;
using GameConfig;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 消除匹配的服务接口
    /// </summary>
    public interface IMatchService
    {
        /// <summary>
        /// 消除服务类型
        /// </summary>
        MatchServiceType MatchServiceType { get; }
        
        /// <summary>
        /// 特殊元素ID数组
        /// </summary>
        int[] SpecialElements { get; }

        /// <summary>
        /// 判断某个配置ID的元素是否会被视为“阻挡物”
        /// 如果返回 true，则该位置不能再生成基础棋子
        /// </summary>
        bool IsBlockingBaseElement(int elementId);

        /// <summary>
        /// 是否为特殊元素
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        bool IsSpecialElement(int elementId);

        /// <summary>
        /// 判断两个元素是否可以相连
        /// </summary>
        /// <returns></returns>
        bool CanConnect(EcsWorld world, in ElementComponent fromComponent, in ElementComponent toComponent,
            int matchConfigId, int fromEntity, int toEntity, List<int> currentSelectedEntities);

        /// <summary>
        /// 元素类型转配置ID
        /// </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        int ElementType2ConfigId(ElementType elementType);

        /// <summary>
        /// 获取元素入场策略
        /// </summary>
        /// <returns></returns>
        IBoardSpawnStrategy GetSpawnStrategy();

        /// <summary>
        /// 检查单个元素是否会触发消除请求
        /// </summary>
        /// <param name="world"></param>
        /// <param name="elementEntity"></param>
        void CheckOneElementRequest(EcsWorld world, int elementEntity);

        /// <summary>
        /// 形成闭环后的生成规则
        /// </summary>
        /// <returns></returns>
        List<AtomicAction> MatchRuleAction(MatchRuleContext context, List<Vector2Int> closedLoopPaths);

        /// <summary>
        /// 获取炸弹的爆炸范围
        /// </summary>
        /// <param name="bombPos"></param>
        /// <returns></returns>
        List<Vector2Int> GetBombPos(Vector2Int bombPos);
        
        /// <summary>
        /// 根据连线的棋子获取消除规则,然后执行 Evaluate 函数
        /// </summary>
        /// <returns></returns>
        IMatchRule GetMatchRule(EcsWorld world, List<int> selectEntities);
        
        /// <summary>
        /// 判断是否形成几何上的方格闭环
        /// </summary>
        /// <param name="currentPathGridIds">当前已选中的格子路径</param>
        /// <param name="nextGridId">即将连接的格子</param>
        /// <returns>是否构成闭环</returns>
        bool IsGeometricSquare(List<int> currentPathGridIds, int nextGridId);

        /// <summary>
        /// 判断是否满足触发方格效果的数量条件
        /// </summary>
        /// <param name="connectCount">当前连线数量</param>
        /// <returns>是否满足</returns>
        bool IsCountSquare(int connectCount);

        /// <summary>
        /// 随机获取功能棋子Id
        /// </summary>
        /// <returns></returns>
        int RandomFunctionElement();
    }
}