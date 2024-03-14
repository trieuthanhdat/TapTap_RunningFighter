using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerController : NetworkBehaviour
{
    private GameObject player_body;
    private Animator anim;
    private Rigidbody rb;

    private void OnEnable()
    {
        player_body = transform.GetChild(0).gameObject;
        rb = player_body.GetComponent<Rigidbody>();
        anim = player_body.GetComponent<Animator>();
        GameplayUI.Instance.OnPlayerDash += Move;
    }

    void FixedUpdate()
    {
        if (!IsLocalPlayer || !Application.isFocused) return;
    }
    // private void OnPlayerDash()
    // {
    //     // Move the player forward by changing transform.position
    //     // This will be synced across the network
    //     transform.position += transform.right * 2;
    // }

    void Move()
    {
        if (IsOwner) {
            transform.position += transform.right * 2;
            MoveOnServerRpc();
        }
    }

    [ServerRpc]
    void MoveOnServerRpc()
    {
        MoveOnClientRpc();
    }

    [ClientRpc]
    void MoveOnClientRpc()
    {
        if (IsOwner) return;
        transform.position += transform.right * 2;
    }



}
