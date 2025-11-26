namespace GameEditor.BuildPipeline
{
    public class APP_SignPipeline : IBuildPipeline
    {
        public EPipeLine PipeLine => EPipeLine.APP_Sign;

        public void Execute(BuildContext context)
        {
        }
    }
}