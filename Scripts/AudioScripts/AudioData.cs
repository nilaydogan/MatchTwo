using UnityEngine;
using UnityEngine.Audio;

namespace MatchTwo.Client.Data
{
    [CreateAssetMenu(menuName = "ScriptableObjects/AudioData", fileName = "AudioData")]
    public class AudioData : ScriptableObject
    {
        public AudioMixerGroup SFXGroup;
        [Header("SoundEffects")] public AudioObject[] SFXAudio;
    }
}