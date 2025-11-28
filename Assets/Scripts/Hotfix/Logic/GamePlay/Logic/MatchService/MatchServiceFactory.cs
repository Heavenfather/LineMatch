using System;
using System.Collections.Generic;
using System.Reflection;
using HotfixCore.Utils;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 消除服务工厂实现类
    /// </summary>
    public class MatchServiceFactory : IMatchServiceFactory
    {
        private Dictionary<MatchRequestType, IMatchRule> _matchRules = null;

        public IMatchService GetService(MatchServiceType serviceType)
        {
            if (serviceType == MatchServiceType.Normal)
            {
                return MatchBoot.Container.RegisterSingleton<IMatchService, NormalMatchService>();
            }

            if (serviceType == MatchServiceType.TowDots)
                return MatchBoot.Container.RegisterSingleton<IMatchService, TowDotsMatchService>();

            Logger.Error($"MatchServiceFactory: 未找到匹配服务类型 {serviceType}");
            return null;
        }

        public IElementFactoryService GetElementFactoryService()
        {
            return MatchBoot.Container.Resolve<IElementFactoryService>();
        }

        public IMatchRule GetMatchRule(MatchRequestType ruleType)
        {
            if (_matchRules == null)
            {
                _matchRules = new Dictionary<MatchRequestType, IMatchRule>(50);
                //---------- 反射添加不同消除规则 ----------------
                Assembly assembly = AssemblyUtil.GetAssembly("GameHotfix");
                if (assembly != null)
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.GetInterface("IMatchRule") != null)
                        {
                            var instance = (IMatchRule)Activator.CreateInstance(type);
                            _matchRules.TryAdd(instance.RuleKey, instance);
                        }
                    }
                }
                else
                {
                    Logger.Error("无法获取程序集:GameHotfix,Rule初始化失败!");
                }
            }

            if (_matchRules.TryGetValue(ruleType, out var rule))
                return rule;

            Logger.Error($"MatchServiceFactory: 未实现的匹配规则类型 {ruleType}");
            return null;
        }

        /// <summary>
        /// 创建原子级别的消除Action
        /// </summary>
        /// <param name="type">类型 必传</param>
        /// <param name="gridPos">作用到的格子坐标 必传</param>
        /// <param name="value">作用参数 必传</param>
        /// <param name="targetEntity">作用到的对象 可选为指定的对象</param>
        /// <param name="extraData">额外参数 可选</param>
        /// <returns></returns>
        public AtomicAction CreateAtomicAction(MatchActionType type, Vector2Int gridPos = default, int value = 0,
            int targetEntity = -1, object extraData = null)
        {
            return new AtomicAction
            {
                Type = type,
                TargetEntity = targetEntity,
                GridPos = gridPos,
                Value = value,
                ExtraData = extraData
            };
        }
    }
}