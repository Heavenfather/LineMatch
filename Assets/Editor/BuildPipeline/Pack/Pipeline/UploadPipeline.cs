using System;
using System.Diagnostics;
using System.IO;
using GameCore.Logic;
using GameCore.Settings;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GameEditor.BuildPipeline
{
    public class UploadPipeline : IBuildPipeline
    {
        public EPipeLine PipeLine => EPipeLine.Upload;

        public void Execute(BuildContext context)
        {
            GlobalSettings settings =
                AssetDatabase.LoadAssetAtPath<GlobalSettings>("Assets/Resources/Config/GlobalSettings.asset");
            string projectName = settings.ProductName;
            string buildType = context.IsApp ? "app" : "res";
            string platform = BuildUtils.GetTransitionPlatformName(context.Target);
            string version = context.Version;
            string packPath = context.BuildOutputPath;
            if (platform == "win")
            {
                for (int i = 0; i < context.PackageNames.Length; i++)
                {
                    CopyWindowsBundle(platform, context.PackageNames[i], version);
                }
            }
            else
            {
                UploadCli(projectName, buildType, platform, version, context.AppMode, packPath,context.UploadVersion);
            }
        }

        private void UploadCli(string projectName, string buildType, string platform, string version, EAppMode mode,
            string packPath,bool uploadVersion, string resTag = "")
        {
            // 验证必须参数
            Debug.Log($"UploadPipeline: projectName:[{projectName}] buildType:[{buildType}] platform:[{platform}] version:[{version}] mode:[{mode}] packPath:[{packPath}] uploadVersion:[{uploadVersion}] resTag:[{resTag}]");
            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(buildType) ||
                string.IsNullOrEmpty(platform) ||
                string.IsNullOrEmpty(version) || string.IsNullOrEmpty(packPath))
            {
                Debug.LogError("UploadCli argument error");
                return;
            }

            string commandPath = Path.Combine(Application.dataPath, "../Publish/tools/JYUploader.exe");
            if (!File.Exists(commandPath))
            {
                Debug.LogError("UploadCli upload tools can not found.");
                return;
            }

            string arguments =
                $"\"{projectName}\" \"{buildType}\" \"{platform}\" \"{version}\" \"{mode.ToString()}\" \"{packPath}\" \"{uploadVersion}\"";
            if (!string.IsNullOrWhiteSpace(resTag))
            {
                arguments += $" \"{resTag}\"";
            }

            BuildUtils.LaunchProcess(commandPath, arguments);
        }

        private void CopyWindowsBundle(string platform, string packageName, string version)
        {
            string sourcePath = Path.Combine(Application.dataPath, $"../Bundles/{platform}/{packageName}/{version}/");
            string commandPath = Path.Combine(Application.dataPath, "../Publish/tools/win_copy.bat");
            string arguments = $"{sourcePath}";
            BuildUtils.LaunchProcess(commandPath, arguments);
        }
        
    }
}