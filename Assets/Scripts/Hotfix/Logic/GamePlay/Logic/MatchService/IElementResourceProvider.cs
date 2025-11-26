using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 关卡元素资源提供器
    /// </summary>
    public interface IElementResourceProvider
    {
        /// <summary>
        /// 当前关卡需要加载的元素ID
        /// </summary>
        HashSet<int> CurrentLevelRequiredElementIds { get;}
        
        /// <summary>
        /// 分析关卡数据，计算出所有可能出现的元素ID
        /// </summary>
        HashSet<int> AnalyzeLevelRequiredElements(LevelData levelData, IMatchService matchService);

        /// <summary>
        /// 执行预加载
        /// </summary>
        UniTask PreloadResources(HashSet<int> requiredElementIds, Transform poolRoot);
    }
}