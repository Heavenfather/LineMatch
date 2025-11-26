namespace GameEditor.BuildPipeline
{
    public class ShutdownPipeline : IBuildPipeline
    {
        public EPipeLine PipeLine => EPipeLine.Shutdown;

        public void Execute(BuildContext context)
        {
        }
    }
}