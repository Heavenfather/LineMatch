using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 消除服务工厂接口
    /// </summary>
    public interface IMatchServiceFactory
    {
        /// <summary>
        /// 获取消除服务实例
        /// </summary>
        /// <param name="serviceType">消除服务类型</param>
        /// <returns></returns>
        IMatchService GetService(MatchServiceType serviceType);

        /// <summary>
        /// 获取元素工厂服务实例
        /// </summary>
        /// <returns></returns>
        IElementFactoryService GetElementFactoryService();

        /// <summary>
        /// 获取匹配规则实例
        /// </summary>
        /// <param name="ruleType"></param>
        /// <returns></returns>
        IMatchRule GetMatchRule(MatchRequestType ruleType);

        /// <summary>
        /// 创建原子级别的消除Action
        /// </summary>
        /// <param name="type">类型 必传</param>
        /// <param name="gridPos">作用到的格子坐标 如果Action是Damage就必须要传</param>
        /// <param name="value">作用参数 可选</param>
        /// <param name="targetEntity">作用到的对象 可选为指定的对象</param>
        /// <param name="extraData">额外参数 可选</param>
        /// <returns></returns>
        AtomicAction CreateAtomicAction(MatchActionType type, Vector2Int gridPos = default, int value = 0,
            int targetEntity = -1, object extraData = null);
    }
}