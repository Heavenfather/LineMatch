using System;
using GameCore.Logic;
using UnityEditor;
using YooAsset.Editor;

namespace GameEditor.BuildPipeline
{
    public class BuildContext
    {
        public bool IsCLI;

        public bool IsApp;

        public bool IsOnlyQrCode;
        
        public BuildTarget Target;
        
        public BuildTargetGroup TargetGroup;
        
        public EWebGLTarget WebGLTarget;
        
        public EAppMode AppMode;
        
        public EBuildinFileCopyOption BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;

        public bool UploadVersion;
        
        public string Version;

        public string OriginalSymbols;

        public string MainCDN;

        public string FallbackCDN;

        public string GameAddress;
        
        public string CompanyName;

        public string BuildOutputPath;
        
        public string[] PackageNames;
        
        public string[] EnableSymbols;
        
        public string[] DisableSymbols;
        
        public BuildOptions BuildOptions;
        
        public BuildContext()
        {
            
        }
        
        public BuildContext(bool isCli,BuildSetting setting)
        {
            IsCLI = isCli;
            IsApp = setting.buildApp;
            Target = setting.buildTarget;
            WebGLTarget = setting.webGLTarget;
            AppMode = setting.buildType;
            PackageNames = setting.packageNames;
            EnableSymbols = setting.enableSymbols;
            DisableSymbols = setting.disableSymbols;
            BuildOptions = setting.buildAppOptions;
            Version = setting.version;
            MainCDN = setting.MainCDN;
            FallbackCDN = setting.FallbackCDN;
            CompanyName = setting.CompanyName;
            GameAddress = setting.gameAddress;
            string uploadVersion = Environment.GetEnvironmentVariable("UPLOAD_VERSION");
            if (!string.IsNullOrEmpty(uploadVersion) && uploadVersion.Equals("false"))
            {
                UploadVersion = false;
            }
            else
            {
                UploadVersion = true;
            }
            
            string isOnlyQrCode = Environment.GetEnvironmentVariable("ONLY_QR_CODE");
            if (!string.IsNullOrEmpty(isOnlyQrCode) && isOnlyQrCode.Equals("false"))
            {
                IsOnlyQrCode = false;
            }
            else
            {
                IsOnlyQrCode = true;
            }
        }

        public override string ToString()
        {
            return $"IsCLI : {IsCLI} IsApp: {IsApp} : IsQrMode : {IsOnlyQrCode} Target : {Target} BuildOptions : {BuildOptions} Version:{Version} GameAdress : {GameAddress} IsUploadVersion : {UploadVersion}";
        }
    }
}