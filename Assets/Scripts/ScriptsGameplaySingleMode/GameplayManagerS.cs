using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class GameplayManagerS : BaseManager<GameplayManagerS>
{
    public enum GAME_STATE
    {
        WAITING,
        PREPARING,
        PLAYING,
        FINISHED
    }

    [SerializeField]
    private GAME_STATE currentState;
    public GAME_STATE CurrentState
    {
        get { return currentState; }
    }

    public event Action<GAME_STATE> OnStateChangeAction;
    public event Action OnGameWaitingAction;
    public event Action OnGamePreparingAction;
    public event Action OnGamePlayingAction;
    public event Action OnGameFinishedAction;

    void Awake()
    {
        Init();
    }

    void SubcribeEvents()
    {
        OnStateChangeAction += OnStateChange;
        OnGameWaitingAction += OnGameWaiting;
        OnGamePreparingAction += OnGamePreparing;
        OnGamePlayingAction += OnGamePlaying;
        OnGameFinishedAction += OnGameFinished;
    }

    public override void Init()
    {
        SubcribeEvents();
        OnStateChangeAction?.Invoke(currentState);
    }

    public void OnStateChange(GAME_STATE state)
    {
        currentState = state;
        switch (currentState)
        {
            case GAME_STATE.WAITING:
                OnGameWaitingAction?.Invoke();
                break;
            case GAME_STATE.PREPARING:
                OnGamePreparingAction?.Invoke();
                break;
            case GAME_STATE.PLAYING:
                OnGamePlayingAction?.Invoke();
                break;
            case GAME_STATE.FINISHED:
                OnGameFinishedAction?.Invoke();
                break;
        }
    }

    public void OnGameWaiting()
    {
        Debug.Log("Game is waiting for players...");
        OnStateChangeAction?.Invoke(GAME_STATE.PREPARING);
    }

    public void OnGamePreparing()
    {
        Debug.Log("Game is preparing...");
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(3f);
        OnStateChangeAction?.Invoke(GAME_STATE.PLAYING);
    }

    public void OnGamePlaying()
    {
        Debug.Log("Game is playing...");
    }

    public void OnGameFinished()
    {
        Debug.Log("Game is finished...");
    }
}
