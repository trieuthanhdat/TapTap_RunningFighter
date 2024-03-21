using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowing : MonoBehaviour
{
    // this component is attached to the player
    public Camera mainCamera;
    private Transform playerTransform;
    // offset between the player and the camera
    [SerializeField] private Vector3 offset;

    void Start()
    {
        // find tag "MainCamera" and assign it to mainCamera
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        playerTransform = GetComponent<Transform>();
    }

    void LateUpdate()
    {
        // Look at the player
        mainCamera.transform.position = playerTransform.position + offset;
        mainCamera.transform.LookAt(playerTransform);
    }

}
