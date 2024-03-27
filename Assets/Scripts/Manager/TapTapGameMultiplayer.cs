using System;
using System.Collections;
using System.Collections.Generic;
using TD.Networks.Data;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TapTapGameMultiplayer : NetworkBehaviour
{
    public const int MAX_PLAYER_AMOUNT = 4;
    public static TapTapGameMultiplayer Instance { get; private set; }
    public static bool playMultiplayer = true;
    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;

    [SerializeField]
    private NetworkPrefabs networkPrefabs;

    [SerializeField]
    private List<Color> playerColorList;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    
}
