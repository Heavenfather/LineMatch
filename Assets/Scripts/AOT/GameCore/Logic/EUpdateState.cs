namespace GameCore.Logic
{
    /// <summary>
    /// 更新进度
    /// </summary>
    public enum EUpdateState
    {
        /// <summary>
        /// 未知状态，进入游戏时的默认状态
        /// </summary>
        Unknown,
        
        /// <summary>
        /// 重试状态
        /// </summary>
        RetryState,
        
        /// <summary>
        /// 游戏启动
        /// </summary>
        Launch,
        
        /// <summary>
        /// 校验远端版本
        /// </summary>
        CheckRemoteVersion,
        
        /// <summary>
        /// 校验远端版本成功
        /// </summary>
        CheckRemoteVersionSuccess,
        
        /// <summary>
        /// 校验远端版本失败
        /// </summary>
        CheckRemoteVersionFail,
        
        /// <summary>
        /// 初始化资源包
        /// </summary>
        InitializePackage,
        
        /// <summary>
        /// 检查App是否需要更新
        /// </summary>
        CheckAppUpdate,
        
        /// <summary>
        /// 开始更新游戏资源版本
        /// </summary>
        UpdateResVersion,
        
        /// <summary>
        /// 游戏预加载阶段
        /// </summary>
        UpdatePreloading,
        
        /// <summary>
        /// 开始更新游戏资源清单文件
        /// </summary>
        UpdateResManifest,
        
        /// <summary>
        /// 开始下载游戏资源
        /// </summary>
        DownloadRes,
        
        /// <summary>
        /// 资源下载完成，开始清理本地缓存资源
        /// </summary>
        DownloadDoneClearCache,
        
        /// <summary>
        /// 加载程序集 HybridCLR处理
        /// </summary>
        LoadAssembly,
        
        /// <summary>
        /// 所有操作已成功完成
        /// </summary>
        DoneSuccess,
    }
}