namespace GameEditor.BuildPipeline
{
    public interface IBuildPlatform
    {
        void BeforeBuild(BuildContext context);

        void Build(BuildContext context);
        
        void AfterBuild(BuildContext context);
    }
}