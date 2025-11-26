namespace HotfixCore.Module
{
    public static class G
    {
        private static EventModule _eventModule;

        /// <summary>
        /// 全局事件模块
        /// </summary>
        public static EventModule EventModule => _eventModule ??= Module.CreateModule<EventModule>();

        private static FsmModule _fsmModule;

        /// <summary>
        /// 有限状态机模块
        /// </summary>
        public static FsmModule FsmModule => _fsmModule ??= Module.CreateModule<FsmModule>();

        private static ObjectPoolModule _objectPoolModule;

        /// <summary>
        /// 资产对象池模块
        /// </summary>
        public static ObjectPoolModule ObjectPoolModule =>
            _objectPoolModule ??= Module.CreateModule<ObjectPoolModule>();


        private static ResourceModule _resourceModuleModule;

        /// <summary>
        /// 资源管理模块
        /// </summary>
        public static ResourceModule ResourceModule => _resourceModuleModule ??= Module.CreateModule<ResourceModule>();


        private static SceneModule _sceneModule;

        /// <summary>
        /// 资源管理模块
        /// </summary>
        public static SceneModule SceneModule => _sceneModule ??= Module.CreateModule<SceneModule>();

        private static TimerModule _timerModule;

        /// <summary>
        /// 时间管理模块
        /// </summary>
        public static TimerModule TimerModule => _timerModule ??= Module.CreateModule<TimerModule>();

        private static UIModule _uiModule;

        /// <summary>
        /// 时间管理模块
        /// </summary>
        public static UIModule UIModule => _uiModule ??= Module.CreateModule<UIModule>();

        private static IAudioModule _audioModule;

        /// <summary>
        /// 音频模块
        /// </summary>
        public static IAudioModule AudioModule => _audioModule ??= Module.CreateModule<AudioModule>();

        private static HttpModule _httpModule;

        /// <summary>
        /// http模块
        /// </summary>
        public static HttpModule HttpModule => _httpModule ??= Module.CreateModule<HttpModule>();

        private static UserInfoModule _userinfoModule;

        /// <summary>
        /// 用户信息模块
        /// </summary>
        public static UserInfoModule UserInfoModule => _userinfoModule ??= Module.CreateModule<UserInfoModule>();

        private static IAPModule _iapModule;

        /// <summary>
        /// 用户信息模块
        /// </summary>
        public static IAPModule IAPModule => _iapModule ??= Module.CreateModule<IAPModule>();


        private static GameItemModule _gameItemModule;

        /// <summary>
        /// 道具模块
        /// </summary>
        public static GameItemModule GameItemModule => _gameItemModule ??= Module.CreateModule<GameItemModule>();

        private static LocalizationModule _localizationModule;

        /// <summary>
        /// 多语言模块
        /// </summary>
        public static LocalizationModule LocalizationModule =>
            _localizationModule ??= Module.CreateModule<LocalizationModule>();
        
        private static TouchModule _touchModule;
        public static TouchModule TouchModule => _touchModule ??= Module.CreateModule<TouchModule>();


        private static TargetTaskModule _targetTaskModule;
        public static TargetTaskModule TargetTaskModule => _targetTaskModule ??= Module.CreateModule<TargetTaskModule>();

        private static RankModule _rankModule;
        public static RankModule RankModule => _rankModule ??= Module.CreateModule<RankModule>();


        private static CardModule _cardModule;
        public static CardModule CardModule => _cardModule ??= Module.CreateModule<CardModule>();


        private static RedDotModule _redDotModule;
        public static RedDotModule RedDotModule => _redDotModule ??= Module.CreateModule<RedDotModule>();

        private static AdvModule _advModule;
        public static AdvModule AdvModule => _advModule ??= Module.CreateModule<AdvModule>();

        private static TimerGiftModule _timerGiftModule;
        public static TimerGiftModule TimerGiftModule => _timerGiftModule ??= Module.CreateModule<TimerGiftModule>();


        /// <summary>
        /// 月卡模块
        /// </summary>
        private static MonthCardModule _monthCardModule;
        public static MonthCardModule MonthCardModule => _monthCardModule ??= Module.CreateModule<MonthCardModule>();

        /// <summary>
        /// 开关模块
        /// </summary>
        private static SwitchModule _switchModule;
        public static SwitchModule SwitchModule => _switchModule ??= Module.CreateModule<SwitchModule>();


        /// <summary>
        /// 弹框队列
        /// </summary>
        private static PopModule _popModule;
        public static PopModule PopModule => _popModule ??= Module.CreateModule<PopModule>();

        /// <summary>
        /// 大师试炼
        /// </summary>
        private static TrainMasterModule _trainMasterModule;
        public static TrainMasterModule TrainMasterModule => _trainMasterModule ??= Module.CreateModule<TrainMasterModule>();

        /// <summary>
        /// 邮件
        /// </summary>
        private static EmailModule _emailModule;
        public static EmailModule EmailModule => _emailModule ??= Module.CreateModule<EmailModule>();


        /// <summary>
        /// 邮件
        /// </summary>
        private static InviteModule _inviteModule;
        public static InviteModule InviteModule => _inviteModule ??= Module.CreateModule<InviteModule>();

        /// <summary>
        /// 邮件
        /// </summary>
        private static NoticeModule _noticeModule;
        public static NoticeModule NoticeModule => _noticeModule ??= Module.CreateModule<NoticeModule>();
        
    }
}