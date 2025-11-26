using HotfixCore.MemoryPool;

namespace Hotfix.Logic.GamePlay
{
    public class MatchRuleContext : IMemory
    {
        public EcsWorld World;
        public IBoard Board;
        public IMatchService MatchService;
        public MatchRequestComponent Request;
        
        public void Clear()
        {
            Request = default;
        }
    }
}