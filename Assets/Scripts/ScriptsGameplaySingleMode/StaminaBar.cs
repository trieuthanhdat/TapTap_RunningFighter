using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    Slider playerStaminaBar;
    private ICharacter player;

    void Start()
    {
        playerStaminaBar = GetComponent<Slider>();
    }

    void LateUpdate()
    {
        // playerStaminaBar.value = player.CurrentPercentageStamina();
        playerStaminaBar.value = Mathf.Lerp(playerStaminaBar.value, player.CurrentPercentageStamina(), Time.deltaTime * 10);
    }

    public void CreateStaminaBar(ICharacter player)
    {
        this.player = player;
    }
}
