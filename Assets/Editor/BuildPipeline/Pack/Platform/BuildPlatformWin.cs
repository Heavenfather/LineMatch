using System;
using System.IO;
using GameCore.Logic;
using UnityEditor;
using UnityEngine;

namespace GameEditor.BuildPipeline
{
    public class BuildPlatformWin : IBuildPlatform
    {
        public void BeforeBuild(BuildContext context)
        {
            
        }

        public void Build(BuildContext context)
        {
            if (context.IsApp)
            {
                BuildAppSetting(context);
                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.scenes = new string[] { "Assets/Resources/Scenes/GameEnter.unity" };
                var outputDir = BuildUtils.GetAppOutputPath(context.Target, context.AppMode);
                buildPlayerOptions.locationPathName = $"{outputDir}Manbanpai.exe";
                buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
                buildPlayerOptions.options = context.BuildOptions & ~BuildOptions.BuildScriptsOnly & BuildOptions.CompressWithLz4;
                var report = UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);
                var summary = report.summary;
                if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    Debug.Log("Build succeeded: " + summary.totalSize + " bytes" + " outputPath " + summary.outputPath);
                }
                else if (summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
                {
                    Debug.Log("Build failed");
                }
            }
            else
            {
                foreach (var packageName in context.PackageNames)
                {
                    BuildUtils.BuildAssetBundle(packageName, context);
                }
            }
        }

        public void AfterBuild(BuildContext context)
        {
            string compressDir = BuildUtils.GetAppOutputPath(context.Target, context.AppMode, false);
            if (context.IsApp)
            {
                if (!File.Exists($"{compressDir}Manbanpai.exe"))
                {
                    throw new FileNotFoundException($"未找到构建结果文件:{compressDir}Manbanpai.exe");
                }

                string outputDir = $"D:/cdn/app/win/win_{DateTime.Now:yyyyMMddHHmm}";
                string commandPath = Path.Combine(Application.dataPath, "../Publish/tools/win_app_compress.cmd");

                BuildUtils.LaunchProcess(commandPath, $"{outputDir} {compressDir}");
            }
        }

        private void BuildAppSetting(BuildContext context)
        {
            PlayerSettings.SetScriptingBackend(context.TargetGroup, ScriptingImplementation.IL2CPP);
            if (context.AppMode == EAppMode.Debug)
                PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
        }
    }
}