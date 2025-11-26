namespace GameEditor.BuildPipeline
{
    public class BuildPipeline : IBuildPipeline
    {
        public EPipeLine PipeLine => EPipeLine.Build;
        
        public void Execute(BuildContext context)
        {
            IBuildModePipeline mode;
            if (context.IsApp)
            {
                mode = new BuildModeApp();
            }
            else
            {
                mode = new BuildModeRes();
            }

            mode.Run(context);
        }
    }
}