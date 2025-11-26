using Cysharp.Threading.Tasks;

namespace HotfixLogic.Match
{
    public class SpreadFireElementItem : BlockElementItem
    {
        protected override bool OnDestroy(ElementDestroyContext context)
        {
            context.HasDestroyFireElement = true;
            bool result = base.OnDestroy(context);
            return result;
        }
    }
}