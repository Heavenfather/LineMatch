using System;
using System.Collections.Generic;
using System.Reflection;
using GameCore.Log;
using HotfixCore.Utils;

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
    }
}