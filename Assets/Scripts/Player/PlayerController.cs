using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float dashForce = 10f;
    private Rigidbody rb;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log("PlayerController OnEnable");
        GameplayUI.Instance.OnPlayerDash += DashServerRpc;
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
       
    }

    [ServerRpc]
    void DashServerRpc()
    {
        Debug.Log("DashServerRpc");
        StartCoroutine(Dash());
    }
    private IEnumerator Dash()
    {
        rb.AddForce(Vector3.right * dashForce, ForceMode.Impulse);
        yield return new WaitForSeconds(0.5f);
        rb.velocity = Vector3.zero;
    }
}
