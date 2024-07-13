using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Utilities
{
    public class GenericPool<T> : MonoBehaviour where T : Component
    {
        public T prefab;
        public int initialSize = 10;
        public bool canGrow = true; // Whether the pool can dynamically resize

        private Queue<T> objectPool = new Queue<T>();

        
        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            if (!prefab) return;
            for (int i = 0; i < initialSize; i++)
            {
                T obj = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
                obj.gameObject.SetActive(false);
                objectPool.Enqueue(obj);
            }
        }

        public T GetObjectFromPool()
        {
            if (objectPool.Count > 0)
            {
                T obj = objectPool.Dequeue();
                obj.gameObject.SetActive(true);
                return obj;
            }
            else if (canGrow)
            {
                Debug.LogWarning("Object pool exhausted. Increasing pool size.");
                T obj = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
                return obj;
            }
            else
            {
                Debug.LogWarning("Object pool exhausted. No objects available.");
                return null;
            }
        }

        public void ReturnObjectToPool(T obj)
        {
            obj.gameObject.SetActive(false);
            objectPool.Enqueue(obj);
        }
    }

}
