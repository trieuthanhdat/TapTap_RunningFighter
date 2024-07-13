using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TD.MonoAudioSFX
{
    [System.Serializable]
    public class Sound
    {
        [Header("Basic Settings")]
        public string name;
        public AudioClip clip;
        public bool playOnAwake;
        public bool isBackgroundSound = false;
        [Range(0, 1)]
        public float volume = 1f;
        [Range(0f, 2f)]
        public float pitch = 1f;

        [Header("Fade Settings")]
        [Tooltip("Duration of the fade-in effect in seconds")]
        [Range(0f, 60f)]
        public float fadeInDuration = 1f;
        [Tooltip("Duration of the fade-out effect in seconds")]
        [Range(0f, 60f)]
        public float fadeOutDuration = 1f;

        [HideInInspector]
        public AudioSource audioSource;
        [HideInInspector]
        public bool useFadeInEffect = false;
        [HideInInspector]
        public bool useFadeOutEffect = false;
        [HideInInspector]
        public MonoAudioPlayer player;

    }

    public class MonoAudioManager : MonoSingleton<MonoAudioManager>
    {
        [SerializeField] private MonoAudioPlayer audioPlayerPrefab;
        [SerializeField] private SoundConfigSO soundSO;
        [SerializeField] private int poolSize = 5;

        private bool _isInited = false;
        public bool IsInited => _isInited;

        private MonoAudioPlayerPool audioPlayerPool;

        private void Awake()
        {
            audioPlayerPool = new MonoAudioPlayerPool(audioPlayerPrefab, poolSize, transform);

            foreach (Sound s in soundSO.sounds)
            {
                // Get an available player from the pool and setup
                s.player = audioPlayerPool.Get();
                s.player.Setup(s, audioPlayerPool);
                // Deactivate to save performance
                ToggleActivationPlayer(false, s.player);
            }

            _isInited = true;
        }

        public void ToggleActivationPlayer(bool isOn, MonoAudioPlayer player)
        {
            player.gameObject.SetActive(isOn);
        }

        public void PlaySound(string name, bool isLoop = false, bool isGradient = false, float delay = 0)
        {
            try
            {
                if (!_isInited) return;
                Sound s = Array.Find(soundSO.sounds, sound => sound.name == name);
                if (s == null)
                {
                    Debug.LogWarning("MONOAUDIOMANAGER: Sound name: " + name + " is missing!!!");
                    return;
                }

                // Get an available player from the pool
                s.player = audioPlayerPool.Get();
                s.player.Setup(s, audioPlayerPool);

                s.audioSource.loop = isLoop;
                s.useFadeInEffect = isGradient;
                s.player.PlaySound(delay);
            }
            catch (Exception ex)
            {
                Debug.Log("MONOAUDIOMANAGER: exception at PlaySound " + ex);
            }
        }

        public void StopSound(string name, bool isGradient = false, float modifiedFadeoutTime = -1)
        {
            try
            {
                if (!_isInited) return;
                Sound s = Array.Find(soundSO.sounds, sound => sound.name == name);
                if (s == null)
                {
                    Debug.LogWarning("Sound name: " + name + " is missing!!!");
                    return;
                }
                s.useFadeOutEffect = isGradient;

                if (modifiedFadeoutTime > -1)
                    s.fadeOutDuration = modifiedFadeoutTime;
                s.player.StopSound();
            }
            catch (Exception ex)
            {
                Debug.Log("MONOAUDIOMANAGER: exception at StopSound " + ex);
            }
        }
    }

}

