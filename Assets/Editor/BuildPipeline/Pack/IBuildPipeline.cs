namespace GameEditor.BuildPipeline
{
    public interface IBuildPipeline
    {
        EPipeLine PipeLine { get; }
        
        void Execute(BuildContext context);
    }
}