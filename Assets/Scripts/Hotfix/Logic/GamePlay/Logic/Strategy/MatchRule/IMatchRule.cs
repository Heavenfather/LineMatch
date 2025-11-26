using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 消除的规则-策略模式
    /// 每个具体的消除都是一种策略，它只负责把请求翻译成指令
    /// </summary>
    public interface IMatchRule
    {
        // 这个规则处理什么情况
        MatchRequestType RuleKey { get; }

        // 计算指令：输入上下文，输出指令列表
        void Evaluate(MatchRuleContext ctx, ref List<AtomicAction> outActions);
    }
}