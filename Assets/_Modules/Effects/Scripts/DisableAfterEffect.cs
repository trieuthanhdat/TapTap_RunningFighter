using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DisableAfterEffect : MonoBehaviour
{
    private ParticleSystem particleSystem;

    void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        // Check if the particle system is not playing
        if (particleSystem != null && !particleSystem.isPlaying)
        {
            // Disable the GameObject
            gameObject.SetActive(false);
        }
    }
}
