using UnityEditor;

namespace GameEditor.BuildPipeline
{
    public class ResumePipeline : IBuildPipeline
    {
        public EPipeLine PipeLine => EPipeLine.Resume;

        public void Execute(BuildContext context)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(context.TargetGroup, context.OriginalSymbols);
        }
    }
}