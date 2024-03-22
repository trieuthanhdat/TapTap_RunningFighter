using UnityEngine;

public class CameraFollowing : MonoBehaviour
{
    public Camera mainCamera;
    private Transform playerTransform;
    [SerializeField] private Vector3 offset;

    private void Start()
    {
        FindMainCamera();
        FindPlayerTransform();
    }

    private void FindMainCamera()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found. Make sure it is tagged as 'MainCamera'.");
        }
    }

    private void FindPlayerTransform()
    {
        playerTransform = transform;
    }

    private void LateUpdate()
    {
        if (mainCamera != null && playerTransform != null)
        {
            UpdateCameraPositionAndRotation();
        }
    }

    private void UpdateCameraPositionAndRotation()
    {
        mainCamera.transform.position = playerTransform.position + offset;
        mainCamera.transform.LookAt(playerTransform);
    }
}
