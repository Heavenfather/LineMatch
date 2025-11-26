using System;
using System.Collections.Generic;
using GameCore.Logic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GameEditor.BuildPipeline
{
    [CreateAssetMenu(menuName = "Game/Build Pipeline Settings", fileName = "BuildSettings")]
    public class BuildSetting : ScriptableObject
    {
        [Title("基础设置", "必填")] [LabelText("是否构建App")]
        public bool buildApp;

        [LabelText("构建目标")] public BuildTarget buildTarget;

        [LabelText("构建目标"), ShowIf("IsShowWebGLTarget")]
        public EWebGLTarget webGLTarget;

        [LabelText("版本号"),InlineButton("SyncNowTime",ShowIf = "IsBuildRes")]
        public string version;

        public void SyncNowTime()
        {
            DateTime dateTime = DateTime.Now;
            version = ($"{dateTime.Year}{dateTime.Month}{dateTime.Day}" +
                       $"{dateTime.Hour:D2}{dateTime.Minute:D2}{dateTime.Second:D2}");
            EditorUtility.SetDirty(this);
        }

        [LabelText("构建类型")] public EAppMode buildType;
        [LabelText("构建管线")] public EPipeLine pipeLine;

        [HideIf("buildApp")] public string[] packageNames = new string[] { "AssetMain" };

        [Title("构建参数")] [ShowIf("buildApp"), LabelText("游戏服域名")]
        public string gameAddress;
        
        [Title("构建参数")] [ShowIf("buildApp"), LabelText("添加宏定义")]
        public string[] enableSymbols;

        [ShowIf("buildApp"), LabelText("禁用宏定义")]
        public string[] disableSymbols;

        [ShowIf("buildApp"), LabelText("构建App参数")]
        public BuildOptions buildAppOptions;

        [LabelText("工作室名称")][ShowIf("buildApp")]
        public string CompanyName;

        [Title("项目设置"), LabelText("主CDN地址")]
        public string MainCDN;

        [LabelText("备用CDN地址")]
        public string FallbackCDN;
        
        // [LabelText("Http安全选项"),ShowIf("buildApp")]
        // public InsecureHttpOption InsecureHttpOption = InsecureHttpOption.NotAllowed;

        [Button("开始构建")]
        public void Build()
        {
            BuildContext context = new BuildContext(false, this);
            BuildCLI.StartBuild(pipeLine, context);
        }

        private bool IsShowWebGLTarget()
        {
            return buildApp && buildTarget == BuildTarget.WebGL;
        }

        private bool IsBuildRes()
        {
            return buildApp == false;
        }
    }
}