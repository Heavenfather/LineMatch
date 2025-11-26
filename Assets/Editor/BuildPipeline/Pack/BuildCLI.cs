using System;
using System.Linq;
using GameCore.Logic;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

namespace GameEditor.BuildPipeline
{
    public static class BuildCLI
    {
        public static void StartBuild(EPipeLine pipeLines, BuildContext buildContext)
        {
            var result = BuildUtils.ExecuteFlow(pipeLines, buildContext);
            if (result.IsSuccess)
            {
                Debug.Log($"构建成功！总耗时: {result.ExecutionTimes.Values.Sum(t => t.TotalSeconds):F2}s");
                foreach (var record in result.ExecutionTimes)
                {
                    Debug.Log($"- {record.Key}: {record.Value.TotalMilliseconds}ms");
                }
            }
        }

        #region Jekins

        public static void Build_Win_Res_Debug()
        {
            BuildSetting setting =
                AssetDatabase.LoadAssetAtPath<BuildSetting>("Assets/Editor/BuildPipeline/Pack/Win_Res_Debug.asset");
            BuildContext context = new BuildContext(true, setting);
            context.BuildinFileCopyOption = EBuildinFileCopyOption.None;
            string version = Environment.GetEnvironmentVariable("BUILD_VERSION_NUMBER");
            if (!string.IsNullOrEmpty(version) && !version.Equals("now"))
            {
                context.Version = version;
            }
            BuildUtils.ExecuteFlow(setting.pipeLine, context);
        }
        
        public static void Build_Win_App_Debug()
        {
            //构建App需要构建一次资源
            BuildSetting resSetting =
                AssetDatabase.LoadAssetAtPath<BuildSetting>("Assets/Editor/BuildPipeline/Pack/Win_Res_Debug.asset");
            BuildContext resContext = new BuildContext(true, resSetting);
            resContext.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;
            BuildUtils.ExecuteFlow(resSetting.pipeLine, resContext);
            
            BuildSetting setting =
                AssetDatabase.LoadAssetAtPath<BuildSetting>("Assets/Editor/BuildPipeline/Pack/Win_App_Debug.asset");
            BuildContext context = new BuildContext(true, setting);
            string version = Environment.GetEnvironmentVariable("BUILD_VERSION_NUMBER");
            if (!string.IsNullOrEmpty(version) && !version.Equals("now"))
            {
                context.Version = version;
            }
            BuildUtils.ExecuteFlow(setting.pipeLine, context);
        }

        public static void Build_WebGL_Res_Debug()
        {
            BuildSetting setting =
                AssetDatabase.LoadAssetAtPath<BuildSetting>("Assets/Editor/BuildPipeline/Pack/WebGL_Res_Debug.asset");
            BuildContext context = new BuildContext(true, setting);
            context.BuildinFileCopyOption = EBuildinFileCopyOption.None;
            string version = Environment.GetEnvironmentVariable("BUILD_VERSION_NUMBER");
            if (!string.IsNullOrEmpty(version) && !version.Equals("now"))
            {
                context.Version = version;
            }
            BuildUtils.ExecuteFlow(setting.pipeLine, context);
        }
        
        public static void Build_WebGL_App_Debug()
        {
            //构建App需要构建一次资源 需要写入预加载的bundle
            BuildSetting resSettings =
                AssetDatabase.LoadAssetAtPath<BuildSetting>("Assets/Editor/BuildPipeline/Pack/WebGL_Res_Debug.asset");
            BuildContext resContext = new BuildContext(true, resSettings);
            resContext.BuildinFileCopyOption = EBuildinFileCopyOption.None;
            BuildUtils.ExecuteFlow(resSettings.pipeLine, resContext);

            string tag = Environment.GetEnvironmentVariable("BUILD_TAG");
            if (!string.IsNullOrEmpty(tag))
            {
                BuildSetting setting =
                    AssetDatabase.LoadAssetAtPath<BuildSetting>($"Assets/Editor/BuildPipeline/Pack/{tag}_App_Debug.asset");
                BuildContext context = new BuildContext(true, setting);
                string version = Environment.GetEnvironmentVariable("BUILD_VERSION_NUMBER");
                if (!string.IsNullOrEmpty(version) && !version.Equals("now"))
                {
                    context.Version = version;
                }
                BuildUtils.ExecuteFlow(setting.pipeLine, context);
            }
        }
        
        public static void Build_WebGL_Res_PreRelease()
        {
            BuildSetting setting =
                AssetDatabase.LoadAssetAtPath<BuildSetting>("Assets/Editor/BuildPipeline/Pack/WebGL_Res_PreRelease.asset");
            BuildContext context = new BuildContext(true, setting);
            context.BuildinFileCopyOption = EBuildinFileCopyOption.None;
            string version = Environment.GetEnvironmentVariable("BUILD_VERSION_NUMBER");
            if (!string.IsNullOrEmpty(version) && !version.Equals("now"))
            {
                context.Version = version;
            }
            BuildUtils.ExecuteFlow(setting.pipeLine, context);
        }
        
        public static void Build_WebGL_App_PreRelease()
        {
            //构建App需要构建一次资源 需要写入预加载的bundle
            BuildSetting resSettings =
                AssetDatabase.LoadAssetAtPath<BuildSetting>("Assets/Editor/BuildPipeline/Pack/WebGL_Res_PreRelease.asset");
            BuildContext resContext = new BuildContext(true, resSettings);
            resContext.BuildinFileCopyOption = EBuildinFileCopyOption.None;
            BuildUtils.ExecuteFlow(resSettings.pipeLine, resContext);
            
            string tag = Environment.GetEnvironmentVariable("BUILD_TAG");
            if (!string.IsNullOrEmpty(tag))
            {
                BuildSetting setting =
                    AssetDatabase.LoadAssetAtPath<BuildSetting>($"Assets/Editor/BuildPipeline/Pack/{tag}_App_PreRelease.asset");
                BuildContext context = new BuildContext(true, setting);
                string version = Environment.GetEnvironmentVariable("BUILD_VERSION_NUMBER");
                if (!string.IsNullOrEmpty(version) && !version.Equals("now"))
                {
                    context.Version = version;
                }
                BuildUtils.ExecuteFlow(setting.pipeLine, context);
            }
        }
        
        public static void Build_WebGL_Res_Release()
        {
            BuildSetting setting = AssetDatabase.LoadAssetAtPath<BuildSetting>("Assets/Editor/BuildPipeline/Pack/WebGL_Res_Release.asset");
            BuildContext context = new BuildContext(true, setting);
            context.BuildinFileCopyOption = EBuildinFileCopyOption.None;
            string version = Environment.GetEnvironmentVariable("BUILD_VERSION_NUMBER");
            if (!string.IsNullOrEmpty(version) && !version.Equals("now"))
            {
                context.Version = version;
            }
            BuildUtils.ExecuteFlow(setting.pipeLine, context);
        }
        
        public static void Build_WebGL_App_Release()
        {
            //构建App需要构建一次资源 需要写入预加载的bundle
            BuildSetting resSettings =
                AssetDatabase.LoadAssetAtPath<BuildSetting>("Assets/Editor/BuildPipeline/Pack/WebGL_Res_Release.asset");
            BuildContext resContext = new BuildContext(true, resSettings);
            resContext.BuildinFileCopyOption = EBuildinFileCopyOption.None;
            BuildUtils.ExecuteFlow(resSettings.pipeLine, resContext);
            
            string tag = Environment.GetEnvironmentVariable("BUILD_TAG");
            if (!string.IsNullOrEmpty(tag))
            {
                BuildSetting setting =
                    AssetDatabase.LoadAssetAtPath<BuildSetting>($"Assets/Editor/BuildPipeline/Pack/{tag}_App_Release.asset");
                BuildContext context = new BuildContext(true, setting);
                string version = Environment.GetEnvironmentVariable("BUILD_VERSION_NUMBER");
                if (!string.IsNullOrEmpty(version) && !version.Equals("now"))
                {
                    context.Version = version;
                }
                BuildUtils.ExecuteFlow(setting.pipeLine, context);
            }
        }
        #endregion
    }
}