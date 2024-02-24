using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AntiSpamButton : MonoBehaviour
{
    private Button _thisButton;
    
    void Start()
    {
        _thisButton = GetComponent<Button>();
        _thisButton.onClick.AddListener(AntiSpam);
    }

    private void AntiSpam()
    {
        _thisButton.enabled = false;
        StartCoroutine(WaitToRepawn());
    }

    IEnumerator WaitToRepawn()
    {
        yield return WaitForSecondCache.WAIT_TIME_ONE;
        _thisButton.enabled = true;
    }
}
