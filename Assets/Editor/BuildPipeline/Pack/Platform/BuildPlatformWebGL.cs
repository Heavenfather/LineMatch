using System;
using System.IO;
using GameCore.Logic;
using GameCore.Settings;
using UnityEditor;
using UnityEngine;

namespace GameEditor.BuildPipeline
{
    public class BuildPlatformWebGL : IBuildPlatform
    {
        public void BeforeBuild(BuildContext context)
        {
            if (context.IsApp)
            {
                //构建App 提前修改微信设置文件
                WriteWechatMiniGameSetting(context);
            }
        }

        public void Build(BuildContext context)
        {
            if (context.IsApp)
            {
                if (context.WebGLTarget == EWebGLTarget.WechatMiniGame)
                {
                    if (context.IsOnlyQrCode)
                    {
                        Debug.Log("Only Export QrCode");
                        BuildUtils.BuildWechatMiniGameQrCode(context.AppMode);
                    }
                    else
                    {
                        Debug.Log("Starting Export MiniGame");
                        DoExportWxMiniGame();
                    }
                }
            }
            else
            {
                foreach (var packageName in context.PackageNames)
                {
                    BuildUtils.BuildAssetBundle(packageName, context);
                }

                if (context.WebGLTarget == EWebGLTarget.WechatMiniGame)
                {
                    //构建资源完成后写入预加载的bundle
                    WriteWechatMiniGameSetting(context);
                }
            }
        }

        public void AfterBuild(BuildContext context)
        {
            // if (context.IsApp && !context.IsOnlyQrCode)
            //     BuildUtils.BuildWechatMiniGameQrCode(context.AppMode);
        }

        private void DoExportWxMiniGame()
        {
            var result = WeChatWASM.WXEditorWin.DoExport();
            if (result == WeChatWASM.WXConvertCore.WXExportError.SUCCEED)
            {
                Debug.Log("微信微信小程序导出成功!");
            }
            else
            {
                Debug.LogError($"微信微信小程序导出失败 state : {result}");
            }
        }
        
        private void WriteWechatMiniGameSetting(BuildContext context)
        {
            var wxConfig = WeChatWASM.WXConvertCore.config;
            bool isDirty = false;
            
            string dest = GetWechatOutputPath(context.AppMode);
            if (dest != wxConfig.ProjectConf.DST)
            {
                wxConfig.ProjectConf.DST = dest;
                if (!Directory.Exists(dest))
                    Directory.CreateDirectory(dest);
                isDirty = true;
            }
            
            //使用正式的cdn
            string downloadCdn = GetWechatCdn(context.Target);
            if (!string.IsNullOrEmpty(downloadCdn))
            {
                string cdnBase = downloadCdn.Replace("/webgl", "");
                Debug.Log($"wechat minigame cdn base :{cdnBase}");
                if (cdnBase != wxConfig.ProjectConf.CDN)
                {
                    wxConfig.ProjectConf.CDN = cdnBase;
                    isDirty = true;
                }
            }
            GamePreloadEditor.BuildBundleMap(context.Version, context.Target);
            string preloadList = GamePreloadEditor.GetFormatPreloadList(downloadCdn);
            if (!string.IsNullOrEmpty(preloadList))
            {
                wxConfig.ProjectConf.preloadFiles = preloadList;
                isDirty = true;
            }


            if (isDirty)
            {
                EditorUtility.SetDirty(wxConfig);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private string GetWechatOutputPath(EAppMode mode)
        {
            return BuildUtils.GetAppOutputPath(BuildTarget.WebGL, mode);
        }

        private string GetWechatCdn(BuildTarget target)
        {
            GlobalSettings settings =
                AssetDatabase.LoadAssetAtPath<GlobalSettings>("Assets/Resources/Config/GlobalSettings.asset");
            if (settings == null)
                return string.Empty;
            string platformName = BuildUtils.GetTransitionPlatformName(target);
            return settings.GetCDNDownLoadPath(platformName);
        }
    }
}