using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;
using TMPro; 

public class GameplayUI : BaseManager<GameplayUI>
{
    [SerializeField] private Button dashButton;
    [SerializeField] private TextMeshProUGUI countdownText;

    public event Action OnPlayerDash;

    void Awake()
    {
        Init();
    }

    public override void Init()
    {
        dashButton.onClick.AddListener(() => OnPlayerDash?.Invoke());
        GameplayManager.Instance.OnGameplayWaiting += OnWaitingState;
        GameplayManager.Instance.OnGameplayPreStart += OnPreStartState;
        GameplayManager.Instance.OnGameplayPlaying += OnPlayingState;
        GameplayManager.Instance.OnGameplayGameOver += OnGameOverState;
    }

    void OnWaitingState(){
        dashButton.interactable = false;
        countdownText.text = "Waiting for players...";
    }

    void OnPreStartState(){
        // Countdown 3,2,1
        dashButton.interactable = false;
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        countdownText.text = "3";
        yield return new WaitForSeconds(1);
        countdownText.text = "2";
        yield return new WaitForSeconds(1);
        countdownText.text = "1";
        yield return new WaitForSeconds(1);
        countdownText.text = "GO!";
        yield return new WaitForSeconds(1);
        countdownText.text = "";
    }

    void OnPlayingState(){
        dashButton.interactable = true;
        countdownText.gameObject.SetActive(false);
    }

    void OnGameOverState(){
        dashButton.interactable = false;
        countdownText.text = "Game Over";
        countdownText.gameObject.SetActive(true);
    }




}
