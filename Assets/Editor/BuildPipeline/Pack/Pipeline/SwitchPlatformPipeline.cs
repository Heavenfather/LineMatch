using System;
using UnityEditor;
using UnityEngine;

namespace GameEditor.BuildPipeline
{
    public class SwitchPlatformPipeline : IBuildPipeline
    {
        public EPipeLine PipeLine => EPipeLine.SwitchPlatform;
        
        public void Execute(BuildContext context)
        {
            BuildTarget buildTarget = context.Target;
            context.TargetGroup = GetBuildTargetGroup(buildTarget);
            SwitchPlatform(buildTarget);
            
            context.OriginalSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(context.TargetGroup);
        }

        private void SwitchPlatform(BuildTarget target)
        {
            BuildTarget currentTarget = EditorUserBuildSettings.activeBuildTarget;
            if (currentTarget != target)
            {
                Debug.Log($"Switching platform {currentTarget} to {target}");
                EditorUserBuildSettings.SwitchActiveBuildTarget(GetBuildTargetGroup(target), target);
            }
        }

        private BuildTargetGroup GetBuildTargetGroup(BuildTarget buildTarget)
        {
            BuildTargetGroup buildTargetGroup;
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    buildTargetGroup = BuildTargetGroup.Standalone;
                    break;
                case BuildTarget.iOS:
                    buildTargetGroup = BuildTargetGroup.iOS;
                    break;
                case BuildTarget.Android:
                    buildTargetGroup = BuildTargetGroup.Android;
                    break;
                case BuildTarget.WebGL:
                    buildTargetGroup = BuildTargetGroup.WebGL;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildTarget), buildTarget, null);
            }
            return buildTargetGroup;
        }
    }
}