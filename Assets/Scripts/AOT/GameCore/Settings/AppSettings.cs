using System;
using System.Collections.Generic;
using System.IO;
using GameCore.Logic;
using UnityEngine;
using YooAsset;

namespace GameCore.Settings
{
    /// <summary>
    /// 游戏全局相关配置
    /// </summary>
    [CreateAssetMenu(fileName = "AppSettings", menuName = "Game/App Settings")]
    public class AppSettings : ScriptableObject
    {
        [SerializeField] private string _appID;
        public string AppID => _appID;

        [SerializeField] private string _appKey;
        public string AppKey => _appKey;

        [SerializeField] private string _appSecret;
        public string AppSecret => _appSecret;

        [SerializeField] private string _channelID;
        public string ChannelID => _channelID;
    }
}