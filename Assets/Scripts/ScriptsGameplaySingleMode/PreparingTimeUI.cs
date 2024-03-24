using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreparingTimeUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI countdownText;

    private void OnEnable()
    {
        countdownText = GetComponent<TMPro.TextMeshProUGUI>();
        SubcribeEvents();
    }

    private void SubcribeEvents()
    {
        GameplayManagerS.Instance.OnGamePreparingAction += OnGamePreparing;
    }

    private void OnGamePreparing()
    {
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true);
        countdownText.text = "3";
        yield return new WaitForSeconds(1f);
        countdownText.text = "2";
        yield return new WaitForSeconds(1f);
        countdownText.text = "1";
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }



}