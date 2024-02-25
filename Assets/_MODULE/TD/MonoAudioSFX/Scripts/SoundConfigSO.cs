using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.MonoAudioSFX
{
    [CreateAssetMenu(fileName = "NewSound", menuName = "MonoAudio/SoundConfig")]
    public class SoundConfigSO : ScriptableObject
    {
        public Sound[] sounds;
    }

}
