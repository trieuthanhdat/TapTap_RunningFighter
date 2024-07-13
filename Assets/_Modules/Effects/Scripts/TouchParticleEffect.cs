using System.Collections.Generic;
using TD.Utilities;
using UnityEngine;

public class TouchParticleEffect : MonoBehaviour
{
    [SerializeField] private GameObject m_particlePrefab;
    [SerializeField] private int        m_poolSize = 10;
    [SerializeField] private Camera     m_mainCamera;

    private Queue<GameObject> _particlePool = new Queue<GameObject>();

    private void Start()
    {
        if (m_mainCamera == null)
            m_mainCamera = Camera.main;

        // Create the particle pool
        CreateParticlePool();
    }

    private void CreateParticlePool()
    {
        for (int i = 0; i < m_poolSize; i++)
        {
            GameObject particle = Instantiate(m_particlePrefab, transform);
            particle.SetActive(false);
            _particlePool.Enqueue(particle);
        }
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
                {
                    OnSpawnEffect(touch.position);
                }
            }
        }
    }

    public void OnSpawnEffect(Vector3 touchPosition)
    {
        if (m_mainCamera == null)
        {
            Debug.LogError("Main camera is not assigned or found.");
            return;
        }

        Vector3 worldPosition = m_mainCamera.ScreenToWorldPoint(touchPosition);

        GameObject particleEffect = GetPooledObject();
        if (particleEffect != null)
        {
            particleEffect.transform.position = worldPosition;
            particleEffect.SetActive(true);
            ParticleSystem particleSystem = ComponentCache.Get<ParticleSystem>(m_particlePrefab);

            if (particleSystem != null)
            {
                particleSystem.Play();
            }
            else
            {
                Debug.LogError("Particle system component not found in the cache.");
            }
        }
    }

    private GameObject GetPooledObject()
    {
        if (_particlePool.Count == 0)
        {
            Debug.LogWarning("Particle pool is empty. Consider increasing the pool size.");
            return null;
        }

        GameObject pooledObject = _particlePool.Dequeue();
        _particlePool.Enqueue(pooledObject); // Re-enqueue the object for reuse
        return pooledObject;
    }
}
