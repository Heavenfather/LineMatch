using YooAsset.Editor;

namespace GameEditor.BuildPipeline
{
    public class CustomScriptableBuildParameters : ScriptableBuildParameters
    {
        private string _packageOutputDirectory = string.Empty;
        private string _packageRootDirectory = string.Empty;
        private string _pipelineOutputDirectory = string.Empty;

        public override string GetPackageOutputDirectory()
        {
            if (string.IsNullOrEmpty(_packageOutputDirectory))
            {
                _packageOutputDirectory =
                    $"{BuildOutputRoot}/{BuildUtils.GetTransitionPlatformName(BuildTarget)}/{PackageName}/{PackageVersion}";
            }

            return _packageOutputDirectory;
        }

        public override string GetPackageRootDirectory()
        {
            if (string.IsNullOrEmpty(_packageRootDirectory))
            {
                _packageRootDirectory = $"{BuildOutputRoot}/{BuildUtils.GetTransitionPlatformName(BuildTarget)}/{PackageName}";
            }
            return _packageRootDirectory;
        }

        public override string GetPipelineOutputDirectory()
        {
            if (string.IsNullOrEmpty(_pipelineOutputDirectory))
            {
                _pipelineOutputDirectory = $"{BuildOutputRoot}/{BuildUtils.GetTransitionPlatformName(BuildTarget)}/{PackageName}/OutputCache";
            }
            return _pipelineOutputDirectory;
        }
    }
}