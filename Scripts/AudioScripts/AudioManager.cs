using System;
using System.Collections.Generic;
using MatchTwo.Client.Data;
using UnityEngine;
using UnityEngine.Audio;

namespace MatchTwo.Client.Feedback
{
    public class AudioManager : MonoBehaviour
    {
        #region Fields

        private Dictionary<AudioName, AudioObject> _audioTable;
        private AudioObject _currentPlayingMusic = null;
        private AudioData _audioData;
        private AudioMixerGroup _sfxGroup;
        
        #endregion

        #region Public Methods
        public void Setup(AudioData audioData)
        {
            _audioData = audioData;

            _audioTable = new Dictionary<AudioName, AudioObject>();
            _sfxGroup = _audioData.SFXGroup;
            CreateAudioTable(_audioData.SFXAudio, _sfxGroup);
        }
        
        public void Play(AudioName audioName, float fadeInDuration = 0f,bool incrementPitch = false)
        {
            if (!_audioTable.ContainsKey(audioName))
            {
                Debug.Log("Sound: " + audioName + " not found!");
                return;
            }

            AudioObject audio = _audioTable[audioName];
            
            audio.Source.Play();
        }
        #endregion

        #region Private Methods
        private void CreateAudioTable(AudioObject[] soundArray, AudioMixerGroup audioGrp)
        {
            foreach (AudioObject audioObject in soundArray)
            {
                if (_audioTable.ContainsKey(audioObject.AudioName))
                {
                    Debug.Log("audioTable already contains the key: " + audioObject.AudioName);
                }
                else
                {
                    audioObject.AudioMixerGrp = audioGrp;

                    audioObject.Source = gameObject.AddComponent<AudioSource>();
                    audioObject.Source.clip = audioObject.Clip;
                    //audioObject.Source.loop = audioObject.Loop;
                    audioObject.Source.outputAudioMixerGroup = audioObject.AudioMixerGrp;

                    _audioTable.Add(audioObject.AudioName, audioObject);
                }
            }
        }
        #endregion
    }
}