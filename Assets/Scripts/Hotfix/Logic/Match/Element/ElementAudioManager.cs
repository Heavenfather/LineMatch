using System.Collections.Generic;
using GameConfig;
using GameCore.Settings;
using GameCore.Singleton;
using HotfixCore.Module;

namespace HotfixLogic.Match
{
    public class ElementAudioManager : LazySingleton<ElementAudioManager>
    {
        private Dictionary<int, string> _eleAudioPathCache;
        private Dictionary<int, string> _collectAudioPathCache;
        private List<string> _extraAudioPath;
        private HashSet<string> _playingAudios;

        // 可以重复播放的音效
        private HashSet<int> _multiPlayIDs;

        protected override void OnInitialized()
        {
            _eleAudioPathCache = new Dictionary<int, string>(20);
            _collectAudioPathCache = new Dictionary<int, string>(20);
            _extraAudioPath = new List<string>(20);
            _playingAudios = new HashSet<string>(20);
            _multiPlayIDs = new HashSet<int>();

            InitMultiPlayIDs();
        }

        public void Play(int elementId)
        {
            if (_eleAudioPathCache.TryGetValue(elementId, out string path))
            {
                if(!CanPlayMultiAudio(elementId, path))
                    return;
                PlayAudio(elementId, path, false);
                return;
            }

            path = GetAudioPath(elementId);
            if(!CanPlayMultiAudio(elementId, path))
                return;

            if (!string.IsNullOrEmpty(path))
            {
                PlayAudio(elementId, path, isCollect:false);
            }
        }

        public void Play(string audioName)
        {
            if (string.IsNullOrEmpty(audioName))
                return;

            string path = GetAudioPath(audioName);

            if(!CanPlayMultiAudio(path))
                return;

            if (!_extraAudioPath.Contains(path))
            {
                _extraAudioPath.Add(path);
            }

            _playingAudios.Add(path);
            G.AudioModule.Play(AudioType.Sound, path, false, 0.7f, true, OnPlayAudioComplete);
        }

        public void PlayCollect(int elementId)
        {
            if (_collectAudioPathCache.TryGetValue(elementId, out string path))
            {
                PlayAudio(elementId, path, false);
                return;
            }

            path = GetCollectAudioPath(elementId);

            if (!string.IsNullOrEmpty(path))
            {
                PlayAudio(elementId, path, isCollect:true);
            }
        }

        public bool CanPlayMultiAudio(int elementId, string path) {
            if (_multiPlayIDs.Contains(elementId)) return true;
            return !_playingAudios.Contains(path);
        }

        public bool CanPlayMultiAudio(string path) {
            // 允许播放重复音频
            if (path.Contains("pluck_")) return true;
            return !_playingAudios.Contains(path);
        }

        private void PlayAudio(int elementId, string path, bool pushCache = true, bool isCollect = false) {
            
            if (!isCollect) _playingAudios.Add(path);
            G.AudioModule.Play(AudioType.Sound, path, false, 0.7f, true,OnPlayAudioComplete);
            if (pushCache) {
                PushCache(elementId, path, isCollect);
            }
        }

        private void PushCache(int elementId, string path, bool isCollect = false) {
            if (isCollect) {
                if (!_collectAudioPathCache.ContainsValue(path)) {
                    _collectAudioPathCache.Add(elementId, path);
                }
            }
            else {
                if (!_eleAudioPathCache.ContainsValue(path)) {
                    _eleAudioPathCache.Add(elementId, path);
                }
            }
        }



        public void ReleaseAudio()
        {
            foreach (var path in _eleAudioPathCache.Values)
            {
                G.AudioModule.RemoveClipFromPool(path);
            }
            foreach (var path in _extraAudioPath)
            {
                G.AudioModule.RemoveClipFromPool(path);
            }
            foreach (var path in _collectAudioPathCache.Values)
            {
                G.AudioModule.RemoveClipFromPool(path);
            }

            _extraAudioPath.Clear();
            _eleAudioPathCache.Clear();
            _collectAudioPathCache.Clear();
            _playingAudios.Clear();
        }

        public void ClearPlaying()
        {
            _playingAudios.Clear();
        }

        private void OnPlayAudioComplete(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                _playingAudios.Remove(path);
            }
        }

        private string GetAudioPath(int elementId)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            if(!db.IsContain(elementId))
                return string.Empty;
            if(string.IsNullOrEmpty(db[elementId].audio))
                return string.Empty;
            return GetAudioPath(db[elementId].audio);
        }

        private string GetCollectAudioPath(int elementId) 
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            if(!db.IsContain(elementId))
                return string.Empty;
            if(string.IsNullOrEmpty(db[elementId].collectAudio))
                return string.Empty;
            return GetAudioPath(db[elementId].collectAudio);
        }

        private string GetAudioPath(string audioName)
        {
            return $"audio/match/{audioName}".ToLower();
        }

        private void InitMultiPlayIDs() {
            _multiPlayIDs.Add(1001);
        }

        public void PlayMatchLink(int count = 1) {
            if (count < 1) count = 1;
            if (count > 10) count = 10;

            var audioName = "pluck_" + count;
            Play(audioName);
        }
    }
}