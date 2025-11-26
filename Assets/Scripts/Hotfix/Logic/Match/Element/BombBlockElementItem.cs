
namespace HotfixLogic.Match
{
    public class BombBlockElementItem : DropBlockElementItem
    {
        protected override bool OnDestroy(ElementDestroyContext context)
        {
            // PlayBombEffect(context).Forget();
            MatchManager.Instance.PlayBombAudio();
            PlayEffect();
            return true;
        }
    }
}