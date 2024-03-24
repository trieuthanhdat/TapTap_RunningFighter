using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;
using TMPro;

public class GameplayUI : NetworkBehaviour
{
    [SerializeField] private Button dashButton;
    [SerializeField] private TextMeshProUGUI countdownText;
    private static GameplayUI _instance;
    public static GameplayUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameplayUI>();
            }
            return _instance;
        }
    }

    public event Action OnPlayerDash;

    void Awake() {
        Init();
    }

    public void Init()
    {
        dashButton.onClick.AddListener(() => OnPlayerDash?.Invoke());
        GameplayManager.Instance.OnGameplayWaiting += OnWaitingState;
        GameplayManager.Instance.OnGameplayPreStart += OnPreStartState;
        GameplayManager.Instance.OnGameplayPlaying += OnPlayingState;
        GameplayManager.Instance.OnGameplayGameOver += OnGameOverState;
    }

    private void OnWaitingState()
    {
        dashButton.interactable = false;
        countdownText.text = "Waiting for players...";
    }

    private void OnPreStartState()
    {
        dashButton.interactable = false;
        countdownText.text = "Game starting in 3...";
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        yield return new WaitForSeconds(1);
        countdownText.text = "Game starting in 2...";
        yield return new WaitForSeconds(1);
        countdownText.text = "Game starting in 1...";
        yield return new WaitForSeconds(1);
        countdownText.text = "GO!";
    }

    private void OnPlayingState()
    {
        dashButton.interactable = true;
    }

    private void OnGameOverState()
    {
        dashButton.interactable = false;
    }
}
