using System;
using UnityEngine;
using UnityEngine.Audio;

namespace  MatchTwo.Client.Data
{
    [Serializable]
    public class AudioObject
    {
        public AudioName AudioName;
        public AudioClip Clip;
        
        [HideInInspector] public AudioSource Source;
        [HideInInspector] public AudioMixerGroup AudioMixerGrp;
    }
    
    public enum AudioName
    {
        CubeCollectSfx = 0,
        CubeExplodeSfx = 1
    }
}