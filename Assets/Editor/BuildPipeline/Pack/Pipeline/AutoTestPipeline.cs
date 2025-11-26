using System;

namespace GameEditor.BuildPipeline
{
    public class AutoTestPipeline : IBuildPipeline
    {
        public EPipeLine PipeLine => EPipeLine.AutoTest;

        public void Execute(BuildContext context)
        {
            

        }

    }
}