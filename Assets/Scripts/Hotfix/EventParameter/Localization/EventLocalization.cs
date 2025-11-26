using HotfixCore.Module;

namespace Hotfix.EventParameter
{
    public struct EventLocalizationChanged : IEventParameter
    {
        public LanguageType Language;
    }
}