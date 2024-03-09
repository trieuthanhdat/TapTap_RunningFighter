using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{

    public float jumpForce = 5f;
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

     private void Update(){
        if (IsOwner)
        {
            // get touch
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    JumpServerRpc();
                }
            }
        }
     }


    [ServerRpc]
    private void JumpServerRpc()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }


  

}

