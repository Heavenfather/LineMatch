using UnityEngine;
using UnityEngine.Audio;

namespace GameCore.Settings
{
    [CreateAssetMenu(menuName = "Game/AudioSetting", fileName = "AudioSetting")]
    public class AudioSetting : ScriptableObject
    {
        public AudioMixer mixer;
        public AudioGroupConfig[] audioGroupConfigs = null;
    }
}