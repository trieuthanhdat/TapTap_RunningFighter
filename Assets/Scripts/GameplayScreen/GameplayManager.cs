using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class GameplayManager : NetworkBehaviour
{
    public enum GAMEPLAY_STATE { WAITING, PRE_START, PLAYING, GAME_OVER }
    public GAMEPLAY_STATE CurrentState { get; private set; }

    private static GameplayManager _instance;
    public static GameplayManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameplayManager>();
            }
            return _instance;
        }
    }

    public event Action OnGameplayWaiting;
    public event Action OnGameplayPreStart;
    public event Action OnGameplayPlaying;
    public event Action OnGameplayGameOver;

    // catch event if a player is connected
    public override void OnNetworkSpawn()
    {
        OnGameplayWaiting?.Invoke();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    [ServerRpc]
    void ChangeStateServerRpc(GAMEPLAY_STATE state)
    {
        ChangeState(state);
        ChangeStateClientRpc(state);
    }

    [ClientRpc]
    void ChangeStateClientRpc(GAMEPLAY_STATE state)
    {
        ChangeState(state);
    }

    // check if all players are in the game
    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            CheckAllPlayersReady();
        }
    }
    void CheckAllPlayersReady()
    {
        Debug.Log("Checking if all players are ready");
        Debug.Log("Connected clients: " + NetworkManager.Singleton.ConnectedClientsList.Count);

        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            ChangeStateServerRpc(GAMEPLAY_STATE.PRE_START);
        }
    }
    // function to change next state
    void ChangeState(GAMEPLAY_STATE state)
    {
        CurrentState = state;
        switch (state)
        {
            case GAMEPLAY_STATE.WAITING:
                Debug.Log("Gameplay state: WAITING");
                OnGameplayWaiting?.Invoke();
                break;
            case GAMEPLAY_STATE.PRE_START:
                Debug.Log("Gameplay state: PRE_START");
                OnGameplayPreStart?.Invoke();
                StartGame();
                break;
            case GAMEPLAY_STATE.PLAYING:
                Debug.Log("Gameplay state: PLAYING");                
                OnGameplayPlaying?.Invoke();
                break;
            case GAMEPLAY_STATE.GAME_OVER:
                Debug.Log("Gameplay state: GAME_OVER");
                OnGameplayGameOver?.Invoke();
                break;
        }
    }

    // after 3 seconds, change state to PLAYING
    public void StartGame()
    {
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(3);
        ChangeStateServerRpc(GAMEPLAY_STATE.PLAYING);
    }
}
