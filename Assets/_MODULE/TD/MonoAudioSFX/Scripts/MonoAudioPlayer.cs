using UnityEngine;
using Time = UnityEngine.Time;

namespace TD.MonoAudioSFX
{
    public class MonoAudioPlayer : MonoBehaviour
    {
        [SerializeField] protected Sound sound;
        [SerializeField] protected float fadeInTimer = 2f;
        [SerializeField] protected float fadeOutTimer = 2f;

        private float _timeGradient = 0;
        private bool _startPlaying = false;
        private bool _stopPlaying = false;
        private float _originVolume = 1;

        private bool _isInited = false;
        private MonoAudioPlayerPool _pool;

        public void Setup(Sound sound, MonoAudioPlayerPool pool)
        {
            this.sound = sound;
            this._pool = pool;
            fadeInTimer = sound.fadeInDuration;
            fadeOutTimer = sound.fadeOutDuration;
            // SETUP SOUND
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            sound.audioSource = audioSource;
            audioSource.clip = sound.clip;
            audioSource.playOnAwake = sound.playOnAwake;
            audioSource.pitch = sound.pitch;
            if (sound.isBackgroundSound)
                audioSource.volume = 0;
            else
                audioSource.volume = sound.volume;

            _isInited = true;
        }

        private void Start()
        {
            _originVolume = sound.volume;

            if (sound.playOnAwake)
                PlaySound();
        }

        private void Update()
        {
            if (!_isInited) return;
            if (_startPlaying && !_stopPlaying)
            {
                if (sound.useFadeInEffect)
                {
                    PlayFadeInOutEffect(true);
                }
                else
                {
                    sound.audioSource.Play();
                    _startPlaying = false;
                }
            }
            else if (_stopPlaying && !_startPlaying)
            {
                if (sound.useFadeOutEffect)
                {
                    PlayFadeInOutEffect(false);
                }
                else
                {
                    sound.audioSource.Stop();
                    _stopPlaying = false;
                    ReturnToPool();
                }
            }
            else if (!_stopPlaying && !_startPlaying)
            {
                if (!sound.audioSource.isPlaying)
                {
                    ReturnToPool();
                }
            }
        }

        public void PlaySound(float delay = 0)
        {
            ResetCounter();

            _startPlaying = true;
            _stopPlaying = false;
            MonoAudioManager.Instance.ToggleActivationPlayer(true, this);

            if (delay > 0)
                sound.audioSource.PlayDelayed(delay);
            else
                sound.audioSource.Play();
        }

        public void StopSound()
        {
            if (!sound.audioSource.isPlaying) return;
            ResetCounter();
            _stopPlaying = true;
            _startPlaying = false;
        }

        public void ResetCounter()
        {
            _timeGradient = 0;
        }

        private void PlayFadeInOutEffect(bool isStart)
        {
            sound.audioSource.volume = isStart ? 0 : sound.audioSource.volume;

            _timeGradient += Time.deltaTime;

            if (isStart)
            {
                sound.audioSource.volume += _timeGradient / fadeInTimer;
                if (sound.audioSource.volume >= _originVolume)
                {
                    _startPlaying = false;
                }
            }
            else
            {
                sound.audioSource.volume -= _timeGradient / fadeOutTimer;
                if (sound.audioSource.volume <= 0)
                {
                    _stopPlaying = false;
                    sound.audioSource.Stop();
                    ReturnToPool();
                }
            }
        }

        private void ReturnToPool()
        {
            _pool.Return(this);
        }
    }

}

