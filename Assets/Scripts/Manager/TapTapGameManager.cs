using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TD;
using System;

public class TapTapGameManager : NetworkBehaviour
{
    public static TapTapGameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameResumed;

    private enum State {
        WaitingToStart,
        CountdownToStart,
        GamePlaying, 
        GameOver
    }
    [SerializeField] private GameObject playerPrefab;

    private NetworkVariable<State> gameState = new NetworkVariable<State>(State.WaitingToStart);
    private bool isLocalPlayerReady;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(5f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingDuration = 60f;
    private Dictionary<ulong, bool> playerReadyDictionary;

    private void Awake()
    {
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    private void Start(){
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        Debug.Log("GameInput_OnInteractAction");
    }

}
