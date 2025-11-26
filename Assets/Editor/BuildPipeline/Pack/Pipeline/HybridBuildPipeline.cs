using System.Collections.Generic;
using System.IO;
using GameCore.FileHelper;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Installer;
using UnityEditor;
using UnityEngine;

namespace GameEditor.BuildPipeline
{
    public class HybridBuildPipeline : IBuildPipeline
    {
        private BuildContext _context;

        public EPipeLine PipeLine => EPipeLine.HybridBuild;

        public void Execute(BuildContext context)
        {
            _context = context;

            Install();

            if (context.IsApp)
            {
                GenerateAppMode();
            }
            else
            {
                GenerateResMode();
            }
        }

        private void Install()
        {
            var ic = new InstallerController();
            if (ic.HasInstalledHybridCLR())
            {
                Debug.Log("Hybrid CLR was ready.");
                return;
            }

            Debug.Log("Start install hybrid clr.");
            ic.InstallDefaultHybridCLR();
        }

        private void GenerateAppMode()
        {
            PrebuildCommand.GenerateAll();
            CopyMetaDll();
            AssetDatabase.Refresh();
        }

        private void GenerateResMode()
        {
            CompileDllCommand.CompileDll(_context.Target);
            CopyHotUpdateAssemblies();
            AssetDatabase.Refresh();
        }

        private void CopyMetaDll()
        {
            List<string> aotDlls = SettingsUtil.AOTAssemblyNames;
            Debug.Log($"HybridCLR copy aot meta : {string.Join(";", aotDlls)}");
            string dst = $"{Application.dataPath}/ArtLoad/Dll/MetaDll";
            string dir = Path.GetDirectoryName(dst);
            FileHelper.MkDir(dir);
            string source = SettingsUtil.GetAssembliesPostIl2CppStripDir(_context.Target);
            for (int i = 0; i < aotDlls.Count; i++)
            {
                if(!aotDlls[i].EndsWith(".dll"))
                    aotDlls[i] += ".dll";
                string sourcePath = $"{source}/{aotDlls[i]}";
                if (!File.Exists(sourcePath))
                {
                    Debug.LogError($"Fail:ab中添加AOT补充元数据dll:{sourcePath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                    EditorApplication.Exit(1);
                    break;
                }
                string dllBytesPath = $"{dst}/{aotDlls[i]}.bytes";
                Debug.Log($"Copying from {sourcePath} to {dllBytesPath}");
                File.Copy(sourcePath, dllBytesPath, true);
            }
        }
        
        
        private void CopyHotUpdateAssemblies()
        {
            var target = _context.Target;
            string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            string hotfixAssembliesDstDir = $"{Application.dataPath}/ArtLoad/Dll/Hotfix";
            string dir = Path.GetDirectoryName(hotfixDllSrcDir);
            FileHelper.MkDir(dir);
            foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            {
                string dllPath = $"{hotfixDllSrcDir}/{dll}";
                string dllBytesPath = $"{hotfixAssembliesDstDir}/{dll}.bytes";
                File.Copy(dllPath, dllBytesPath, true);
                Debug.Log($"[CopyHotUpdateAssemblies] copy hotfix dll {dllPath} ----> {dllBytesPath}");
            }
        }
    }
}