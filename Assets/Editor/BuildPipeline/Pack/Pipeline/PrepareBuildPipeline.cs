using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using GameCore.Logic;
using GameCore.Settings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YooAsset.Editor;

namespace GameEditor.BuildPipeline
{
    public class PrepareBuildPipeline : IBuildPipeline
    {
        private BuildContext _context;
        
        public EPipeLine PipeLine => EPipeLine.PrepareBuild;

        public void Execute(BuildContext context)
        {
            _context = context;

            if (context.IsApp && context.AppMode == EAppMode.Release)
            {
                // DeleteGmReference();
            }

            // DeleteDirectory();
            ExportExcel();
            DisableBundle();
            ModifyProjectSettings();
        }

        private void ModifyProjectSettings()
        {
            GlobalSettings settings = AssetDatabase.LoadAssetAtPath<GlobalSettings>("Assets/Resources/Config/GlobalSettings.asset");
            if (settings == null)
            {
                Debug.LogWarning($"无法解析项目配置文件 {nameof(GlobalSettings)}");
                return;
            }

            PlayerSettings.companyName = string.IsNullOrEmpty(_context.CompanyName) ? "LYStudio" : _context.CompanyName;
            if (_context.IsApp)
            {
                PlayerSettings.bundleVersion = _context.Version;
            }

            if (!string.IsNullOrEmpty(_context.GameAddress))
            {
                settings.GameAddress = _context.GameAddress;
            }
            settings.AppMode = _context.AppMode;
            if (!string.IsNullOrEmpty(_context.MainCDN))
            {
                settings.CDN = _context.MainCDN;
            }

            if (!string.IsNullOrEmpty(_context.FallbackCDN))
            {
                settings.FallBackCDN = _context.FallbackCDN;
            }

            //覆盖
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void DeleteGmReference()
        {
            EditorSceneManager.OpenScene("Resources/Scenes/GameEnter.unity");
            var debugConsole = GameObject.Find("IngameDebugConsole");
            if (debugConsole)
            {
                GameObject.DestroyImmediate(debugConsole);
                string consolePath = $"{Application.dataPath}/Plugins/IngameDebugConsole";
                if (Directory.Exists(consolePath))
                {
                    Directory.Delete(consolePath, true);
                }
            }
            
            var inspector = GameObject.Find("UIRoot/UICanvas/RuntimeInspector");
            if (inspector)
            {
                GameObject.DestroyImmediate(inspector);
                string inspectorPath = $"{Application.dataPath}/Plugins/RuntimeInspector";
                if (Directory.Exists(inspectorPath))
                    Directory.Delete(inspectorPath, true);
            }

            var hierarchy = GameObject.Find("UIRoot/UICanvas/RuntimeHierarchy");
            if (hierarchy)
            {
                GameObject.DestroyImmediate(hierarchy);
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        private void DeleteDirectory()
        {
            //删除掉MatchEditor的代码
            string resPath = $"{Application.dataPath}/ArtLoad/MatchEditor";
            if(Directory.Exists(resPath))
                Directory.Delete(resPath, true);
            string scriptPath = $"{Application.dataPath}/Scripts/Hotfix/MatchEditor";
            if(Directory.Exists(scriptPath))
                Directory.Delete(scriptPath, true);
            string uiPath = $"{Application.dataPath}/ArtLoad/UI/Prefab/MatchEditor";
            if(Directory.Exists(uiPath))
                Directory.Delete(uiPath, true);
            
            //删除OdinDemo
            string odin = $"{Application.dataPath}/Plugins/Sirenix/Demos";
            if(Directory.Exists(odin))
                Directory.Delete(odin, true);
        }

        private void DisableBundle()
        {
            var bundlePackage = AssetBundleCollectorSettingData.Setting.GetPackage("AssetMain");
            if (bundlePackage != null)
            {
                List<string> disableGroups = new List<string>() { "MatchEditor" };
                var groups = bundlePackage.Groups;
                for (int i = 0; i < groups.Count; i++)
                {
                    if (disableGroups.Contains(groups[i].GroupName))
                    {
                        groups[i].ActiveRuleName = nameof(DisableGroup);
                        AssetBundleCollectorSettingData.ModifyGroup(bundlePackage, groups[i]);
                        AssetBundleCollectorSettingData.SaveFile();
                        break;
                    }
                }
            }
        }

        private void ExportExcel()
        {
            //导一次客户端表数据
            string excelTool = "ExportExcel_Client.cmd";
            string commandPath = $"{Application.dataPath}/../Design/Tools/{excelTool}";
            if(!File.Exists(commandPath))
                return;
            BuildUtils.LaunchProcess(commandPath, "");
            AssetDatabase.Refresh();
        }
    }
}