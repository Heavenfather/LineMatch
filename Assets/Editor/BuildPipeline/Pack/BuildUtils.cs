using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GameCore.Logic;
using GameCore.Resource;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;
using Debug = UnityEngine.Debug;

namespace GameEditor.BuildPipeline
{
    public static class BuildUtils
    {
        private static readonly Dictionary<EPipeLine, TimeSpan> _executionRecords = new();
        private static readonly SortedDictionary<int, IBuildPipeline> _pipelines = new();

        [InitializeOnLoadMethod]
        public static void RegisterPipeLines()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("GameEditor"));
            foreach (var assembly in assemblies)
            {
                var pipelines = assembly.GetTypes().Where(t =>
                    typeof(IBuildPipeline).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var type in pipelines)
                {
                    try
                    {
                        var instance = Activator.CreateInstance(type) as IBuildPipeline;
                        if (instance != null && !_pipelines.ContainsKey(GetPipelineOrderKey(instance.PipeLine)))
                        {
                            RegisterPipeLine(instance);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"构建管道 {type.FullName} 实例化失败:{e.Message}");
                        throw;
                    }
                }
            }
        }

        public static void RegisterPipeLine(IBuildPipeline pipeline)
        {
            var oderKey = GetPipelineOrderKey(pipeline.PipeLine);
            if (!_pipelines.TryAdd(oderKey, pipeline))
            {
                Debug.LogError($"重复注册的构建管道:{pipeline.PipeLine}");
            }
        }

        public static PipelineExecutionResult ExecuteFlow(EPipeLine executePipeline, BuildContext context)
        {
            RegisterPipeLines();
            _executionRecords.Clear();
            Debug.Log($"Build Context : {context}");
            var result = new PipelineExecutionResult();
            var stopwatch = new System.Diagnostics.Stopwatch();
            try
            {
                foreach (var pipeline in _pipelines.Values)
                {
                    if (executePipeline.HasFlag(pipeline.PipeLine))
                    {
                        stopwatch.Restart();
                        Debug.Log($"[BuildFlow] 执行阶段 : {pipeline.PipeLine}");
                        var (success, error) = ExecutePipeline(pipeline, context);
                        stopwatch.Stop();

                        RecordExecution(pipeline.PipeLine, stopwatch.Elapsed, success);

                        if (!success)
                        {
                            result.IsSuccess = false;
                            result.FailedPipeline = pipeline.PipeLine;
                            result.Error = error;
                            break;
                        }

                        result.IsSuccess = true;
                    }
                }
            }
            finally
            {
                result.ExecutionTimes = new Dictionary<EPipeLine, TimeSpan>(_executionRecords);
                if (!result.IsSuccess)
                {
                    Debug.LogError($"构建失败于 {result.FailedPipeline}\n" +
                                   $"错误信息: {result.Error.Message}");
                    if (context.IsCLI)
                    {
                        EditorApplication.Exit(1);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取App输出路径
        /// </summary>
        /// <param name="target"></param>
        /// <param name="mode"></param>
        /// <param name="deleteDir"></param>
        /// <returns></returns>
        public static string GetAppOutputPath(BuildTarget target,EAppMode mode,bool deleteDir = true)
        {
            string outputPath = Path.Combine(Application.dataPath, $"../App/{mode.ToString()}/{GetTransitionPlatformName(target)}/");
            if (deleteDir)
            {
                if (Directory.Exists(outputPath))
                    Directory.Delete(outputPath, true);
                Directory.CreateDirectory(outputPath);
            }

            return outputPath;
        }

        /// <summary>
        /// 转换目标平台成游戏适用的名称
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static string GetTransitionPlatformName(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "win";
                case BuildTarget.iOS:
                    return "ios";
                case BuildTarget.Android:
                    return "android";
                case BuildTarget.WebGL:
                    return "webgl";
                default:
                    throw new NotSupportedException($"Platform '{Application.platform.ToString()}' is not supported.");
            }
        }

        /// <summary>
        /// 构建Bundle
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="context"></param>
        public static void BuildAssetBundle(string packageName, BuildContext context)
        {
            CustomScriptableBuildParameters buildParameters = new CustomScriptableBuildParameters();
            buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            if (Directory.Exists(buildParameters.BuildOutputRoot))
                Directory.Delete(buildParameters.BuildOutputRoot, true);
            buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
            buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
            buildParameters.BuildTarget = context.Target;
            buildParameters.PackageName = packageName;
            string packageVersion = "";
            if (!string.IsNullOrEmpty(context.Version))
            {
                packageVersion = context.Version;
            }
            else
            {
                DateTime dateTime = DateTime.Now;
                packageVersion = ($"{dateTime.Month}{dateTime.Day}" +
                                  $"{dateTime.Hour:D2}{dateTime.Minute:D2}");
                context.Version = packageVersion;
            }

            buildParameters.PackageVersion = packageVersion;
            buildParameters.EnableSharePackRule = true;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.BuildinFileCopyOption = context.BuildinFileCopyOption;
            buildParameters.BuildinFileCopyParams =
                AssetBundleBuilderSetting.GetPackageBuildinFileCopyParams(packageName,
                    EBuildPipeline.ScriptableBuildPipeline);
            buildParameters.CompressOption =
                AssetBundleBuilderSetting.GetPackageCompressOption(packageName,
                    EBuildPipeline.ScriptableBuildPipeline);
            buildParameters.ClearBuildCacheFiles =
                AssetBundleBuilderSetting.GetPackageClearBuildCache(packageName,
                    EBuildPipeline.ScriptableBuildPipeline);
            buildParameters.UseAssetDependencyDB =
                AssetBundleBuilderSetting.GetPackageUseAssetDependencyDB(packageName,
                    EBuildPipeline.ScriptableBuildPipeline);
            buildParameters.BuiltinShadersBundleName = GetBuiltinShaderBundleName(packageName);
            if (context.Target != BuildTarget.WebGL)
                buildParameters.EncryptionServices = new FileOffsetEncryption();
            buildParameters.WriteLinkXML = true;

            ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);
            if (buildResult.Success)
            {
                Debug.Log(
                    $"Build YooAsset Success,PackageName: {packageName} Version:{packageVersion} buildTarget:{context.Target}");
            }
        }

        /// <summary>
        /// 内置着色器资源包名称
        /// 注意：和自动收集的着色器资源包名保持一致！
        /// </summary>
        private static string GetBuiltinShaderBundleName(string packageName)
        {
            var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
            var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
            return packRuleResult.GetBundleName(packageName, uniqueBundleName);
        }

        public static void BuildWechatMiniGameQrCode(EAppMode mode)
        {
            string command = $"{Application.dataPath}/../Publish/tools/wechat_qr.cmd";
            string projectPath = GetAppOutputPath(BuildTarget.WebGL, mode, false);
            if (!Directory.Exists(projectPath))
                throw new IOException($"未找到项目路径:{projectPath}");
            string basePath = "D:/cdn/app/webgl";
            //输出登录二维码
            string loginQrPath = $"{basePath}/login.png";
            //输出预览二维码
            string qrPath = $"{PrepareQrPath(basePath, mode)}/qr_code.png";

            string args = $"{projectPath} {loginQrPath} {qrPath}";
            LaunchProcess(command, args);
        }
        
        /// <summary>
        /// 启动外部进程
        /// </summary>
        /// <param name="commandPath"></param>
        /// <param name="arguments"></param>
        /// <exception cref="Exception"></exception>
        public static void LaunchProcess(string commandPath, string arguments,bool throwError = true)
        {
            if (!File.Exists(commandPath))
            {
                Debug.LogError($"执行命令文件不存在:{commandPath}");
                return;
            }
            Debug.Log($"Launch Process: {commandPath}");
            // 配置进程启动参数
            var processInfo = new ProcessStartInfo
            {
                FileName = commandPath,
                Arguments = arguments,
                UseShellExecute = false, // 不使用系统shell
                RedirectStandardOutput = true, // 重定向输出
                RedirectStandardError = true, // 重定向错误
                CreateNoWindow = true, // 不创建窗口
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };

            // 启动进程
            using (var process = new Process { StartInfo = processInfo })
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        string log = $"[OUTPUT] {e.Data}";
                        Debug.Log(log);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        string error = $"[ERROR] {e.Data}";
                        Debug.LogError(error);
                    }
                };

                process.Start();

                // 开始异步读取输出
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // 等待进程结束
                process.WaitForExit();

                // 检查退出代码
                if (throwError && process.ExitCode != 0)
                {
                    throw new Exception($"launch  process fail,exit code: {process.ExitCode}");
                }

                Debug.Log("launch  process Success!!");
            }
        }
        private static (bool, Exception) ExecutePipeline(IBuildPipeline pipeline, BuildContext context)
        {
            try
            {
                pipeline.Execute(context);
                return (true, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"管道 {pipeline.PipeLine} 执行失败 : {e.Message}");
                return (false, e);
            }
        }

        private static void RecordExecution(EPipeLine pipeline, TimeSpan duration, bool success)
        {
            _executionRecords[pipeline] = duration;
            Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] {(success ? "✓" : "✗")} " +
                      $"{pipeline.ToString(),-20} {duration.TotalMilliseconds}ms");
        }

        private static int GetPipelineOrderKey(EPipeLine pipeline)
        {
            var values = Enum.GetValues(typeof(EPipeLine));
            for (int i = 0; i < values.Length; i++)
            {
                if ((EPipeLine)values.GetValue(i) == pipeline)
                    return i;
            }

            return int.MaxValue;
        }
        
        private static string PrepareQrPath(string basePath,EAppMode mode)
        {
            string resultPath = $"{basePath}/{mode.ToString()}";
            if(!Directory.Exists(resultPath))
                Directory.CreateDirectory(resultPath);
            return resultPath;
        }
    }
}