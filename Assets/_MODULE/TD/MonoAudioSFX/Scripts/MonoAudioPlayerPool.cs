using System.Collections.Generic;
using UnityEngine;


namespace TD.MonoAudioSFX
{
    public class MonoAudioPlayerPool
    {
        private Queue<MonoAudioPlayer> availablePlayers;
        private MonoAudioPlayer prefab;
        private Transform parent;

        public MonoAudioPlayerPool(MonoAudioPlayer prefab, int initialSize, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;
            availablePlayers = new Queue<MonoAudioPlayer>();

            for (int i = 0; i < initialSize; i++)
            {
                MonoAudioPlayer instance = Object.Instantiate(prefab, parent);
                instance.gameObject.SetActive(false);
                availablePlayers.Enqueue(instance);
            }
        }

        public MonoAudioPlayer Get()
        {
            if (availablePlayers.Count > 0)
            {
                MonoAudioPlayer instance = availablePlayers.Dequeue();
                instance.gameObject.SetActive(true);
                return instance;
            }
            else
            {
                MonoAudioPlayer instance = Object.Instantiate(prefab, parent);
                instance.gameObject.SetActive(true);
                availablePlayers.Enqueue(instance);
                return instance;
            }
        }

        public void Return(MonoAudioPlayer instance)
        {
            instance.gameObject.SetActive(false);
            availablePlayers.Enqueue(instance);
        }
    }

}

