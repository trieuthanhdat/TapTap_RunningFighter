using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InactivityDetector : MonoBehaviour
{
    [SerializeField] float inactivityThreshold = 300f; // Time in seconds before considering the player inactive
    private float lastInteractionTime; 

    private void Start()
    {
        lastInteractionTime = Time.time;
    }

    private void Update()
    {
        // Check for keyboard/mouse input or touches
        if (Input.anyKey                  ||
            Input.GetAxis("Mouse X") != 0 ||
            Input.GetAxis("Mouse Y") != 0 ||
            Input.touchCount > 0)
        {
            ResetTimer();
        }
        else
        {
            float timeSinceLastInteraction = Time.time - lastInteractionTime;
            if (timeSinceLastInteraction >= inactivityThreshold)
            {
                Debug.Log("INACTIVITY DETECTOR: Player has been inactive for too long => collect garbage");
                CollectGarbage();
                ResetTimer();
            }
        }

#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.V))
        {
            CollectGarbage();
        }
#endif
    }
    private void ResetTimer()
    {
        lastInteractionTime = Time.time;
    }
    private void CollectGarbage()
    {
        GC.Collect();
    }
}

