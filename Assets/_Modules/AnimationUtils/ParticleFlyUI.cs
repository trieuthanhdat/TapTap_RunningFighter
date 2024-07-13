using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleFlyUI : MonoBehaviour
{

    private ParticleSystem m_system;
    private ParticleSystem.Particle[] m_particles;
    bool isSpawn;
    int idProduct;
    private void Awake()
    {
        m_system = GetComponent<ParticleSystem>();
    }
    private void OnEnable()
    {
        isSpawn = false;
    }
    
    //public void SetInfoProduct(int id)
    //{
    //    idProduct = id;
    //    if (idProduct != 1000)
    //    {
    //        m_system.GetComponent<ParticleSystemRenderer>().mesh = DataManager.instance.dataProduct.listProduct[id].mesh;
    //        m_system.GetComponent<ParticleSystemRenderer>().material = DataManager.instance.dataProduct.listProduct[id].mat_product;
    //    }
    //}
    //private void LateUpdate()
    //{
    //    if (m_system.isPlaying)
    //    {
    //        if (m_particles == null || m_particles.Length < m_system.main.maxParticles)
    //            m_particles = new ParticleSystem.Particle[m_system.main.maxParticles];
    //        int numParticlesAlive = m_system.GetParticles(m_particles);

    //        for (int i = 0; i < numParticlesAlive; i++)
    //        {
    //            if (m_particles[i].remainingLifetime < .1f && !isSpawn)
    //            {
    //                isSpawn = true;
    //                //if (idProduct != 1000)
    //                    //EffectManager.instance.SpawnFxProductOnUI(transform.TransformPoint(m_particles[i].position), idProduct);
    //                //else
    //                    //EffectManager.instance.SpawnFxGemOnUI(transform.TransformPoint(m_particles[i].position));
    //                Invoke("Off", 1f);
    //            }
    //        }

    //    }
    //}
    void Off()
    {
        gameObject.SetActive(false);
    }
}
