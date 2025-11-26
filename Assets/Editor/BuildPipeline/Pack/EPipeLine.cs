using Sirenix.OdinInspector;

namespace GameEditor.BuildPipeline
{
    [System.Flags]
    public enum EPipeLine : int
    {
        [LabelText("前置检测")]AutoTest = 1 << 0,
        [LabelText("切换平台")] SwitchPlatform = 1 << 1,
        [LabelText("构建前准备")] PrepareBuild = 1 << 2,
        [LabelText("HybridCLR准备")] HybridBuild = 1 << 3,
        [LabelText("App-宏定义处理")] APP_Symbols = 1 << 4,
        [LabelText("执行构建")] Build = 1 << 5,
        [LabelText("App-签名")] APP_Sign = 1 << 6,
        [LabelText("上传")] Upload = 1 << 7,
        [LabelText("恢复工程")] Resume = 1 << 8,
        [LabelText("结束关闭Unity")] Shutdown = 1 << 9,

        All = AutoTest | SwitchPlatform | PrepareBuild | HybridBuild | Build | APP_Symbols | APP_Sign | Upload | Resume | Shutdown,
    }
}