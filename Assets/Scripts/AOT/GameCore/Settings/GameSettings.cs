using GameCore.Logic;
using UnityEngine;

namespace GameCore.Settings
{
    /// <summary>
    /// 项目设置帮助类
    /// </summary>
    public class GameSettings : MonoBehaviour
    {
        private static GameSettings _instance;

        public static GameSettings Instance => _instance;

        private void Awake()
        {
            _instance = this;
        }

        [SerializeField] private GlobalSettings _globalSettings;

        public GlobalSettings ProjectSetting => _globalSettings;

        [SerializeField] private AudioSetting _audioSettings;

        public AudioSetting AudioSetting => _audioSettings;

        [SerializeField] private AppSettings _appSettings;
        public AppSettings AppSetting => _appSettings;

        public bool IsShowDebugConsole()
        {
            return _globalSettings.AppMode == EAppMode.Debug;
        }
    }
}