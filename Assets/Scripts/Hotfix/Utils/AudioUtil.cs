using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using GameCore.Localization;
using GameCore.SDK;
using GameCore.Utils;
using HotfixCore.Module;
using HotfixLogic;
using UnityEngine;
using AudioType = GameCore.Settings.AudioType;

namespace Hotfix.Utils
{
    public static class AudioUtil
    {
        private static PuzzleCollectAudioDB _audioDB = ConfigMemoryPool.Get<PuzzleCollectAudioDB>();
        private static Dictionary<string, float> _playIntervalDict = new Dictionary<string, float>();

        private static float _playInterval = 0.05f;

        public static void PlayBgm(SceneType sceneType)
        {
            switch (sceneType)
            {
                case SceneType.Main:
                    G.AudioModule.Play(AudioType.Music, "audio/preload/bgm_lobby", true, 0.2f);
                    break;
                case SceneType.Match:
                    G.AudioModule.Play(AudioType.Music, "audio/music/bgm_match", true, 0.2f);
                    break;
                case SceneType.Puzzle:
                    G.AudioModule.Play(AudioType.Music, "audio/music/bgm_puzzle", true, 0.25f);
                    break;
            }
        }

        public static void PlayBtnClick()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/common/click_btn", false, 0.3f);
        }

        public static void PlayPuzzleCollect(int id)
        {
            var audioName = _audioDB.GetAudioName(id);
            if (audioName == "")
            {
                G.AudioModule.Play(AudioType.Sound, "audio/common/puzzle_collect", false, 0.25f);
            }
            else
            {
                G.AudioModule.Play(AudioType.Sound, "audio/puzzle/" + audioName, false, 0.25f);
            }
        }

        public static void PlayMatchLink(int count = 1) {
            if (count < 1) count = 1;
            if (count > 10) count = 10;

            if (CommonUtil.IsWechatMiniGame() && SDKMgr.Instance.GetDeviceSystemInfo().Platform == "android") {
                var httpUrl = "https://leyou-cdn.game.jingyougz.com/manbanpai/Audio/Pluck_{0}.ogg";
                var audioUrl = string.Format(httpUrl, count);
                if (PlayerPrefsUtil.GetInt("Effect", 1) == 0) return;
                SDKMgr.Instance.CallSDKMethod("playWXAudio", audioUrl, "", null);
            } else {
                var audioName = "audio/match/pluck_" + count;
                G.AudioModule.Play(AudioType.Sound, audioName, bInPool:true);
            }
        }

        public static async UniTask PreloadMatchLinkAudio() {
            if (CommonUtil.IsWechatMiniGame() && SDKMgr.Instance.GetDeviceSystemInfo().Platform == "android") {
                for (int i = 1; i <= 10; i++) {
                    var httpUrl = "https://leyou-cdn.game.jingyougz.com/manbanpai/Audio/Pluck_{0}.ogg";
                    var audioUrl = string.Format(httpUrl, i);
                    SDKMgr.Instance.CallSDKMethod("preloadWXAudio", audioUrl, "", null);
                }
            }
            await UniTask.CompletedTask;
        }

        public static void PlayPuzzleCollectFinish()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/common/puzzle_collect_finish", false, 0.2f);
        }

        public static void PlayMatchWin()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/common/match_win", false, 0.7f);
        }

        public static void PlayMatchLose()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/common/match_lose", false, 0.7f);
        }

        public static void PlayMatchEliminate()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/common/match_eliminate", false, 0.5f);
        }

        public static void PlayMatchEliminateObstacle()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/common/match_obstacle", false, 0.5f);
        }

        public static void PlayUseColored()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/common/match_colored", false, 0.7f);
        }

        public static void PlayRocker()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/common/match_rocker3", false, 0.35f);
        }

        public static void PlayUseDice()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/common/match_dice", false, 0.7f);
        }

        public static void PlayMatchLine()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/common/match_line1", false, 0.7f);
        }

        public static void PlayMatchSquare() {
            PlaySound("audio/match/match_collect_square");
        }

        public static void PlayBomb()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/common/match_bomb", false, 0.55f);
        }

        public static void PlaySignDraw()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/sign/sign_draw", false, volume: 0.7f);
        }

        public static void PlaySignDrawDrop()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/sign/sign_drawdrop", false, volume: 0.7f);
        }

        public static void PlaySignGetReward()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/sign/sign_getreward", false, volume: 0.7f);
        }

        public static void PlaySignShowSign()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/sign/sign_showsign", false, volume: 0.7f);
        }

        public static void PlayCoinCollect()
        {
            G.AudioModule.Play(AudioType.Sound, "audio/puzzle/puzzle_chicken_coin", false, volume: 0.7f);
        }

        public static void PlayGetCoin()
        {
            var audioPath = "audio/common/common_collect_coin";
            if (!CheckInterval(audioPath)) return;
            PlaySound("audio/common/common_collect_coin", false, 0.5f);
        }

        public static void PlayPopWindow() {
            PlaySound("audio/common/common_pop", false, 0.7f);
        }

        public static void PlayGetItem() {
            var audioPath = "audio/common/common_collect_item";
            if (!CheckInterval(audioPath)) return;

            PlaySound(audioPath, false, 0.5f);
        }

        public static void PlaySound(string path, bool bLoop = false, float volume = 1f) {
            G.AudioModule.Play(AudioType.Sound, path, bLoop, volume);
        }

        public static void PlayCHuanglianShouji() {
            var audioPath = "audio/match/chuanglianshouji";
            if (!CheckInterval(audioPath)) return;
            PlaySound(audioPath);
        }

        private static bool CheckInterval(string audioName) {
            if (_playIntervalDict.ContainsKey(audioName)) {
                if (Time.time - _playIntervalDict[audioName] < _playInterval) {
                    return false;
                }
            }

            _playIntervalDict[audioName] = Time.time;
            return true;
        }

        public static void PlayFlyXuyuanPing() {
            var audioPath = "audio/match/xuyuanpingtuowei";
            if (!CheckInterval(audioPath)) return;
            PlaySound(audioPath);
        }

        public static void PlayFlyButterfly() {
            var audioPath = "audio/match/hudie";
            if (!CheckInterval(audioPath)) return;
            PlaySound(audioPath);
        }
    }
}