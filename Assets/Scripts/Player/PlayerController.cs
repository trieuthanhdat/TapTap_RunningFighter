using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float dashForce = 10f;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Debug.Log("I am the owner");
        }
        else
        {
            Debug.Log("I am not the owner");
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            // phone input touch
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    DashServerRpc();
                }
            }
        }
    }

    // Dash
    [ServerRpc]
   void DashServerRpc()
    {
        // dash to the right
        rb.AddForce(Vector3.right * dashForce, ForceMode.Impulse);
    }
}
