using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class GameplayManager : BaseManager<GameplayManager>
{
    public enum GAMEPLAY_STATE { WAITING, PRE_START, PLAYING, GAME_OVER }
    public GAMEPLAY_STATE CurrentState { get; private set; }

    public event Action OnGameplayWaiting;
    public event Action OnGameplayPreStart;
    public event Action OnGameplayPlaying;
    public event Action OnGameplayGameOver;
    
    void Start()
    {
        Init();
        NetworkManager.Singleton.OnClientConnectedCallback += CheckAllPlayersReady;
    }

    public override void Init()
    {
        base.Init();
        ChangeState(GAMEPLAY_STATE.WAITING);
    }

    public void StartGame()
    {
        StartCoroutine(StartGameCoroutine());
        ChangeState(GAMEPLAY_STATE.PRE_START);
    }

    private IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(3);
        ChangeState(GAMEPLAY_STATE.PLAYING);
    }

    public void EndGame()
    {
        ChangeState(GAMEPLAY_STATE.GAME_OVER);
    }

    // check if all players are in the game
    public void CheckAllPlayersReady()
    {
        // check if have 2 players 
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            ChangeState(GAMEPLAY_STATE.PRE_START);
        }
    }

    // function to change next state
    public void ChangeState(GAMEPLAY_STATE state)
    {
        switch (state)
        {
            case GAMEPLAY_STATE.WAITING:
                OnGameplayWaiting?.Invoke();
                break;
            case GAMEPLAY_STATE.PRE_START:
                OnGameplayPreStart?.Invoke();
                break;
            case GAMEPLAY_STATE.PLAYING:
                OnGameplayPlaying?.Invoke();
                break;
            case GAMEPLAY_STATE.GAME_OVER:
                OnGameplayGameOver?.Invoke();
                break;
        }
    }

}
