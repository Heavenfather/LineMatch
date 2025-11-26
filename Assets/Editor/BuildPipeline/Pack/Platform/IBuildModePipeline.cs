using UnityEditor;
using UnityEngine;

namespace GameEditor.BuildPipeline
{
    public interface IBuildModePipeline
    {
        void Run(BuildContext context);
    }

    public class BuildModePipelineBase : IBuildModePipeline
    {
        private readonly IBuildPlatform[] _buildPlatforms =
        {
            new BuildPlatformAndroid(),
            new BuildPlatformWebGL(),
            new BuildPlatformWin(),
        };

        public virtual void Run(BuildContext context)
        {
            var buildPlatform = GetBuildPlatform(context.Target);
            context.BuildOutputPath = $"{Application.dataPath}/../";
            buildPlatform.BeforeBuild(context);
            buildPlatform.Build(context);
            buildPlatform.AfterBuild(context);
        }

        private IBuildPlatform GetBuildPlatform(BuildTarget target)
        {
            if (target == BuildTarget.Android)
                return _buildPlatforms[0];
            if (target == BuildTarget.WebGL)
                return _buildPlatforms[1];

            return _buildPlatforms[2];
        }
    }
}